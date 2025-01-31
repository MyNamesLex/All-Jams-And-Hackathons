using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;


#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    GridManager gridManager => (GridManager)target;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous Grid"))
        {
            LoadBlockDefinitions();
            gridManager.SwitchToGrid(gridManager.gridDataAssets.IndexOf(gridManager.activeGridDataAsset) - 1);
        }
        if (GUILayout.Button("Next Grid"))
        {
            LoadBlockDefinitions();
            gridManager.SwitchToGrid(gridManager.gridDataAssets.IndexOf(gridManager.activeGridDataAsset) + 1);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Cache All Grids"))
        {
            CacheAllGrids();
        }

        if (GUILayout.Button("Load Active Grid"))
        {
            LoadBlockDefinitions();
            gridManager.LoadGridDataAsset(gridManager.activeGridDataAsset);
        }

        if (GUILayout.Button("Remove Active Grid"))
        {
            gridManager.RemoveActiveGrid();
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Load Block Definitions"))
        {
            LoadBlockDefinitions();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Block Definitions", EditorStyles.boldLabel);
        foreach (BlockDefinition def in gridManager.blockList)
        {
            EditorGUILayout.LabelField($"ID={def.blockID}", def.blockName);
        }
    }

    public void CacheAllGrids()
    {
        gridManager.gridDataAssets.Clear();

        string[] guids = AssetDatabase.FindAssets("t:GridDataAsset");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<GridDataAsset>(path);
            if (asset != null)
                gridManager.gridDataAssets.Add(asset);
        }
    }

    private void LoadBlockDefinitions()
    {
        gridManager.blockList.Clear();
        gridManager.blockDict.Clear();

        // Find all block definitions
        string[] guids = AssetDatabase.FindAssets("t:BlockDefinition");
        HashSet<int> usedIDs = new HashSet<int>();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var def = AssetDatabase.LoadAssetAtPath<BlockDefinition>(path);
            if (def != null && !usedIDs.Contains(def.blockID))
            {
                gridManager.blockList.Add(def);
                gridManager.blockDict[def.blockID] = def;
                usedIDs.Add(def.blockID);
            }
        }

        // Sort by ID
        gridManager.blockList.Sort((a, b) => a.blockID.CompareTo(b.blockID));
    }
}

#endif

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    private PlayerMovement PlayerMovement => PlayerMovement.Instance;
    private AudioManager AudioManager => AudioManager.Instance;

    [Header("Available Grid Data Assets")]
    public List<GridDataAsset> gridDataAssets;
    public GridDataAsset activeGridDataAsset;
    public Transform Player;
    public Transform Camera;

    [Header("Level Transition")]
    public Image LevelSplash;

    [Header("NavMesh")]
    public GameObject NavMeshBuilder;
    public List<LocalNavMeshBuilder> NavMeshBuilders = new List<LocalNavMeshBuilder>();

    [Header("Block Grid")]
    public Dictionary<int, BlockDefinition> blockDict = new();
    private BlockDefinition[,,] blockGrid;
    public List<BlockDefinition> blockList = new();
    public GameObject ActiveDecoParent;
    public Material PostProcessMaterial;
    private int[,,] activeGrid;
    private int gridSizeX = 10;
    private int gridSizeY = 10;
    private int gridSizeZ = 10;


    private void Awake()
    {
        Instance = this;

        foreach (var def in blockList)
        {
            if (!blockDict.ContainsKey(def.blockID))
                blockDict.Add(def.blockID, def);
        }
    }

    private void Start()
    {
        LoadGridDataAsset(gridDataAssets[0]);
    }

    public void SwitchToGrid(int index)
    {
        if (index >= gridDataAssets.Count)
        {
            index = 0;
        }

        if (index < 0)
        {
            index = gridDataAssets.Count - 1;
        }

        LoadGridDataAsset(gridDataAssets[index]);

        // AudioManager.BGMChange(index);
    }

    public void ReplaceBlock(Vector3Int position, int newBlockID)
    {
        if (position.x < 0 || position.x >= gridSizeX ||
            position.y < 0 || position.y >= gridSizeY ||
            position.z < 0 || position.z >= gridSizeZ)
        {
            Debug.LogWarning("Position out of grid bounds.");
            return;
        }

        // Update activeGrid (int IDs)
        activeGrid[position.x, position.y, position.z] = newBlockID;

        // Update blockGrid (BlockDefinitions)
        BlockDefinition newDef = null;
        if (newBlockID != 0 && !blockDict.TryGetValue(newBlockID, out newDef))
            Debug.LogWarning($"Block ID {newBlockID} not found in blockDict.");
        blockGrid[position.x, position.y, position.z] = newDef;

        // Regenerate affected meshes
        List<Vector3Int> affectedPositions = GetAffectedPositions(position);
        GridMeshGenerator.RegenerateArea(affectedPositions, blockGrid);
    }

    private List<Vector3Int> GetAffectedPositions(Vector3Int position)
    {
        List<Vector3Int> affected = new()
        {
            position,
            // Add adjacent positions
            position + Vector3Int.right,
            position + Vector3Int.left,
            position + Vector3Int.up,
            position + Vector3Int.down,
            position + Vector3Int.forward,
            position + Vector3Int.back
        };
        // Filter valid positions
        return affected.FindAll(pos =>
            pos.x >= 0 && pos.x < gridSizeX &&
            pos.y >= 0 && pos.y < gridSizeY &&
            pos.z >= 0 && pos.z < gridSizeZ
        );
    }

    public void LoadGridDataAsset(GridDataAsset gridAsset)
    {
        if (Application.isPlaying && PlayerMovement.isPossessing)
        {
            PlayerMovement.ResetPlayerState();
        }

        if (Application.isPlaying)
        {
            AudioManager.PlayBGM(gridAsset.LevelMusic);
            AnimateLevelSplash(gridAsset);
        }

        RemoveActiveGrid();

        if (gridAsset == null)
        {
            Debug.LogWarning("GridDataAsset is null, cannot load.");
            return;
        }
        activeGridDataAsset = gridAsset;

        if (gridAsset.gridDecorationPrefab != null)
        {
            ActiveDecoParent = Instantiate(activeGridDataAsset.gridDecorationPrefab, transform);
        }

        // Synchronize the local fields with the asset's size
        gridSizeX = gridAsset.width;
        gridSizeY = gridAsset.length;
        gridSizeZ = gridAsset.height;

        // Allocate activeGrid
        activeGrid = new int[gridSizeX, gridSizeY, gridSizeZ];

        int index = 0;
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    activeGrid[x, y, z] = gridAsset.cells[index++];
                }
            }
        }

        Camera.transform.position = gridAsset.CamStartPos;
        Camera.transform.eulerAngles = gridAsset.CamStartRot;
        PostProcessMaterial.SetFloat("_ColorThreshhold", gridAsset.ThresholdAmount);
        Player.position = gridAsset.SpawnLocation;

        GenerateMeshes();

        RebuildNmb();
    }

    private void AnimateLevelSplash(GridDataAsset asset)
    {
        LevelSplash.rectTransform.localScale = Vector3.zero;
        LevelSplash.sprite = asset.LevelSplash;
        if (Application.isPlaying)
        {
            LevelSplash.gameObject.SetActive(true);

            LevelSplash.rectTransform.DOScale(1, 0.25f).SetEase(Ease.OutBack);

            LevelSplash.DOFade(1, 0.1f).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                LevelSplash.DOFade(0, 1).SetEase(Ease.OutCubic).SetDelay(2.5f).OnComplete(() =>
                {
                    LevelSplash.gameObject.SetActive(false);
                });
            });
        }
    }

    private void RebuildNmb()
    {
        var meshFilters = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
        foreach (var mf in meshFilters)
        {
            string n = mf.gameObject.name;
            if (n.EndsWith("_Floors"))
            {
                if (mf.sharedMesh.vertexCount == 0 || mf.sharedMesh.triangles.Length == 0)
                {
                    Debug.LogWarning($"Skipping empty mesh: {mf.gameObject.name}");
                    continue;
                }

                LocalNavMeshBuilder lnmb = NavMeshBuilder.AddComponent<LocalNavMeshBuilder>();
                lnmb.m_Size = new Vector3(80, 40, 80);
                lnmb.m_Tracked = mf.gameObject.transform;
                NavMeshBuilders.Add(lnmb);
            }
        }
    }

    public void RemoveActiveGrid()
    {
        GridMeshGenerator.DestroyExistingMeshObjects();

        // Destroy decoration parent
        if (ActiveDecoParent != null)
        {
            if (Application.isPlaying) Destroy(ActiveDecoParent);
            else DestroyImmediate(ActiveDecoParent);
        }

        activeGrid = null;

        // Remove all LocalNavMeshBuilder components from NavMeshBuilder
        var existingBuilders = NavMeshBuilder.GetComponents<LocalNavMeshBuilder>();
        foreach (var builder in existingBuilders)
        {
            if (Application.isPlaying)
                Destroy(builder);
            else
                DestroyImmediate(builder);
        }
        NavMeshBuilders.Clear();
    }


    public void GenerateMeshes()
    {
        BlockDefinition[,,] blockGrid = new BlockDefinition[gridSizeX, gridSizeY, gridSizeZ];
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    int id = activeGrid[x, y, z];
                    if (id == 0)
                        blockGrid[x, y, z] = null;
                    else if (blockDict.TryGetValue(id, out BlockDefinition def))
                        blockGrid[x, y, z] = def;
                    else
                        blockGrid[x, y, z] = null; // unknown ID => treat as empty
                }
            }
        }

        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        GridMeshGenerator.GenerateMeshesFromGrid(blockGrid);
        sw.Stop();
        Debug.Log($"Mesh generation took {sw.Elapsed.TotalMilliseconds:F3}ms");
    }



    public void RegenerateBlocks()
    {

    }


}
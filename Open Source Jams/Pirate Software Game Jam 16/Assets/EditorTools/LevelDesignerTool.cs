#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LevelDesignerTool : EditorWindow
{
    // 3D grid
    private static int gridSizeX = 10;
    private static int gridSizeY = 10;
    private static int gridSizeZ = 10;
    private static int lastGridSizeX;
    private static int lastGridSizeY;
    private static int lastGridSizeZ;
    private static int[,,] gridData;

    private static GridDataAsset activeGridDataAsset;

    private List<BlockDefinition> blockList = new List<BlockDefinition>();
    private Dictionary<int, BlockDefinition> blockDict = new Dictionary<int, BlockDefinition>();

    private int selectedBlockIndex = 0;
    private int currentYSlice = 0;
    private bool isPainting = false;

    // Brush sliders
    private int brushWidth = 1;
    private int brushHeight = 1;
    private int brushLength = 1;

    [MenuItem("Tools/LevelDesignerTool")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<LevelDesignerTool>("Level Designer Tool");
        wnd.Show();
    }

    private void OnEnable()
    {
        gridData = new int[gridSizeX, gridSizeY, gridSizeZ];

        LoadBlockDefinitions();

        // Hook into SceneView
        SceneView.duringSceneGui += OnSceneGUI;
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void LoadBlockDefinitions()
    {
        blockList.Clear();
        blockDict.Clear();

        // Find all block definitions
        string[] guids = AssetDatabase.FindAssets("t:BlockDefinition");
        HashSet<int> usedIDs = new HashSet<int>();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var def = AssetDatabase.LoadAssetAtPath<BlockDefinition>(path);
            if (def != null && !usedIDs.Contains(def.blockID))
            {
                blockList.Add(def);
                blockDict[def.blockID] = def;
                usedIDs.Add(def.blockID);
            }
        }

        // Sort by ID
        blockList.Sort((a, b) => a.blockID.CompareTo(b.blockID));
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene Block Painter", EditorStyles.boldLabel);

        // Grid size
        EditorGUILayout.BeginHorizontal();
        lastGridSizeX = EditorGUILayout.IntField("Grid Size X", gridSizeX, GUILayout.ExpandWidth(true), GUILayout.MinWidth(50));
        lastGridSizeY = EditorGUILayout.IntField("Grid Size Y", gridSizeY, GUILayout.ExpandWidth(true), GUILayout.MinWidth(50));
        lastGridSizeZ = EditorGUILayout.IntField("Grid Size Z", gridSizeZ, GUILayout.ExpandWidth(true), GUILayout.MinWidth(50));
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Create Grid Data Asset from Current Grid"))
        {
            CreateGridDataAsset();
        }

        if (lastGridSizeX != gridSizeX || lastGridSizeY != gridSizeY || lastGridSizeZ != gridSizeZ)
        {
            gridSizeX = lastGridSizeX;
            gridSizeY = lastGridSizeY;
            gridSizeZ = lastGridSizeZ;
            gridData = new int[gridSizeX, gridSizeY, gridSizeZ];
            GenerateMeshes();
        }

        // "Y slice" slider
        currentYSlice = EditorGUILayout.IntSlider("Y Slice (vertical)", currentYSlice, 0, gridSizeY - 1);

        // Brush size sliders
        brushWidth = EditorGUILayout.IntSlider("Brush Width (X)", brushWidth, 1, gridSizeX);
        brushHeight = EditorGUILayout.IntSlider("Brush Height (Y)", brushHeight, 1, gridSizeY);
        brushLength = EditorGUILayout.IntSlider("Brush Length (Z)", brushLength, 1, gridSizeZ);

        if (GUILayout.Button("Reload Block Definitions"))
        {
            LoadBlockDefinitions();
        }
        ;

        activeGridDataAsset = EditorGUILayout.ObjectField("Active Grid Data Asset", activeGridDataAsset, typeof(GridDataAsset), false) as GridDataAsset;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Import Grid"))
        {
            ImportGrid(activeGridDataAsset);
        }

        if (GUILayout.Button("Update Active Grid"))
        {
            UpdateActiveGrid();
        }
        EditorGUILayout.EndHorizontal();

        // Build an array for the block palette: first is "Empty"
        int totalCount = blockList.Count + 1;
        string[] names = new string[totalCount];
        names[0] = "Empty";
        for (int i = 0; i < blockList.Count; i++)
        {
            names[i + 1] = blockList[i].blockName + $" (ID={blockList[i].blockID})";
        }

        selectedBlockIndex = GUILayout.SelectionGrid(selectedBlockIndex, names, 3);
        if (selectedBlockIndex == 0)
        {
            GUILayout.Label("Selected: Empty");
        }
        else
        {
            int idx = selectedBlockIndex - 1;
            if (idx >= 0 && idx < blockList.Count)
            {
                var bdef = blockList[idx];
                GUILayout.Label($"Selected: {bdef.blockName} (ID={bdef.blockID})");
            }
        }

        // Manual mesh generation button
        if (GUILayout.Button("Generate Meshes"))
        {
            GenerateMeshes();
        }

        if (GUILayout.Button("Close Window"))
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            Close();
        }
    }

    private void ImportGrid(GridDataAsset GDA)
    {
        gridSizeX = GDA.width;
        gridSizeY = GDA.length;
        gridSizeZ = GDA.height;
        gridData = new int[gridSizeX, gridSizeY, gridSizeZ];

        int index = 0;
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    gridData[x, y, z] = GDA.cells[index++];
                }
            }
        }

        GenerateMeshes();
    }

    private void UpdateActiveGrid()
    {
        if (activeGridDataAsset == null)
        {
            Debug.LogWarning("No active GridDataAsset to update.");
            return;
        }

        activeGridDataAsset.width = gridSizeX;
        activeGridDataAsset.length = gridSizeY;
        activeGridDataAsset.height = gridSizeZ;

        activeGridDataAsset.cells = new int[gridSizeX * gridSizeY * gridSizeZ];
        int index = 0;
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    activeGridDataAsset.cells[index++] = gridData[x, y, z];
                }
            }
        }

        EditorUtility.SetDirty(activeGridDataAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void CreateGridDataAsset()
    {
        string path = "Assets/Levels/SavedGrids/NewGridDataAsset.asset";
        path = AssetDatabase.GenerateUniqueAssetPath(path);

        if (!string.IsNullOrEmpty(path))
        {
            // 1. Create the ScriptableObject instance in memory
            GridDataAsset asset = CreateInstance<GridDataAsset>();

            // 2. Copy dimensions
            asset.width = gridSizeX;
            asset.length = gridSizeY;
            asset.height = gridSizeZ;

            // 3. Copy cell data (convert int[,,] to int[])
            asset.cells = new int[gridSizeX * gridSizeY * gridSizeZ];
            int index = 0;
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    for (int x = 0; x < gridSizeX; x++)
                    {
                        asset.cells[index++] = gridData[x, y, z];
                    }
                }
            }

            int nonZero = 0;
            for (int y = 0; y < gridSizeY; y++)
                for (int x = 0; x < gridSizeX; x++)
                    for (int z = 0; z < gridSizeZ; z++)
                        if (gridData[x, y, z] != 0) nonZero++;
            Debug.Log($"There are {nonZero} blocks in activeGrid.");


            // 4. Create it in the Project as an actual .asset file
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Created new GridDataAsset at: " + path);
        }
        else
        {
            Debug.Log("Save operation canceled.");
        }
    }


    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        float planeY = currentYSlice;
        Plane slicePlane = new(Vector3.up, new Vector3(0, planeY, 0));

        Vector3 p0 = new(0, planeY, 0);
        Vector3 p1 = new(gridSizeX, planeY, 0);
        Vector3 p2 = new(gridSizeX, planeY, gridSizeZ);
        Vector3 p3 = new(0, planeY, gridSizeZ);
        Handles.DrawSolidRectangleWithOutline(new Vector3[] { p0, p1, p2, p3 },
                                              new Color(1, 1, 0, 0.1f),
                                              Color.yellow);

        // Draw grid lines
        Handles.color = new Color(1f, 1f, 0f, 0.2f);

        // Vertical lines (along Z-axis)
        for (int x = 0; x <= gridSizeX; x++)
        {
            Vector3 start = new Vector3(x, planeY, 0);
            Vector3 end = new Vector3(x, planeY, gridSizeZ);
            Handles.DrawLine(start, end);
        }

        // Horizontal lines (along X-axis)
        for (int z = 0; z <= gridSizeZ; z++)
        {
            Vector3 start = new Vector3(0, planeY, z);
            Vector3 end = new Vector3(gridSizeX, planeY, z);
            Handles.DrawLine(start, end);
        }

        if (e.button == 0 && !e.alt)
        {
            if (e.type == EventType.MouseDown)
            {
                isPainting = true;
                PaintCellUnderMouse(e, slicePlane);
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && isPainting)
            {
                PaintCellUnderMouse(e, slicePlane);
                e.Use();
            }
            else if (e.type == EventType.MouseUp)
            {
                isPainting = false;
                e.Use();
            }
        }
    }

    private void PaintCellUnderMouse(Event e, Plane slicePlane)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (slicePlane.Raycast(ray, out float enter))
        {
            Vector3 hitPos = ray.GetPoint(enter);

            int baseX = Mathf.FloorToInt(hitPos.x);
            int baseZ = Mathf.FloorToInt(hitPos.z);

            // For each cell in the brush region:
            for (int bx = 0; bx < brushWidth; bx++)
            {
                for (int by = 0; by < brushHeight; by++)
                {
                    for (int bz = 0; bz < brushLength; bz++)
                    {
                        int xCoord = baseX + bx;
                        int yCoord = currentYSlice + by;
                        int zCoord = baseZ + bz;

                        // clamp
                        if (xCoord < 0 || xCoord >= gridSizeX) continue;
                        if (yCoord < 0 || yCoord >= gridSizeY) continue;
                        if (zCoord < 0 || zCoord >= gridSizeZ) continue;

                        if (selectedBlockIndex == 0)
                        {
                            // remove
                            gridData[xCoord, yCoord, zCoord] = 0;
                        }
                        else
                        {
                            int idx = selectedBlockIndex - 1;
                            if (idx >= 0 && idx < blockList.Count)
                            {
                                int chosenID = blockList[idx].blockID;
                                gridData[xCoord, yCoord, zCoord] = chosenID;
                            }
                        }
                    }
                }
            }

            GenerateMeshes();
        }
    }

    /// <summary>
    /// Convert int grid to BlockDefinition grid, then call your GridMeshGenerator
    /// </summary>
    private static void GenerateMeshes()
    {
        if (gridData == null) return;

        var window = GetWindow<LevelDesignerTool>();
        if (window == null) return;
        Dictionary<int, BlockDefinition> dict = window.blockDict;

        int sizeX = gridSizeX;
        int sizeY = gridSizeY;
        int sizeZ = gridSizeZ;

        var blockGrid = new BlockDefinition[sizeX, sizeY, sizeZ];
        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    int id = gridData[x, y, z];
                    if (id == 0)
                        blockGrid[x, y, z] = null;
                    else if (dict.TryGetValue(id, out BlockDefinition def))
                        blockGrid[x, y, z] = def;
                    else
                        blockGrid[x, y, z] = null;
                }
            }
        }

        // Now call your actual mesh generator
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        GridMeshGenerator.GenerateMeshesFromGrid(blockGrid);
        sw.Stop();
        Debug.Log($"Mesh generation took {sw.Elapsed.TotalMilliseconds:F3}ms");
    }
}
#endif
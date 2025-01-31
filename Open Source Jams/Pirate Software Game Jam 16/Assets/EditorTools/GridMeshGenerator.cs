using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public static class GridMeshGenerator
{
    private static Dictionary<BlockDefinition, GameObject> floorObjects = new();
    private static Dictionary<BlockDefinition, GameObject> ceilingObjects = new();
    private static Dictionary<BlockDefinition, GameObject> wallObjects = new();
    private static Dictionary<BlockDefinition, GeometryBuffers> geometryMap = new();

    public static float cellSize = 1f;

    public static void GenerateMeshesFromGrid(BlockDefinition[,,] grid)
    {
        // Remove old objects
        DestroyExistingMeshObjects();

        int sizeX = grid.GetLength(0);
        int sizeY = grid.GetLength(1);
        int sizeZ = grid.GetLength(2);

        // Dictionary: blockDef -> its “buffers” (floor, ceiling, wall)
        var geometryMap = new Dictionary<BlockDefinition, GeometryBuffers>();

        // Build geometry
        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    var block = grid[x, y, z];
                    if (block == null) continue;
                    if (block.blockType == BlockDefinition.BlockType.Breakable)
                        continue;

                    if (!geometryMap.ContainsKey(block))
                    {
                        geometryMap[block] = new GeometryBuffers();
                    }

                    var buffers = geometryMap[block];
                    var neighbors = GetNeighbors(grid, x, y, z);

                    // Position in world
                    Vector3 pos = new Vector3(x * cellSize, y * cellSize, z * cellSize);

                    block.GenerateFaces(
                        neighbors, pos,
                        buffers.floorVerts, buffers.floorTris,
                        buffers.ceilingVerts, buffers.ceilingTris,
                        buffers.wallVerts, buffers.wallTris,
                        cellSize, cellSize
                    );
                }
            }
        }

        //TODO: 
        // Build a breakable block object that can be destroyed that updates the mesh when destroyed
        // combine mesh into 1 unified collider
        // make it good

        // Mesh Optimization, Greedy Meshing, or Breadth First Search Meshing, or Voxel Ray Meshing
        // idk whichever works best probably, Greedy would likely be easiest to implement 
        // while being a tradeoff with not being the fastest


        foreach (var kvp in geometryMap)
        {
            var blockDef = kvp.Key;
            var geoBuf = kvp.Value;

            BuildFloorObject(blockDef, geoBuf);
            BuildCeilingObject(blockDef, geoBuf);
            BuildWallObject(blockDef, geoBuf);
        }
    }

    public static void RegenerateArea(List<Vector3Int> positions, BlockDefinition[,,] grid)
    {
        HashSet<BlockDefinition> affectedBlocks = new();
        foreach (var pos in positions)
        {
            var block = grid[pos.x, pos.y, pos.z];
            if (block != null && block.blockType != BlockDefinition.BlockType.Breakable)
                affectedBlocks.Add(block);
        }

        foreach (var blockDef in affectedBlocks)
        {
            if (!geometryMap.ContainsKey(blockDef))
                geometryMap[blockDef] = new GeometryBuffers();

            // Reprocess all blocks of this type
            for (int x = 0; x < grid.GetLength(0); x++)
                for (int y = 0; y < grid.GetLength(1); y++)
                    for (int z = 0; z < grid.GetLength(2); z++)
                    {
                        if (grid[x, y, z] != blockDef) continue;
                        var neighbors = GetNeighbors(grid, x, y, z);
                        Vector3 posWorld = new(x * cellSize, y * cellSize, z * cellSize);
                        blockDef.GenerateFaces(
                            neighbors, posWorld,
                            geometryMap[blockDef].floorVerts, geometryMap[blockDef].floorTris,
                            geometryMap[blockDef].ceilingVerts, geometryMap[blockDef].ceilingTris,
                            geometryMap[blockDef].wallVerts, geometryMap[blockDef].wallTris,
                            cellSize, cellSize
                        );
                    }

            // Update or create mesh objects
            BuildFloorObject(blockDef, geometryMap[blockDef]);
            BuildCeilingObject(blockDef, geometryMap[blockDef]);
            BuildWallObject(blockDef, geometryMap[blockDef]);
        }
    }

    private static void BuildBReakableObjects(BlockDefinition blockDef, GeometryBuffers geoBuf)
    {
        // split the blocks to seperate game objects per breakable object
        // merge adjacent breakable blocks into one object
        // when rammed replace with room blocks

        //probably seperate component for breakables that grabs the grid and updates the mesh when destroyed
        //grid manager needed for this
    }

    // public static void DestoryTargetBlock(Vector3Int targetBlock)
    // {
    //     BlockDefinition[,,] grid = GridManager.GetGrid();
    // }

    // private static List<Vector3Int> GetAffectedArea(Vector3Int changedBlock)
    // {
    //     var affected = new List<Vector3Int> { changedBlock };

    //     // Add direct neighbors (6 directions)
    //     affected.Add(changedBlock + Vector3Int.right);
    //     affected.Add(changedBlock + Vector3Int.left);
    //     affected.Add(changedBlock + Vector3Int.up);
    //     affected.Add(changedBlock + Vector3Int.down);
    //     affected.Add(changedBlock + Vector3Int.forward);
    //     affected.Add(changedBlock + Vector3Int.back);

    //     return affected.Where(IsValidGridPosition).ToList();
    // }

    // private static void RegenerateArea(List<Vector3Int> affectedPositions, BlockDefinition[,,] grid)
    // {
    //     var buffers = new GeometryBuffers();

    //     foreach (var pos in affectedPositions)
    //     {
    //         var block = grid[pos.x, pos.y, pos.z];
    //         if (block == null) continue;

    //         var neighbors = GetNeighbors(grid, pos.x, pos.y, pos.z);
    //         Vector3 worldPos = GridToWorld(pos);

    //         block.GenerateFaces(
    //             neighbors, worldPos,
    //             buffers.floorVerts, buffers.floorTris,
    //             buffers.ceilingVerts, buffers.ceilingTris,
    //             buffers.wallVerts, buffers.wallTris,
    //             cellSize, cellSize
    //         );
    //     }

    //     UpdateExistingMeshes(buffers, affectedPositions);
    // }

    private static void UpdateExistingMeshes(GeometryBuffers buffers, List<Vector3Int> affectedPositions)
    {
        // Remove old geometry for this position from all buffers
        // RemoveGeometryAtPosition(pos);
        // Regenerate geometry for this position based on current grid state
        // RegenerateGeometryAtPosition(pos);

        // Rebuild affected meshes
        // RebuildAffectedMeshes();
    }

    /// <summary>
    /// Build a separate “Floors” object using the floorVerts/floorTris
    /// </summary>
    private static void BuildFloorObject(BlockDefinition blockDef, GeometryBuffers geoBuf)
    {
        if (geoBuf.floorVerts.Count == 0) return;

        GameObject go = null;
        go = new GameObject(blockDef.blockName + "_Floors");
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        MeshCollider mc = go.AddComponent<MeshCollider>();
        NavMeshSurface nms = go.AddComponent<NavMeshSurface>();
        NavMeshSourceTag nmst = go.AddComponent<NavMeshSourceTag>();

        floorObjects[blockDef] = go;

        Mesh mesh = new Mesh();
        mf.sharedMesh = mesh;

        mesh.vertices = geoBuf.floorVerts.ToArray();
        mesh.triangles = geoBuf.floorTris.ToArray();
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        mc.sharedMesh = mesh;
        mesh.UploadMeshData(false);

        go.GetComponent<MeshRenderer>().material = blockDef.FloorMaterial;
        go.layer = LayerMask.NameToLayer("Obstacle");
        //Mesh Optimization pass
        //POSSIBLY CUT FOR TIME because it caused so many fucking bugs
    }

    public static void UpdateMeshesForBlocks(List<Vector3Int> affectedPositions)
    {
        foreach (var pos in affectedPositions)
        {
            // Remove old geometry for this position from all buffers

            // RemoveGeometryAtPosition(pos);
            // Regenerate geometry for this position based on current grid state

            // RegenerateGeometryAtPosition(pos);
        }
        // Rebuild affected meshes
        // RebuildAffectedMeshes();
    }

    /// <summary>
    /// Build a separate “Ceilings” object
    /// </summary>
    private static void BuildCeilingObject(BlockDefinition blockDef, GeometryBuffers geoBuf)
    {
        if (geoBuf.ceilingVerts.Count == 0) return;

        GameObject go = new GameObject(blockDef.blockName + "_Ceilings");
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        MeshCollider mc = go.AddComponent<MeshCollider>();

        Mesh ceilMesh = new Mesh();
        ceilMesh.vertices = geoBuf.ceilingVerts.ToArray();
        ceilMesh.triangles = geoBuf.ceilingTris.ToArray();
        ceilMesh.RecalculateNormals();
        mf.mesh = ceilMesh;
        mc.sharedMesh = ceilMesh;

        if (blockDef.CielingMaterial != null) mr.material = blockDef.CielingMaterial;
        go.layer = LayerMask.NameToLayer("Obstacle");

        //Mesh Optimization pass
    }

    /// <summary>
    /// Build a separate “Walls” object
    /// </summary>
    private static void BuildWallObject(BlockDefinition blockDef, GeometryBuffers geoBuf)
    {
        if (geoBuf.wallVerts.Count == 0) return;

        GameObject go = new GameObject(blockDef.blockName + "_Walls");
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        MeshCollider mc = go.AddComponent<MeshCollider>();

        Mesh wallMesh = new Mesh();
        wallMesh.vertices = geoBuf.wallVerts.ToArray();
        wallMesh.triangles = geoBuf.wallTris.ToArray();
        wallMesh.RecalculateNormals();
        mf.mesh = wallMesh;
        mc.sharedMesh = wallMesh;

        if (blockDef.WallMaterial != null) mr.material = blockDef.WallMaterial;
        go.layer = LayerMask.NameToLayer("Obstacle");

        //Mesh Optimization pass
    }


    public static void DestroyExistingMeshObjects()
    {
        var meshFilters = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
        foreach (var mf in meshFilters)
        {
            string n = mf.gameObject.name;
            if (n.EndsWith("_Floors") || n.EndsWith("_Walls") || n.EndsWith("_Ceilings") || n.EndsWith("_Mesh"))
            {
                if (Application.isPlaying)
                    Object.Destroy(mf.gameObject);
                else
                    Object.DestroyImmediate(mf.gameObject);
            }
        }
    }

    private static BlockDefinition[] GetNeighbors(BlockDefinition[,,] grid, int x, int y, int z)
    {
        int sizeX = grid.GetLength(0);
        int sizeY = grid.GetLength(1);
        int sizeZ = grid.GetLength(2);

        // neighbor order: [0=+X, 1=-X, 2=+Y, 3=-Y, 4=+Z, 5=-Z]
        BlockDefinition[] result = new BlockDefinition[6];

        if (x + 1 < sizeX) result[0] = grid[x + 1, y, z];
        if (x - 1 >= 0) result[1] = grid[x - 1, y, z];
        if (y + 1 < sizeY) result[2] = grid[x, y + 1, z];
        if (y - 1 >= 0) result[3] = grid[x, y - 1, z];
        if (z + 1 < sizeZ) result[4] = grid[x, y, z + 1];
        if (z - 1 >= 0) result[5] = grid[x, y, z - 1];

        return result;
    }
}

/// <summary>
/// Storage for floor/ceiling/wall geometry lists
/// for a single BlockDefinition
/// </summary>
public class GeometryBuffers
{
    public List<Vector3> BreakableVerts = new List<Vector3>();
    public List<int> BreakableTris = new List<int>();

    public List<Vector3> floorVerts = new List<Vector3>();
    public List<int> floorTris = new List<int>();

    public List<Vector3> ceilingVerts = new List<Vector3>();
    public List<int> ceilingTris = new List<int>();

    public List<Vector3> wallVerts = new List<Vector3>();
    public List<int> wallTris = new List<int>();
}

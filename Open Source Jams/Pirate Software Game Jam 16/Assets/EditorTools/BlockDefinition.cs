using UnityEngine;
using System.Collections.Generic;

public abstract class BlockDefinition : ScriptableObject
{
    public int blockID;
    public string blockName;

    public Material FloorMaterial;
    public Material CielingMaterial;
    public Material WallMaterial;

    //public GameObjectDecoration[] decorations;
    public float decoChance;

    public enum BlockType
    {
        Empty,
        Room,
        Pipe,
        Breakable,
    }
    public BlockType blockType;

    /// <summary>
    /// Generate geometry for floors, ceilings, walls in separate lists.
    /// </summary>
    public abstract void GenerateFaces(
        BlockDefinition[] neighbors,
        Vector3 position,

        // Floor geometry
        List<Vector3> floorVerts,
        List<int> floorTris,

        // Ceiling geometry
        List<Vector3> ceilingVerts,
        List<int> ceilingTris,

        // Walls geometry
        List<Vector3> wallVerts,
        List<int> wallTris,

        float cellSize,
        float cellHeight
    );
}

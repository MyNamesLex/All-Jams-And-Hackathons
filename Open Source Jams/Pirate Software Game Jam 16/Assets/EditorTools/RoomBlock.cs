using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Block/Room")]
public class RoomBlock : BlockDefinition
{
    private void OnValidate()
    {
        blockType = BlockType.Room;
    }

    public override void GenerateFaces(
        BlockDefinition[] neighbors,
        Vector3 position,

        // Floor geometry
        List<Vector3> floorVerts,
        List<int> floorTris,

        // Ceiling geometry
        List<Vector3> ceilingVerts,
        List<int> ceilingTris,

        // Wall geometry
        List<Vector3> wallVerts,
        List<int> wallTris,

        float cellSize,
        float cellHeight
    )
    {
        // neighbors array: [0= +X, 1= -X, 2= +Y, 3= -Y, 4= +Z, 5= -Z]

        // If up is empty -> add top face -> goes to ceiling geometry
        if (neighbors[2] == null || neighbors[2].blockType == BlockType.Empty)
        {
            MeshUtility.AddFaceUp(ceilingVerts, ceilingTris, position, cellSize, cellHeight);
        }
        // If down is empty -> bottom face -> floor geometry
        if (neighbors[3] == null || neighbors[3].blockType == BlockType.Empty)
        {
            MeshUtility.AddFaceDown(floorVerts, floorTris, position, cellSize, cellHeight);
        }

        // +X -> if we need wall -> add to wall geometry
        if (NeedWallWithNeighbor(neighbors[0]))
        {
            MeshUtility.AddFaceRight(wallVerts, wallTris, position, cellSize, cellHeight);
        }
        // -X
        if (NeedWallWithNeighbor(neighbors[1]))
        {
            MeshUtility.AddFaceLeft(wallVerts, wallTris, position, cellSize, cellHeight);
        }
        // +Z
        if (NeedWallWithNeighbor(neighbors[4]))
        {
            MeshUtility.AddFaceForward(wallVerts, wallTris, position, cellSize, cellHeight);
        }
        // -Z
        if (NeedWallWithNeighbor(neighbors[5]))
        {
            MeshUtility.AddFaceBack(wallVerts, wallTris, position, cellSize, cellHeight);
        }
    }

    private bool NeedWallWithNeighbor(BlockDefinition neighbor)
    {
        if (neighbor == null) return true;
        if (neighbor.blockType == BlockType.Empty) return true;
        if (neighbor.blockType == BlockType.Pipe)
        {
            // "Pipes remove the voxel wall if adjacent to room"
            return false;
        }
        if (neighbor.blockType == BlockType.Breakable) return true;
        // if neighbor is also Room -> skip internal wall
        return false;
    }
}

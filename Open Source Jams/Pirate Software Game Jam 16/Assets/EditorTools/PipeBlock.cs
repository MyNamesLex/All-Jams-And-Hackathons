using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Block/Pipe")]
public class PipeBlock : BlockDefinition
{
    public enum FaceDir
    {
        Up,
        Down,
        Side
    }

    private void OnValidate()
    {
        blockType = BlockType.Pipe;
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
        // neighbors[2] => up
        if (ShouldPlaceFace(neighbors[2], FaceDir.Up))
        {
            MeshUtility.AddFaceUp(ceilingVerts, ceilingTris, position, cellSize, cellHeight);
        }
        // neighbors[3] => down
        if (ShouldPlaceFace(neighbors[3], FaceDir.Down))
        {
            MeshUtility.AddFaceDown(floorVerts, floorTris, position, cellSize, cellHeight);
        }

        // For +X, -X, +Z, -Z => FaceDir.Side
        if (ShouldPlaceFace(neighbors[0], FaceDir.Side))
        {
            MeshUtility.AddFaceRight(wallVerts, wallTris, position, cellSize, cellHeight);
        }
        if (ShouldPlaceFace(neighbors[1], FaceDir.Side))
        {
            MeshUtility.AddFaceLeft(wallVerts, wallTris, position, cellSize, cellHeight);
        }
        if (ShouldPlaceFace(neighbors[4], FaceDir.Side))
        {
            MeshUtility.AddFaceForward(wallVerts, wallTris, position, cellSize, cellHeight);
        }
        if (ShouldPlaceFace(neighbors[5], FaceDir.Side))
        {
            MeshUtility.AddFaceBack(wallVerts, wallTris, position, cellSize, cellHeight);
        }
    }

    private bool ShouldPlaceFace(BlockDefinition neighbor, FaceDir faceType)
    {
        // if neighbor is null => always place (out of bounds)
        if (neighbor == null)
            return true;

        // If neighbor is empty or breakable -> place
        if (neighbor.blockType == BlockType.Empty ||
            neighbor.blockType == BlockType.Breakable)
            return true;

        // If neighbor is Pipe -> skip the face => open connection
        if (neighbor.blockType == BlockType.Pipe)
        {
            return false;
        }

        // If neighbor is Room -> skip for "side" to open pass,
        if (neighbor.blockType == BlockType.Room)
        {
            // You can do separate logic:
            if (faceType == FaceDir.Up)
            {
                // skip or place?
                return false;
            }
            else if (faceType == FaceDir.Down)
            {
                return false;
            }
            else // side
            {
                return false;
            }
        }

        // default
        return false;
    }

}
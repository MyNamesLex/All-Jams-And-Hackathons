using UnityEngine;
using System.Collections.Generic;

public static class MeshUtility
{
    // I know this is a lot of code, but it's all just copy-pasting the same thing with different values cuz I don't want
    // to make a gigantic switch statement. I'm sure there's a better way to do this, but I don't feel like making it

    // migrate all of this into grid mesh generator for mesh optimization so we do a 1 pass approach
    // will likely need to say rules for each block type and how they generate their geometry but that should be fine

    /// <summary>
    /// Top face at y = pos.y + cellHeight, 
    /// spanning x from pos.x..pos.x+cellSize,
    ///        z from pos.z..pos.z+cellSize.
    /// </summary>
    public static void AddFaceUp(List<Vector3> verts, List<int> tris, Vector3 pos, float cellSize, float cellHeight)
    {
        float yTop = pos.y + cellHeight;
        int start = verts.Count;

        // corners
        Vector3 v0 = new Vector3(pos.x, yTop, pos.z);
        Vector3 v1 = new Vector3(pos.x + cellSize, yTop, pos.z);
        Vector3 v2 = new Vector3(pos.x + cellSize, yTop, pos.z + cellSize);
        Vector3 v3 = new Vector3(pos.x, yTop, pos.z + cellSize);

        verts.Add(v0); verts.Add(v1); verts.Add(v2); verts.Add(v3);

        // Tri winding (v0->v1->v2, v0->v2->v3)
        tris.Add(start + 0);
        tris.Add(start + 1);
        tris.Add(start + 2);
        tris.Add(start + 0);
        tris.Add(start + 2);
        tris.Add(start + 3);
    }

    /// <summary>
    /// Bottom face at y=pos.y, same xz coverage.
    /// </summary>
    public static void AddFaceDown(
        List<Vector3> verts, List<int> tris,
        Vector3 pos, float cellSize, float cellHeight)
    {
        float yBottom = pos.y;
        int start = verts.Count;

        Vector3 v0 = new Vector3(pos.x, yBottom, pos.z);
        Vector3 v1 = new Vector3(pos.x + cellSize, yBottom, pos.z);
        Vector3 v2 = new Vector3(pos.x + cellSize, yBottom, pos.z + cellSize);
        Vector3 v3 = new Vector3(pos.x, yBottom, pos.z + cellSize);

        verts.Add(v0); verts.Add(v1); verts.Add(v2); verts.Add(v3);

        tris.Add(start + 2);
        tris.Add(start + 1);
        tris.Add(start + 0);
        tris.Add(start + 3);
        tris.Add(start + 2);
        tris.Add(start + 0);
    }

    /// <summary>
    /// Right face at x=pos.x+cellSize, 
    /// spanning z from pos.z..pos.z+cellSize, 
    /// and y from pos.y..pos.y+cellHeight.
    /// </summary>
    public static void AddFaceRight(
        List<Vector3> verts, List<int> tris,
        Vector3 pos, float cellSize, float cellHeight)
    {
        float xRight = pos.x + cellSize;
        int start = verts.Count;

        // corners, from bottom-left to top-right along y, then z
        Vector3 v0 = new Vector3(xRight, pos.y, pos.z);
        Vector3 v1 = new Vector3(xRight, pos.y, pos.z + cellSize);
        Vector3 v2 = new Vector3(xRight, pos.y + cellHeight, pos.z + cellSize);
        Vector3 v3 = new Vector3(xRight, pos.y + cellHeight, pos.z);

        verts.Add(v0); verts.Add(v1); verts.Add(v2); verts.Add(v3);

        // Triangles
        // Typically (v0->v1->v2, v0->v2->v3) or some consistent winding
        tris.Add(start + 0);
        tris.Add(start + 1);
        tris.Add(start + 2);
        tris.Add(start + 0);
        tris.Add(start + 2);
        tris.Add(start + 3);
    }

    /// <summary>
    /// Left face at x=pos.x
    /// </summary>
    public static void AddFaceLeft(
        List<Vector3> verts, List<int> tris,
        Vector3 pos, float cellSize, float cellHeight)
    {
        float xLeft = pos.x;
        int start = verts.Count;

        Vector3 v0 = new(xLeft, pos.y, pos.z + cellSize);
        Vector3 v1 = new(xLeft, pos.y, pos.z);
        Vector3 v2 = new(xLeft, pos.y + cellHeight, pos.z);
        Vector3 v3 = new(xLeft, pos.y + cellHeight, pos.z + cellSize);

        verts.Add(v0); verts.Add(v1); verts.Add(v2); verts.Add(v3);

        // Triangles
        tris.Add(start + 0);
        tris.Add(start + 1);
        tris.Add(start + 2);
        tris.Add(start + 0);
        tris.Add(start + 2);
        tris.Add(start + 3);
    }

    /// <summary>
    /// Forward face at z=pos.z+cellSize
    /// </summary>
    public static void AddFaceForward(
        List<Vector3> verts, List<int> tris,
        Vector3 pos, float cellSize, float cellHeight)
    {
        float zFront = pos.z + cellSize;
        int start = verts.Count;

        Vector3 v0 = new Vector3(pos.x, pos.y, zFront);
        Vector3 v1 = new Vector3(pos.x + cellSize, pos.y, zFront);
        Vector3 v2 = new Vector3(pos.x + cellSize, pos.y + cellHeight, zFront);
        Vector3 v3 = new Vector3(pos.x, pos.y + cellHeight, zFront);

        verts.Add(v0); verts.Add(v1); verts.Add(v2); verts.Add(v3);

        tris.Add(start + 2);
        tris.Add(start + 1);
        tris.Add(start + 0);
        tris.Add(start + 3);
        tris.Add(start + 2);
        tris.Add(start + 0);
    }

    /// <summary>
    /// Back face at z=pos.z
    /// </summary>
    public static void AddFaceBack(
        List<Vector3> verts, List<int> tris,
        Vector3 pos, float cellSize, float cellHeight)
    {
        float zBack = pos.z;
        int start = verts.Count;

        Vector3 v0 = new Vector3(pos.x + cellSize, pos.y, zBack);
        Vector3 v1 = new Vector3(pos.x, pos.y, zBack);
        Vector3 v2 = new Vector3(pos.x, pos.y + cellHeight, zBack);
        Vector3 v3 = new Vector3(pos.x + cellSize, pos.y + cellHeight, zBack);

        verts.Add(v0); verts.Add(v1); verts.Add(v2); verts.Add(v3);

        tris.Add(start + 2);
        tris.Add(start + 1);
        tris.Add(start + 0);
        tris.Add(start + 3);
        tris.Add(start + 2);
        tris.Add(start + 0);
    }
}

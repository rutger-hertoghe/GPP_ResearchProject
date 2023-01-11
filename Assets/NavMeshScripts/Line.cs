using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Line
{
    public Line(int idx1, int idx2)
    {
        startIdx = idx1;
        endIdx = idx2;
        connectedTriangles = new List<Triangle>();
    }
    public int startIdx;
    public int endIdx;
    List<Triangle> connectedTriangles;

    public static bool operator ==(Line left, Line right)
    {
        if (left.startIdx == right.startIdx && left.endIdx == right.endIdx)
        {
            return true;
        }

        if (left.endIdx == right.startIdx && left.startIdx == right.endIdx)
        {
            return true;
        }
        return false;
    }

    public static bool operator !=(Line left, Line right)
    {
        if (left.startIdx != right.startIdx && left.startIdx != right.endIdx)
        {
            return true;
        }

        if (left.endIdx != right.startIdx && left.endIdx != right.endIdx)
        {
            return true;
        }

        return false;
    }

    public void AddTriangle(Triangle tri)
    {
        connectedTriangles.Add(tri);
    }

    public int GetNrAdjacentTriangles()
    {
        return connectedTriangles.Count;
    }

    public List<Triangle> GetAdjacentTriangles()
    {
        return connectedTriangles;
    }
}

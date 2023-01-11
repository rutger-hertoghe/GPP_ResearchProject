using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Triangle
{
    public Triangle(int idx1, int idx2, int idx3)
    {
        v1Idx = idx1;
        v2Idx = idx2;
        v3Idx = idx3;

        lines = new List<Line>();

        id = idAssigner;    // ID of triangle = current value of static member
        ++idAssigner;       // Increment ID assigner to ensure next triangle has unique id
    }

    // Vertex indices in polygon
    public int v1Idx;
    public int v2Idx;
    public int v3Idx;

    // Triangle ID to compare triangle sameness
    int id;

    public List<Line> lines;

    public static int idAssigner = 1;

    public static bool operator== (Triangle a, Triangle b)
    {
        if(a.id == b.id) return true;
        return false;
    }

    public static bool operator !=(Triangle a, Triangle b)
    {
        if (a.id != b.id) return true;
        return false;
    }
}

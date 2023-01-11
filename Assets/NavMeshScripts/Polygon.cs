using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


// TODO: comment & cleanup code everywhere
public class Polygon
{
    public List<Vector2> vertices;
    public int[] vertexOrder;

    public void DebugDraw(Color color)
    {
        for (int i = 0; i < vertexOrder.Length - 1; ++i)
        {
            Vector2 v1 = vertices[vertexOrder[i]];
            Vector2 v2 = vertices[vertexOrder[i + 1]];
            Debug.DrawLine(v1, v2, color);
        }
        Debug.DrawLine(vertices[vertexOrder[0]], vertices[vertexOrder[vertexOrder.Length - 1]], color);
    }

    public void CreateDefaultVertexOrder()
    {
        vertexOrder = new int[vertices.Count];
        for (int i = 0; i < vertexOrder.Length; ++i)
        {
            vertexOrder[i] = i;
        }
    }

    // Ear clipping algorithm
    public TriangulatedPolygon Triangulate()
    {
        TriangulatedPolygon triangulatedPolygon = new TriangulatedPolygon();
        triangulatedPolygon.vertices = vertices;
        triangulatedPolygon.triangleList = new List<Triangle>();

        List<int> remainingVertices = vertexOrder.ToList();
        int previousCount = 0;

        // Find ear
        while (remainingVertices.Count > 3 && previousCount != remainingVertices.Count)
        {
        previousCount = remainingVertices.Count;
            for (int i = 0; i < remainingVertices.Count; ++i)
            {
                int currentVertexIndex = remainingVertices[i];
                int previousVertexIndex = GetPreviousVertexIndex(i, remainingVertices);
                int nextVertexIndex = GetNextVertexIndex(i, remainingVertices);
                bool invalid = false;

                Vector2 v0 = vertices[currentVertexIndex];
                Vector2 v1 = vertices[nextVertexIndex];
                Vector2 v2 = vertices[previousVertexIndex];
                if (IsConcaveAngle(v0, v1, v2))
                {
                    continue;
                }

                foreach(Vector2 vertex in vertices)
                {
                    if(vertex == v0 || vertex == v1 || vertex == v2)
                    {
                        continue;
                    }

                    if(IsInsideTriangle(vertex, v0, v1, v2))
                    {
                        invalid = true;
                        break;
                    }
                }

                if (invalid)
                {
                    continue;
                }

                triangulatedPolygon.triangleList.Add(new Triangle(currentVertexIndex, nextVertexIndex, previousVertexIndex));
                remainingVertices.RemoveAt(i);
                break;
            }
        }
        triangulatedPolygon.triangleList.Add(new Triangle(remainingVertices[0], remainingVertices[1], remainingVertices[2]));
        return triangulatedPolygon;
    }

    bool IsConcaveAngle(Vector2 vertex, Vector2 nextVertex, Vector2 previousVertex)
    {
        Vector3 nextLeg = nextVertex - vertex;
        Vector3 previousLeg = previousVertex - vertex;

        if(Vector3.Cross(nextLeg, previousLeg).z > 0)
        {
            return true;
        }
        return false;
    }

    bool IsInsideTriangle(Vector2 vertexToTest, Vector2 v0Triangle, Vector2 v1Triangle, Vector2 v2Triangle)
    {
        Vector3 v0ToV1 = v1Triangle - v0Triangle;
        Vector3 v1ToV2 = v2Triangle - v1Triangle;
        Vector3 v2ToV0 = v0Triangle - v2Triangle;

        Vector3 v0ToTest = vertexToTest - v0Triangle;
        Vector3 v1ToTest = vertexToTest - v1Triangle;
        Vector3 v2ToTest = vertexToTest - v2Triangle;

        if(Vector3.Cross(v0ToV1, v0ToTest).z > 0)
        {
            return false;
        }

        if(Vector3.Cross(v1ToV2, v1ToTest).z > 0)
        {
            return false;
        }

        if (Vector3.Cross(v2ToV0, v2ToTest).z > 0)
        {
            return false;
        }

        return true;
    }

    int GetPreviousVertexIndex(int i, List<int> remainingVertices)
    {
        if(i == 0)
        {
            return remainingVertices[remainingVertices.Count - 1];
        }
        
        return remainingVertices[i - 1];
    }

    int GetNextVertexIndex(int i, List<int> remainingVertices)
    {
        if (i == remainingVertices.Count - 1)
        {
            return remainingVertices[0];
        }

        return remainingVertices[i + 1];
    }
}

public class TriangulatedPolygon
{
    public List<Vector2> vertices;
    public List<Triangle> triangleList;

    public void DebugDraw(UnityEngine.Color color)
    {
        foreach(Triangle tri in triangleList)
        {
            Debug.DrawLine(vertices[tri.v1Idx], vertices[tri.v2Idx], color);
            Debug.DrawLine(vertices[tri.v2Idx], vertices[tri.v3Idx], color);
            Debug.DrawLine(vertices[tri.v3Idx], vertices[tri.v1Idx], color);
        }
    }
}
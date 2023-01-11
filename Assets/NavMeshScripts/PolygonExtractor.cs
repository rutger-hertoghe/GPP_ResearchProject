using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PolygonExtractor
{     
    public static Polygon ExtractPolygon(GameObject polygonObject)
    {
        // Get vertices from sprite
        SpriteRenderer spriteRenderer = polygonObject.GetComponent<SpriteRenderer>();
        Polygon extractedPolygon = new Polygon();
        extractedPolygon.vertices = new List<Vector2>(); ;
        if (spriteRenderer == null)
        {
            Debug.Log("No SpriteRendere found! Polygon not extracted");
            return extractedPolygon;
        }

        // Copy over vertices from sprite
        Sprite sprite = spriteRenderer.sprite;
        Vector2[] unsortedVertices = new Vector2[sprite.vertices.Length];
        sprite.vertices.CopyTo(unsortedVertices, 0);

        // Sort vertices in winding order
        Vector2[] sortedVertices = SortVertices(unsortedVertices, sprite.triangles);

        // Apply object transformation to every vertex
        foreach (Vector2 vert in sortedVertices)
        {
            extractedPolygon.vertices.Add(polygonObject.transform.TransformPoint(vert));
        }
        // Add vertex indices in clockwise winding order
        extractedPolygon.CreateDefaultVertexOrder();

        return extractedPolygon;
    }

    public static List<Polygon> ExtractPolygonsFromGameObjects(GameObject[] gameObjects)
    {
        List<Polygon> polygons = new List<Polygon>();
        for (int i = 0; i < gameObjects.Length; i++)
        {
            polygons.Add(ExtractPolygon(gameObjects[i]));
        }
        return polygons;
    }

    private static Vector2[] SortVertices(Vector2[] unsortedVertices, ushort[] triangles)
    {
        Vector2[] sortedVertices = new Vector2[unsortedVertices.Length];
        List<ushort> sortingOrder = GetVertexOrderBasedOnTriangleList(triangles);

        for (int i = 0; i < unsortedVertices.Length; ++i)
        {
            sortedVertices[i] = unsortedVertices[sortingOrder[i]];
        }

        return sortedVertices;
    }

    private static List<ushort> GetVertexOrderBasedOnTriangleList(ushort[] triangles)
    {
        List<ushort> order = new List<ushort>();

        order.Add(triangles[0]);
        order.Add(triangles[1]);
        order.Add(triangles[2]);

        for (int triIdx = 1; triIdx < triangles.Length / 3; ++triIdx)
        {
            ushort v1 = triangles[triIdx * 3];
            ushort v2 = triangles[triIdx * 3 + 1];
            ushort v3 = triangles[triIdx * 3 + 2];

            int i1 = order.IndexOf(v1);
            int i2 = order.IndexOf(v2);
            int i3 = order.IndexOf(v3);

            if (i1 == -1)
            {
                if (i2 < i3)
                {
                    order.Insert(i2 + 1, v1);
                }
                else
                {
                    order.Insert(i3 + 1, v1);
                }
            }
            else if (i2 == -1)
            {
                if (i1 < i3)
                {
                    order.Insert(i1 + 1, v1);
                }
                else
                {
                    order.Insert(i3 + 1, v1);
                }
            }
            else if (i3 == -1)
            {
                if (i1 < i2)
                {
                    order.Insert(i1 + 1, v1);
                }
                else
                {
                    order.Insert(i2 + 1, v1);
                }
            }
        }
        return order;
    }
}

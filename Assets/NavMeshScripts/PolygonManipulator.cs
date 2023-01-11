using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PolygonManipulator
{
    public static Polygon ExpandPolygon(Polygon polygon, float expansionSize)
    {

        Polygon expandedPolygon = new Polygon();
        expandedPolygon.vertices = new List<Vector2>();

        for (int vertexIndex = 0; vertexIndex < polygon.vertices.Count; ++vertexIndex)
        {
            Vector2 currentVertex = polygon.vertices[vertexIndex];

            Vector3 thisToNext = GetNextVertex(vertexIndex, polygon) - currentVertex;
            Vector3 previousToThis = currentVertex - GetPreviousVertex(vertexIndex, polygon);
            Vector3 crossed = Vector3.Cross(previousToThis, thisToNext);

            // Calculate vector perpendicular on vector from vertex to previous vertex and set length to expansionSize
            Vector3 expandForPrevious = Vector3.Cross(previousToThis, crossed);
            expandForPrevious.Normalize();
            expandForPrevious *= expansionSize;

            // Calculate vector perpendicular on vector from vertex to next vertex and set length to expansionSize
            Vector3 expandForNext = Vector3.Cross(thisToNext, crossed);
            expandForNext.Normalize();
            expandForNext *= expansionSize;

            // Calculate concave bisector vertex and set length to expansionSize
            Vector3 expandAlongVertex = new Vector2(expandForPrevious.x, expandForPrevious.y) + new Vector2(expandForNext.x, expandForNext.y);
            expandAlongVertex.Normalize();
            expandAlongVertex *= expansionSize;

            // Add vertices to final polygon in correct order
            expandedPolygon.vertices.Add(currentVertex + new Vector2(expandForPrevious.x, expandForPrevious.y));
            expandedPolygon.vertices.Add(currentVertex + new Vector2(expandAlongVertex.x, expandAlongVertex.y));
            expandedPolygon.vertices.Add(currentVertex + new Vector2(expandForNext.x, expandForNext.y));
        }
        // Fill vertexOrder array with default values (vertexOrder[0] = 0, vertexOrder[1], ...)
        // Vertices are in clockwise winding order anyway
        expandedPolygon.CreateDefaultVertexOrder();
        return expandedPolygon;
    }

    public static List<Polygon> ExpandAllPolygons(List<Polygon> polygons, float expansionSize)
    {
        List<Polygon> expandedPolygons = new List<Polygon>();
        foreach(Polygon polygon in polygons)
        {
            expandedPolygons.Add(ExpandPolygon(polygon, expansionSize));
        }
        return expandedPolygons;
    }

    public static Polygon AddHolesToShape(Polygon baseShape, List<Polygon> holes)
    {
        float shortestDist = float.MaxValue;
        int closestBaseVertIdx = 0;
        int closestHoleVertIdx = 0;
        int closestHoleIdx = 0;

        // Get the combination of floorplane and obstacle vertices that have the shortest distance to one another = "connection combination"
        // Bad growth: O(n²)
        for (int fpVertIdx = 0; fpVertIdx < baseShape.vertices.Count; ++fpVertIdx)
        {
            Vector2 currentFpVert = baseShape.vertices[fpVertIdx];
            for (int obstIdx = 0; obstIdx < holes.Count; ++obstIdx)
            {
                Polygon currentPolygon = holes[obstIdx];
                for (int obstVertIdx = 0; obstVertIdx < currentPolygon.vertices.Count; ++obstVertIdx)
                {
                    Vector2 currentObstVert = currentPolygon.vertices[obstVertIdx];
                    float currentDist = Vector2.Distance(currentFpVert, currentObstVert);
                    if (currentDist < shortestDist)
                    {
                        shortestDist = currentDist;
                        closestBaseVertIdx = fpVertIdx;
                        closestHoleVertIdx = obstVertIdx;
                        closestHoleIdx = obstIdx;
                    }
                }
            }
        }

        Polygon currentHole = holes[closestHoleIdx];

        int[] newHoleVertexOrder = currentHole.vertexOrder;
        // Make sure the vertex indices of the hole refer to the correct vertex (added to final shape later)
        for (int i = 0; i < newHoleVertexOrder.Length; ++i)
        {
            newHoleVertexOrder[i] += baseShape.vertices.Count;
        }

        List<int> vertexOrder = new List<int>();
        for (int i = 0; i < baseShape.vertexOrder.Length; ++i)
        {
            // Add vertex index of the base shape to the vertex order
            vertexOrder.Add(baseShape.vertexOrder[i]);

            // If that vertex is part of the "connection combination", add the previously corrected vertices
            // of the hole in reverse winding order, starting from the hole vertex of the "connection combination"
            if (baseShape.vertexOrder[i] == closestBaseVertIdx)
            {
                int indexToAdd = newHoleVertexOrder[closestHoleVertIdx];
                for (int j = 0; j <= newHoleVertexOrder.Length; ++j) // <= because we need to add the start Index of the shape twice
                {
                    if (indexToAdd < newHoleVertexOrder[0]) indexToAdd = newHoleVertexOrder[newHoleVertexOrder.Length - 1];
                    vertexOrder.Add(indexToAdd);
                    indexToAdd--;
                }
                // Add the base shape connection vertex again to properly close the loop
                vertexOrder.Add(baseShape.vertexOrder[i]);
            }
        }

        Polygon finalShape = new Polygon();
        // Add the vertices of the hole to the the vertices of the final shape
        finalShape.vertices = baseShape.vertices.Concat(currentHole.vertices).ToList();
        finalShape.vertexOrder = vertexOrder.ToArray();

        holes.Remove(currentHole);
        // Check if holes remain in the list and rerun algorithm
        if (holes.Count > 0)
        {
            return AddHolesToShape(finalShape, holes);
        }
        return finalShape;
    }

    private static Vector2 GetPreviousVertex(int vertexIndex, Polygon polygon)
    {
        if (vertexIndex > 0)
        {
            return polygon.vertices[vertexIndex - 1];
        }
        return polygon.vertices[polygon.vertices.Count - 1];
    }

    private static Vector2 GetNextVertex(int vertexIndex, Polygon polygon)
    {
        if (vertexIndex < polygon.vertices.Count - 1)
        {
            return polygon.vertices[vertexIndex + 1];
        }
        return polygon.vertices[0];
    }

    // UNUSED
    public static bool DoLinesIntersect(Vector2 va1, Vector2 va2, Vector2 vb1, Vector2 vb2)
    {
        Vector3 p1 = va1;
        Vector3 p2 = va2;
        Vector3 p3 = vb1;
        Vector3 p4 = vb2;

        float d1 = Vector3.Cross(p1 - p3, p4 - p3).z;
        float d2 = Vector3.Cross(p2 - p3, p4 - p3).z;
        float d3 = Vector3.Cross(p3 - p1, p2 - p1).z;
        float d4 = Vector3.Cross(p4 - p1, p2 - p1).z;

        if(d1 < 0 && d2 > 0 && d3 > 0 && d4 < 0)
        {
            return true;
        }
        return false;
    }

    // UNUSED: Analytic implementation to find line intersection, does not properly work for parallel lines
    public static Vector2 FindLineIntersection(Vector2 va1, Vector2 va2, Vector2 vb1, Vector2 vb2)
    {
        if(va1.x != va2.x && vb1.x != vb2.x)
        {
            float ma = (va2.y - va1.y)/(va2.x - va1.x);
            float mb = (vb2.y - vb1.y)/(vb2.x - vb1.x);

            float x = mb * vb1.x - vb1.y - ma * va1.x + va1.y;
            float y = ma * x - ma * va1.x + va1.y;
            return new Vector2(x, y);
        }

        if(va1.x == va2.x)
        {
            float mb = (vb2.y - vb1.y) / (vb2.x - vb1.x);
            float x = va1.x;
            float y = mb * x - mb * vb1.x + vb1.y;
            return new Vector2(x, y);
        }
        else
        {
            float ma = (va2.y - va1.y) / (va2.x - va1.x);
            float x = vb1.x;
            float y = ma * x - ma * va1.x + va1.y;
            return new Vector2(x, y);
        }
    }
}

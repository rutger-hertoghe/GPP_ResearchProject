using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public struct NavMeshConstructionPlan
{
    public Polygon m_FloorPlane;
    public List<Polygon> m_Obstacles;
}

public class NavMeshGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] float m_PolygonExpansionSize = 0.5f;
    [SerializeField] Material m_MeshDrawMaterial;
    List<Polygon> m_ExpandedPolygons;
    TriangulatedPolygon m_TriangulatedPolygon;
    List<Line> m_Lines = new List<Line>();

    NavGraph m_FinalGraph;

    TriangulatedPolygon m_TestPoly;

    Polygon m_NavMeshPoly;
    void Start()
    {
        GameObject[] obstacles;
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        // For now there is only one walkable area to keep it simple.
        GameObject floorPlane = GameObject.FindGameObjectWithTag("Walkable");
        if(!floorPlane)
        {
            Debug.Log("ERROR: No walkable floor plane found!");
            return;
        }
        // Create Nav Mesh Construction Plan: holds the floor plane & all obstacles
        NavMeshConstructionPlan navMeshToBuild = new NavMeshConstructionPlan();
        // Add floorplane
        navMeshToBuild.m_FloorPlane = PolygonExtractor.ExtractPolygon(floorPlane);

        // Extract obstacles from scene, expand & add to construction plan
        List<Polygon> extractedPolygons = PolygonExtractor.ExtractPolygonsFromGameObjects(obstacles);
        m_ExpandedPolygons = PolygonManipulator.ExpandAllPolygons(extractedPolygons, m_PolygonExpansionSize);
        navMeshToBuild.m_Obstacles = m_ExpandedPolygons;

        // Create single polygon from nav mesh construction plan polygons
        Polygon navMeshPoly = ConstructNavMeshPolygon(navMeshToBuild);

        // Triangulate to create actual NavMesh layout
        m_TriangulatedPolygon = navMeshPoly.Triangulate();

        // Create a NavGraph based on the final NavMesh layout
        m_FinalGraph = GenerateNavGraph(m_TriangulatedPolygon);
    }

    Polygon ConstructNavMeshPolygon(NavMeshConstructionPlan navMesh)
    {
        if (navMesh.m_Obstacles.Count > 0)
        {
            // Call to recursive hole punching algorithm
            return PolygonManipulator.AddHolesToShape(navMesh.m_FloorPlane, navMesh.m_Obstacles);
        }
        return navMesh.m_FloorPlane;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        UnityEngine.Color color = new UnityEngine.Color(0.0f, 1.0f, 0.0f);
        m_TriangulatedPolygon.DebugDraw(color);
        m_FinalGraph.DebugDraw();
    }

    private NavGraph GenerateNavGraph(TriangulatedPolygon navMeshPoly)
    {
        // Create new NavGraph, initialized with throwaway value
        // --> Ensures that custom constructor is called & members are initialized with empty containers where needed.
        NavGraph createdGraph = new NavGraph(0);

        // Link all triangles and lines from the triangulated NavMeshPolygon to one another
        GenerateLineTriNetwork(navMeshPoly, m_Lines);

        // Find the first line eligible for a GraphNode
        Line startLine = new Line();
        bool validStartLine = false;
        foreach(Line line in m_Lines)
        {
            if(line.GetNrAdjacentTriangles() == 2)
            {
                startLine = line;
                validStartLine = true;
                break;
            }
        }

        if(validStartLine == false)
        {
            Debug.Log("ERROR: no lines found with 2 adjacent triangles! No NavGraph created!");
        }

        // checkedTriangles will hold every triangle that has been checked yet, so the algorithm doesn't run forever
        var checkedTriangles = new List<Triangle>();

        // Throwaway GraphNode to feed the algorithm as the previous node, is not used anywhere, but makes sure the recursion works properly
        GraphNode graphNode = new GraphNode();

        // true as last argument to indicate this is the first iteration of the algorithm and no linking back to a previous node needs to be done
        RecursiveNavGraphCreation(createdGraph, navMeshPoly, checkedTriangles, graphNode, startLine, true); 

        return createdGraph;
    }

    // Returns a GraphNode so that previous call of algorithm can link the nodes created by its own previous node as well.
    public GraphNode RecursiveNavGraphCreation(NavGraph destGraph, TriangulatedPolygon navMeshPoly, List<Triangle> checkedTriangles, GraphNode previousNode, Line line, bool isStart = false)
    {
        var adjacentTriangles = line.GetAdjacentTriangles();
        
        // A line with two adjacent triangles requires a node
        if (adjacentTriangles.Count == 2)
        {
            // Location of the GraphNode
            Vector2 newlineCenter = (navMeshPoly.vertices[line.endIdx] + navMeshPoly.vertices[line.startIdx]) / 2;
            var graphNode = new GraphNode(newlineCenter);
            destGraph.AddNode(graphNode);

            // No linking back during first iteration
            if (isStart == false)
            {
                var graphConnection = new GraphConnection(graphNode, previousNode);
                graphConnection.AddSelfToNodes();
                destGraph.AddConnection(graphConnection);
            }

            // For both triangles check if the triangle has already been dealt with
            foreach (Triangle tri in adjacentTriangles)
            {
                if (checkedTriangles.Contains(tri))
                {
                    continue;
                }
                checkedTriangles.Add(tri);

                // Nodes created for the triangle
                List<GraphNode> createdNodes = new List<GraphNode>();
                foreach (Line nextLine in tri.lines)
                {
                    if (nextLine == line) continue;

                    // Call algorithm for a line, other than this line in the next triangle
                    GraphNode createdNode = RecursiveNavGraphCreation(destGraph, navMeshPoly, checkedTriangles, graphNode, nextLine);
                    createdNodes.Add(createdNode); // Returns valid node, added to the NavGraph OR an invalid node that isn't used for anything
                }
                // Two valid nodes created in the next triangle? Link them!
                if (createdNodes[0].isValid && createdNodes[1].isValid)
                {
                    GraphConnection missingLink = new GraphConnection(createdNodes[0], createdNodes[1]);
                    missingLink.AddSelfToNodes();
                    destGraph.AddConnection(missingLink);
                }

            }
            return graphNode;
        }
        // If the current line has no adjacent triangles, return an invalid node, this is never used and not added to the NavGraph
        return new GraphNode(false); 
    }

    private List<Line> GenerateLineTriNetwork(TriangulatedPolygon navMesh, List<Line> storeList)
    {
        foreach(Triangle tri in navMesh.triangleList)
        {
            Line line1 = new Line(tri.v1Idx, tri.v2Idx);
            Line line2 = new Line(tri.v2Idx, tri.v3Idx);
            Line line3 = new Line(tri.v3Idx, tri.v1Idx);

            tri.lines.Add(CheckAndUpdateLineInList(line1, storeList, tri));
            tri.lines.Add(CheckAndUpdateLineInList(line2, storeList, tri));
            tri.lines.Add(CheckAndUpdateLineInList(line3, storeList, tri));
        }
        return storeList;
    }

    private Line CheckAndUpdateLineInList(Line lineToCheck, List<Line> lineList, Triangle tri)
    {
        int retrievedIdx = GetLineInListIdx(lineToCheck, lineList);
        // Line in list
        if (retrievedIdx == -1)
        {
            lineToCheck.AddTriangle(tri);
            lineList.Add(lineToCheck);
            return lineToCheck;
        }
        // Line not in list
        else
        {
            Line retrievedLine = lineList[retrievedIdx];
            retrievedLine.AddTriangle(tri);
            return retrievedLine;
        }
    }

    private int GetLineInListIdx(Line lineToCheck, List<Line> list)
    {
        for (int i = 0; i < list.Count; ++i)
        {
            if (list[i] == lineToCheck) return i;
        }
        return -1;
    }
}
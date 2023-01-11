using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct GraphConnection
{
    public GraphConnection(GraphNode start, GraphNode end)
    {
        startNode = start;
        endNode = end;
        cost = CalculateCost(start.location, end.location);
    }
    public float cost;
    public GraphNode startNode;
    public GraphNode endNode;

    public void AddSelfToNodes()
    {
        startNode.AddConnection(this);
        endNode.AddConnection(this);
    }

    static private float CalculateCost(Vector2 startLoc, Vector2 endLoc)
    {
        return Vector2.Distance(startLoc, endLoc);
    }

    // Travel to the node on the other side of the connection, given a StartNode
    public GraphNode TravelToOther(GraphNode node)
    {
        if(node.location == startNode.location)
        {
            return endNode;
        }

        if(node.location == endNode.location)
        {
            return startNode;
        }

        Debug.Log("Node not part of this connection! Returning self!");
        return node;
    }
}

public struct GraphNode
{
    public GraphNode(bool invalid)
    {
        location = new Vector2();
        graphConnections = new List<GraphConnection>();
        isValid = false;
    }
    public GraphNode(Vector2 loc)
    {
        location = loc;
        graphConnections = new List<GraphConnection>();
        isValid = true;
    }
    public Vector2 location;
    public List<GraphConnection> graphConnections;
    public bool isValid;

    public void AddConnection(GraphConnection connection)
    {
        graphConnections.Add(connection);
    }

    public void DebugDraw()
    {
        // Draw magenta diamond
        float offset = 0.1f;
        Vector2 p1 = new Vector2(location.x, location.y + offset);
        Vector2 p2 = new Vector2(location.x + offset, location.y);
        Vector2 p3 = new Vector2(location.x, location.y - offset);
        Vector2 p4 = new Vector2(location.x - offset, location.y);
        Debug.DrawLine(p1, p2, Color.magenta);
        Debug.DrawLine(p3, p2, Color.magenta);
        Debug.DrawLine(p3, p4, Color.magenta);
        Debug.DrawLine(p1, p4, Color.magenta);
    }

    public static bool operator==(GraphNode left, GraphNode right)
    {
        return (left.location == right.location);
    }

    public static bool operator !=(GraphNode left, GraphNode right)
    {
        return (left.location != right.location);
    }
}

public struct NavGraph
{
    public NavGraph(int i)
    {
        m_Nodes = new List<GraphNode>();
        m_Connections = new List<GraphConnection>();
    }

    public List<GraphNode> m_Nodes;
    public List<GraphConnection> m_Connections;

    public void AddNode(GraphNode node)
    {
        m_Nodes.Add(node);
    }
    public void AddConnection(GraphConnection connection)
    {
        m_Connections.Add(connection);
    }
    public void DebugDraw()
    {
        foreach (GraphConnection connection in m_Connections)
        {
            Color color = new Color(connection.cost/8, 0.0f, 1 - connection.cost/8);
            Debug.DrawLine(connection.startNode.location, connection.endNode.location, color);
        }
        foreach (GraphNode node in m_Nodes)
        {
            node.DebugDraw();
        }
    }
}
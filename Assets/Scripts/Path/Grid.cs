using UnityEngine;
using System.Collections.Generic;

//***********************************************************************//
// Adapted from Sebastion Lague: https://github.com/SebLague/Pathfinding //
//***********************************************************************//

public class Grid : MonoBehaviour
{
    private static float nodeDiameter;

    public LayerMask unwalkableMask;
    public LayerMask unbuildableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public bool drawNodes;
    public Node[,] nodeGrid;
    public int gridSizeX, gridSizeY;
    public Transform endZone;
    public static float nodeRadiusSquared;

    void Awake()
    {
        nodeRadiusSquared = nodeRadius * nodeRadius;
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
        Node.endZoneNode = NodeFromWorldPoint(endZone.position);
        Node.endZoneNodeZPos = endZone.position.z;
    }

    public void CreateGrid()
    {
        nodeGrid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius - 0.01f, unwalkableMask));
                bool buildable = !(Physics.CheckSphere(worldPoint, nodeRadius - 0.01f, unbuildableMask));
                if (!walkable)
                    buildable = false;
                nodeGrid[x, y] = new Node(walkable, buildable, worldPoint, x, y);
            }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                if ((x == -1 && y == -1) || (x == 1 && y == -1) || (x == 1 && y == 1) || (x == -1 && y == 1))
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    neighbours.Add(nodeGrid[checkX, checkY]);
            }
        
        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return nodeGrid[x, y];
    }

    public void ChangeNodeStatus(bool walkable, int topLeftX, int topLeftY)
    {
        nodeGrid[topLeftX, topLeftY].walkable = walkable;
        nodeGrid[topLeftX + 1, topLeftY].walkable = walkable;
        nodeGrid[topLeftX, topLeftY - 1].walkable = walkable;
        nodeGrid[topLeftX + 1, topLeftY - 1].walkable = walkable;
    }

    void OnDrawGizmos()
    {
        if (!drawNodes)
            return;

        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
            {
                Color color = Color.white;
                if (nodeGrid[x, y].walkable == false)
                    color = Color.red;

                Gizmos.color = color;
                Gizmos.DrawWireCube(nodeGrid[x, y].worldPosition, Vector3.one * nodeRadius);
            }
    }
}

using UnityEngine;

//***********************************************************************//
// Adapted from Sebastion Lague: https://github.com/SebLague/Pathfinding //
//***********************************************************************//

public class Node : IHeapItem<Node>
{
    public bool walkable;			//If the node can be traversed
    public bool buildable;          //If towers can be built on the node
    public Vector3 worldPosition;   //The Nodes physical location in the world
    public int gridX;               //The Node's X-coordinate in the grid
    public int gridY;               //The Node's Y-coordinate in the grid

    public int gCost;
    public int hCost;
    public Node parent;
    public int heapIndex;
    public GameObject tower = null;
    public static Node endZoneNode;
    public static float endZoneNodeZPos;

    //Node Constructor for Initialisation
    public Node(bool _walkable, bool _buildable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        buildable = _buildable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }

    public int fCost
    {
        get { return gCost + hCost; }
    }

    public int HeapIndex
    {
        get { return heapIndex; }
        set { heapIndex = value; }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
            compare = hCost.CompareTo(nodeToCompare.hCost);
        if (compare == 0)
            return (endZoneNode.worldPosition.z - worldPosition.z).CompareTo(endZoneNode.worldPosition.z - nodeToCompare.worldPosition.z);
        return -compare;
    }
}

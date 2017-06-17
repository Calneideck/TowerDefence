using UnityEngine;
using System.Collections.Generic;
using System.Threading;

//***********************************************************************//
// Adapted from Sebastion Lague: https://github.com/SebLague/Pathfinding //
//***********************************************************************//

public class Pathfinding : MonoBehaviour
{
    //private const int NUMBER_OF_THREADS = 5;
    private Grid grid;
    private Queue<PathInfo> pathsFound = new Queue<PathInfo>();
    private Queue<PathRequest> pathRequests = new Queue<PathRequest>();
    private Thread pathThread;
    //private Thread[] threads = new Thread[NUMBER_OF_THREADS];
    private readonly object requestLock = new object();
    private readonly object foundLock = new object();

    public static readonly object gridLock = new object();

    public struct PathRequest
    {
        public GameObject target;
        public Vector3 startPos;

        public PathRequest(GameObject target, Vector3 startPos)
        {
            this.target = target;
            this.startPos = startPos;
        }
    }

    public struct PathInfo
    {
        public GameObject unit;
        public Vector3[] path;

        public PathInfo(GameObject unit, Vector3[] path)
        {
            this.unit = unit;
            this.path = path;
        }
    }

    void Awake()
    {
        grid = GetComponent<Grid>();
    }

    void Update()
    {
        lock (foundLock)
        {
            while (pathsFound.Count > 0)
            {
                PathInfo pathInfo = pathsFound.Dequeue();
                if (pathInfo.unit)
                    pathInfo.unit.GetComponent<Unit>().PathReceived(pathInfo.path);
            }
        }
    }

    public void RequestPath(GameObject target)
    {
        if (pathThread == null || !pathThread.IsAlive)
        {
            pathThread = new Thread(new ParameterizedThreadStart(HandleFindPath));
            pathThread.IsBackground = true;
            pathThread.Start(new PathRequest(target, target.transform.position));
        }
        else
            lock (requestLock)
            {
                pathRequests.Enqueue(new PathRequest(target, target.transform.position));
            }
    }

    private void HandleFindPath(object aPathRequest)
    {
        PathRequest pathRequest = (PathRequest)aPathRequest;
        Vector3[] path;
        lock (gridLock)
        {
            path = FindPath(pathRequest.startPos);
        }
        lock (foundLock)
        {
            pathsFound.Enqueue(new PathInfo(pathRequest.target, path));
        }

        lock (requestLock)
        {
            if (pathRequests.Count > 0)
            {
                pathRequest = pathRequests.Dequeue();
                pathThread = new Thread(new ParameterizedThreadStart(HandleFindPath));
                pathThread.IsBackground = true;
                pathThread.Start(pathRequest);
            }
        }
    }

    public Vector3[] FindPath(Vector3 startPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);

        Heap<Node> openSet = new Heap<Node>((int)Mathf.Ceil(grid.gridSizeX * grid.gridSizeY * 0.5f));
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode.worldPosition.z == Node.endZoneNodeZPos)
                return RetracePath(startNode, currentNode).ToArray();

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, Node.endZoneNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                    else
                        openSet.UpdateItem(neighbour);
                }
            }
        }

        Debug.LogError("Path not found");
        return new Vector3[0];
    }

    List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.worldPosition);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return (dstX + dstY);
    }
}
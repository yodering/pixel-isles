using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AStarPathfinding : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private Vector2 gridWorldSize = new Vector2(100f, 100f);
    [SerializeField] private float nodeRadius = 0.25f; // Size of each grid cell
    [SerializeField] private LayerMask unwalkableMask;

    [Header("Performance")]
    [SerializeField] private int maxPathSearchIterations = 1000; // Prevent infinite loops
    [SerializeField] private float pathUpdateInterval = 0.5f; // How often to recalculate paths

    private Node[,] grid;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    public static AStarPathfinding Instance { get; private set; }

    // Debug visualization
    [Header("Debug Visualization")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private bool showPaths = true;
    [SerializeField] private bool showSearchProcess = false;

    private List<Node> debugOpenSet;
    private List<Node> debugClosedSet;
    private List<Vector3> debugLastPath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        CreateGrid();
    }

    private void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius * 0.8f, unwalkableMask);
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = NodeFromWorldPoint(startPos);
        Node targetNode = NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null || !targetNode.walkable)
        {
            return null;
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        int iterations = 0;

        while (openSet.Count > 0 && iterations < maxPathSearchIterations)
        {
            iterations++;

            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                List<Vector3> path = RetracePath(startNode, targetNode);
                debugLastPath = path;
                if (showSearchProcess)
                {
                    debugOpenSet = new List<Node>(openSet);
                    debugClosedSet = new List<Node>(closedSet);
                }
                return path;
            }

            foreach (Node neighbour in GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                float newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }

        // No path found
        debugLastPath = null;
        return null;
    }

    private List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();

        // Simplify path by removing unnecessary waypoints
        List<Vector3> waypoints = SimplifyPath(path);
        return waypoints;
    }

    private List<Vector3> SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 0; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i].gridX - path[0].gridX, path[i].gridY - path[0].gridY);
            if (directionNew != directionOld || i == path.Count - 1)
            {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }

        return waypoints;
    }

    private float GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        // Using octile distance for 8-directional movement
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    private List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        Vector3 localPos = worldPosition - transform.position;
        float percentX = (localPos.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (localPos.y + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
        {
            return grid[x, y];
        }

        return null;
    }

    public float GetPathUpdateInterval()
    {
        return pathUpdateInterval;
    }

    // For debugging
    private void OnDrawGizmos()
    {
        // Draw grid bounds
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

        if (grid != null && showGrid)
        {
            foreach (Node n in grid)
            {
                // Color code: white = walkable, red = unwalkable
                Gizmos.color = n.walkable ? new Color(1, 1, 1, 0.1f) : new Color(1, 0, 0, 0.3f);
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.05f));
            }
        }

        // Show search process (open and closed sets)
        if (showSearchProcess)
        {
            if (debugClosedSet != null)
            {
                foreach (Node n in debugClosedSet)
                {
                    Gizmos.color = new Color(1, 0, 0, 0.5f); // Red for closed set
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * nodeDiameter * 0.8f);
                }
            }

            if (debugOpenSet != null)
            {
                foreach (Node n in debugOpenSet)
                {
                    Gizmos.color = new Color(0, 1, 0, 0.5f); // Green for open set
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * nodeDiameter * 0.8f);
                }
            }
        }

        // Show calculated path
        if (showPaths && debugLastPath != null && debugLastPath.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < debugLastPath.Count - 1; i++)
            {
                Gizmos.DrawLine(debugLastPath[i], debugLastPath[i + 1]);
                Gizmos.DrawWireSphere(debugLastPath[i], 0.2f);
            }
            // Draw final waypoint
            Gizmos.DrawWireSphere(debugLastPath[debugLastPath.Count - 1], 0.2f);
        }
    }

    public class Node
    {
        public bool walkable;
        public Vector3 worldPosition;
        public int gridX;
        public int gridY;

        public float gCost; // Distance from start
        public float hCost; // Distance to target
        public Node parent;

        public float fCost { get { return gCost + hCost; } }

        public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
        {
            walkable = _walkable;
            worldPosition = _worldPos;
            gridX = _gridX;
            gridY = _gridY;
        }
    }
}

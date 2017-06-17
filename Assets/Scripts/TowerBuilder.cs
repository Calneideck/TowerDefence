using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerBuilder : MonoBehaviour
{
    private GameObject currentTower;
    private int currentTowerID;
    private Node topLeftNode, topRightNode, bottomRightNode, bottomLeftNode;
    private bool shiftDown;
    private PointerEventData pointer = new PointerEventData(EventSystem.current);
    private KeyCode[] hotkeys = new KeyCode[] { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.Z, KeyCode.X };
    private HUD hud;
    private UnitManager unitManager;
    
    public GameObject[] towerPrefabs;
    public Button[] towerButtons;
    public Grid grid;
    public Material buildableMaterial, notBuildableMaterial;
    public Material towerBaseMaterial;
    public LayerMask unitMask;
    public LayerMask towerMask;
    public GameObject projectilePrefab;

    void Start()
    {
        hud = GetComponent<HUD>();
        unitManager = GetComponent<UnitManager>();
        ObjectPooler.instance.Setup(projectilePrefab, 100);
        
        //Initialise the buttons for each tower
        for (int i = 0; i < towerButtons.Length; i++)
        {
            int towerID = i;
            towerButtons[i].onClick.AddListener(() => BuyTower(towerID));
        }
    }

	void Update()
	{
        DetectHotkeys();

        if (currentTower != null)
        {
            MoveTower();

            if (Input.GetMouseButtonDown(0) && Input.mousePosition.y > 165)
                BuildTower();

            if (Input.GetKeyUp(KeyCode.LeftShift) && shiftDown)
            {
                GameObject.Destroy(currentTower);
                currentTower = null;
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                GameObject.Destroy(currentTower);
                currentTower = null;
            }
        }
	}

    void DetectHotkeys()
    {
        for (int i = 0; i < hotkeys.Length; i++)
            if (Input.GetKeyDown(hotkeys[i]))
                ExecuteEvents.Execute(towerButtons[i].gameObject, pointer, ExecuteEvents.submitHandler);
    }

    void MoveTower()
    {
        //Get world position from mouse
        Vector3 towerPos = GetMousePositionOnXZPlane();
        //Get Node from world position
        Node node = grid.NodeFromWorldPoint(towerPos);
        
        // Clamp node to edges
        if (node.gridX >= grid.gridSizeX - 1)           //Right
            node = grid.nodeGrid[grid.gridSizeX - 2, node.gridY];
        if (node.gridY <= 1)                            //Bottom
            node = grid.nodeGrid[node.gridX, 2];

        // Get the 4 nodes underneath the tower
        topLeftNode = node;
        bottomRightNode = grid.nodeGrid[node.gridX + 1, node.gridY - 1];
        bottomLeftNode = grid.nodeGrid[node.gridX, node.gridY - 1];
        topRightNode = grid.nodeGrid[node.gridX + 1, node.gridY];
        // Update the tower base's colour depending on whether the node is buildable
        SetTowerBaseMaterials(currentTower, false);

        // Position the tower (snap it to the node grid)
        Vector3 dir = new Vector3(0.25f, 0, -0.25f);
        towerPos = node.worldPosition + dir;
        towerPos.y = Info.towers[currentTowerID].height;
        currentTower.transform.position = towerPos;
    }

    void BuyTower(int towerID)
    {
        // Called when player clicks on a tower button
        if (Player.Gold >= Info.towers[towerID].cost)
        {
            Vector3 pos = GetMousePositionOnXZPlane();
            if (currentTower)
                GameObject.Destroy(currentTower);
            currentTower = (GameObject)GameObject.Instantiate(towerPrefabs[towerID], new Vector3(pos.x, Info.towers[towerID].height, pos.z), towerPrefabs[towerID].transform.rotation);
            currentTowerID = towerID;
        }
        else
            ErrorMessage.instance.NewMessage("Not Enough Gold!");
    }

    void BuildTower()
    {
        if (!TowerCanBeBuiltHere())
            return;

        if (Player.Gold >= Info.towers[currentTowerID].cost)
        {
            // Make nodes under tower not walkable
            lock (Pathfinding.gridLock)
            {
                grid.ChangeNodeStatus(false, topLeftNode.gridX, topLeftNode.gridY);
            }
            if (!unitManager.UpdatePaths(new Node[] { topLeftNode, topRightNode, bottomRightNode, bottomLeftNode }, false))
            {
                ErrorMessage.instance.NewMessage("Can't block the lane!");
                lock (Pathfinding.gridLock)
                {
                    grid.ChangeNodeStatus(true, topLeftNode.gridX, topLeftNode.gridY);
                }
                return;
            }

            Player.Gold -= Info.towers[currentTowerID].cost;
            hud.UpdateGold();
            // Make tower base the normal colour
            SetTowerBaseMaterials(currentTower, true);
            currentTower.transform.Find("MinimapIcon").gameObject.SetActive(true);
            // Set the tower's values
            currentTower.GetComponent<Tower>().Setup(currentTowerID, unitMask, new Vector2(topLeftNode.gridX, topLeftNode.gridY), projectilePrefab);
            SoundFX.player.BuildTower();
            for (int i = 0; i < 4; i++)
                currentTower.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Tower");

            // Continue building
            if (Input.GetKey(KeyCode.LeftShift))
            {
                shiftDown = true;
                Vector3 pos = GetMousePositionOnXZPlane();
                currentTower = (GameObject)GameObject.Instantiate(towerPrefabs[currentTowerID], new Vector3(pos.x, Info.towers[currentTowerID].height, pos.z), towerPrefabs[currentTowerID].transform.rotation);
                MoveTower();
            }
            else
                currentTower = null;
        }
        else
            ErrorMessage.instance.NewMessage("Not Enough Gold!");
    }

    public void SetTowerBaseMaterials(GameObject tower, bool placed)
    {
        tower.transform.GetChild(0).GetComponent<Renderer>().material = placed ? towerBaseMaterial : BaseMaterial(topRightNode);
        tower.transform.GetChild(1).GetComponent<Renderer>().material = placed ? towerBaseMaterial : BaseMaterial(bottomRightNode);
        tower.transform.GetChild(2).GetComponent<Renderer>().material = placed ? towerBaseMaterial : BaseMaterial(bottomLeftNode);
        tower.transform.GetChild(3).GetComponent<Renderer>().material = placed ? towerBaseMaterial : BaseMaterial(topLeftNode);
    }

    Material BaseMaterial(Node n)
    {
        return (n.walkable && n.buildable) ? buildableMaterial : notBuildableMaterial;
    }

    bool TowerCanBeBuiltHere()
    {
        if (!topRightNode.walkable || !topRightNode.buildable)
            return false;
        if (!bottomRightNode.walkable || !bottomRightNode.buildable)
            return false;
        if (!bottomLeftNode.walkable || !bottomLeftNode.buildable)
            return false;
        if (!topLeftNode.walkable || !topLeftNode.buildable)
            return false;
        return true;
    }

    Vector3 GetMousePositionOnXZPlane()
    {
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane XZPlane = new Plane(Vector3.up, Vector3.zero);
        if (XZPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            hitPoint.y = 0;
            return hitPoint;
        }
        return Vector3.zero;
    }

    public bool IsBuilding
    {
        get { return currentTower != null; }
    }
}

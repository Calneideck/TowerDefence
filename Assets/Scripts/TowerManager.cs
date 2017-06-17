using UnityEngine;
using UnityEngine.EventSystems;

public class TowerManager : MonoBehaviour
{
    private GameObject selectionCircle;
    private Tower selectedTower;
    private TowerBuilder towerBuilder;
    private LayerMask towerMask;
    private LayerMask unitMask;
    private GameObject[] towerPrefabs;
    private GameObject projectilePrefab;
    private HUD hud;
    private UnitManager unitManager;
    private PointerEventData pointer = new PointerEventData(EventSystem.current);

    public GameObject circlePrefab;
    public Grid grid;
    public GameObject sellTowerButton;
    public GameObject upgradeTowerButton;

    void Start()
    {
        selectionCircle = (GameObject)GameObject.Instantiate(circlePrefab);
        selectionCircle.SetActive(false);
        towerBuilder = GetComponent<TowerBuilder>();
        unitManager = GetComponent<UnitManager>();
        towerMask = towerBuilder.towerMask;
        unitMask = towerBuilder.unitMask;
        projectilePrefab = towerBuilder.projectilePrefab;
        towerPrefabs = towerBuilder.towerPrefabs;
        hud = GetComponent<HUD>();
    }
	
	void Update()
    {
        SelectTower();
        if (Input.GetButtonDown("Sell"))
            ExecuteEvents.Execute(sellTowerButton, pointer, ExecuteEvents.submitHandler);

        if (Input.GetButtonDown("Upgrade"))
            ExecuteEvents.Execute(upgradeTowerButton, pointer, ExecuteEvents.submitHandler);
    }

    void SelectTower()
    {
        if (towerBuilder.IsBuilding || EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 10000, towerMask);
            if (hit.collider != null)
            {
                Vector3 towerPos = hit.collider.transform.position;
                selectionCircle.SetActive(true);
                selectionCircle.transform.position = new Vector3(towerPos.x, 0.04f, towerPos.z);
                hud.SetSelectedTowerID(hit.collider.GetComponent<Tower>().TowerID);
                selectedTower = hit.collider.GetComponent<Tower>();
            }
            else
            {
                selectionCircle.SetActive(false);
                hud.SetBottomBar(false);
                selectedTower = null;
            }
        }
    }

    public void UpgradeTower()
    {
        if (selectedTower == null || selectedTower.TowerID == towerPrefabs.Length - 1)
            return;

        int newID = selectedTower.TowerID + 1;
        if (Player.Gold >= Info.towers[newID].cost)
        {
            Vector3 pos = new Vector3(selectedTower.transform.position.x, Info.towers[newID].height, selectedTower.transform.position.z);
            GameObject newTower = (GameObject)GameObject.Instantiate(towerPrefabs[newID], pos, towerPrefabs[newID].transform.rotation);

            Player.Gold -= Info.towers[newID].cost;
            hud.UpdateGold();
            hud.HideTooltip();

            newTower.transform.Find("MinimapIcon").gameObject.SetActive(true);
            newTower.GetComponent<Tower>().Setup(newID, unitMask, selectedTower.TopLeftNode, projectilePrefab);
            towerBuilder.SetTowerBaseMaterials(newTower, true);

            for (int i = 0; i < 4; i++)
                newTower.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Tower");

            GameObject.Destroy(selectedTower.gameObject);

            // Select new tower
            hud.SetSelectedTowerID(newID);
            selectedTower = newTower.GetComponent<Tower>();
            selectionCircle.SetActive(true);
            hud.UpgradeTowerPoint();
            SoundFX.player.UpgradeTower();
        }
        else
            ErrorMessage.instance.NewMessage("Not Enough Gold!");
    }

    public void SellTower()
    {
        if (selectedTower == null)
            return;

        selectionCircle.SetActive(false);
        hud.SetBottomBar(false);

        int gold = Mathf.CeilToInt(0.75f * Info.towers[selectedTower.TowerID].cost);
        hud.Bounty(gold, selectedTower.transform.position);
        hud.HideTooltip();

        grid.ChangeNodeStatus(true, (int)selectedTower.TopLeftNode.x, (int)selectedTower.TopLeftNode.y);
        unitManager.UpdatePaths(new Node[0], true);
        
        GameObject.Destroy(selectedTower.gameObject);
        SoundFX.player.SellTower();
    }
}

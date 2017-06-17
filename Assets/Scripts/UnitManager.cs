using UnityEngine;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{
    private const int UNIT_AMOUNT = 20;

    private Vector3[] leftPath;
    private Vector3[] rightPath;
    private List<Unit> units = new List<Unit>();
    private GameObject[] unitsToMove = new GameObject[UNIT_AMOUNT * 2];
    private int moveNumber = 0;
    private bool useMultiThreading;

    public GameObject[] unitPrefabs;
    public string[] unitNames;
    public Material[] tierMaterials;
    public Transform leftSpawn, rightSpawn;
    public Pathfinding pathfinding;
    public float unitSpawnSpacing;
    public float moveDelay;
    public bool drawPaths;
    
    void Start()
	{
        leftPath = pathfinding.FindPath(leftSpawn.position);
        rightPath = pathfinding.FindPath(rightSpawn.position);
	}

	public void SpawnWave(int roundNumber)
    {
        // Round number is synonymous with unit id
        // Round number is not zero-based so subtract 1
        roundNumber--;
        moveNumber = 0;
        MoveAll();

        int modelID = roundNumber == 24 ? 6 : roundNumber % 6;
        string tier = GetTierText(Mathf.FloorToInt(roundNumber / 6f));
        EventText.instance.AddWhiteText("Round " + (roundNumber + 1) + " - " + unitNames[modelID] + " " + tier);

        for (int i = 0; i < (roundNumber == 24 ? 1 : UNIT_AMOUNT); i++)
        {
            // Spawn units
            Vector3 leftPos = leftSpawn.position + new Vector3(i % 5 * -unitSpawnSpacing, 0, Mathf.Floor(i / 5f) * unitSpawnSpacing);
            Vector3 rightPos = rightSpawn.position + new Vector3(i % 5 * unitSpawnSpacing, 0, Mathf.Floor(i / 5f) * unitSpawnSpacing);
            GameObject leftUnit = (GameObject)GameObject.Instantiate(unitPrefabs[modelID], leftPos, unitPrefabs[modelID].transform.rotation);
            GameObject rightUnit = (GameObject)GameObject.Instantiate(unitPrefabs[modelID], rightPos, unitPrefabs[modelID].transform.rotation);

            unitsToMove[i * 2] = leftUnit;
            unitsToMove[i * 2 + 1] = rightUnit;
            Unit leftUnitClass = leftUnit.GetComponent<Unit>();
            Unit rightUnitClass = rightUnit.GetComponent<Unit>();

            // Setup units
            units.Add(leftUnitClass);
            leftUnitClass.Setup(roundNumber, this, leftPath, unitPrefabs[modelID].transform.Find("Canvas").rotation);
            units.Add(rightUnitClass);
            rightUnitClass.Setup(roundNumber, this, rightPath, unitPrefabs[modelID].transform.Find("Canvas").rotation);
        }

        MoveUnits();
    }

    public bool UpdatePaths(Node[] nodes, bool forceUpdate)
    {
        bool left = false, right = false;

        if (forceUpdate)
        {
            lock (Pathfinding.gridLock)
            {
                leftPath = pathfinding.FindPath(leftSpawn.position);
                rightPath = pathfinding.FindPath(rightSpawn.position);
            }
        }
        else
        {
            Vector3[] leftTemp = leftPath;
            Vector3[] rightTemp = rightPath;

            for (int i = 0; i < nodes.Length; i++)
            {
                if (!left && System.Array.IndexOf(leftPath, nodes[i].worldPosition) >= 0)
                {
                    lock (Pathfinding.gridLock)
                        leftPath = pathfinding.FindPath(leftSpawn.position);

                    if (leftPath.Length == 0)
                    {
                        leftPath = leftTemp;
                        rightPath = rightTemp;
                        return false;
                    }
                    left = true;
                }

                if (!right && System.Array.IndexOf(rightPath, nodes[i].worldPosition) >= 0)
                {
                    lock (Pathfinding.gridLock)
                        rightPath = pathfinding.FindPath(rightSpawn.position);

                    if (rightPath.Length == 0)
                    {
                        leftPath = leftTemp;
                        rightPath = rightTemp;
                        return false;
                    }
                    right = true;
                }
            }
        }

        CheckUnitPaths();
        return true;
    }

    void MoveUnits()
    {
        for (int i = moveNumber; i < moveNumber + 10; i++)
            if (unitsToMove[i])
                unitsToMove[i].GetComponent<Unit>().AbleToMove();

        moveNumber += 10;
        if (moveNumber <= 30)
            Invoke("MoveUnits", moveDelay);
    }

    void MoveAll()
    {
        CancelInvoke();
        for (int i = 0; i < UNIT_AMOUNT * 2; i++)
            if (unitsToMove[i])
                unitsToMove[i].GetComponent<Unit>().AbleToMove();
    }

    public void CheckUnitPaths()
    {
        foreach (Unit unit in units)
            GetPath(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);
    }

    public void GetPath(Unit unit)
    {
        if (useMultiThreading)
            pathfinding.RequestPath(unit.gameObject);
        else
            unit.GetComponent<Unit>().PathReceived(pathfinding.FindPath(unit.transform.position));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            useMultiThreading = !useMultiThreading;
            EventText.instance.AddWhiteText("Multithreading: " + (useMultiThreading ? "ON" : "OFF"));
        }
    }

    public void GameOver()
    {
        foreach (Unit unit in units)
            GameObject.Destroy(unit.gameObject);

        units.Clear();
    }

    string GetTierText(int tier)
    {
        switch (tier)
        {
            case 0:
                return "I";
            case 1:
                return "II";
            case 2:
                return "III";
            case 3:
                return "IV";
        }
        return "";
    }
}

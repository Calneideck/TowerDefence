using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    private int selectedTowerID;
    private RoundManager roundManager;
    private UnitManager unitManager;

    public Text gameOverText;
    public Text goldText;
    public Text livesText;
    
    //Tower Stuff
    public GameObject TToolTip;
    public Text TNameLabel;
    public Text TCostLabel;
    public Text TDamageLabel;
    public Text TSpeedLabel;
    public Text TRangeLabel;
    public Image THotkey;
    public GameObject SellToolTip;
    public Text SellAmountLabel;

    public GameObject bountyCanvasPrefab;
    public Sprite[] hotkeyImages;

    public Transform bottomBar;
    public Sprite[] towerIcons;
    public Text selectedTowerName;
    public Image selectedTowerIcon;
    public Text selectedTowerDamage;
    public Text selectedTowerCooldown;
    public Text selectedTowerRange;


    void Start()
    {
        roundManager = GetComponent<RoundManager>();
        unitManager = GetComponent<UnitManager>();
    }

    public void TowerPoint(int towerID)
    {
        THotkey.enabled = true;
        THotkey.sprite = hotkeyImages[towerID];
        ShowTowerTooltip(towerID);
    }

    public void UpgradeTowerPoint()
    {
        THotkey.enabled = false;
        if (selectedTowerID < Info.towers.Length - 1)
            ShowTowerTooltip(selectedTowerID + 1);
    }

    void ShowTowerTooltip(int towerID)
    {
        TToolTip.SetActive(true);
        TNameLabel.text = Info.towers[towerID].name;
        TCostLabel.text = string.Format("{0:n0}", Info.towers[towerID].cost);
        TDamageLabel.text = string.Format("{0:n0}", Info.towers[towerID].damage);
        TSpeedLabel.text = GetSpeed(Info.towers[towerID].cooldown);
        TRangeLabel.text = Info.towers[towerID].range.ToString();
    }

    public void HideTooltip()
    {
        TToolTip.SetActive(false);
        SellToolTip.SetActive(false);
    }

    public void SetSelectedTowerID(int id) // Must call BEFORE calling UpgradePoint()
    {
        selectedTowerID = id;
        SetBottomBar(true);
        selectedTowerName.text = Info.towers[id].name;
        selectedTowerIcon.sprite = towerIcons[id];
        selectedTowerDamage.text = Info.towers[id].damage.ToString("n0");
        selectedTowerCooldown.text = GetSpeed(Info.towers[id].cooldown);
        selectedTowerRange.text = Info.towers[id].range.ToString();
    }

    public void SetBottomBar(bool active)
    {
        for (int i = 0; i < bottomBar.childCount; i++)
            bottomBar.GetChild(i).gameObject.SetActive(active);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            Player.Gold += 100000;
            UpdateGold();
        }

    }


    public void UpdateGold()
    {
        goldText.text = Player.Gold.ToString("n0");
    }

    public void SellPoint()
    {
        SellToolTip.SetActive(true);
        SellAmountLabel.text = string.Format("{0:n0}", Mathf.Ceil(0.75f * Info.towers[selectedTowerID].cost));
    }

    public void LifeLost()
    {
        Player.Lives--;
        livesText.text = "Lives: " + Player.Lives;
        EventText.instance.AddRedText("Life lost! " + Player.Lives + " lives remain!");

        if (Player.Lives == 0)
        {
            unitManager.GameOver();
            roundManager.StopTimer();
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "Game Over" + System.Environment.NewLine +
                "You survived to round " + roundManager.Round + System.Environment.NewLine +
                "Thanks for playing!";
        }
    }

    public void Bounty(int amount, Vector3 pos)
    {
        Player.Gold += amount;
        UpdateGold();

        pos.y = 0.5f;
        GameObject bountyText = (GameObject)GameObject.Instantiate(bountyCanvasPrefab, pos, bountyCanvasPrefab.transform.rotation);
        bountyText.transform.GetChild(0).GetComponent<Text>().text = "+" + amount.ToString("n0");
    }

    string GetSpeed(float cooldown)
    {
        if (cooldown <= 0.3f)
            return "Very Fast";
        if (cooldown <= 0.8f)
            return "Fast";
        if (cooldown <= 1.0f)
            return "Normal";
        return "Slow";
    }
}

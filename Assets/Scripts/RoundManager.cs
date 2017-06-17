using UnityEngine;
using UnityEngine.UI;

public class RoundManager : MonoBehaviour
{
    private int currentRound = 0;
    private float startTime;
    private bool started;
    private UnitManager unitManager;

    public float roundDelay = 60;
    public float forceNextRoundDelay = 10;
    public Text roundTimeText;
    public Text roundText;
    public GameObject nextWaveButton;
    public GameObject roundTimerWindow;
    
    void Start()
    {
        unitManager = GetComponent<UnitManager>();
        roundTimeText.text = roundDelay.ToString("n2");
        roundText.text = "Round: " + currentRound.ToString();
    }
    
    public void StartCountdown()
    {
        startTime = Time.time;
        started = true;
    }

    void Update()
    {
        if (!started)
            return;

        float remainingTime = roundDelay - (Time.time - startTime);
        remainingTime = Mathf.Clamp(remainingTime, 0, roundDelay);
        roundTimeText.text = remainingTime.ToString("n2");

        if (remainingTime <= (roundDelay - forceNextRoundDelay) && nextWaveButton.activeSelf == false)
            nextWaveButton.SetActive(true);

        if (remainingTime == 0 || Input.GetKeyDown(KeyCode.P))
            StartNextWave();
    }

    public void StartNextWave()
    {
        // Start next round
        currentRound++;
        roundText.text = "Round: " + currentRound.ToString();
        // creepmanager -> new round
        unitManager.SpawnWave(currentRound);

        if (currentRound < 25)
            startTime = Time.time;
        else
            StopTimer();
    }

    public void StopTimer()
    {
        roundTimerWindow.SetActive(false);
        nextWaveButton.SetActive(false);
        started = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public int Round
    {
        get { return currentRound; }
    }
}

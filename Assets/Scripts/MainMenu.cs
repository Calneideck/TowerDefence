using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public Camera towerbaseCamera;
    public HUD hud;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            StartGame();
    }

    public void StartGame()
	{
        Camera.main.GetComponent<CameraController>().enabled = true;
        Camera.main.GetComponent<Animator>().SetTrigger("Move");
        GameObject.Instantiate(towerbaseCamera);
        
        hud.UpdateGold();
        gameObject.SetActive(false);
    }
}

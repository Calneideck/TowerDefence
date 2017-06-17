using UnityEngine;

public class SoundFX : MonoBehaviour
{
    private AudioSource sfxPlayer;

    public AudioClip[] build;
    public AudioClip lifeLost;
    public AudioClip sell;
    public AudioClip upgrade;
    public static SoundFX player;

    void Start()
    {
        player = this;
        sfxPlayer = GetComponent<AudioSource>();
    }

    public void BuildTower()
    {
        sfxPlayer.clip = build[Random.Range(0, build.Length)];
        sfxPlayer.Play();
    }

    public void SellTower()
    {
        sfxPlayer.clip = sell;
        sfxPlayer.Play();
    }

    public void UpgradeTower()
    {
        sfxPlayer.clip = upgrade;
        sfxPlayer.Play();
    }

    public void LifeLost()
    {
        sfxPlayer.clip = lifeLost;
        sfxPlayer.Play();
    }
}

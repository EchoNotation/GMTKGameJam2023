using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject goon;
    public float spawnX, spawnY;
    public GameObject winPanel, losePanel;
    private const int maxGoonsAlive = 15;
    private int numberGoonsAlive;
    private int reinforcementCount;
    private float lastGoonSpawn;
    private const float goonSpawnCooldown = 1;
    public bool levelCompleted = false;
    private int nextGoonPitch = 0;
    public AudioClip levelWon, levelLost;
    public AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();

        lastGoonSpawn = Time.time;
        numberGoonsAlive = 0;
        reinforcementCount = 50;

        Time.timeScale = 1;

        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(reinforcementCount > 0)
        {
            //Try and spawn a goon
            if(numberGoonsAlive < maxGoonsAlive && Time.time > lastGoonSpawn + goonSpawnCooldown)
            {
                //Spawn a goon
                lastGoonSpawn = Time.time;
                reinforcementCount--;

                GameObject spawnee = Instantiate(goon, new Vector3(spawnX, spawnY, -1), Quaternion.identity);
                spawnee.GetComponent<GoonController>().SetPitch(nextGoonPitch);
                nextGoonPitch = (nextGoonPitch + 1) % 3;
                numberGoonsAlive++;
            }
        }
    }

    public void OnGoonDeath()
    {
        numberGoonsAlive--;
        if(CheckGameOver()) LevelFailed();
    }

    public bool OnPlayerDeath()
    {
        if(numberGoonsAlive < 1 && reinforcementCount == 0)
        {
            //Game over
            if(CheckGameOver())
            {
                LevelFailed();
                return true;
            }
        }
        return false;
    }

    private bool CheckGameOver()
    {
        if(levelCompleted) return false;
        GameObject[] goons = GameObject.FindGameObjectsWithTag("Goon");
        
        for(int i = 0; i < goons.Length; i++)
        {
            if(!goons[i].GetComponent<GoonController>().dying) return false;
        }

        Player player = FindAnyObjectByType<Player>();
        if(player && !player.isDead) return false;
        return true;
    }

    public void ExitReached()
    {
        levelCompleted = true;
        GameObject.FindAnyObjectByType<Player>().Win();
        winPanel.SetActive(true);
        source.clip = levelWon;
        source.Play();
    }

    public void LevelFailed()
    {
        losePanel.SetActive(true);
        source.clip = levelLost;
        source.Play();
    }
}

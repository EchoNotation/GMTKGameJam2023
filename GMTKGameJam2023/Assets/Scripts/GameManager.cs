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
    public int reinforcementCount;
    private float lastGoonSpawn;
    private const float goonSpawnCooldown = 1;
    public bool levelCompleted = false;

    // Start is called before the first frame update
    void Start()
    {
        lastGoonSpawn = Time.time;
        numberGoonsAlive = 0;

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

                Instantiate(goon, new Vector3(spawnX, spawnY, -1), Quaternion.identity);

                numberGoonsAlive++;
            }
        }
    }

    public void OnGoonDeath()
    {
        numberGoonsAlive--;
    }

    public bool OnPlayerDeath()
    {
        if(numberGoonsAlive < 1 && reinforcementCount == 0)
        {
            //Game over
            LevelFailed();
            return true;
        }
        return false;
    }

    public void ExitReached()
    {
        levelCompleted = true;
        GameObject.FindAnyObjectByType<Player>().isDead = true; //Just used to prevent movement after winning
        winPanel.SetActive(true);
    }

    public void LevelFailed()
    {
        if(levelCompleted) return;
        losePanel.SetActive(true);
    }
}

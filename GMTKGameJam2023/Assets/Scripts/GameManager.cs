using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject goon;
    public float spawnX, spawnY;
    private const int maxGoonsAlive = 10;
    private int numberGoonsAlive;
    private int reinforcementCount;
    private float lastGoonSpawn;
    private const float goonSpawnCooldown = 1;

    // Start is called before the first frame update
    void Start()
    {
        lastGoonSpawn = Time.time;
        reinforcementCount = 50;
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

    public void OnPlayerDeath()
    {
        if(numberGoonsAlive == 0 && reinforcementCount == 0)
        {
            //Game over
            LevelFailed();
        }
    }

    public void ExitReached()
    {
        Debug.Log("Beat the level!");
    }

    public void LevelFailed()
    {
        Debug.Log("Game Over!");
    }
}

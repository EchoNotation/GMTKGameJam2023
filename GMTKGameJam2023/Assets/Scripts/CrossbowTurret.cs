using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CrossbowTurret : MonoBehaviour
{
    public enum TurretMode {
        SINGLE,
        BURST
    }

    public TurretMode mode;
    private bool firing;
    public Sprite normalCrossbow, drawnbackCrossbow;
    public GameObject bolt;
    public GameObject bow;
    private List<GameObject> targets;
    private GameObject tgt;
    private Stopwatch timer;
    private long lastTime, fireTime, recoverTime;
    private int shotsFired;
    private const float turnRate = 50;

    // Start is called before the first frame update
    void Start()
    {
        targets = new List<GameObject>();

        timer = new Stopwatch();
        timer.Start();
        firing = false;
        lastTime = 0; //Change to add a starting delay
        fireTime = 500;
        recoverTime = 2000;

        shotsFired = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(targets.Count == 0) return;
        Aim();

        //UnityEngine.Debug.Log($"Tgt count: {targets.Count}");

        if(firing && mode == TurretMode.BURST && timer.ElapsedMilliseconds >= lastTime + fireTime)
        {
            lastTime = timer.ElapsedMilliseconds;
            Fire();
        }
        else if(!firing && timer.ElapsedMilliseconds >= lastTime + recoverTime)
        {
            lastTime = timer.ElapsedMilliseconds;
            firing = true;

            Fire();
        }
    }

    private void Aim()
    {
        float minDistance = 99999;
        tgt = null;
        for(int i = 0; i < targets.Count; i++)
        {
            float distance = (targets[i].transform.position - transform.position).magnitude;
            if(distance < minDistance)
            {
                minDistance = distance;
                tgt = targets[i];
            }
        }


        if(tgt == null) return;

    }

    private void Fire()
    {
        if(tgt == null) return;

        Vector3 position = transform.position;
        position.z = -5;
        Quaternion rotation = bow.transform.rotation;
        Object justSpawned = Instantiate(bolt, position, rotation);

        if(mode == TurretMode.SINGLE)
        {
            firing = false;
        }
        else
        {
            shotsFired++;
            if(shotsFired >= 3)
            {
                shotsFired = 0;
                firing = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Goon") || other.CompareTag("Player"))
        {
            targets.Add(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Goon") || collision.CompareTag("Player"))
        {
            targets.Remove(collision.gameObject);
        }
    }
}

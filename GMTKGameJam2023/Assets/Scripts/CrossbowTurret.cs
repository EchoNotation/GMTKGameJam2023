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
    private float lastTime, fireTime, recoverTime;
    private int shotsFired;
    private const float turnRate = 50;
    private AudioSource source;

    public float radius = 5f;
    CircleCollider2D circleCollider;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        targets = new List<GameObject>();

        firing = false;
        lastTime = Time.time; //Change to add a starting delay
        fireTime = 0.5f;
        recoverTime = 2f;

        shotsFired = 0;

        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.radius = radius;
    }

    // Update is called once per frame
    void Update()
    {
        if(targets.Count == 0) return;
        Aim();

        //UnityEngine.Debug.Log($"Tgt count: {targets.Count}");

        if(firing && mode == TurretMode.BURST && Time.time >= lastTime + fireTime)
        {
            lastTime = Time.time;
            Fire();
        }
        else if(!firing && Time.time >= lastTime + recoverTime)
        {
            lastTime = Time.time;
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
            if(!targets[i]) continue;
            float distance = (targets[i].transform.position - transform.position).magnitude;
            if(distance < minDistance)
            {
                minDistance = distance;
                tgt = targets[i];
            }
        }


        if(tgt == null) return;

        Vector3 rayToTgt = tgt.transform.position - transform.position;
        float angle = (Mathf.Atan(rayToTgt.y / rayToTgt.x) * Mathf.Rad2Deg) - 90;

        if(rayToTgt.x < 0)
        {
            angle = (angle + 180) % 360;
        }
        bow.transform.rotation = Quaternion.RotateTowards(bow.transform.rotation, Quaternion.Euler(new Vector3(0, 0, angle)), 720*Time.deltaTime);
    }

    private void Fire()
    {
        if(tgt == null) return;

        source.Stop();
        source.Play();

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
            if(targets.Count == 0)
            {
                firing = false;
                shotsFired = 0;
                lastTime = Time.time;
            }
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

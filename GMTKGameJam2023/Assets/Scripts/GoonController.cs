using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

public class GoonController : MonoBehaviour
{
    MapManager mapManager;

    public float speed = 2f;
    public float restChance = 0.10f;
    public float restTime = 2f;

    bool hasDestination = false;
    Vector3 destination;

    float restUntil = 0;

    SpriteRenderer spriteRenderer;

    Color goonColor;

    public bool dying;
    private AudioSource source;
    private int pitch;
    public AudioClip[] pops, yips, yelps;
    private float nextYip;
    private float yipOffset;
    private float yipCooldown;

    public GameObject particleSystemObject;

    public GameObject splat;

    // Start is called before the first frame update
    void Start()
    {
        mapManager = FindAnyObjectByType<MapManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        dying = false;
        goonColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.2f, 0.8f, 0.9f, 1.0f);
        spriteRenderer.color = goonColor;

        Player player = GameObject.FindObjectOfType<Player>();
        if(player && player.isDead) StartGlowing();
    }

    private void OnEnable() {
        Player.OnPlayerDeath += StartGlowing;
        Player.OnPlayerRespawn += StopGlowing;
    }

    private void OnDisable() {
        Player.OnPlayerDeath -= StartGlowing;
        Player.OnPlayerRespawn -= StopGlowing;
    }

    void StartGlowing() {
        StartCoroutine("Glow");
    }

    void StopGlowing() {
        StopCoroutine("Glow");
        spriteRenderer.color = goonColor;
    }

    IEnumerator Glow() {
        float glowTime = 0.5f * Time.timeScale;
        while(true) {
            for(float t = 0f; t <= glowTime; t += Time.deltaTime) {
                spriteRenderer.color = Color.Lerp(Color.white, Color.yellow, t/glowTime);
                yield return new WaitForEndOfFrame();
            }
        }
    }

    void PickDestination() {
        destination = mapManager.GetDownFlowField(transform.position);
        hasDestination = true;
    }

    private void OnMouseDown() {
        Player player = GameObject.FindObjectOfType<Player>();
        if(player && player.isDead) {
            restUntil = Time.time + 3f;
            player.Respawn(transform.position, gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision) {
        string tag = collision.GetComponent<Collider2D>().gameObject.tag;
        if(tag == "Whirly" || tag == "SpikeTrap") {
            mapManager.RegisterDeath(collision.gameObject.transform.position);
            Die();
        }
        else if(tag == "Bolt")
        {
            Destroy(collision.gameObject);
            Die();
        }
    }

    void Die() {
        Destroy(gameObject);
        enabled = false;
        dying = true;

        GameObject particleObject = Instantiate(particleSystemObject, transform.position, Quaternion.identity);
        ParticleSystem.MainModule sys = particleObject.GetComponent<ParticleSystem>().main;
        sys.startColor = goonColor;
        AudioSource source = particleObject.GetComponent<AudioSource>();
        source.clip = yelps[pitch];
        source.Play();

        GameObject spawnedSplat = Instantiate(splat, transform.position, Quaternion.identity);
        spawnedSplat.GetComponent<SplatSelector>().SetColor(goonColor);

        GameObject.FindAnyObjectByType<GameManager>().OnGoonDeath();
    }

    public void SetPitch(int pitch)
    {
        this.pitch = pitch;
        //Debug.Log(pitch);
        source = GetComponent<AudioSource>();
        source.clip = pops[0];
        source.Play();
        nextYip = Time.time + UnityEngine.Random.Range(1, 6);
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > nextYip)
        {
            if(UnityEngine.Random.Range(0, 100) == 0)
            {
                nextYip = Time.time + UnityEngine.Random.Range(8, 24);
                source.Stop();
                source.clip = yips[pitch];
                source.Play();
            }
        }


        if(!hasDestination) {
            // 10% chance to rest
            if(UnityEngine.Random.value > restChance)
                PickDestination();
            else
                restUntil = Time.time + restTime;
        }

        else if(Time.time > restUntil) {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            Debug.DrawLine(transform.position, destination);

            float distance = Vector3.Distance(transform.position, destination);
            if(distance < 0.05f || distance > 2f) hasDestination = false;
        }
    }
}

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

    public GameObject particleSystemObject;

    // Start is called before the first frame update
    void Start()
    {
        mapManager = FindAnyObjectByType<MapManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        goonColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.2f, 0.8f, 0.9f, 1.0f);
        spriteRenderer.color = goonColor;

        if(GameObject.FindObjectOfType<Player>().isDead) StartGlowing();
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
        while(true) {
            for(float t = 0f; t <= 1f; t += 0.01f) {
                spriteRenderer.color = Color.Lerp(Color.white, Color.yellow, t);
                yield return new WaitForEndOfFrame();
            }
        }
    }

    void PickDestination() {
        destination = mapManager.GetDownFlowField(transform.position);
        hasDestination = true;
    }

    private void OnMouseDown() {
        Debug.Log("clicked goon");
        Player player = GameObject.FindObjectOfType<Player>();
        if(player && player.isDead) {
            restUntil = Time.time + 3f;
            player.Respawn(transform.position, gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision) {
        string tag = collision.GetComponent<Collider2D>().gameObject.tag;
        if(tag == "Whirly" || tag == "SpikeTrap") {
            Die();
        }
        else if(tag == "Bolt")
        {
            Destroy(collision.gameObject);
            Die();
        }
    }

    void Die() {
        mapManager.RegisterDeath(transform.position);
        Destroy(gameObject);
        enabled = false;

        GameObject particleObject = Instantiate(particleSystemObject, transform.position, Quaternion.identity);
        ParticleSystem.MainModule sys = particleObject.GetComponent<ParticleSystem>().main;
        sys.startColor = goonColor;

        GameObject.FindAnyObjectByType<GameManager>().OnGoonDeath();
    }

    // Update is called once per frame
    void Update()
    {
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

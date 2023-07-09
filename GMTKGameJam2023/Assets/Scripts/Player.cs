using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour {
    public float speed = 2f;
    public float hatTime = 3f;

    Rigidbody2D rigidBody;

    new Collider2D collider2D;
    SpriteRenderer spriteRenderer;

    public bool isDead;

    public static event Action OnPlayerDeath;
    public static event Action OnPlayerRespawn;

    public GameObject hatGameObject;

    bool isRespawning = false;

    private AudioSource source;
    public AudioClip playerDeath, hatJump, hatTransfer;

    public ParticleSystem deathParticleSystem;

    public GameObject splat;

    public Sprite standing, walk1, walk2;
    private float lastSpriteChange = 0;
    private const float spriteCooldown = 0.15f;
    private bool stepTracker, facingRight;

    public float fadeTime = 3f;

    private void Start () {
        source = GetComponent<AudioSource>();
        rigidBody = GetComponent<Rigidbody2D>();
        isDead = false;
        stepTracker = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider2D = GetComponent<CircleCollider2D>();
        hatGameObject.SetActive(false);
        deathParticleSystem = GetComponentInChildren<ParticleSystem>();
    }

    private void OnCollisionEnter2D (Collision2D collision) {
        string tag = collision.gameObject.tag;
        if(tag == "Whirly" || tag == "SpikeTrap") {
            Die();
        }
        else if(tag == "Bolt") {
            Destroy(collision.gameObject);
            Die();
        }
    }

    private void OnTriggerEnter2D (Collider2D collision) {
        string tag = collision.gameObject.tag;
        if(tag == "Exit") {
            GameObject.FindObjectOfType<GameManager>().ExitReached();
        }
    }

    public void Respawn (Vector3 position, GameObject goon) {
        StartCoroutine(DoRespawnProcess(position, goon));
    }

    IEnumerator DoRespawnProcess (Vector3 position, GameObject goon) {
        if(isRespawning || !goon) yield break;
        isRespawning = true;

        source.Stop();
        source.clip = hatJump;
        source.Play();

        // disable goon glowing
        OnPlayerRespawn?.Invoke();

        Vector3 startingPosition = hatGameObject.transform.position;

        for(float t = 0f; t <= hatTime * Time.timeScale; t += Time.deltaTime) {
            hatGameObject.transform.position = Vector3.Slerp(startingPosition, position, t / (hatTime * Time.timeScale));
            // Debug.Log("hat moving " + t.ToString());
            yield return new WaitForEndOfFrame();
        }

        if(!goon)
        {
            OnPlayerDeath?.Invoke();
            isRespawning = false;
            yield break;
        }

        Destroy(goon);
        GameObject.FindObjectOfType<GameManager>().OnGoonDeath();

        hatGameObject.SetActive(false);

        source.Stop();
        source.clip = hatTransfer;
        source.Play();

        // reset hat position
        hatGameObject.transform.position = transform.position;

        // respawn player
        transform.position = position;
        isDead = false;
        collider2D.enabled = true;
        spriteRenderer.enabled = true;

        Time.timeScale = 1f;

        rigidBody.simulated = true;

        isRespawning = false;

        yield break;
    }

    public void Win() {
        rigidBody.simulated = false;
        collider2D.enabled = false;

        StartCoroutine(DoFadeOut());
    }

    IEnumerator DoFadeOut() {
        Color color = spriteRenderer.color;
        for(float t = 0f; t < fadeTime; t += Time.deltaTime) {
            float strength = Mathf.Lerp(1, 0, t / (fadeTime * Time.timeScale));
            spriteRenderer.color = new Color(color.r, color.g, color.b, strength);
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
        yield break;
    }

    void Die() {
        rigidBody.simulated = false;
        isDead = true;
        collider2D.enabled = false;
        spriteRenderer.enabled = false;
        OnPlayerDeath?.Invoke();

        hatGameObject.SetActive(true);
        hatGameObject.transform.position = transform.position;

        bool gameOver = GameObject.FindAnyObjectByType<GameManager>().OnPlayerDeath();

        if(!gameOver) Time.timeScale = 0.2f;

        GameObject spawnedSplat = Instantiate(splat, transform.position, Quaternion.identity);
        spawnedSplat.GetComponent<SplatSelector>().SetColor(new Color(1f, 0.7490196f, 0f));

        deathParticleSystem.Play();
        source.Stop();
        source.clip = playerDeath;
        source.Play();
    }

    void FixedUpdate()
    {
        if(!isDead) {
            Vector3 desiredDirection = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
            desiredDirection = desiredDirection.normalized;

            rigidBody.velocity = desiredDirection * speed;

            if(Time.time > lastSpriteChange + spriteCooldown)
            {
                stepTracker = !stepTracker;
                lastSpriteChange = Time.time;
            }

            if(desiredDirection.magnitude == 0)
            {
                if(facingRight) GetComponent<SpriteRenderer>().sprite = standing;
                else { };
                
            }
            else
            {
                if(desiredDirection.x != 0) facingRight = desiredDirection.x > 0;

                if(stepTracker) GetComponent<SpriteRenderer>().sprite = walk1;
                else GetComponent<SpriteRenderer>().sprite = walk2;
            }

            GetComponent<SpriteRenderer>().flipX = !facingRight;
        }
    }
}

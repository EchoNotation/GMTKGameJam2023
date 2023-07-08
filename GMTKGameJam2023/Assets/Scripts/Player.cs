using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
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

    private void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        isDead = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider2D = GetComponent<CircleCollider2D>();
        hatGameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        string tag = collision.gameObject.tag;
        if(tag == "Whirly" || tag == "SpikeTrap") {
            Die();
        }
        else if(tag == "Bolt")
        {
            Destroy(collision.gameObject);
            Die();
        }
        
    }

    public void Respawn(Vector3 position, GameObject goon) {
        StartCoroutine(DoRespawnProcess(position, goon));
    }

    IEnumerator DoRespawnProcess(Vector3 position, GameObject goon) {
        if(isRespawning) yield break;
        isRespawning = true;

        // disable goon glowing
        OnPlayerRespawn?.Invoke();

        hatGameObject.SetActive(true);
        hatGameObject.transform.position = transform.position;

        Vector3 startingPosition = transform.position;

        for(float t = 0f; t <= hatTime*Time.timeScale; t += Time.deltaTime) {
            hatGameObject.transform.position = Vector3.Slerp(startingPosition, position, t/ (hatTime * Time.timeScale));
            // Debug.Log("hat moving " + t.ToString());
            yield return new WaitForEndOfFrame();
        }

        Destroy(goon);
        GameObject.FindObjectOfType<GameManager>().OnGoonDeath();

        hatGameObject.SetActive(false);

        // reset hat position
        hatGameObject.transform.position = transform.position;

        // respawn player
        transform.position = position;
        isDead = false;
        collider2D.enabled = true;
        spriteRenderer.enabled = true;
        
        Debug.Log("OnPlayerRespawn");
        Time.timeScale = 1f;

        rigidBody.simulated = true;

        isRespawning = false;

        yield break;
    }

    void Die() {
        rigidBody.simulated = false;
        isDead = true;
        collider2D.enabled = false;
        spriteRenderer.enabled = false;
        OnPlayerDeath?.Invoke();
        Debug.Log("OnPlayerDeath");
        Time.timeScale = 0.2f;
        hatGameObject.SetActive(true);
    }

    void FixedUpdate()
    {
        if(!isDead) {
            Vector3 desiredDirection = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
            desiredDirection = desiredDirection.normalized;

            rigidBody.velocity = desiredDirection * speed;
        }
    }
}

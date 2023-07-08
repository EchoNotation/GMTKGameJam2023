using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    public float speed = 2f;
    Rigidbody2D rigidBody;

    new Collider2D collider2D;
    SpriteRenderer spriteRenderer;

    public bool isDead;

    public static event Action OnPlayerDeath;
    public static event Action OnPlayerRespawn;

    private void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        isDead = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider2D = GetComponent<CircleCollider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        string tag = collision.gameObject.tag;
        if(tag == "Bolt" || tag == "Whirly" || tag == "SpikeTrap") {
            Die();
        }
        
    }

    public void Respawn(Vector3 position) {
        transform.position = position;
        isDead = false;
        collider2D.enabled = true;
        spriteRenderer.enabled = true;
        OnPlayerRespawn?.Invoke();
        Debug.Log("OnPlayerRespawn");
        Time.timeScale = 1f;
    }

    void Die() {
        isDead = true;
        collider2D.enabled = false;
        spriteRenderer.enabled = false;
        OnPlayerDeath?.Invoke();
        Debug.Log("OnPlayerDeath");
        Time.timeScale = 0.2f;
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

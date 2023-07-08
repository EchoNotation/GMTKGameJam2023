using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GoonController : MonoBehaviour
{
    MapManager mapManager;

    public float speed = 2f;

    bool hasDestination = false;
    Vector3 destination;

    float restUntil = 0;

    // Start is called before the first frame update
    void Start()
    {
        mapManager = FindAnyObjectByType<MapManager>();
    }

    private void OnEnable() {
        Player.OnPlayerDeath += Glow;
        Player.OnPlayerRespawn += StopGlowing;
    }

    private void OnDisable() {
        Player.OnPlayerDeath -= Glow;
        Player.OnPlayerRespawn -= StopGlowing;
    }

    void Glow() {

    }

    void StopGlowing() {

    }

    void PickDestination() {
        destination = mapManager.GetDownFlowField(transform.position);
        hasDestination = true;
    }

    private void OnMouseDown() {
        Player player = GameObject.FindObjectOfType<Player>();
        if(player && player.isDead) {
            player.Respawn(transform.position);
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision) {
        string tag = collision.GetComponent<Collider2D>().gameObject.tag;
        if(tag == "Bolt" || tag == "Whirly" || tag == "SpikeTrap") {
            Die();
        }
    }

    void Die() {
        mapManager.RegisterDeath(transform.position);
        Destroy(gameObject);
        enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!hasDestination) {
            // 10% chance to rest
            if(UnityEngine.Random.value < 0.9)
                PickDestination();
            else
                restUntil = Time.time + UnityEngine.Random.Range(0, 5);
        }

        else if(Time.time > restUntil) {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            Debug.DrawLine(transform.position, destination);

            float distance = Vector3.Distance(transform.position, destination);
            if(distance < 0.05f || distance > 2f) hasDestination = false;
        }
    }
}

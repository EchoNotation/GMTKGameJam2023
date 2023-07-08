using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 2f;

    Rigidbody2D rigidBody;

    private void Start() {
        rigidBody = GetComponent<Rigidbody2D>();   
    }

    void FixedUpdate()
    {
        Vector3 desiredDirection = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
        desiredDirection = desiredDirection.normalized;

        rigidBody.velocity = desiredDirection * speed;
    }
}

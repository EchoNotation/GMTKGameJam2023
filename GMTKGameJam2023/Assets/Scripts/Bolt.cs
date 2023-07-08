using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bolt : MonoBehaviour
{
    private float speed = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float angle = (transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad;
        transform.position += new Vector3(Mathf.Cos(angle) * speed * Time.deltaTime, Mathf.Sin(angle) * speed * Time.deltaTime, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}

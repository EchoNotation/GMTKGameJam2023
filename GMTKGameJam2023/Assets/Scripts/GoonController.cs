using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoonController : MonoBehaviour
{
    MapManager mapManager;

    public float speed = 2f;

    bool hasDestination = false;
    Vector3 destination;

    // Start is called before the first frame update
    void Start()
    {
        mapManager = FindAnyObjectByType<MapManager>();
    }

    void PickDestination() {
        destination = mapManager.GetDownFlowField(transform.position);
        hasDestination = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(!hasDestination) PickDestination();

        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        Debug.DrawLine(transform.position, destination);

        float distance = Vector3.Distance(transform.position, destination);
        if(distance < 0.05f || distance > 2f) hasDestination = false;
    }
}

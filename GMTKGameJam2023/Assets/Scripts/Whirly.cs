using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whirly : MonoBehaviour
{
    private float spinRate = -180;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, 0, spinRate * Time.deltaTime));
        //this.transform.rotation.SetEulerAngles(angles.x, angles.y, angles.z += spinRate * Time.deltaTime);
    }
}

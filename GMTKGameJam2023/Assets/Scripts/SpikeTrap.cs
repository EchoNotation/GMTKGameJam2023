using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    private bool isExtended;
    public Sprite retracted, extended;
    public float offset;
    private float lastTime, extendedTime, retractedTime;
    public BoxCollider2D spikeCollider;
    private AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        isExtended = false;

        lastTime = offset;
        extendedTime = 1f;
        retractedTime = 3f;
    }

    // Update is called once per frame
    void Update()
    {
        if(isExtended && Time.time >= lastTime + extendedTime)
        {
            //Retract
            lastTime = Time.time;
            isExtended = false;
            spikeCollider.enabled = false;
            GetComponent<SpriteRenderer>().sprite = retracted;
        }
        else if(!isExtended && Time.time >= lastTime + retractedTime)
        {
            //Extend
            lastTime = Time.time;
            isExtended = true;
            spikeCollider.enabled = true;
            GetComponent<SpriteRenderer>().sprite = extended;
            source.Stop();
            source.Play();
        }
    }
}

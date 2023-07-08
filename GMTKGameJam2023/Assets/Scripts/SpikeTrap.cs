using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    private bool isExtended;
    public Sprite retracted, extended;
    private Stopwatch timer;
    private long lastTime, extendedTime, retractedTime;
    public BoxCollider2D spikeCollider;

    // Start is called before the first frame update
    void Start()
    {
        isExtended = false;
        timer = new Stopwatch();
        timer.Start();

        lastTime = 0;
        extendedTime = 1000;
        retractedTime = 3000;
    }

    // Update is called once per frame
    void Update()
    {
        if(isExtended && timer.ElapsedMilliseconds >= lastTime + extendedTime)
        {
            //Retract
            lastTime = timer.ElapsedMilliseconds;
            isExtended = false;
            spikeCollider.enabled = false;
            GetComponent<SpriteRenderer>().sprite = retracted;
        }
        else if(!isExtended && timer.ElapsedMilliseconds >= lastTime + retractedTime)
        {
            //Extend
            lastTime = timer.ElapsedMilliseconds;
            isExtended = true;
            spikeCollider.enabled = true;
            GetComponent<SpriteRenderer>().sprite = extended;
        }
    }
}

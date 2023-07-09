using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplatSelector : MonoBehaviour
{
    public List<Sprite> splates = new List<Sprite>();
    SpriteRenderer spriteRenderer;
    public float alpha = 0.1f;

    Color color;

    // Start is called before the first frame update
    void Start()
    {
        int idx = Random.Range(0, splates.Count);
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = splates[idx];
        spriteRenderer.color = new Color(color.r, color.g, color.b, alpha);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0f, 360f)));
    }

    public void SetColor(Color color) {
        this.color = color;
    }
}

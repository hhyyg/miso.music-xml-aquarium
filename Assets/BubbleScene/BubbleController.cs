using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleController : MonoBehaviour
{
    public ScoreRenderer ScoreRenderer;
    public float Speed = 100f;
    public float MaxY = 100;

    void Update()
    {
        var distance = this.Speed * UnityEngine.Time.deltaTime;
        transform.Translate(0, distance, 0);

        if (transform.position.y > this.MaxY)
        {
            this.ScoreRenderer?.ReleaseBubble(this.gameObject);
        }
    }
}

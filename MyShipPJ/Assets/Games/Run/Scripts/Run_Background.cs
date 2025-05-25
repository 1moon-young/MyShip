using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Run_Background : MonoBehaviour
{
    Vector3 size;
    public float slow; // ���� = 4, �ٰ� = 1;

    void Start()
    {
        size = GetComponent<SpriteRenderer>().bounds.size;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void Move()
    {
        Vector3 movement = Vector3.left * Time.deltaTime * RunManager.instance.speed;
        transform.position += movement / slow;

        if (transform.position.x < -size.x)
        {
            transform.position += Vector3.right * (size.x * 2 - 0.02f);
        }
    }
}

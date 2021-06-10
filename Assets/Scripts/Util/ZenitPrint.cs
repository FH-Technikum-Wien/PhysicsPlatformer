using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZenitPrint : MonoBehaviour
{
    private Vector3 lastPosition;
    private bool falling = false;
    private void Start()
    {
        lastPosition = transform.position;
    }
    private void FixedUpdate()
    {
        if(!falling)
        {
            if (lastPosition.y >= transform.position.y)
            {
                falling = true;
                Debug.Log(lastPosition.y);
            }
        }
        else
        {
            if (lastPosition.y < transform.position.y)
            {
                falling = false;
            }
        }
        lastPosition = transform.position;
    }
}

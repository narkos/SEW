using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothBodyController : MonoBehaviour
{
    private Vector3 previousPosition;
    private Vector3 nextPosition;
    private bool startMove;
    private float interval;
    private float speed;

    public void SetNewTarget(Vector3 pos, float interval, float speed)
    {
        previousPosition = transform.position;
        nextPosition = pos;
        startMove = true;
    }

    public void Move (float t)
    {
        transform.position = Vector3.Lerp(previousPosition, nextPosition, t);
    }
}

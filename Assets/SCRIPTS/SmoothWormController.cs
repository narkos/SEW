using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothWormController : MonoBehaviour
{
    // public GameObject bodyPrefab;
    public float interval = 0.5f;
    public float moveSpeed = 1.0f;
    
    private Vector3 nextPosition = Vector3.zero;
    private Vector3 storedDirection = Vector3.right;
    private Vector3 previousDirection;
    void Start() 
    {
        StartCoroutine(TimedPlayerUpdate());
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(x) > 0.0f && !(Mathf.Abs(previousDirection.x) > 0.0f))
        {
            storedDirection = new Vector3(x, 0, 0);
        }
        if (Mathf.Abs(y) > 0.0f && !(Mathf.Abs(previousDirection.z) > 0.0f))
        {
            storedDirection = new Vector3(0, 0, y);
        }
    }

    IEnumerator TimedPlayerUpdate()
    {
        nextPosition = storedDirection + transform.position;
        previousDirection = storedDirection;
        StartCoroutine(MovePlayer(transform.position, nextPosition, 1.0f));
        yield return new WaitForSeconds(interval);
        StartCoroutine(TimedPlayerUpdate());
    }
    
    IEnumerator MovePlayer(Vector3 startPosition, Vector3 targetPosition, float speed)
    {
        float step = (moveSpeed / (startPosition - targetPosition).magnitude);
        float t = 0;
        while (t <= 1.0f)
        {
            t += (step * Time.deltaTime);
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return new WaitForEndOfFrame();
        }
        transform.position = targetPosition;
        yield return null;
    }
}

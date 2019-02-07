using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothWormController : MonoBehaviour
{
    // public GameObject bodyPrefab;
    public float interval = 0.5f;
    public float moveSpeed = 1.0f;

    private float step = 0.0f;
    private float timer = 0.0f;

    private Vector3 nextPosition = Vector3.zero;
    private Vector3 previousPosition = Vector3.zero;
    private Vector3 storedDirection = Vector3.right;
    private Vector3 previousDirection;

    public GameObject bodyPrefab;
    private List<Vector3> pastPositions;
    private List<SmoothBodyController> bodyParts;
    private int bodyCount = 0;
    private FoodController foodController;

    public MetaballsRaymarching shaderController;

    void Start()
    {
        shaderController = Camera.main.GetComponent<MetaballsRaymarching>();
        pastPositions = new List<Vector3>();
        bodyParts = new List<SmoothBodyController>();
        foodController = GameObject.FindGameObjectWithTag("FoodController").GetComponent<FoodController>();
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
        if (shaderController != null) {
            shaderController.SetPositions(bodyParts);
        }
    }

    IEnumerator TimedPlayerUpdate()
    {
        nextPosition = storedDirection + transform.position;
        previousDirection = storedDirection;
        previousPosition = transform.position;
        pastPositions.Insert(0, previousPosition);
        if (pastPositions.Count > bodyParts.Count + 1) {
            pastPositions.RemoveAt(pastPositions.Count - 1);
        }
        SetBodyParts();
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
            MoveBodyParts(t);
            yield return new WaitForEndOfFrame();
        }
        transform.position = targetPosition;
        yield return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Food")
        {
            bodyCount++;
            Destroy(other.gameObject);
            foodController.SpawnFood();
            GameObject newPart = Instantiate(bodyPrefab, pastPositions[pastPositions.Count - 1], Quaternion.identity);
            newPart.GetComponent<SmoothBodyController>().SetNewTarget(pastPositions[pastPositions.Count - 1], interval, moveSpeed);
            
            bodyParts.Add(newPart.GetComponent<SmoothBodyController>());
        }
    }

    private void MoveBodyParts(float t)
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].Move(t);
        }
    }

    private void SetBodyParts()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].SetNewTarget(pastPositions[i], interval, moveSpeed);
        }
    }
}

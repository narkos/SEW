using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WormController : MonoBehaviour
{
    public GameObject bodyPrefab;
    public float interval = 0.5f;
    public FoodController foodController;
    private Vector3 currentDirection = Vector3.right;
    private IEnumerator moveRoutine;
    private bool move = false;
    private List<GameObject> bodyParts = new List<GameObject>();
    private int length = 1;
    private bool didEat = false;
    private Vector3 spawnPosition;

    void Start()
    {
        if (foodController != null)
        {

        }
        move = true;
        moveRoutine = MoveTimer();
        StartCoroutine(moveRoutine);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.0f)
            {
                currentDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, 0);
            }
            else if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.0f)
            {
                currentDirection = new Vector3(0, 0, Input.GetAxisRaw("Vertical"));
            }
        }
        if (move)
        {
            HandleBodyPart(spawnPosition);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Food")
        {
            length++;
            Destroy(other.gameObject);
            foodController.SpawnFood();
        }
        //else if (other.gameObject != bodyParts[0])
        else
        {
            print(other.gameObject.name);
            //bodyParts.Clear();
            foreach(GameObject part in bodyParts) 
            {
                Destroy(part);
            }
            bodyParts.Clear();
            length = 1;
        }
    }

    private void HandleBodyPart(Vector3 position) 
    {
        if (bodyParts.Count < length)
        {
            bodyParts.Insert(0, (GameObject)Instantiate(bodyPrefab, position, Quaternion.identity));
        }
        else
        {
            bodyParts.Last().transform.position = position;
            bodyParts.Insert(0, bodyParts.Last());
            bodyParts.RemoveAt(bodyParts.Count - 1);
        }
        move = false;
    }

    IEnumerator MoveTimer()
    {
        yield return new WaitForSeconds(interval);
        spawnPosition = transform.position;
        transform.Translate(currentDirection);
        move = true;
        StartCoroutine(MoveTimer());
    }
}

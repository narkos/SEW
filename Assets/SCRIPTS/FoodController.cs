using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodController : MonoBehaviour
{
    public GameObject foodPrefab;
    public int halfWidth = 15;
    public int halfHeight = 15;

    private bool shouldSpawnFood = true;
    private int amount = 5;

    void Start()
    {

    }

    void Update()
    {
        if (shouldSpawnFood)
        {
            Spawn();
            shouldSpawnFood = false;
        }
    }

    public void SpawnFood()
    {
        shouldSpawnFood = true;
    }

    private void Spawn()
    {
        if (foodPrefab != null)
        {
            int missing = amount - transform.childCount;
            for (int i = 0; i < missing; i++)
            {

                //print(x + " " + y);
                //print(pos);
                Instantiate(foodPrefab, FindSpawnPosition(), Quaternion.identity, transform);

            }
        }
    }

    private Vector3 FindSpawnPosition()
    {
        Vector3 pos = Vector3.zero;
        bool foundPosition = false;
        while (!foundPosition)
        {
            int x = (int)Random.Range(0, Screen.width);
            int y = (int)Random.Range(0, Screen.height);
            pos = Camera.main.ScreenToWorldPoint(new Vector3(x, y, 150));
            pos.x = Mathf.Floor(pos.x);
            pos.z = Mathf.Floor(pos.z);
            
            Collider[] hits = Physics.OverlapSphere(pos, 0.4f);
            if(hits.Length == 0)
            {
                foundPosition = true;
            } else {
                print("collided with a thing");
            }
        }
        return pos;
    }
}

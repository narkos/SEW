using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassDisplacementScript : MonoBehaviour
{
    [SerializeField]
    private float radius;
    [SerializeField]
    public Material grassMaterial;

    void Start()
    {
        if (grassMaterial != null)
        {
            //grassMaterial.SetFloat()
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (grassMaterial != null)
        {
            grassMaterial.SetVector("_TargetPosition", transform.position);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddForceAtHeight : MonoBehaviour
{
    [SerializeField] float force;
    [SerializeField] float height;
    private Rigidbody rb;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if(height <= Mathf.Epsilon)
        {
            rb.AddForce(Vector3.down * force, ForceMode.Acceleration);
            return;
        }

        RaycastHit hit;
        if(!Physics.Raycast(this.transform.position, Vector3.down, out hit, height))
        {
            rb.AddForce(Vector3.down * force, ForceMode.Acceleration);
        }
    }
}

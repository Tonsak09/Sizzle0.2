using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorqueTowardsRotation : MonoBehaviour
{

    [SerializeField] Vector3 target;
    [SerializeField] float torque;

    public Vector3 Target { get { return target; } set { target = value; } }

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.AddTorque(Vector3.Cross(target, this.transform.up) * torque);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(this.transform.position, this.transform.position - target * 0.5f);
    }
}

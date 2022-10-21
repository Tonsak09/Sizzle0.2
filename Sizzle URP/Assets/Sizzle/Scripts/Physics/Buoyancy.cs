using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buoyancy : MonoBehaviour
{

    [SerializeField] float maxBuoyancyForce;
    [SerializeField] float fallForce;
    [SerializeField] float height;
    //[SerializeField] float fallForce;
    [SerializeField] LayerMask layer;
    [SerializeField] AnimationCurve buoyancyForceCurve;

    [SerializeField] Vector3 offset;

    [SerializeField] float disBeforeFalling;

    [SerializeField] bool showGizmos;


    private Rigidbody rb;
    private bool addingBuoyancy;
    public float startingHeight { get; set; }

    public float Height { get { return height; } set { height = value; } }
    public bool AddingBuoyancy {  get { return addingBuoyancy; } }

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // ADD force at position for multiple points!!!!

        //UpdateRayCheck();
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(this.transform.position, Vector3.down, out hit, height, layer))
        {
            float disFromGround = Vector3.Distance(this.transform.position, hit.point);
            float forceModifier = buoyancyForceCurve.Evaluate(1 - Mathf.Clamp01(disFromGround / height)) * maxBuoyancyForce;
            rb.AddForce(Vector3.up * forceModifier * Time.deltaTime, ForceMode.Acceleration);
            addingBuoyancy = true;
        }
        else
        {
            if(Vector3.Distance(this.transform.position, hit.point) > disBeforeFalling)
            {
                rb.AddForce(Vector3.down * fallForce * Time.deltaTime, ForceMode.Acceleration);
                addingBuoyancy = false;
            }
        }

    }

    /*private void UpdateRayCheck()
    {
        if(Physics.Raycast(this.transform.position, Vector3.down, out hit, 100, layer))
        {
            float disFromGround = Vector3.Distance(this.transform.position, hit.point);
            float forceModifier = (1 - Mathf.Clamp01(disFromGround / depthBeforeSubmerged)) * maxBuoyancyForce;
            
            
            // Not falling
            *//*if (disFromGround <= maxDisFromGround)
            {
                float xValue = disFromGround / maxBuoyancyForce;

                float force = buoyancyForceCurve.Evaluate(xValue) * maxBuoyancyForce;
                print(force);
                rb.AddForce(force * Vector3.up, ForceMode.Acceleration);
            }*//*
        }
    }*/

    private void OnDrawGizmos()
    {
        if(showGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.transform.position, this.transform.position - Vector3.up * disBeforeFalling);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(this.transform.position, this.transform.position - Vector3.up * height);
            Gizmos.DrawSphere(this.transform.position + offset, 0.01f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Chargeable
{
    [SerializeField] ChargeObj co;
    [SerializeField] Vector3 jumpStartoffset;
    [SerializeField] float jumpForce;
    private Rigidbody rb;
    private bool grounded;

    public Vector3 dir;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    public override void Desperse()
    {
        base.Desperse();

        if (currentCharge <= 0)
        {
            // No longer charges other chargeables 
            co.active = false;

            return;
        }
        else
        {
            // Is able to charge other objects 
            co.active = true;
        }

        if (grounded)
        {
            // Add force oppoiste of direction that charge comes from 
            rb.AddForce((dir + jumpStartoffset) * jumpForce, ForceMode.Impulse);
            print((dir + jumpStartoffset) * jumpForce);
            grounded = false;
        }
    }

    public void AddCharge(float chargeAmount, Vector3 dir)
    {
        base.AddCharge(chargeAmount);
        this.dir = dir;
    }

    private void OnCollisionEnter(Collision collision)
    {
        grounded = true;
    }
}

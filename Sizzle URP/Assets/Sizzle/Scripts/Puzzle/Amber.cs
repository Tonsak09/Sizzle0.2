using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Amber : Chargeable
{
    [SerializeField] ChargeObj co;

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
    }
}

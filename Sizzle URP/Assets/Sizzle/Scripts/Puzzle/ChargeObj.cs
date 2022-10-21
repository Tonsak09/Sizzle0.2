using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Charges objects that it comes in contact with 
public class ChargeObj : MonoBehaviour
{
    
    [SerializeField] float chargeAmount;

    public bool active;

    private void OnTriggerEnter(Collider other)
    {
        if(active)
        {
            Chargeable objChargeable = other.GetComponent<Chargeable>();

            if (objChargeable != null)
            {
                objChargeable.AddCharge(chargeAmount);
                print(other.name);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(active)
        {
            // Some chargeables exit by "destroy" so this is current solution TODO: Change to hold in local array 
            Chargeable objChargeable = other.GetComponent<Chargeable>();

            if (objChargeable != null)
            {
                // Staying in the charge filed continues to add charge 
                if(other.tag == "Slime")
                {
                    // Special case where slime gets moved as well 
                    ((Slime)objChargeable).AddCharge(chargeAmount * Time.deltaTime, (other.transform.position - this.transform.position).normalized);
                }
                else
                {
                    objChargeable.AddCharge(chargeAmount * Time.deltaTime);
                }
            }
        }
    }
}

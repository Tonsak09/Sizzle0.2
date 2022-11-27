using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chargeable : MonoBehaviour
{
    [SerializeField] protected float desperseChargeSpeed;
    [SerializeField] protected float maxCharge;

    [ColorUsage(false, true)]
    [SerializeField] Color baseEmessiveColor;
    [ColorUsage(false, true)]
    [SerializeField] Color targetEmessiveColor;

    [SerializeField] protected Renderer renderer;

    protected private float currentCharge;
    protected Color Base { get { return baseEmessiveColor; } }
    protected Color Target { get { return targetEmessiveColor; } }

    // Only meant for debug to read 
    public float CurrentCharge;

    /// <summary>
    /// Called when trying to charge this object 
    /// </summary>
    public virtual void AddCharge(float chargeAmount)
    {
        if((currentCharge + chargeAmount) > maxCharge)
        {
            currentCharge = maxCharge;
        }
        else
        {
            currentCharge += chargeAmount;
        }
    }

    /// <summary>
    /// Looses charge every update at speed 
    /// </summary>
    public virtual void Desperse()
    {

        // Set emissivity from 0 - 10
        renderer.material.SetColor("_EmissionColor", Color.Lerp(baseEmessiveColor, targetEmessiveColor, Mathf.InverseLerp(0, maxCharge, currentCharge)));

        // Base virtual code will make sure it is never below 0 
        if (currentCharge <= 0)
        {
            // No longer charges other chargeables 
            currentCharge = 0;

            return;
        }

        currentCharge -= Time.deltaTime * desperseChargeSpeed;
    }

    private void Update()
    {
        Desperse();

        CurrentCharge = currentCharge;
    }
}

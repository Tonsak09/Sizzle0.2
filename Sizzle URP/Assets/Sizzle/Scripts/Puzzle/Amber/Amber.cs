using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Amber : Chargeable
{
    [SerializeField] ChargeObj co;
    [SerializeField] float fullyUnlockedCharge;
    [SerializeField] float fullChargeSpeed;
    /*[Tooltip("Once the amber has been fully charged it will lerp between these two values ")]
    [SerializeField] Vector2 lerpValuesToGlow;
    [SerializeField] AnimationCurve glowCurve;*/

    private bool fullyUnlocked;
    private float glowLerp;

    public bool Unlocked { get { return fullyUnlocked; } }

    public override void AddCharge(float chargeAmount)
    {
        // We do not want the addcharge to mess with the glow 
        if(!fullyUnlocked)
        {
            base.AddCharge(chargeAmount);

            if(currentCharge >= fullyUnlockedCharge)
            {
                fullyUnlocked = true;
                StartCoroutine(GoToFullCharge());
                //BeginGlow();
            }
        }
    }

    public override void Desperse()
    {
        if(!fullyUnlocked)
        {
            base.Desperse();
        }

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

    private IEnumerator GoToFullCharge()
    {
        float lerp = Mathf.InverseLerp(0, maxCharge, currentCharge);
        print(lerp);
        while(lerp <= 1)
        {
            renderer.material.SetColor("_EmissionColor", Color.Lerp(Base, Target, lerp));
            print(lerp);
            lerp += Time.deltaTime * fullChargeSpeed;
            yield return null;
        }

        // Start glow 
    }

    /*/// <summary>
    /// Initializes the amber so that it can glow properly 
    /// </summary>
    private void BeginGlow()
    {
        // Gets the lerp between 0 and max charge and then uses that value
        // to show where to begin glow
        glowLerp = Mathf.InverseLerp(lerpValuesToGlow.x, lerpValuesToGlow.y, Mathf.InverseLerp(0, maxCharge, currentCharge));

        StartCoroutine(Glow());
    }

    private IEnumerator Glow()
    {
        float startTime = Time.time;
        float hold = glowLerp;
        while(fullyUnlocked)
        {
            renderer.material.SetColor
                ("_EmissionColor", 
                Color.Lerp( 
                    Color.Lerp(Base, Target, lerpValuesToGlow.x), 
                    Color.Lerp(Base, Target, lerpValuesToGlow.y), 
                    glowCurve.Evaluate(glowLerp))
                );

            // Needs to lerp between two values but needs to be offseted by starting color 
            glowLerp += Mathf.PingPong(Time.time - startTime, 1) - hold;

            yield return null;
        }
    }*/
}

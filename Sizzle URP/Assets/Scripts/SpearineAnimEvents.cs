using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearineAnimEvents : MonoBehaviour
{
    [SerializeField] Spearine spearine;
    [SerializeField] Animator mainAnimator;

    [Header("Effects")]
    [SerializeField] ParticleSystem questionFX;
    [SerializeField] ParticleSystem alarmFX;
    [SerializeField] ParticleSystem trailFX;

    public void DisableAnimator()
    {
        mainAnimator.enabled = false;
    }

    public void SetState(Spearine.SpearineStates newState)
    {
        spearine.state = newState;
    }

    public void ChangeToLookLogic()
    {
        spearine.ChangeToLookLogic();
    }

    public void ChangeToAnim()
    {
        spearine.ChangeToLookLogic(false);
    }

    public void SetAnimatorAttackingToFalse()
    {
        mainAnimator.SetBool("attacking", false);
    }

    public void ShakeCam()
    {

    }

    public void TryKillSizzle()
    {
        if(spearine.IsHittingSizzle())
        {
            // Reset the screen 
            print("Hitting Sizzle");
            LevelManager.Reload();
        }
    }

    public void PlayEffectQuestion()
    {
        questionFX.Play();
    }
    public void PlayEffectAlarm()
    {
        alarmFX.Play();
    }

    public void PlayEffectTrail()
    {
        trailFX.Play();
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearineAnimEvents : MonoBehaviour
{
    [SerializeField] Spearine spearine;
    [SerializeField] Animator mainAnimator;

    [SerializeField] ParticleSystem questionFX;
    [SerializeField] ParticleSystem alarmFX;

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

    public void PlayEffectQuestion()
    {
        questionFX.Play();
    }
    public void PlayEffectAlarm()
    {
        alarmFX.Play();
    }
}

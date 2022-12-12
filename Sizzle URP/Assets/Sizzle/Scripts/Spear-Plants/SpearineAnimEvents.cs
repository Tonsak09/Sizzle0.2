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

    [Header("Values")]
    [SerializeField] float pauseBeforeReload;

    private Transitions transition;
    private bool reseting;

    private void Start()
    {
        transition = GameObject.FindObjectOfType<Transitions>();

        reseting = false;
    }

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
        if(spearine.IsHittingSizzle() && !reseting)
        {
            reseting = true;
            // Shake Sizzle


            // Reset the screen
            //transition.TryBlackOut();
            StartCoroutine(ResetScene(pauseBeforeReload));
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

    private IEnumerator ResetScene(float pauseBeforeReload)
    {
        GameObject.FindObjectOfType<Transitions>().ResetToCheckPoint();

        yield return new WaitForSeconds(pauseBeforeReload);
        reseting = false;
    }

}

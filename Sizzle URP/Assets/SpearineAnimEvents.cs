using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearineAnimEvents : MonoBehaviour
{
    [SerializeField] Spearine spearine;
    [SerializeField] Animator mainAnimator;

    public void DisableAnimator()
    {
        mainAnimator.enabled = false;
    }

    public void SetState(Spearine.SpearineStates newState)
    {
        spearine.state = newState;
    }
}

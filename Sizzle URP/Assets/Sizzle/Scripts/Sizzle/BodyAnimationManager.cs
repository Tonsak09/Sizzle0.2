using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyAnimationManager : MonoBehaviour
{
    private Dictionary<string, Coroutine> animations;

    private void Start()
    {
        animations = new Dictionary<string, Coroutine>();
    }

    /// <summary>
    /// Tries to animate the head if it is free to do so
    /// </summary>
    /// <param name="anim">The coroutine animation that is passed</param>
    /// <param name="priority">Will ovveride the current animation playing and immediatly go to the passed animation</param>
    /// <returns></returns>
    public bool TryAnimation(IEnumerator anim, string _key, bool priority = false)
    {
        // Animation is already contained 
        if(animations.ContainsKey(_key))
        {
            if(animations[_key] == null)
            {
                animations[_key] = StartCoroutine(anim);
                return true;
            }
            else if(priority == true)
            {
                // Priorty ends held animation then plays passed 
                EndAnimation(_key);
                animations[_key] = StartCoroutine(anim);
                return true;
            }
        }
        else
        {
            // Sets new container 
            animations.Add(_key, StartCoroutine(anim));
            return true;
        }

        // Default 
        return false; 
    }

    public void EndAnimation(string _key)
    {
        if (animations.ContainsKey(_key))
        {
            // Animation is stored  
            if (animations[_key] != null)
            {
                StopCoroutine(animations[_key]);
                animations[_key] = null;
            }
            
            // No need to do anything if already null
        }
    }
}

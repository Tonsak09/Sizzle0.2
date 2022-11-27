using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used when multiple amber are needed to activate some event 
/// </summary>
public class AmberSet : MonoBehaviour
{

    [SerializeField] List<Amber> amber;

    /// <summary>
    /// Gets whether all the amber are unlocked or not 
    /// </summary>
    /// <returns></returns>
    protected bool AllAmberUnlocked()
    {
        for (int i = 0; i < amber.Count; i++)
        {
            if(amber[i].Unlocked == false)
            {
                return false;
            }
        }

        return true;
    }
}

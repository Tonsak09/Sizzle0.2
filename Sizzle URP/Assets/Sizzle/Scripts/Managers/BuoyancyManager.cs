using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the speed at which the buoyancies bounce 
/// in relation to the speed of Sizzle 
/// </summary>
public class BuoyancyManager : MonoBehaviour
{
    [Tooltip("Raising order should neighbors and going from back to front of Sizzle model")]
    [SerializeField] Buoyancy[] buoyancies;
    [Tooltip("Used to reference the starting heights of Sizzle based on the model")]
    [SerializeField] Transform[] poseCopyBuoyancyPairs;

    private void Start()
    {
        for (int i = 0; i < buoyancies.Length; i++)
        {
            buoyancies[i].startingHeight = poseCopyBuoyancyPairs[i].localPosition.y;
        }
    }

    /// <summary>
    /// Adjusts the heights of all buoyancies 
    /// </summary>
    /// <param name="lerp">A lerp valued multiplied the default models heights</param>
    public void AdjustHeights(float lerp)
    {
        for (int i = 0; i < buoyancies.Length; i++)
        {
            buoyancies[i].Height = buoyancies[i].startingHeight * lerp;
        }
    }

    /// <summary>
    /// Projects the height twoards the up normal of the buoyancy
    /// </summary>
    public void ProjectHeights()
    {
        for (int i = 0; i < buoyancies.Length; i++)
        {
            buoyancies[i].Height = Vector3.Project(-buoyancies[i].transform.up * buoyancies[i].Height, Vector3.down).magnitude;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKLegPoseCopy : MonoBehaviour
{

    // This class acts like a manager for the whole leg instead of having the
    // main manager work on the individual joints and foot 
    [SerializeField] List<PoseCopy> legJoints;

    public void UpdateLeg()
    {

    }

    /*public override void UpdateTarget()
    {
        base.UpdateTarget();
    }*/

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseCopy : MonoBehaviour
{
    [Tooltip("What bone is being influnced by this object's direction")]
    [SerializeField] protected Transform boneTarget;
    [SerializeField] protected Vector3 rotOffset;

    public virtual Quaternion TargetValue
    {
        get
        {
            return boneTarget.rotation;
        }
    }

    public Vector3 RotOffset { get { return rotOffset; } set { rotOffset = value; } }

    /// <summary>
    /// Sets the rotation of the target to this rotation 
    /// </summary>
    public virtual void UpdateTarget()
    {
        boneTarget.rotation = Quaternion.Inverse(this.transform.rotation);
    }
}

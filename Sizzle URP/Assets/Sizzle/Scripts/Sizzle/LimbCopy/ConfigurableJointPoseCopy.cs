using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurableJointPoseCopy : PoseCopy
{
    private ConfigurableJoint CJoint;

    /// <summary>
    /// Gets the targets current rotational value
    /// </summary>
    /// 

    public override Quaternion TargetValue
    {
        get
        {
            return CJoint.targetRotation;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        CJoint = boneTarget.GetComponent<ConfigurableJoint>();
    }

    /// <summary>
    /// Sets the target of the CJoint to this rotation 
    /// </summary>
    public override void UpdateTarget()
    {
        CJoint.targetRotation = Quaternion.Inverse(Quaternion.Euler(this.transform.localEulerAngles + rotOffset));
    }

    public void UpdateTargetHard()
    {
        base.UpdateTarget();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Reference: https://www.weaverdev.io/blog/bonehead-procedural-animation

public class SkeletonController : MonoBehaviour
{

    // Works using the skeleton custom joint and adjusts
    // where the bone is meant to be instead of directly
    // changing the rotation of the bone.

    // The target we are going to track
    [SerializeField] Transform target;

    // A reference to the bone being rotated 
    [SerializeField] Transform bone;

    [SerializeField] float turnSpeed;
    [SerializeField] float maxTurnAngle;

    [SerializeField] ConfigurableJoint cJoint;

    // The rotation that Sizzle will be at by default 
    private Quaternion baseRot;

    private void Start()
    {
        baseRot = cJoint.targetRotation;
    }

    void LateUpdate()
    {
        UpdateBone();
    }


    /// <summary>
    /// Adjusts the head to look towards the target position 
    /// </summary>
    private void UpdateBone()
    {
        // Store the current head rotation since we will be resetting it
        Quaternion currentLocalRotation = bone.localRotation;
        // Reset the head rotation so our world to local space transformation will use the head's zero rotation. 
        // Note: Quaternion.Identity is the quaternion equivalent of "zero"
        bone.localRotation = Quaternion.identity;

        Vector3 targetWorldLookDir = target.position - bone.position;
        Vector3 targetLocalLookDir = bone.InverseTransformDirection(targetWorldLookDir);

        // Apply angle limit
        targetLocalLookDir = Vector3.RotateTowards(
          Vector3.forward,
          targetLocalLookDir,
          Mathf.Deg2Rad * maxTurnAngle, // Note we multiply by Mathf.Deg2Rad here to convert degrees to radians
          0 // We don't care about the length here, so we leave it at zero
        );

        // Get the local rotation by using LookRotation on a local directional vector
        Quaternion targetLocalRotation = Quaternion.LookRotation(targetLocalLookDir, Vector3.up);

        // Apply smoothing
        //bone.localRotation = Quaternion.Slerp(
        Quaternion smoothedRot = Quaternion.Slerp(
          currentLocalRotation,
          targetLocalRotation,
          1 - Mathf.Exp(-turnSpeed * Time.deltaTime)
        );

        cJoint.targetRotation = smoothedRot; //+ baseRot;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawSphere(target.position, 0.1f);
    }
}

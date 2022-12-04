using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegsController : MonoBehaviour
{
    [Header("Legs")]
    [SerializeField] LegIKSolver frontLeft;
    [SerializeField] LegIKSolver frontRight;
    [SerializeField] LegIKSolver backLeft;
    [SerializeField] LegIKSolver backRight;

    [Header("Speeds")]
    [SerializeField] float footSpeedMoving;
    [SerializeField] float footSpeedNotMoving;

    [Header("Values")]
    [SerializeField] float minlerpBeforePair;

    [Header("Dashing")]
    [SerializeField] LayerMask mask;
    [SerializeField] Vector3 frontDashTarget;
    [SerializeField] Vector3 backDashTarget;
    [SerializeField] Vector3 frontSlowedTarget;
    [SerializeField] Vector3 backSlowedTarget;

    [SerializeField] float minVel;
    [SerializeField] float maxVel;
    [SerializeField] AnimationCurve feetToTargetCurve;

    private BodyAnimationManager animManager;
    private ForceController controller;

    private LegIKSolver[] frontPair;
    private LegIKSolver[] backPair;
    private LegIKSolver[] allLegs;

    private const string KEY = "LEGS";


    public bool Active { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        animManager = GameObject.FindObjectOfType<BodyAnimationManager>();
        controller = this.GetComponent<ForceController>();

        frontPair = new LegIKSolver[] { frontLeft, frontRight };
        backPair = new LegIKSolver[] { backLeft, backRight };

        animManager.TryAnimation(WalkCycleCo(frontPair, backPair), KEY);
    }

    public void Dash(Rigidbody body)
    {
        animManager.TryAnimation(DashCo(body), KEY, true);
    }

    private IEnumerator DashCo(Rigidbody body)
    {
        // Only animates during aciton state 
        while (controller.CurrentSizzleState == ForceController.states.action)
        {
            float mag = body.velocity.magnitude;

            // For each leg move target towards dashTarget
            Vector3 frontPos = frontPair[0].transform.TransformDirection(frontDashTarget);
            frontPair[0].Target = Vector3.Lerp(frontPair[0].HoldFloorTarget, frontPos, feetToTargetCurve.Evaluate(Mathf.InverseLerp(minVel, maxVel, mag)));

            yield return null;
        }

        // Turn normal walking back on 
        // True turns off this coroutine as well 
        animManager.TryAnimation(WalkCycleCo(frontPair, backPair), KEY, true); 
    }


    private IEnumerator WalkCycleCo(LegIKSolver[] front, LegIKSolver[] back)
    {

        if (!Active)
        {
            yield return null;
        }

        // Index 
        while (true)
        {
            RunPair(front);
            RunPair(back);

            yield return null;
        }
    }

    private void RunPair(LegIKSolver[] pair)
    {
        // Find primary leg moving
        if (pair[0].Moving && !pair[1].Moving)
        {
            if (pair[0].Lerp >= minlerpBeforePair)
            {
                pair[1].TryMove(footSpeedMoving, footSpeedNotMoving);
            }
        }
        if (!pair[0].Moving && pair[1].Moving)
        {
            if (pair[1].Lerp >= minlerpBeforePair)
            {
                pair[0].TryMove(footSpeedMoving, footSpeedNotMoving);
            }
        }
        if (!pair[0].Moving && !pair[1].Moving)
        {
            // If neither are moving try to move one randomly 
            pair[Random.Range(0, 2)].TryMove(footSpeedMoving, footSpeedNotMoving);
        }
    }

    private void OnDrawGizmos()
    {
        // Visualization for dash details 
        /*Gizmos.color = Color.blue;
        if(Application.isPlaying)
        {
            Vector3 frontPos = frontPair[0].transform.TransformDirection(frontDashTarget);
            Gizmos.DrawSphere(frontPair[0].transform.position + frontPos, 0.01f);

            Gizmos.DrawSphere(frontPair[0].HoldFloorTarget, 0.1f);
        }*/
    }

}

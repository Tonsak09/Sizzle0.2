using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody baseBody;
    [SerializeField] Transform frontBody;
    [SerializeField] Transform neck;
    [SerializeField] LegsController legsController;
    [Tooltip("Used to get if grounded or not")]
    [SerializeField] Buoyancy midBodyBuoyancy;

    [Header("Orientation")]
    [SerializeField] TorqueTowardsRotation midBoneTorqueCorrection;
    [Tooltip("This is the maximum distance that the correction vec can be from Vector3.down")]
    [SerializeField] float maxVecDisFromDown;
    [SerializeField] float disBetweenChecks;

    [Header("Checks")]
    [SerializeField] Vector3 frontCheckCenter;
    [SerializeField] float frontCheckRadius;
    [SerializeField] LayerMask checkMask;

    [Header("Walking")]
    [SerializeField] float moveForce;
    [SerializeField] float torqueForce;

    [Header("Crouching")]
    [SerializeField] KeyCode crouchkey;
    [SerializeField] float moveForceCrouch;
    [SerializeField] float torqueForceCrouch;
    [SerializeField] float crouchSpeed;
    [SerializeField] float unCrouchSpeed;
    [SerializeField] float minLerp;

    [SerializeField] BuoyancyManager bManager;

    private float crouchLerp;
    private float baseHeightLerp;

    [Header("Dash")]
    [SerializeField] KeyCode dashKey;
    [SerializeField] float dashForceImpulse;
    [SerializeField] float dashForceContinuous;
    [Tooltip("When hitting an object before the dash ends Sizzle is set backwards")]
    [SerializeField] float dashBounceBackImpulse;
    [SerializeField] float dashBounceBackVertical;
    [SerializeField] float dashTime;
    [Tooltip("The minimum speed that Sizzle must maintain to stay in dash")]
    [SerializeField] float minSqrtSpeedForDash;

    [SerializeField] AnimationCurve dashForceoverLerp;

    private Coroutine DashCo;

    [Header("Jump")]
    [SerializeField] KeyCode jumpKey;
    [SerializeField] float jumpForceImpulse;
    [SerializeField] float jumpForceContinuous;
    [SerializeField] float jumpTime;

    [SerializeField] AnimationCurve jumpForceOverLerp;

    private Coroutine JumpCo;

    [Header("Sliding")]
    [SerializeField] float angleBeforeSlide;

    [Space]
    [SerializeField] private states SizzleState;
    public states CurrentSizzleState { get { return SizzleState; } }
    public enum states
    {
        movement,
        crouch,
        action
    };

    // This is decided by the main buoyancy, midBody
    private bool isGrounded;


    // Start is called before the first frame update
    void Start()
    {
        SizzleState = states.movement;
    }

    // Update is called once per frame
    void Update()
    {
        /*Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;*/
        isGrounded = midBodyBuoyancy.AddingBuoyancy;

        SurfaceLogic();
        Statemachine();

        bManager.AdjustHeights(crouchLerp * baseHeightLerp);
        //bManager.ProjectHeights();

        print(CanMoveForwad());
    }

    private void Statemachine()
    {
        switch (SizzleState)
        {
            case states.movement:
                ForceControl(moveForce, torqueForce);

                if (Input.GetKey(crouchkey))
                {
                    // Change to crouch state 
                    SizzleState = states.crouch;
                }
                else
                {
                    crouchLerp = Mathf.Clamp(crouchLerp + Time.deltaTime * unCrouchSpeed, minLerp, 1);
                }

                baseHeightLerp *= crouchLerp;

                TryDash(); // Checks whether the dash state should begin 


                break;
            case states.crouch:

                ForceControl(moveForceCrouch, torqueForceCrouch); // Slower Movement 
                CrouchLogic();
                TryJump();

                break;
            case states.action:
                break;

        }
    }

    /// <summary>
    /// The basic controls of Sizzle that allow directional movement
    /// and turning
    /// </summary>
    /// <param name="moveForce"></param>
    /// <param name="torqueForce"></param>
    private void ForceControl(float moveForce, float torqueForce)
    {



        // Get the input 
        float VInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");

        baseBody.AddTorque(torqueForce * baseBody.transform.up * hInput * Time.deltaTime, ForceMode.Acceleration);

        if(VInput > 0)
        {
            if(!CanMoveForwad())
            {
                return;
            }
        }

        baseBody.AddForce(moveForce * Vector3.ProjectOnPlane(frontBody.transform.forward, -base.transform.up) * VInput * Time.deltaTime, ForceMode.Acceleration);
    }

    private void SurfaceLogic()
    {

        // Get average between two points 
        RaycastHit hitA;
        RaycastHit hitB;
        if (Physics.Raycast(baseBody.transform.position + baseBody.transform.forward * (disBetweenChecks / 2), Vector3.down, out hitA, 5) && Physics.Raycast(baseBody.transform.position - baseBody.transform.forward * (disBetweenChecks / 2), Vector3.down, out hitB, 5))
        {
            // Average normal 
            midBoneTorqueCorrection.Target = -(hitA.normal + hitB.normal).normalized;

            // Height from ground should be the line formed by the two hit points 
            Vector3 midPoint = Vector3.Lerp(hitA.point, hitB.point, 0.5f);

            float totalDis = Mathf.Abs(baseBody.position.y - midPoint.y);
            float b = totalDis - midBodyBuoyancy.startingHeight; // B value that needs to be eliminated 
            float unitValueOfB = b / midBodyBuoyancy.startingHeight;

            baseHeightLerp = (1 - unitValueOfB);

            return;
        }



        RaycastHit hit;
        if (Physics.Raycast(baseBody.transform.position, Vector3.down, out hit, 5))
        {
            // Get the distance between down and hit.normal 
            float disSquared = (Vector3.up - hit.normal).sqrMagnitude;
            //print(Vector3.SignedAngle(Vector3.up, hit.normal, Vector3.up));

            if(disSquared <= maxVecDisFromDown)
            {
                midBoneTorqueCorrection.Target = -hit.normal;
            }
            else
            {
                // Get closest vector towards 

                // Apply slide force 
            }
        }
    }

    /// <summary>
    /// If Possible begins the crouch action 
    /// </summary>
    private void CrouchLogic()
    {
        if(Input.GetKey(crouchkey))
        {
            crouchLerp = Mathf.Clamp(crouchLerp - Time.deltaTime * crouchSpeed, minLerp, 1);
        }
        else if (Input.GetKeyUp(crouchkey))
        {
            SizzleState = states.movement;
        }
    }

    /// <summary>
    /// If Possible begins the dash action 
    /// </summary>
    private void TryDash()
    {
        
        // Activates the dash state 
        if (Input.GetKeyDown(dashKey) && DashCo == null && isGrounded)
        {
            SizzleState = states.action;
            baseBody.AddForce(dashForceImpulse * baseBody.transform.forward, ForceMode.Impulse);
            legsController.Dash(baseBody); // Activates the dash leg animations 

            DashCo = StartCoroutine(DashSubroutine());
        }
    }

    /// <summary>
    /// If Possible begins the jump action 
    /// </summary>
    private void TryJump()
    {
        if(Input.GetKeyDown(jumpKey) && JumpCo == null && isGrounded)
        {
            //SizzleState = states.action;
            baseBody.AddForce(-midBoneTorqueCorrection.Target.normalized * jumpForceImpulse, ForceMode.Impulse);

            JumpCo = StartCoroutine(JumpSubroutine(-midBoneTorqueCorrection.Target.normalized));
        }
    }

    private bool CanMoveForwad()
    {
        Vector3 pos = neck.TransformDirection(frontCheckCenter);
        return !Physics.CheckSphere(neck.position + pos, frontCheckRadius, checkMask);
    }



    private IEnumerator DashSubroutine()
    {
        float timer = dashTime;
        while (timer >= 0)
        {
            /*if (frontBody.velocity.sqrMagnitude < minSqrtSpeedForDash)
            {
                // No longer fast enough
                break;
            }*/

            // Stopped by obstruction 
            if(!CanMoveForwad())
            {
                // Bounce back 
                BounceBack();
                break;
            }

            baseBody.AddForce(dashForceContinuous * dashForceoverLerp.Evaluate((timer / dashTime)) * baseBody.transform.forward * Time.deltaTime, ForceMode.Acceleration);

            timer -= Time.deltaTime;
            yield return null;
        }

        SizzleState = states.movement;
        if(DashCo != null) // Can be null if breaks right away 
        {
            StopCoroutine(DashCo);
        }
        DashCo = null;
    }

    /// <summary>
    /// Bounces Sizzle in the opposite direction that they are facing 
    /// </summary>
    private void BounceBack()
    {
        baseBody.velocity = Vector3.zero; // Sets main part of body to 0
        baseBody.AddForce(-baseBody.transform.forward * dashBounceBackImpulse + Vector3.up * dashBounceBackVertical, ForceMode.Impulse);
    }

    private IEnumerator JumpSubroutine(Vector3 dir)
    {

        // Need to check if grounded 

        float timer = jumpTime;
        while (timer >= 0)
        {
            // Jumps away from surface 
            baseBody.AddForce(jumpForceContinuous * jumpForceOverLerp.Evaluate((timer / dashTime)) * dir * Time.deltaTime, ForceMode.Acceleration);

            timer -= Time.deltaTime;
            yield return null;
        }

        SizzleState = states.movement;
        StopCoroutine(JumpCo);
        JumpCo = null;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 pos = neck.TransformDirection(frontCheckCenter);
        Gizmos.DrawWireSphere(neck.position + pos, frontCheckRadius);

        Gizmos.DrawSphere(baseBody.transform.position + baseBody.transform.forward * (disBetweenChecks / 2), 0.01f);
        Gizmos.DrawSphere(baseBody.transform.position - baseBody.transform.forward * (disBetweenChecks / 2), 0.01f);

        RaycastHit hitA;
        RaycastHit hitB;
        if (Physics.Raycast(baseBody.transform.position + baseBody.transform.forward * (disBetweenChecks / 2), Vector3.down, out hitA, 5) && Physics.Raycast(baseBody.transform.position - baseBody.transform.forward * (disBetweenChecks / 2), Vector3.down, out hitB, 5))
        {
            Gizmos.DrawLine(hitA.point, hitB.point);


            Gizmos.color = Color.yellow;
            Vector3 midPoint = Vector3.Lerp(hitA.point, hitB.point, 0.5f);
            Gizmos.DrawWireSphere(midPoint, 0.03f);
        }
    }
}

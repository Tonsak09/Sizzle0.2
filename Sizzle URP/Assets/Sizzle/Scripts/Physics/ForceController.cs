using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ForceController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody baseBody;
    [SerializeField] Transform frontBody;
    [SerializeField] Transform neck;
    [SerializeField] LegsController legsController;
    [Tooltip("Used to get if grounded or not")]
    [SerializeField] Buoyancy midBodyBuoyancy;

    [Header("Direction")]
    [SerializeField] Transform cam;
    [SerializeField] float autoTurnSpeed;
    [SerializeField] float minAngle;

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
    [Space]
    [SerializeField] float dashTime;
    [SerializeField] float coolDownTimeDash;
    [Space]
    [Tooltip("When hitting an object before the dash ends Sizzle is set backwards")]
    [SerializeField] float dashBounceBackImpulse;
    [SerializeField] float dashBounceBackVertical;
    [SerializeField] AnimationCurve dashForceoverLerp;
    [Space]
    [SerializeField] float dashVerticalForce;
    [SerializeField] AnimationCurve dashVerticalOverLerp;
    [SerializeField] AudioClip dashSound;
    [Space]
    [SerializeField] float camOutSpeed;
    [SerializeField] AnimationCurve camOutCurve;
    [SerializeField] float camBackSpeed;
    [SerializeField] AnimationCurve camBackCurve;
    [SerializeField] float dashTargetFOV;

    private Coroutine DashCo;
    private Coroutine dashFOVCo;
    float holdFOV;
    private float dashFOVLerp;

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

    private SoundManager sm;
    private CamManager cm;
    private CinemachineFreeLook cc;
    private Rigidbody frontRigid;


    private Vector3 normal;

    // This is decided by the main buoyancy, midBody
    private bool isGrounded;
    private float dashCoolDownTimer;


    // Start is called before the first frame update
    void Start()
    {
        SizzleState = states.movement;
        frontRigid = frontBody.GetComponent<Rigidbody>();
        sm = GameObject.FindObjectOfType<SoundManager>();
        cm = this.GetComponent<CamManager>();


        if (cam == null)
        {
            cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        }

        dashCoolDownTimer = coolDownTimeDash;

        cc = cm.CommonCam.GetComponent<CinemachineFreeLook>();
        holdFOV = cc.m_Lens.FieldOfView; 
    }

    // Update is called once per frame
    void Update()
    {
        /*Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;*/
        isGrounded = midBodyBuoyancy.AddingBuoyancy;

        //SurfaceLogic();
        Statemachine();

        bManager.AdjustHeights(baseHeightLerp);
        //bManager.AdjustHeights(crouchLerp * baseHeightLerp);
        //bManager.ProjectHeights();

    }

    private void Statemachine()
    {
        switch (SizzleState)
        {
            case states.movement:

                ForceControl(moveForce);
                RotateToCameraDirection(torqueForce);
                TryDash(); // Checks whether the dash state should begin 

                /*if (Input.GetKey(crouchkey))
                {
                    // Change to crouch state 
                    //SizzleState = states.crouch;
                }
                else
                {
                    crouchLerp = Mathf.Clamp(crouchLerp + Time.deltaTime * unCrouchSpeed, minLerp, 1);
                }

                baseHeightLerp *= crouchLerp;
                */


                break;
            case states.crouch:

                //ForceControl(moveForceCrouch, torqueForceCrouch); // Slower Movement 
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
    private void ForceControl(float moveForce)
    {
        // Get the input 
        float VInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");


        //baseBody.AddTorque(torqueForce * baseBody.transform.up * hInput * Time.deltaTime, ForceMode.Acceleration);

        // Make sure that there are no obstructions in front of Sizzle 
        if(VInput > 0)
        {
            if(!CanMoveForwad())
            {
                return;
            }
        }

        //baseBody.AddForce(moveForce * frontBody.transform.forward * VInput * Time.deltaTime, ForceMode.Acceleration);
        baseBody.AddForce(moveForce * baseBody.transform.forward * VInput * Time.deltaTime, ForceMode.Acceleration);
    }

    /// <summary>
    /// Aims Sizzle's body towards where the camera is looking 
    /// </summary>
    private void RotateToCameraDirection(float torqueForce)
    {
        //Quaternion deltaRotation = Quaternion.Euler(new Vector3(0, 100, 0) * Time.deltaTime);
        //baseBody.MoveRotation(baseBody.rotation * deltaRotation);

        float hInput = Input.GetAxis("Horizontal");
        Vector3 dir = baseBody.transform.forward;

        Vector3 camDir = GetCamDirection();
        float dot = Vector3.Dot(-baseBody.transform.right, camDir);

        if (Mathf.Abs(hInput) >= Mathf.Epsilon)
        {
            if(hInput > 0)
            {
                dir = -baseBody.transform.right;
                dot = Vector3.Dot(-baseBody.transform.forward, camDir);
            }
            else
            {
                dir = baseBody.transform.right;
                dot = Vector3.Dot(baseBody.transform.forward, camDir);
            }
        }



        float angle = Vector3.Angle(dir, camDir);

        if(Mathf.Abs(angle) <= minAngle)
        {
            return;
        }

        if (dot > 0)
        {
            angle = -angle;
        }

        Vector3 eulerAngleVelocity = new Vector3(0, angle, 0);
        Vector3 angleDis = eulerAngleVelocity * autoTurnSpeed * Time.deltaTime;

        Quaternion deltaRotation = Quaternion.Euler(angleDis);
        baseBody.MoveRotation(baseBody.rotation * deltaRotation );
        //baseBody.AddRelativeTorque(angleDis * torqueForce);
    }

    /// <summary>
    /// Oritentates the body to the current surface, or lack thereof, that Sizzle is on 
    /// </summary>
    private void SurfaceLogic()
    {

        // Get average between two points 
        RaycastHit hitA;
        RaycastHit hitB;
        if (Physics.Raycast(baseBody.transform.position + baseBody.transform.forward * (disBetweenChecks / 2), Vector3.down, out hitA, 5) && Physics.Raycast(baseBody.transform.position - baseBody.transform.forward * (disBetweenChecks / 2), Vector3.down, out hitB, 5, checkMask))
        {
            // Average normal 
            //midBoneTorqueCorrection.Target = -(hitA.normal + hitB.normal).normalized;
            print(hitB.normal);

            // Height from ground should be the line formed by the two hit points 
            Vector3 midPoint = Vector3.Lerp(hitA.point, hitB.point, 0.5f);
            normal = Vector3.Lerp(hitA.normal, hitB.normal, 0.5f);

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

            normal = hit.normal;

            if (disSquared <= maxVecDisFromDown)
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
        if(dashCoolDownTimer <= 0)
        {
            // Activates the dash state 
            if (Input.GetKeyDown(dashKey) && DashCo == null && isGrounded)
            {
                dashCoolDownTimer = coolDownTimeDash;

                SizzleState = states.action;
                baseBody.AddForce(dashForceImpulse * baseBody.transform.forward, ForceMode.Impulse);
                legsController.Dash(baseBody); // Activates the dash leg animations 

                sm.PlaySoundFX(dashSound, this.transform.position, "DASH");
                DashCo = StartCoroutine(DashSubroutine());

                if(dashFOVCo != null)
                {
                    StopCoroutine(dashFOVCo);
                }

                // Dash can sometimes occur before fov returns to normalw so it
                // needs to start with that offset in mind 
                dashFOVCo = StartCoroutine(DashFOV(Mathf.InverseLerp(holdFOV, dashTargetFOV, cc.m_Lens.FieldOfView)));
            }
        }
        else
        {
            dashCoolDownTimer -= Time.deltaTime;
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

    private Vector3 GetCamDirection()
    {
        Vector3 camToMid = baseBody.position - cam.position;

        // Project onto the forward vector allows for speed to be relative
        // to how much the camera is faced towards the movement direciton
        // Now when Sizzle is orientating themself speed should speed up as it's correcting its orientation
        return Vector3.ProjectOnPlane(camToMid, normal).normalized;
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
            baseBody.AddForce(dashVerticalForce * dashVerticalOverLerp.Evaluate((timer / dashTime)) * Vector3.up * Time.deltaTime, ForceMode.Acceleration);
            frontRigid.AddForce(dashVerticalForce * dashVerticalOverLerp.Evaluate((timer / dashTime)) * Vector3.up * Time.deltaTime, ForceMode.Acceleration);

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
    /// Changes the FOV of the common cam when dashing 
    /// </summary>
    /// <returns></returns>
    private IEnumerator DashFOV(float startLerp)
    {

        dashFOVLerp = startLerp; 
        while(dashFOVLerp <= 1)
        {
            cc.m_Lens.FieldOfView = Mathf.LerpUnclamped(holdFOV, dashTargetFOV, camOutCurve.Evaluate(dashFOVLerp));

            dashFOVLerp += Time.deltaTime * camOutSpeed;
            yield return null;
        }

        dashFOVLerp = 0;
        while (dashFOVLerp <= 1)
        {
            cc.m_Lens.FieldOfView = Mathf.LerpUnclamped(dashTargetFOV, holdFOV, camBackCurve.Evaluate(dashFOVLerp));

            dashFOVLerp += Time.deltaTime * camBackSpeed;
            yield return null;
        }
    }

    /// <summary>
    /// Bounces Sizzle in the opposite direction that they are facing 
    /// </summary>
    public void BounceBack()
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
            frontRigid.AddForce(jumpForceContinuous * jumpForceOverLerp.Evaluate((timer / dashTime)) * dir * Time.deltaTime, ForceMode.Acceleration);

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

        if(cam != null)
        {
            // Cam direction 
            Vector3 camToBase = Vector3.ProjectOnPlane(baseBody.position - cam.position, normal).normalized * 2;
            Gizmos.DrawLine(baseBody.position, baseBody.position + camToBase);
            Gizmos.DrawWireSphere(baseBody.position + camToBase, 0.1f);
        }
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 Todo:
    - Rect stays in one place and doesn't rotate when not foot doesn't move
    - Max length a foot can be from the root before it needs to move
    - Choose which part of the compass for next rayCheckStart
 */

public class LegIKSolver : MonoBehaviour
{
    public LayerMask mask;

    [Header("Leg IK parts")]
    //[SerializeField] Transform IKStart;
    [SerializeField] Transform IKHint;
    [SerializeField] Transform end;

    [Header("Bone transform references")]
    [SerializeField] Transform forwardBone;
    [SerializeField] Transform root;
    [SerializeField] Transform foot;

    [Header("Step variables")]
    [Tooltip("The curve that will determine how high the foot will be from the floor over the step duration")]
    [SerializeField] AnimationCurve heightCurve;
    [SerializeField] float stepMaxHeight;
    [SerializeField] float MaxDisFromFloor;
    [SerializeField] float stepDistance;
    [SerializeField] float footOffset;
    [Tooltip("What are the dimensions of the cube that a new position can be in without causing the leg to be able to move")]
    [SerializeField] Vector2 rangeAreaBeforeMove;

    [Header("Offsets")]
    [Tooltip("The position where a raycast will check down to see where the foot should be put next")] 
    [SerializeField] Vector3 rayCheckStartOffset;
    [Tooltip("In what directionthe joint will bend towards")] 
    [SerializeField] Vector3 IKHintOffset;

    [Header("Leg Compass")]
    [SerializeField] Vector3[] compassDirections;
    [SerializeField] float maxLinearVel;
    [SerializeField] float maxMagnitudeOfCompass;
    [Tooltip("The rate that the linear velocity will impact the magnitude of the direction chosen from the compass")]
    [SerializeField] AnimationCurve linearVelEffectOnMag;
    [SerializeField] float maxAngularVelInfluence;
    [Tooltip("The rate that the angular velocity will impact the compass")]
    [SerializeField] AnimationCurve angularVelEffectOnTurnAmout;
    [SerializeField] bool isBackwards;

    [Header("GUI")]
    [SerializeField] bool showGizmos;
    public float rotValueTest;


    /// <summary>
    /// The forward vector of the body section this leg is attached to 
    /// </summary>
    private Vector3 axisVectorForward { get { return forwardBone.transform.forward; } }
    private Vector3 axisVectorRight { get { return forwardBone.transform.right; } }
    private Vector3 axisVectorUp { get { return forwardBone.transform.up; } }

    /// <summary>
    /// The position where the the leg begins 
    /// </summary>
    private Vector3 RootPos 
    { get 
        { 
            return root.position; 
        } 
    }

    /// <summary>
    /// Where the joint should tend to bend 
    /// </summary>
    private Vector3 offsetedIKHint 
    { get 
        { 
            return RootPos + 
                axisVectorForward* IKHintOffset.z +
                axisVectorRight * IKHintOffset.x +
                axisVectorUp * IKHintOffset.y;
        } 
    }

    /// <summary>
    /// The position where a raycast will check down to see where the foot should be put next 
    /// </summary>
    private Vector3 rayCheckStart 
    { 
        get 
        { 
            return RootPos + ChooseCompassDir();
        } 
    }

    private Rigidbody rb;

    // Info for moving the limb from one spot to next 
    private Vector3[] processedRangePlane;
    private Vector3 origin;
    private Vector3 target;
    private Vector3 holdFloorTarget;
    private float lerp;
    private bool moving;

    public bool Moving { get { return moving; } }
    public float Lerp { get { return lerp; } }

    /// <summary>
    /// Get: Where the end of the leg is currently
    /// Set: Set the position of the end of the leg 
    /// </summary>
    public Vector3 Target { get { return end.position; } set { end.position = value; } }
    /// <summary>
    /// Get the raw raycast point that the leg would attempt to be
    /// </summary>
    public Vector3 HoldFloorTarget { get { return holdFloorTarget; } }

    // Start is called before the first frame update
    void Start()
    {
        //target = offsetedStart;
        lerp = 0;
        processedRangePlane = GetLocalizedRangePlane();

        rb = forwardBone.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //IKStart.position = startFootPosition;
        IKHint.position = offsetedIKHint;
        end.position = target;
    }

    private void LateUpdate()
    {
        ChooseCompassDir();

    }

    public void TryMove(float footSpeedMoving, float footSpeedNotMoving)
    {
        if(moving)
        {
            return;
        }

        RaycastHit hit;
        // Checking for new position 
        if (Physics.Raycast(rayCheckStart, Vector3.down, out hit, MaxDisFromFloor, mask))
        {

            // TODO: Be able to find the when foot is within or outta rect range
            //bool testValue; 
            //print(Maths.IsPointWithinRect(hit.point, processedRangePlane, out testValue) + ": " + this.gameObject.name);
            //print(testValue + ": " + this.gameObject.name);

            //print(Maths.IsPointWithinRect(hit.point, processedRangePlane));

            // New position found 


            if (Vector3.Distance(hit.point, target) > stepDistance)
            //if(!Maths.IsPointWithinRect(hit.point, processedRangePlane, out testValue))
            {
                lerp = 0;
                target = hit.point + hit.normal * footOffset;
                holdFloorTarget = hit.point + hit.normal * footOffset;
                StartCoroutine(Move(footSpeedMoving, footSpeedNotMoving));
            }
            else
            {
                end.position = origin;
            }
        }
        else
        {
            // Nothing to step down on 
            
        }
    }

    private IEnumerator Move(float footSpeedMoving, float footSpeedNotMoving)
    {
        moving = true;

        while(lerp <= 1)
        {
            // Moves smoothly to new point 
            Vector3 footPos = Vector3.Lerp(origin, target, lerp);
            footPos.y += heightCurve.Evaluate(lerp) * stepMaxHeight; 

            end.position = footPos;

            if (rb.velocity.magnitude >= 0.5f) // Moving forward 
            {
                lerp += Time.deltaTime * footSpeedMoving;
            }
            else
            {
                lerp += Time.deltaTime * footSpeedNotMoving;
            }
            yield return null;
        }

        // Once point is reached 

        // Sets new plane 
        processedRangePlane = GetLocalizedRangePlane();
        // Makes sure position is where needed 
        end.transform.position = target;
        origin = target;
        moving = false;
    }

    /// <summary>
    /// Sets the raycast down position to the best compass vector 
    /// </summary>
    /// <returns></returns>
    private Vector3 ChooseCompassDir()
    {
        // Normalized compass based on the parent bones transform direction
        Vector3[] localizedCompass = GetLocalizedCompass();

        // Linear Velocity 
        Vector3 lVel = forwardBone.GetComponent<Rigidbody>().velocity;

        // Only need to consider the y rotation
        float aVel = forwardBone.GetComponent<Rigidbody>().angularVelocity.y;


        // Get the direction of the next step 
        // Todo : Angular Vel rotates the lvel

        Vector3 dir = localizedCompass[0];
        //float initial = Vector3.Dot(lVel.normalized, localizedCompass[0]);
        float hold = Vector3.Dot(lVel.normalized, localizedCompass[0]);

        // Compare the unit direction of the lVel and see which it matches with the closest 
        for (int i = 1; i < localizedCompass.Length; i++)
        {
            // The bigger the dot product the more parrallel they are 
            //float current = Vector3.Dot(lVel.normalized, localizedCompass[i]);
            float current = Vector3.Dot(lVel.normalized, localizedCompass[i]);

            if (current > hold) // Swapped sign
            {
                dir = localizedCompass[i];
                hold = current;
            }
        }

        // Get the magnitude of the next step 
        float mag = linearVelEffectOnMag.Evaluate(Mathf.Clamp01(lVel.magnitude / maxLinearVel)) * maxMagnitudeOfCompass;
        return dir * mag;

        /*if (isBackwards)
        {
            return -dir * mag;
        }
        else
        {
            return dir * mag;
        }*/
    }

    /// <summary>
    /// Applies the directions and coordinates of the compass as if the forward bone 
    /// were its parent matrix 
    /// </summary>
    /// <returns></returns>
    private Vector3[] GetLocalizedCompass()
    {
        if (compassDirections != null)
        {
            Vector3[] newCompass = new Vector3[compassDirections.Length];

            for (int i = 0; i < compassDirections.Length; i++)
            {
                // Get direction as if root is parent 
                Vector3 worldDir = forwardBone.TransformDirection(compassDirections[i]);

                newCompass[i] = worldDir;
            }
            return newCompass;
        }
        return null;
    }

    /// <summary>
    /// Applies the directions and coordinates of the plane as if the forward bone 
    /// were its parent matrix. Plane is set around foot position indicating its range
    /// before updating to move 
    /// </summary>
    /// <returns></returns>
    private Vector3[] GetLocalizedRangePlane()
    {
        // Represents where a downward newPos can be and not update the foot to move
        Vector3[] areaPlane = Maths.FormPlaneFromSize(rangeAreaBeforeMove);
        Vector3[] proccessedAreaPlane = new Vector3[areaPlane.Length];

        for (int i = 0; i < areaPlane.Length; i++)
        {
            proccessedAreaPlane[i] = forwardBone.TransformDirection(areaPlane[i]);
            proccessedAreaPlane[i] = foot.position + Vector3.ProjectOnPlane(proccessedAreaPlane[i], Vector3.up);
        }

        return proccessedAreaPlane;
    }

    private void OnDrawGizmos()
    {
        if(showGizmos)
        {
            //Vector3 difference = axisVector.normalized - Vector3.forward;
            Gizmos.color = Color.blue;
            //Gizmos.DrawSphere(rayCheckStart, 0.02f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(offsetedIKHint, 0.02f);

            RaycastHit hit;
            // Checking for new position 
            if (Physics.Raycast(rayCheckStart, Vector3.down, out hit, MaxDisFromFloor, mask))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(hit.point, 0.02f);

                Gizmos.color = Color.green;
                if(processedRangePlane != null)
                {
                    Gizmos.DrawWireSphere(new Vector3(hit.point.x, processedRangePlane[0].y, hit.point.z), 0.01f); // Change to get middle of plane instead of corner
                }
            }

            // Visualizes compass directions
            if(compassDirections != null)
            {
                Gizmos.color = Color.yellow;
                // Draw each vector as if 
                foreach (Vector3 dir in compassDirections)
                {
                    // Get direction as if root is parent 
                    Vector3 worldDir = forwardBone.TransformDirection(dir);

                    // Change to world coords 

                    // Draw point 
                    Gizmos.DrawWireSphere(RootPos + worldDir * dir.magnitude, 0.01f);
                }
            }

            // Updates rangeAreaBeforeMoving even when not in game 
            if(!Application.isPlaying)
            {
                target = hit.point + hit.normal * footOffset;
                processedRangePlane = GetLocalizedRangePlane();
            }

            for (int i = 0; i < processedRangePlane.Length; i++)
            {
                Gizmos.DrawSphere(processedRangePlane[i], 0.01f);

                // Draws line connceted to next in line 
                if(i < processedRangePlane.Length - 1)
                {
                    Gizmos.DrawLine(processedRangePlane[i], processedRangePlane[i + 1]);
                }
                else
                {
                    Gizmos.DrawLine(processedRangePlane[i], processedRangePlane[0]);
                }
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(RootPos + ChooseCompassDir(), 0.02f);
        }    
    }
}

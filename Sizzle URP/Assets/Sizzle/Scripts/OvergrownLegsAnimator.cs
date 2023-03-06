using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvergrownLegsAnimator : MonoBehaviour
{
    [Header("Transforms")]
    [SerializeField] LegSet front;
    [SerializeField] LegSet back;

    [Header("Animation")]
    [SerializeField] AnimationDetails frontAnimationDetails;
    [SerializeField] AnimationDetails backAnimationDetails;
    [SerializeField] ForceAnimationDetails frontForceAnimation;

    [Header("Settings")]
    [Tooltip("The speed that the animation plays in ratio to the distance travlled")]
    [SerializeField] float disToAngle;
    [Tooltip("Speed for foot to turn to idle")]
    [SerializeField] float footTransitionSpeedToIdle;
    [SerializeField] AnimationCurve footTransitionToIdleCurve; // Speed of transition is modified over curve 
    [Tooltip("Speed for foot to turn to animation")]
    [SerializeField] float footTransitionSpeedToAnim;
    [SerializeField] AnimationCurve footTransitionToAnimCurve;// Speed of transition is modified over curve 
    [Space]
    [SerializeField] bool printVel;
    [SerializeField] int velWhenToAnim;
    [SerializeField] int velCompletelyAnim;
    [Space]
    [SerializeField] float maxRaycastDis = 10.0f;
    [SerializeField] LayerMask raycastLayer;

    [Header("Debug Gizmos")]
    [SerializeField] DisplayMode display;
    private enum DisplayMode
    {
        Wheel, 
        Animation,
        None
    }

    [SerializeField] Color wheelColor;
    [Tooltip("How far the wheels will be from the sides")]
    [SerializeField] float wheelSideOffset;
    [SerializeField] float wheelRadius;
    [SerializeField] Vector3 frontOffsetCenter;
    [Space]
    [SerializeField] Color animationColor;
    [SerializeField] Color animationFloorColor;
    [SerializeField] float keySize = 0.01f;
    [SerializeField] float detailSize = 0.002f;

    // Start is called before the first frame update
    void Start()
    {
        front.RotRight = front.legRotOffset;
        back.RotLeft = back.legRotOffset;

        StartCoroutine(StartCoAfterTime(0.5f));
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.R))
        {
            frontForceAnimation.rb.AddRelativeTorque(frontForceAnimation.torqueForce * Vector3.forward * Time.deltaTime);
        }
    }

    private IEnumerator StartCoAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        StartCoroutines(front, frontAnimationDetails);
        StartCoroutines(back, backAnimationDetails);
    }


    private void StartCoroutines(LegSet set, AnimationDetails details)
    {
        StartCoroutine(UpdateLegVelAndTransitions(set));
        StartCoroutine(UpdateLegSetRot(set));
        StartCoroutine(LegPair(set, details));
        StartCoroutine(AnimationLogic(set, details));
    }

    private int ProcessLeg(LegSet pair, AnimationDetails details, float lerpPerFrame, ref int frameIndexHold, List<Vector3> cacheLinePoints, bool isRightLeg = false)
    {
        // Gets lerp that goes across whole animation 
        // Just takes the rotation and turns it into a 0 to 1 scale 
        float currentLerp = isRightLeg ? pair.RotRight / 360.0f : pair.RotLeft / 360.0f;

        // Derrives from: 
        // index * lerpPerFrame <= currentLerp;
        int index = Mathf.FloorToInt(currentLerp / lerpPerFrame);
        int nextIndex = index + 1;

        AnimationCurve curve = details.keyConnectionCurves[index];
        Vector3 previousPos = isRightLeg ? Maths.MirrorOnY(details.animationKeys[index]) : details.animationKeys[index];
        Vector3 nextPos;

        if(nextIndex >= details.animationKeys.Count)
        {
            nextIndex = 0;
        }
        nextPos = isRightLeg ? Maths.MirrorOnY(details.animationKeys[nextIndex]) : details.animationKeys[nextIndex];


        // Check if next index is a raycast 
        if (details.raycastIndexList.Contains(nextIndex))
        {
            RaycastHit hit;
            if (Physics.Raycast(pair.parentBone.TransformPoint(nextPos), Vector3.down, out hit, maxRaycastDis, raycastLayer))
            {
                nextPos = pair.parentBone.InverseTransformPoint(hit.point + Vector3.up * details.footOffset);
            }
        }

        // Check if the current index should be a raycast 
        if (details.raycastIndexList.Contains(index))
        {
            RaycastHit hit;
            if (Physics.Raycast(pair.parentBone.TransformPoint(previousPos), Vector3.down, out hit, maxRaycastDis, raycastLayer))
            {
                previousPos = pair.parentBone.InverseTransformPoint(hit.point + Vector3.up * details.footOffset);
            }
        }


        if (isRightLeg)
        {
            pair.FootPosRight = previousPos;
        }
        else
        {
            pair.FootPosLeft = previousPos;
        }

        float lerpPerDetail = 1.0f / details.levelOfDetail[index];
        // Check if cache needs to be changed 
        if (index != frameIndexHold)
        {
            frameIndexHold = index;
            cacheLinePoints.Clear();

            for (int i = 0; i < details.levelOfDetail[index]; i++)
            {
                // Adds points based on the line that is creates between
                // two key frames 
                float lerp = i * lerpPerDetail;
                Vector3 point = Vector3.Slerp(previousPos, nextPos, curve.Evaluate(lerp));
                cacheLinePoints.Add(point);
            }

            // Adds a final position so it does not reset to start of frame 
            cacheLinePoints.Add(nextPos);

        }

        // Used to locate position along the detail path 

        // Finds out what is the lerp that is currently between the current index point and its next destination 
        // Used to find the current index 
        float detailLerp = Mathf.InverseLerp(index * lerpPerFrame, (index + 1) * lerpPerFrame, currentLerp);


        // Derrives from: 
        // index * lerpPer <= currentLerp;
        int detailCurrentIndex = Mathf.FloorToInt(detailLerp / lerpPerDetail);
        int detailNextindex = detailCurrentIndex + 1; // Since nextPos is added to cache don't need to worry about index out of range 

        // The lerp value between two details 
        float minorDetailLerp = Mathf.InverseLerp(detailCurrentIndex * lerpPerDetail, detailNextindex * lerpPerDetail, detailLerp);

        // Set feet position
        if (isRightLeg)
        {
            pair.FootPosRight = Vector3.Lerp(cacheLinePoints[detailCurrentIndex], cacheLinePoints[detailNextindex], minorDetailLerp);
        }
        else
        {
            pair.FootPosLeft = Vector3.Lerp(cacheLinePoints[detailCurrentIndex], cacheLinePoints[detailNextindex], minorDetailLerp);
        }
        return frameIndexHold;
    }


    /// <summary>
    /// This coroutine is in charge of bringing the foot to 
    /// either the animation or resting positions 
    /// </summary>
    /// <param name="set"></param>
    /// <returns></returns>
    private IEnumerator AnimationLogic(LegSet set, AnimationDetails details)
    {
        while(true)
        {
            float difference = Mathf.Abs(set.TargetLerp - set.restingToAnimation);
            print(difference);
            if(set.TargetLerp > set.restingToAnimation)
            {
                // Logic to turn the foot smoothly towards the resting position 
                if (set.restingToAnimation < 1)
                {
                    set.restingToAnimation += footTransitionToAnimCurve.Evaluate(difference) * footTransitionSpeedToAnim * Time.deltaTime;
                }
                else
                {
                    set.restingToAnimation = 1.0f;
                }
            }
            else
            {
                if (set.restingToAnimation > 0)
                {
                    set.restingToAnimation -= footTransitionToIdleCurve.Evaluate(difference) * footTransitionSpeedToIdle * Time.deltaTime;
                }
                else
                {
                    set.restingToAnimation = 0.0f;
                }
            }


            if (set.restingToAnimation < 1)
            {
                // Since our logic is not simply refering
                // to the raycast done in the leg pair coroutine
                // we must make another raycast calculation to lerp to 

                // The leg pair logic also does not compute the 
                // raycast unless on specific indexes making it
                // not consisent for us 

                // This section could probably be simplified to a function called twice 

                RaycastHit hit;

                // Lerps position to the ground 
                Vector3 pointLeft = details.animationKeys[0];
                if (Physics.Raycast(set.parentBone.TransformPoint(pointLeft), Vector3.down, out hit, maxRaycastDis, raycastLayer))
                {
                    pointLeft = hit.point + Vector3.up * details.footOffset;
                }
                
                // Positioning is calculated locally rather than globally 
                set.ikTargetLeft.position = Vector3.Lerp(pointLeft, set.parentBone.TransformPoint(set.FootPosLeft), set.restingToAnimation);


                Vector3 pointRight = Maths.MirrorOnY(details.animationKeys[0]);
                if (Physics.Raycast(set.parentBone.TransformPoint(pointRight), Vector3.down, out hit, maxRaycastDis, raycastLayer))
                {
                    pointRight = hit.point + Vector3.up * details.footOffset;
                }

                // Positioning is calculated locally rather than globally 
                set.ikTargetRight.position = Vector3.Lerp(pointRight, set.parentBone.TransformPoint(set.FootPosRight), set.restingToAnimation);

            }
            else
            {
                set.ikTargetLeft.position = set.parentBone.TransformPoint(set.FootPosLeft);
                set.ikTargetRight.position = set.parentBone.TransformPoint(set.FootPosRight);
            }

            yield return null;
        }
    }

    /// <summary>
    /// The coroutine that controls a pair of legs 
    /// </summary>
    /// <returns></returns>
    private IEnumerator LegPair(LegSet pair, AnimationDetails details)
    {
        // Should not be changed during play 
        // This is how much of a  0 to 1 scale each frame gets 
        float lerpPerFrame = 1.0f / (float)details.animationKeys.Count;

        // Caching
        int frameIndexHoldRight = -1;
        int frameIndexHoldLeft = -1;
        List<Vector3> cacheLinePointsRight = new List<Vector3>();
        List<Vector3> cacheLinePointsLeft = new List<Vector3>();

        while (true)
        {
            // Left leg
            frameIndexHoldLeft = ProcessLeg(pair, details, lerpPerFrame, ref frameIndexHoldLeft, cacheLinePointsLeft, false);

            // Right leg
            frameIndexHoldRight = ProcessLeg(pair, details, lerpPerFrame, ref frameIndexHoldRight, cacheLinePointsRight, true);

            yield return null;
        }
    }


    /// <summary>
    /// Updates the rotation of a leg set based 
    /// on the distance its parent bone has moved 
    /// in the world space 
    /// </summary>
    /// <param name="pair"></param>
    /// <returns></returns>
    private IEnumerator UpdateLegSetRot(LegSet pair)
    {
        Vector3 holdPos = pair.parentBone.position;

        while (true)
        {

            // Adds distance from previous to new 
            Vector3 holdToNew = pair.parentBone.position - holdPos;
            float newDis = holdToNew.sqrMagnitude;
            holdPos = pair.parentBone.position;

            pair.RotRight += newDis * disToAngle;
            pair.RotLeft += newDis * disToAngle;

            // Loops value if over 360 
            if (pair.RotRight >= 360)
            {
                // Gets overflow even if multiple 360's over 
                float overFlow = pair.RotRight % 360;
               
                // Loop values 
                pair.RotRight = overFlow;
            }

            if (pair.RotLeft >= 360)
            {
                // Gets overflow even if multiple 360's over 
                float overFlow = pair.RotLeft % 360;

                // Loop values 
                pair.RotLeft = overFlow;
            }

            yield return null;
        }    
    }

    /// <summary>
    /// This coroutine is used to caluclate the velocity of 
    /// each setction 
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateLegVelAndTransitions(LegSet pair)
    {
        // The position that will be added to 
        Vector3 holdPos = pair.parentBone.position;

        // Just makes the value more human readable 
        float multiplier = 10000.0f;
        float time = 0.1f;

        // v = s/t
        while(true)
        {

            Vector3 holdToNew = pair.parentBone.position - holdPos;
            holdPos = pair.parentBone.position;

            int vel = (int)(holdToNew.sqrMagnitude * multiplier);

            // Just in case we want to see how the vel
            // value changes over time. Easier than just
            // typing out another print statement 
            if(printVel)
            {
                print(vel);
            }

            // The calculations to change a set's lerp value
            // is not done here because this is done at a consistent
            // frame time which would NOT look smooth for a user 
            if(vel > velWhenToAnim && vel < velCompletelyAnim)
            {
                pair.TargetLerp = Mathf.InverseLerp(velWhenToAnim, velCompletelyAnim, vel);
            }
            else
            {
                pair.TargetLerp = vel >= velCompletelyAnim ? 1 : 0;
            }

            yield return new WaitForSeconds(time);
        }
    }

    [System.Serializable]
    public class LegSet
    {
        [SerializeField] public Transform parentBone;
        [SerializeField] public Transform left;
        [SerializeField] public Transform right;
        [Space]
        [SerializeField] public Transform ikTargetLeft;
        [SerializeField] public Transform ikTargetRight;
        [SerializeField] [Range(0,1)] public float restingToAnimation;
        [SerializeField] public float legRotOffset;

        private float rotLeft;
        private float rotRight;

        // Avaliable for code use but not meant for editor
        public float RotLeft { get { return rotLeft; } set { rotLeft = value; } }
        public float RotRight { get { return rotRight; } set { rotRight = value; } }
        public Vector3 FootPosLeft { get; set; }
        public Vector3 FootPosRight { get; set; }
        public float TargetLerp { get; set; }
    }

    [System.Serializable]
    public class AnimationDetails
    {
        [SerializeField] public List<Vector3> animationKeys;
        [SerializeField] public List<AnimationCurve> keyConnectionCurves;
        [SerializeField] public List<int> levelOfDetail;
        [Tooltip("What keyframes use raycast positioning to decide their position")]
        [SerializeField] public List<int> raycastIndexList;
        [Space]
        [SerializeField] public float footOffset;
    }

    [System.Serializable]
    public class ForceAnimationDetails
    {
        [SerializeField] public Rigidbody rb;
        [SerializeField] public float torqueForce;
    }


    private void OnDrawGizmos()
    {
        DisplayGizmos(front, frontAnimationDetails, frontOffsetCenter);
        DisplayGizmos(back, backAnimationDetails, Vector3.zero);
    }

    private void DisplayGizmos(LegSet set, AnimationDetails details, Vector3 offset)
    {
        // Transform of gizmos is based on the parentBone 
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(set.parentBone.position, set.parentBone.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;


        switch (display)
        {
            case DisplayMode.Wheel:
                Gizmos.color = wheelColor;

                // Draws center of wheel
                Gizmos.DrawSphere(offset, 0.01f);

                // Visualizes the wheels 
                DrawWheel(offset + Vector3.right * wheelSideOffset, set.RotRight);
                DrawWheel(offset - Vector3.right * wheelSideOffset, set.RotLeft);
                break;
            case DisplayMode.Animation:
                // Draws out the animation curves and points 
                DrawAnimationPath(set, details);

                // Draws current Point along path 
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(set.FootPosRight, 0.01f);

                break;
            case DisplayMode.None:
                break;
        }
    }

    private void DrawWheel(Vector3 center, float wheelRot)
    {
        Gizmos.DrawSphere(center, 0.01f);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if(!(x == 0 && y == 0))
                {
                    Vector3 vector = Quaternion.Euler(Vector3.right * wheelRot) * new Vector3(0, x, y).normalized;

                    if(x + y == 0 || Mathf.Abs(x + y) == 2)
                    {
                        vector /= 2;
                    }

                    Gizmos.DrawLine(center, center + vector * wheelRadius);
                    Gizmos.DrawSphere(center + vector * wheelRadius, 0.01f);
                }
            }
        }
    }

    private void DrawAnimationPath(LegSet set, AnimationDetails details)
    {
        Gizmos.color = animationColor;

        // Draw for each side 
        for (int i = 0; i < details.animationKeys.Count; i++)
        {
            Gizmos.DrawSphere(details.animationKeys[i], keySize);
            Gizmos.DrawSphere(new Vector3(-details.animationKeys[i].x, details.animationKeys[i].y, details.animationKeys[i].z), keySize);

            if(i + 1 < details.animationKeys.Count)
            {
                // Line from current frame to next 
                DrawAnimationCurve(set, details, i, i + 1);
                DrawAnimationCurve(set, details, i, i + 1, true);
            }
            else
            {
                // Loops
                DrawAnimationCurve(set, details, i, 0);
                DrawAnimationCurve(set, details, i, 0, true);
            }
        }
        
    }

    /// <summary>
    /// Draws the animation curve between two of the animation keys 
    /// </summary>
    private void DrawAnimationCurve(LegSet set, AnimationDetails details, int currentIndex, int nextIndex, bool inverseX = false)
    {
        float lerp = 0;
        float changeInLerp = 1.0f / details.levelOfDetail[currentIndex];

        Vector3 currentPoint;
        Vector3 nextPoint;


        if(!inverseX)
        {
            currentPoint = details.animationKeys[currentIndex];
            nextPoint = details.animationKeys[nextIndex];
        }
        else
        {
            currentPoint = new Vector3(-details.animationKeys[currentIndex].x, details.animationKeys[currentIndex].y, details.animationKeys[currentIndex].z);
            nextPoint = new Vector3(-details.animationKeys[nextIndex].x, details.animationKeys[nextIndex].y, details.animationKeys[nextIndex].z);
        }

        // Check if next index is a raycast 
        if (details.raycastIndexList.Contains(nextIndex))
        {
            RaycastHit hit;
            Vector3 point = Vector3.zero;
            if (Physics.Raycast(set.parentBone.TransformPoint(nextPoint), Vector3.down, out hit, maxRaycastDis, raycastLayer))
            {
                Gizmos.color = animationFloorColor;
                point = hit.point + Vector3.up * details.footOffset;
                Gizmos.DrawSphere(set.parentBone.InverseTransformPoint(point), keySize);
            }

            nextPoint = set.parentBone.InverseTransformPoint(point);
        }

        // Check if the current index should be a raycast 
        if (details.raycastIndexList.Contains(currentIndex))
        {
            RaycastHit hit;
            Vector3 point = Vector3.zero;
            if (Physics.Raycast(set.parentBone.TransformPoint(currentPoint), Vector3.down, out hit, maxRaycastDis, raycastLayer))
            {
                Gizmos.color = animationFloorColor;
                point = hit.point + Vector3.up * details.footOffset;
                Gizmos.DrawSphere(set.parentBone.InverseTransformPoint(point), keySize);
            }

            currentPoint = set.parentBone.InverseTransformPoint(point);
        }


        /*if (currentIndex == 0)
        {
            // Bring foot to ground rather than just animation point 

            RaycastHit hit;
            Vector3 point = Vector3.zero;
            if (Physics.Raycast(set.parentBone.TransformPoint(currentPoint), Vector3.down, out hit, maxRaycastDis, raycastLayer))
            {
                Gizmos.color = animationFloorColor;
                point = hit.point + Vector3.up * details.footOffset;
                Gizmos.DrawSphere(set.parentBone.InverseTransformPoint(point), keySize);
            }

            currentPoint = set.parentBone.InverseTransformPoint(point);
        }
        else if(currentIndex == details.animationKeys.Count - 1)
        {
            // Draws from detail path to the ground

            RaycastHit hit;
            Vector3 point = Vector3.zero;
            if (Physics.Raycast(set.parentBone.TransformPoint(nextPoint), Vector3.down, out hit, maxRaycastDis, raycastLayer))
            {
                Gizmos.color = animationFloorColor;
                point = hit.point + Vector3.up * details.footOffset;
                Gizmos.DrawSphere(set.parentBone.InverseTransformPoint(point), keySize);
            }

            nextPoint = set.parentBone.InverseTransformPoint(point);
        }
        else
        {
            
        }*/
        Gizmos.color = animationColor;
        for (int j = 0; j < details.levelOfDetail[currentIndex]; j++)
        {
            // Curved points from one keyframe to the next 
            Vector3 beginline = Vector3.Slerp(currentPoint, nextPoint, details.keyConnectionCurves[currentIndex].Evaluate(lerp));
            Vector3 endLine = Vector3.Slerp(currentPoint, nextPoint, details.keyConnectionCurves[currentIndex].Evaluate(lerp + changeInLerp));

            Gizmos.DrawLine(beginline, endLine);
            Gizmos.DrawSphere(endLine, detailSize);

            lerp += changeInLerp;
        }
    }
}

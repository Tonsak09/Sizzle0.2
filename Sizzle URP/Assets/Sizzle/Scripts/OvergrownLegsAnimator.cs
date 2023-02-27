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

    [Header("Settings")]
    [Tooltip("The speed that the animation plays in ratio to the distance travlled")]
    [SerializeField] float disToAngle;
    [SerializeField] float footTransitionSpeedToIdle;
    [SerializeField] float footTransitionSpeedToAnim;
    [Space]
    [SerializeField] bool printVel;
    [SerializeField] int minVelToMove;
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
        StartCoroutine(UpdateLegVelAndTransitions(front));
        StartCoroutine(UpdateLegSetRot(front));

        StartCoroutine(LegPair(front, frontAnimationDetails));
        StartCoroutine(AnimationLogic(front, frontAnimationDetails));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// This coroutine is in charge of bringing the foot to 
    /// either the animation or resting positions 
    /// </summary>
    /// <param name="pair"></param>
    /// <returns></returns>
    private IEnumerator AnimationLogic(LegSet pair, AnimationDetails details)
    {
        while(true)
        {

            if(pair.TurningToIdle)
            {
                // Logic to turn the foot smoothly towards the resting position 
                if (pair.restingToAnimation < 1)
                {
                    pair.restingToAnimation += footTransitionSpeedToAnim * Time.deltaTime;
                }
                else
                {
                    pair.restingToAnimation = 1.0f;
                }
            }
            else
            {
                if (pair.restingToAnimation > 0)
                {
                    pair.restingToAnimation -= footTransitionSpeedToIdle * Time.deltaTime;
                }
                else
                {
                    pair.restingToAnimation = 0.0f;
                }
            }


            if (pair.restingToAnimation < 1)
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
                if (Physics.Raycast(front.parentBone.TransformPoint(pointLeft), Vector3.down, out hit, maxRaycastDis, raycastLayer))
                {
                    pointLeft = hit.point + Vector3.up * details.footOffset;
                }
                
                // Positioning is calculated locally rather than globally 
                pair.ikTargetLeft.position = Vector3.Lerp(pointLeft, pair.parentBone.TransformPoint(pair.FootPosLeft), pair.restingToAnimation);


                Vector3 pointRight = Maths.MirrorOnY(details.animationKeys[0]);
                if (Physics.Raycast(front.parentBone.TransformPoint(pointRight), Vector3.down, out hit, maxRaycastDis, raycastLayer))
                {
                    pointRight = hit.point + Vector3.up * details.footOffset;
                }

                // Positioning is calculated locally rather than globally 
                pair.ikTargetRight.position = Vector3.Lerp(pointRight, pair.parentBone.TransformPoint(pair.FootPosRight), pair.restingToAnimation);

            }
            else
            {
                pair.ikTargetLeft.position = pair.parentBone.TransformPoint(pair.FootPosLeft);
                pair.ikTargetRight.position = pair.parentBone.TransformPoint(pair.FootPosRight);
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

    private int ProcessLeg(LegSet pair, AnimationDetails details, float lerpPerFrame, ref int frameIndexHold, List<Vector3> cacheLinePoints, bool oppositeLeg = false)
    {
        // Gets lerp that goes across whole animation 
        // Just takes the rotation and turns it into a 0 to 1 scale 
        float currentLerp = pair.Rot / 360.0f;

        // Derrives from: 
        // index * lerpPerFrame <= currentLerp;
        int index = Mathf.FloorToInt(currentLerp / lerpPerFrame);

        AnimationCurve curve = details.keyConnectionCurves[index];
        Vector3 previousPos = oppositeLeg ? Maths.MirrorOnY(details.animationKeys[index]) : details.animationKeys[index];
        Vector3 nextPos;


        // Gets next position. If necessary then loop 
        if (index + 1 < details.animationKeys.Count)
        {
            nextPos = oppositeLeg ? Maths.MirrorOnY(details.animationKeys[index + 1]) : details.animationKeys[index + 1];
        }
        else
        {
            nextPos = oppositeLeg ? Maths.MirrorOnY(details.animationKeys[0]) : details.animationKeys[0];
        }

        if (index == 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(front.parentBone.TransformPoint(previousPos), Vector3.down, out hit, maxRaycastDis, raycastLayer))
            {
                previousPos = pair.parentBone.InverseTransformPoint(hit.point + Vector3.up * details.footOffset);
            }
        }
        else if (index == details.animationKeys.Count - 1)
        {
            RaycastHit hit;
            if (Physics.Raycast(front.parentBone.TransformPoint(nextPos), Vector3.down, out hit, maxRaycastDis, raycastLayer))
            {
                nextPos = pair.parentBone.InverseTransformPoint(hit.point + Vector3.up * details.footOffset);
            }
        }
        // Else the position is just normal



        if(oppositeLeg)
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
        if(oppositeLeg)
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

            pair.Rot += newDis * disToAngle;

            // Loops value if over 360 
            if (pair.Rot >= 360)
            {
                // Loop values 
                pair.Rot = 0;
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
            if(vel > minVelToMove)
            {
                pair.TurningToIdle = true;
            }
            else
            {
                pair.TurningToIdle = false;
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


        // Avaliable for code use but not meant for editor
        public float Rot { get; set; }
        public Vector3 FootPosLeft { get; set; }
        public Vector3 FootPosRight { get; set; }
        public bool TurningToIdle { get; set; }
    }

    [System.Serializable]
    public class AnimationDetails
    {
        [SerializeField] public List<Vector3> animationKeys;
        [SerializeField] public List<AnimationCurve> keyConnectionCurves;
        [SerializeField] public List<int> levelOfDetail;
        [Space]
        [SerializeField] public float footOffset;
    }


    private void OnDrawGizmos()
    {
        // Transform of gizmos is based on the parentBone 
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(front.parentBone.position, front.parentBone.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        

        switch (display)
        {
            case DisplayMode.Wheel:
                Gizmos.color = wheelColor;

                Vector3 pairCenter = frontOffsetCenter;
                Gizmos.DrawSphere(pairCenter, 0.01f);

                // Visualizes the wheels 
                DrawWheel(pairCenter + Vector3.right * wheelSideOffset, front.Rot);
                DrawWheel(pairCenter - Vector3.right * wheelSideOffset, front.Rot);
                break;
            case DisplayMode.Animation:
                // Draws out the animation curves and points 
                DrawAnimationPath(frontAnimationDetails);

                // Draws current Point along path 
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(front.FootPosRight, 0.01f);

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

    private void DrawAnimationPath(AnimationDetails details)
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
                DrawAnimationCurve(details, i, i + 1);
                DrawAnimationCurve(details, i, i + 1, true);
            }
            else
            {
                // Loops
                DrawAnimationCurve(details, i, 0);
                DrawAnimationCurve(details, i, 0, true);
            }
        }
        
    }

    /// <summary>
    /// Draws the animation curve between two of the animation keys 
    /// </summary>
    /// <param name="details"></param>
    /// <param name="currentIndex"></param>
    /// <param name="nextIndex"></param>
    private void DrawAnimationCurve(AnimationDetails details, int currentIndex, int nextIndex, bool inverseX = false)
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

        
        if (currentIndex == 0)
        {
            // Bring foot to ground rather than just animation point 

            RaycastHit hit;
            Vector3 point = Vector3.zero;
            if (Physics.Raycast(front.parentBone.TransformPoint(currentPoint), Vector3.down, out hit, maxRaycastDis, raycastLayer))
            {
                Gizmos.color = animationFloorColor;
                point = hit.point + Vector3.up * details.footOffset;
                Gizmos.DrawSphere(front.parentBone.InverseTransformPoint(point), keySize);
            }

            currentPoint = front.parentBone.InverseTransformPoint(point);
        }
        else if(currentIndex == details.animationKeys.Count - 1)
        {
            // Draws from detail path to the ground

            RaycastHit hit;
            Vector3 point = Vector3.zero;
            if (Physics.Raycast(front.parentBone.TransformPoint(nextPoint), Vector3.down, out hit, maxRaycastDis, raycastLayer))
            {
                Gizmos.color = animationFloorColor;
                point = hit.point + Vector3.up * details.footOffset;
                Gizmos.DrawSphere(front.parentBone.InverseTransformPoint(point), keySize);
            }

            nextPoint = front.parentBone.InverseTransformPoint(point);
        }
        else
        {
            Gizmos.color = animationColor;
        }

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

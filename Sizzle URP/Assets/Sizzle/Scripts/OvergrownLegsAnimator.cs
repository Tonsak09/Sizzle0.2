    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvergrownLegsAnimator : MonoBehaviour
{
    [Header("Transforms")]
    [SerializeField] LegSet front;
    [SerializeField] LegSet back;

    [Header("Animation")]
    [SerializeField] WalkAnimationDetails frontAnimationDetails;
    [SerializeField] WalkAnimationDetails backAnimationDetails;
    [Space]
    [SerializeField] WobbleSettings wobbleAnimationDetails;
    [Space]
    [SerializeField] IdleAnimDetails idleAnimDetails;

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
    [SerializeField] int animMinSpeed;
    [SerializeField] int dashMinSpeed;
    [Space]
    [SerializeField] float maxRaycastDis = 10.0f;
    [SerializeField] LayerMask raycastLayer;

    [Header("Debug Gizmos")]
    [SerializeField] DisplayMode display;
    private enum DisplayMode
    {
        Wheel, 
        Animation,
        BoneRot,
        Idle,
        None
    }

    [Header("Wheel")]
    [SerializeField] Color wheelColor;
    [Tooltip("How far the wheels will be from the sides")]
    [SerializeField] float wheelSideOffset;
    [SerializeField] float wheelRadius;
    [SerializeField] Vector3 frontOffsetCenter;
    [Space]
    [Header("Animation Walk")]
    [SerializeField] Color animationColor;
    [SerializeField] Color animationFloorColor;
    [SerializeField] float keySize = 0.01f;
    [SerializeField] float detailSize = 0.002f;
    [Space]
    [Header("Balance")]
    [SerializeField] Color balanceCurrentColor;
    [SerializeField] Color balanceGoalColor;
    [SerializeField] float balanceCurrentSize;
    [SerializeField] float balanceGoalSize;
    [Space]
    [Header("Idle")]
    [SerializeField] Color idleRaycastColor;
    [SerializeField] Color idleHitColor;
    [SerializeField] float idleRaycastSize;
    [SerializeField] float idleHitSize;

    private int velMag;
    private enum SizzleState
    {
        Idle,
        Walk,
        Dash
    }
    private SizzleState currentState = SizzleState.Idle;
    private SizzleState targetState = SizzleState.Idle;

    private bool testBool = false;


    // Start is called before the first frame update
    void Start()
    {
        front.RotRight = front.legRotOffset;
        back.RotLeft = back.legRotOffset;

        currentState = SizzleState.Walk;
        targetState = SizzleState.Walk;

        StartCoroutine(StartCoAfterTime(0.5f));
    }

    private void Update()
    {
        if(testBool)
        {
            AnimationTranslator(front);
            AnimationTranslator(back);
        }
        
    }

    /// <summary>
    /// Starts all necessary coroutines 
    /// </summary>
    /// <param name="set"></param>
    /// <param name="details"></param>
    /// <param name="wobbleSettings"></param>
    private void StartCoroutines(LegSet set, WalkAnimationDetails details, WobbleAnimationDetailsFirst wobbleSettings)
    {
        StartCoroutine(UpdateVelCo(set));
        StartCoroutine(UpdateLegSetRot(set));


        StartCoroutine(WalkLogic(set, details));
        //StartCoroutine(WobbleLogic(set, wobbleSettings, 1.0f / (float)details.animationKeys.Count));
        //StartCoroutine(AnimationLogic(set, details));
    }

    /// <summary>
    /// Updates the velocity of this script
    /// </summary>
    /// <param name="pair"></param>
    /// <param name="holdPos"></param>
    /// <param name="multiplier"></param>
    private void UpdateVel(LegSet pair, ref Vector3 holdPos, float multiplier)
    {
        Vector3 holdToNew = pair.parentBone.position - holdPos;
        holdPos = pair.parentBone.position;

        velMag = (int)(holdToNew.sqrMagnitude * multiplier);

        // Just in case we want to see how the vel
        // value changes over time. Easier than just
        // typing out another print statement 
        if (printVel)
        {
            print(velMag);
        }

        // The calculations to change a set's lerp value
        // is not done here because this is done at a consistent
        // frame time which would NOT look smooth for a user 
        /*if(vel > velWhenToAnim && vel < velCompletelyAnim)
        {
            pair.TargetLerp = Mathf.InverseLerp(velWhenToAnim, velCompletelyAnim, vel);
        }
        else
        {
            pair.TargetLerp = vel >= velCompletelyAnim ? 1 : 0;
        }*/
    }


    /// <summary>
    /// Begins a coroutine after a set amount of time
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private IEnumerator StartCoAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        StartCoroutines(front, frontAnimationDetails, wobbleAnimationDetails.frontWobbleDetails);
        StartCoroutines(back, backAnimationDetails, wobbleAnimationDetails.backWobbleDetails);
        testBool = true;
    }

    /// <summary>
    /// This coroutine is in charge of bringing the foot to 
    /// either the animation or resting positions 
    /// </summary>
    /// <returns></returns>
    private IEnumerator AnimationLogic(LegSet set, WalkAnimationDetails details)
    {
        

        while (true)
        {
            #region Old
            /*// This value is how far the target lerp is from its current lerp
            float difference = Mathf.Abs(set.TargetLerp - set.restingToAnimation);
            if(set.TargetLerp > set.restingToAnimation)
            {
                // Logic to turn the foot smoothly towards the animation logic 
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
                // Logic to turn the foot smoothly towards the resting logic 
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
            }*/
            #endregion

            /*// Find out where on the speed range 
            // Sizzle currently is 

            if (velMag < animMinSpeed)
            {
                // Idle
                target = SizzleState.Idle;
            }
            else if (velMag < dashMinSpeed)
            {
                // Walk 
                target = SizzleState.Walk;

                // Make faster as vel goes up 
            }
            else
            {
                // Dashing 
                target = SizzleState.Dash;
            }*/



            yield return null;
        }
    }

    /// <summary>
    /// This coroutine is used to caluclate the velocity of 
    /// each setction 
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateVelCo(LegSet set)
    {
        // The position that will be added to 
        Vector3 holdPos = set.parentBone.position;

        // Just makes the value more human readable 
        float multiplier = 10000.0f;
        float time = 0.1f;

        // v = s/t
        while (true)
        {
            UpdateVel(set, ref holdPos, multiplier);

            yield return new WaitForSeconds(time);
        }
    }

    

    /// <summary>
    /// Updates the rotation of a leg set based 
    /// on the distance its parent bone has moved 
    /// in the world space. 
    /// 
    /// This is a overgrown class wide use
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

            float nextRight = pair.RotRight + newDis * disToAngle;
            float nextLeft = pair.RotLeft + newDis * disToAngle;

            // Loops value if over 360 
            if (nextRight >= 360)
            {
                // Gets overflow even if multiple 360's over 
                float overFlow = nextRight % 360;

                // Loop values 
                nextRight = overFlow;
            }

            if (nextLeft >= 360)
            {
                // Gets overflow even if multiple 360's over 
                float overFlow = nextLeft % 360;

                // Loop values 
                nextLeft = overFlow;
            }


            // The values are only applied here at the end 
            pair.RotRight = nextRight;
            pair.RotLeft = nextLeft;

            yield return null;
        }
    }

    [System.Serializable]
    public class LegSet
    {
        [Header("Leg Set Transforms")]
        [SerializeField] public Transform parentBone;
        [SerializeField] public Transform left;
        [SerializeField] public Transform right;
        [Space]
        [SerializeField] public Transform ikTargetLeft;
        [SerializeField] public Transform ikTargetRight;
        //[SerializeField][Range(0, 1)] public float restingToAnimation;
        [SerializeField] public float legRotOffset;

        [Header("State Transitions")]
        [SerializeField] public float transitionTime;

        private float rotLeft;
        private float rotRight;

        // Avaliable for code use but not meant for editor
        // public Coroutine LegIdleCo { get; set; }

        public Coroutine LegTransitionCo { get; set; }
        public Coroutine PrimaryLogic { get; set; }

        public float RotLeft { get { return rotLeft; } set { rotLeft = value; } }
        public float RotRight { get { return rotRight; } set { rotRight = value; } }

        /// <summary>
        /// The walk cycle ideal foot position for he left foot 
        /// </summary>
        public Vector3 WalkFootIdealLeft { get; set; }
        /// <summary>
        /// The walk cycle ideal foot position for the right foot 
        /// </summary>
        public Vector3 WalkFootIdealRight { get; set; }

  
        public Vector3 IdleFootIdealLeft { get; set; }
        public Vector3 IdleFootIdealRight { get; set; }

    }


    #region Transitions

    /// <summary>
    /// This is how the script animates from 
    /// one mode to the next 
    /// </summary>
    private void AnimationTranslator(LegSet set)
    {
        set.ikTargetRight.position = GetStateWalkPos(set, true);
        set.ikTargetLeft.position = GetStateWalkPos(set, false);

        return;
        // Find out which state Sizzle should be going to 
        if (velMag < animMinSpeed)
        {
            // Idle
            targetState = SizzleState.Idle;
        }
        else if (velMag < dashMinSpeed)
        {
            // Walk 
            targetState = SizzleState.Walk;
        }
        else
        {
            // Dashing 
            targetState = SizzleState.Dash;
        }

        // Do not continue unless it needs to change 
        if (currentState != targetState)
        {
            // Begin transition to new target logic 
            TryTransition(set, currentState, targetState);
            return;
        }

        // Check if there is a transition going on 
        if (set.LegTransitionCo != null)
        {
            return;
        }

        // If there is not transition going on then
        // set foot position to target 



        set.ikTargetRight.position = GetStatePosition(set, currentState, true);
        set.ikTargetLeft.position = GetStatePosition(set, currentState, false);
    }

    /// <summary>
    /// Tries to transition from one type of animation 
    /// to the next. There are two logic coroutines active
    /// during the the logic of this call
    /// </summary>
    /// <param name="set"></param>
    /// <param name="current"></param>
    /// <param name="target"></param>
    private void TryTransition(LegSet set, SizzleState current, SizzleState target)
    {
        if (set.LegTransitionCo == null)
        {
            set.LegTransitionCo = StartCoroutine(TransitionBetweenStates(set, current, target, set.transitionTime));
        }
    }

    /// <summary>
    /// Get the position that a state wants Sizzle's foot 
    /// to be at. This will not check if a state is active
    /// or not 
    /// </summary>
    /// <param name="set"></param>
    /// <param name="state"></param>
    /// <param name="isRight"></param>
    /// <returns></returns>
    private Vector3 GetStatePosition(LegSet set, SizzleState state, bool isRight = false)
    {
        switch (state)
        {
            case SizzleState.Idle:
                return GetStateIdlePos(set, isRight);
            case SizzleState.Walk:
                return GetStateWalkPos(set, isRight);
            case SizzleState.Dash:
                return Vector3.zero;
            default:
                return Vector3.zero;
        }
    }

    /// <summary>
    /// Begins the coroutine logic based on what state is passed through
    /// </summary>
    /// <param name="set"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    private Coroutine StartStateLogic(LegSet set, SizzleState state)
    {
        switch (state)
        {
            case SizzleState.Idle:
                return StartCoroutine(IdleAdjustLogic(set, idleAnimDetails));
            case SizzleState.Walk:
                return StartCoroutine(IdleAdjustLogic(set, idleAnimDetails));
            case SizzleState.Dash:
                return null;
        }

        return null;
    }

    /// <summary>
    /// This logic holds the two currently active logic and
    /// tries to smoothly transition from logic to the next
    /// before ending the previous one
    /// </summary>
    /// <param name="set"></param>
    /// <param name="current"></param>
    /// <param name="target"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private IEnumerator TransitionBetweenStates(LegSet set, SizzleState current, SizzleState target, float time)
    {
        float t = 0;
        Coroutine targetLogic = StartStateLogic(set, current);

        while (t <= time)
        {
            Vector3 currentLogicPosRight = GetStatePosition(set, current, true);
            Vector3 currentLogicPosLeft = GetStatePosition(set, current, false);

            Vector3 targetLogicPosRight = GetStatePosition(set, target, true);
            Vector3 targetLogicPosLeft = GetStatePosition(set, target, false);

            set.ikTargetRight.position = Vector3.Lerp(currentLogicPosRight, targetLogicPosRight, t / time);
            set.ikTargetLeft.position = Vector3.Lerp(currentLogicPosLeft, targetLogicPosLeft, t / time);

            t += Time.deltaTime;
            yield return null;
        }

        set.PrimaryLogic = targetLogic;
    }

    #endregion

    #region Walking

    /// <summary>
    /// The coroutine that controls a pair of legs 
    /// </summary>
    /// <returns></returns>
    private IEnumerator WalkLogic(LegSet pair, WalkAnimationDetails details)
    {
        // Should not be changed during play 
        // This is how much of a  0 to 1 scale each frame gets 
        float lerpPerFrame = 1.0f / (float)details.animationKeys.Count;

        // Caching
        int frameIndexHoldLeft = -1;
        float detailLerpLeft = 0;
        float lerpPerDetailLeft = 0;

        int frameIndexHoldRight = -1;
        float detailLerpRight = 0;
        float lerpPerDetailRight = 0;

        List<Vector3> cacheLinePointsLeft = new List<Vector3>();
        List<Vector3> cacheLinePointsRight = new List<Vector3>();

        while (true)
        {
            /*// Left leg
            frameIndexHoldLeft = GetDetailIndex(pair, details, lerpPerFrame, frameIndexHoldLeft, ref lerpPerDetailLeft, ref detailLerpLeft, cacheLinePointsLeft, false);
            pair.WalkFootIdealLeft = GetWalkTargetPos(cacheLinePointsLeft, frameIndexHoldLeft, frameIndexHoldLeft + 1, lerpPerDetailLeft, detailLerpLeft);

            // Right leg
            frameIndexHoldRight = GetDetailIndex(pair, details, lerpPerFrame, frameIndexHoldRight, ref lerpPerDetailRight, ref detailLerpRight, cacheLinePointsRight, true);
            pair.WalkFootIdealRight = GetWalkTargetPos(cacheLinePointsRight, frameIndexHoldRight, frameIndexHoldRight + 1, lerpPerDetailRight, detailLerpRight);
*/

            // Left leg
            frameIndexHoldLeft = ProcessLeg(pair, details, lerpPerFrame, ref frameIndexHoldLeft, cacheLinePointsLeft, false);

            // Right leg
            frameIndexHoldRight = ProcessLeg(pair, details, lerpPerFrame, ref frameIndexHoldRight, cacheLinePointsRight, true);

            yield return null;
        }
    }

    private int ProcessLeg(LegSet pair, WalkAnimationDetails details, float lerpPerFrame, ref int frameIndexHold, List<Vector3> cacheLinePoints, bool isRightLeg = false)
    {
        int index = GetLegAnimIndex(pair, lerpPerFrame, isRightLeg);
        int nextIndex = index + 1;

        AnimationCurve curve = details.keyConnectionCurves[index];
        Vector3 previousPos = isRightLeg ? Maths.MirrorOnY(details.animationKeys[index]) : details.animationKeys[index];
        Vector3 nextPos;

        if (nextIndex >= details.animationKeys.Count)
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
            pair.WalkFootIdealRight = previousPos;
        }
        else
        {
            pair.WalkFootIdealLeft = previousPos;
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
        float currentLerp = isRightLeg ? pair.RotRight / 360.0f : pair.RotLeft / 360.0f;
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
            pair.WalkFootIdealRight = Vector3.Lerp(cacheLinePoints[detailCurrentIndex], cacheLinePoints[detailNextindex], minorDetailLerp);
        }
        else
        {
            pair.WalkFootIdealLeft = Vector3.Lerp(cacheLinePoints[detailCurrentIndex], cacheLinePoints[detailNextindex], minorDetailLerp);
        }
        return frameIndexHold;
    }

    /// <summary>
    /// Gets the index of the detail that makes 
    /// the points between each frame 
    /// </summary>
    /// <returns></returns>
    private int GetDetailIndex(LegSet pair, WalkAnimationDetails details, float lerpPerFrame, int frameIndexHold, ref float lerpPerDetail, ref float detailLerp, List<Vector3> cacheLinePoints, bool isRightLeg = false)
    {
        int index = GetLegAnimIndex(pair, lerpPerFrame, isRightLeg);
        int nextIndex = index + 1;

        AnimationCurve curve = details.keyConnectionCurves[index];
        Vector3 previousPos = isRightLeg ? Maths.MirrorOnY(details.animationKeys[index]) : details.animationKeys[index];
        Vector3 nextPos;

        if (nextIndex >= details.animationKeys.Count)
        {
            // Loop value 
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

        lerpPerDetail = 1.0f / details.levelOfDetail[index];
        //print("Current Index: " + index);
        //print("Frame Index Hold: " + frameIndexHold);

        // Check if cache needs to be changed 
        if (index != frameIndexHold)
        {
            cacheLinePoints.Clear();
            
            for (int i = 0; i < details.levelOfDetail[index]; i++)
            {
                // Adds points based on the line that is creates between
                // two key frames 
                float lerp = i * lerpPerDetail;
                Vector3 point = Vector3.Slerp(previousPos, nextPos, curve.Evaluate(lerp));
                cacheLinePoints.Add(point);
            }

            // Add the start of the next list to avoid having reset
            // to the beginning of this cache and makes it more
            // convient for code below to not worry about overflow 
            cacheLinePoints.Add(nextPos);

        }

        // Used to locate position along the detail path 

        // Finds out what is the lerp that is currently between the current index point and its next destination 
        // Used to find the current index 
        float currentLerp = isRightLeg ? pair.RotRight / 360.0f : pair.RotLeft / 360.0f;

        // Get the lerp of the current value between the previous and next frames
        // This is then scaled to a more desirable scope 
        detailLerp = Mathf.InverseLerp(index * lerpPerFrame, (index + 1) * lerpPerFrame, currentLerp);

        // Derrives from: 
        // index * lerpPer <= currentLerp;
        // Essentially how many detail frames have gone by 
        int detailCurrentIndex = Mathf.FloorToInt(detailLerp / lerpPerDetail);

        return detailCurrentIndex;
    }

    /// <summary>
    /// Get the current position that the walk cycle
    /// wants Sizzle's foot to be
    /// </summary>
    /// <returns></returns>
    private Vector3 GetWalkTargetPos(List<Vector3> cacheLinePoints, int detailCurrentIndex, int detailNextindex, float lerpPerDetail, float detailLerp)
    {

        if(detailNextindex < 0 || detailNextindex >= cacheLinePoints.Count)
        {
            print("Next not in bounds");
            print("Current: " + detailCurrentIndex);
            print("Next: " + detailNextindex);
            print("Cache Count: " + cacheLinePoints.Count);
        }

        if (detailCurrentIndex < 0 || detailCurrentIndex >= cacheLinePoints.Count)
        {
            print("Current not in bounds");
            print("Current: " + detailCurrentIndex);
            print("Next: " + detailNextindex);
            print("Cache Count: " + cacheLinePoints.Count);
        }
        float minorDetailLerp = Mathf.InverseLerp((float)detailCurrentIndex * lerpPerDetail, (float)detailNextindex * lerpPerDetail, detailLerp);


        // Set feet position
        return Vector3.Lerp(cacheLinePoints[detailCurrentIndex], cacheLinePoints[detailNextindex], minorDetailLerp);
    }

    /// <summary>
    /// Use to get a legs current animation key index 
    /// </summary>
    /// <param name="pair"></param>
    /// <param name="isRightLeg"></param>
    /// <returns></returns>
    private int GetLegAnimIndex(LegSet pair, float lerpPerFrame, bool isRightLeg)
    {
        // Gets lerp that goes across whole animation 
        // Just takes the rotation and turns it into a 0 to 1 scale 
        float currentLerp = isRightLeg ? pair.RotRight / 360.0f : pair.RotLeft / 360.0f;

        // Derrives from: 
        // index * lerpPerFrame <= currentLerp;
        return Mathf.FloorToInt(currentLerp / lerpPerFrame);
    }

    /// <summary>
    /// Get the current point that this logic wants to put
    /// the foot 
    /// </summary>
    /// <param name="set"></param>
    /// <param name="isRight"></param>
    /// <returns></returns>
    private Vector3 GetStateWalkPos(LegSet set, bool isRight = false)
    {
        return set.parentBone.TransformPoint(isRight ? set.WalkFootIdealRight : set.WalkFootIdealLeft);
    }

    [System.Serializable]
    public class WalkAnimationDetails
    {
        [SerializeField] public List<Vector3> animationKeys;
        [SerializeField] public List<AnimationCurve> keyConnectionCurves;
        [SerializeField] public List<int> levelOfDetail;
        [Tooltip("What keyframes use raycast positioning to decide their position")]
        [SerializeField] public List<int> raycastIndexList;
        [Space]
        [SerializeField] public float footOffset;
    }

    #endregion

    #region Wobble

    /// <summary>
    /// Begins a wobble rotate animation onto the Sizzle
    /// body. Primarily used during walking animation 
    /// </summary>
    /// <param name="details"></param>
    public bool TryWobbleRotate(WobbleAnimationDetailsFirst details, bool alternate)
    {
        if (details.rotationCo == null)
        {
            details.rotationCo = StartCoroutine(TryWobbleRotateCo(details, alternate));
            return true;
        }

        // System was occupied 
        return false;
    }

    /// <summary>
    /// Changes the rotation of Sizzle based on the speed of movement
    /// and the current frame 
    /// </summary>
    /// <returns></returns>
    private IEnumerator WobbleLogic(LegSet pair, WobbleAnimationDetailsFirst wobbleDetails)
    {
        while (true)
        {
            yield return null;
        }
    }

    private IEnumerator WobbleLogic(LegSet pair, WobbleAnimationDetailsFirst wobbleDetails, float lerpPerFrame)
    {
        bool canWobble = true; // As to not repeat if sitting on a frame 
        int hold = -1;

        bool alternate = false;

        while (true)
        {
            if (canWobble)
            {
                // Check when to wobble  
                if (wobbleDetails.indexToWobble.Contains(GetLegAnimIndex(pair, lerpPerFrame, true)))
                {
                    bool attempt = TryWobbleRotate(wobbleDetails, alternate);

                    if (attempt)
                    {
                        alternate = !alternate;
                    }
                }

                canWobble = false;
                hold = GetLegAnimIndex(pair, lerpPerFrame, !alternate);
            }
            else
            {
                // THe moment the frame changes Sizzle can wobble again 
                if (hold != GetLegAnimIndex(pair, lerpPerFrame, !alternate))
                {
                    canWobble = true;
                }
            }


            yield return null;
        }

    }

    /// <summary>
    /// This coroutine moves the bone's target z rotation
    /// over a period of time 
    /// </summary>
    /// <returns></returns>
    private IEnumerator TryWobbleRotateCo(WobbleAnimationDetailsFirst details, bool alternate)
    {
        float lerp = 0;
        float rot = 0;
        float targetRot = alternate ? details.angle : -details.angle;
        Vector3 hold = details.joint.targetRotation.eulerAngles;

        while (lerp <= 1)
        {
            //details.joint.targetRotation = Vector3.Lerp
            rot = Mathf.LerpAngle(0, targetRot, details.upCurve.Evaluate(lerp));

            // The front wobbles on the z axis
            // while the back wobbles the x axis 
            if (details.wobbleZ)
            {
                details.joint.targetRotation = Quaternion.Euler(new Vector3(hold.x, hold.y, rot));
            }
            else
            {
                details.joint.targetRotation = Quaternion.Euler(new Vector3(hold.x, rot, hold.z));
            }

            lerp += Time.deltaTime * details.upSpeed;
            yield return null;
        }

        // Bring back 
        lerp = 0;
        while (lerp <= 1)
        {
            rot = Mathf.LerpAngle(targetRot, 0, details.returnCurve.Evaluate(lerp));

            if (details.wobbleZ)
            {
                details.joint.targetRotation = Quaternion.Euler(new Vector3(hold.x, hold.y, rot));
            }
            else
            {
                details.joint.targetRotation = Quaternion.Euler(new Vector3(hold.x, rot, hold.z));
            }

            lerp += Time.deltaTime * details.returnSpeed;
            yield return null;
        }

        // CLeanup
        details.joint.targetRotation = Quaternion.Euler(new Vector3(hold.x, hold.y, 0));
        details.rotationCo = null;
    }

    [System.Serializable]
    public class WobbleSettings
    {
        [SerializeField] public WobbleAnimationDetailsFirst frontWobbleDetails;
        [SerializeField] public WobbleAnimationDetailsFirst backWobbleDetails;
    }

    [System.Serializable]
    public class WobbleAnimationDetailsFirst
    {
        // Settings for adjusting the wobble of Sizzle during
        // walking 

        [SerializeField] public bool wobbleZ; // Whether to rotae z or x axis 
        [SerializeField] public ConfigurableJoint joint;
        [SerializeField] public List<int> indexToWobble;
        [SerializeField] public float angle;

        [SerializeField] public float upSpeed;
        [SerializeField] public AnimationCurve upCurve;
        [SerializeField] public float returnSpeed;
        [SerializeField] public AnimationCurve returnCurve;

        public Coroutine rotationCo { get; set; }
        public Transform Bone { get { return joint.transform; } }

    }

    [System.Serializable]
    public class WobbleAnimDetails
    {

    }

    #endregion

    #region IdleAdjust

    /*/// <summary>
    /// Attempts to begin the coroutine if 
    /// not already running that adjusts 
    /// feet when moving slowly 
    /// </summary>
    private void TryRunIdleLogic(LegSet set)
    {
        if(set.LegIdleCo == null)
        {
            set.LegIdleCo = StartCoroutine(IdleAdjustLogic(set, idleAnimDetails));
        }
    }*/

    public Vector3 GetStateIdlePos(LegSet set, bool isRight = false)
    {
        return isRight ? set.IdleFootIdealRight : set.IdleFootIdealLeft;
    }

    private IEnumerator IdleAdjustLogic(LegSet set, IdleAnimDetails details)
    {
        bool isMoving = false;

        // TODO: Take into account vertical adjustments 

        while(true)
        {
            // If in animation return
            if(!isMoving)
            {
                // Get the foots current position
                Vector3 nextPos = GetIdleNextPos(details);

                // If the distance to where it is now and the next pos 
                // is greater than a given threshold begin the movement 
                // animation 

                if(Vector3.SqrMagnitude(nextPos - set.right.position) >= details.disToMove)
                {
                    // TODO: Create an animation that updates from one point to another

                    float lerp = 0;
                    Vector3 startRight = set.ikTargetRight.position;

                    while(lerp <= 1)
                    {
                        // Transition between the points 
                        set.IdleFootIdealRight = Vector3.Lerp(startRight, nextPos, lerp);
                        //set.IdleFootIdealLeft = Vector3.Lerp(startLeft, nextPos, lerp);

                        lerp += Time.deltaTime;
                        yield return null;
                    }

                    set.IdleFootIdealRight = nextPos;
                    isMoving = true;
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// Gets the next idle position along the ground. Not 
    /// where it will be in animation. The end and start frames.
    /// </summary>
    /// <param name="details"></param>
    /// <returns></returns>
    private Vector3 GetIdleNextPos(IdleAnimDetails details)
    {
        RaycastHit hit;
        // Not changed by the gizmos matrix 
        Ray ray = new Ray(this.transform.position + this.transform.TransformDirection(details.offsetToRaycast), Vector3.down);


        if (Physics.Raycast(ray, out hit, idleAnimDetails.raycastRange, idleAnimDetails.layer))
        {
            return hit.point;
        }

        // Else return a holding spot for the hand 
        return Vector3.zero;
    }

    [System.Serializable]
    public class IdleAnimDetails
    {
        [SerializeField] public float disToMove;
        [SerializeField] public float speed;
        [SerializeField] public AnimationCurve directCurve;
        [SerializeField] public float height;
        [SerializeField] public AnimationCurve heightCurve;
        [Space]
        [SerializeField] public Vector3 offsetToRaycast;
        [SerializeField] public float raycastRange;
        [SerializeField] public LayerMask layer;
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmos()
    {
        DisplayGizmos(front, frontAnimationDetails, frontOffsetCenter);
        DisplayGizmos(back, backAnimationDetails, Vector3.zero);
    }

    private void DisplayGizmos(LegSet set, WalkAnimationDetails details, Vector3 offset)
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
                Gizmos.DrawSphere(set.WalkFootIdealRight, 0.01f);

                break;

            case DisplayMode.BoneRot:

                DrawBalance(wobbleAnimationDetails.frontWobbleDetails);

                break;

            case DisplayMode.Idle:

                DrawIdle(idleAnimDetails);

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

    private void DrawAnimationPath(LegSet set, WalkAnimationDetails details)
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
    private void DrawAnimationCurve(LegSet set, WalkAnimationDetails details, int currentIndex, int nextIndex, bool inverseX = false)
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

    /// <summary>
    /// Visualize the wobble and balance details 
    /// </summary>
    /// <param name="details"></param>
    private void DrawBalance(WobbleAnimationDetailsFirst details)
    {
        Vector3 dir = new Vector3(Mathf.Cos(details.angle), Mathf.Sin(details.angle), 0);

        Gizmos.color = balanceCurrentColor;
        Gizmos.DrawLine(Vector3.zero, dir * balanceCurrentSize / 2);
        Gizmos.DrawLine(Vector3.zero, -dir * balanceCurrentSize / 2);

        Gizmos.color = balanceGoalColor;
        Gizmos.DrawLine(Vector3.zero, dir * balanceGoalSize / 2);
        Gizmos.DrawLine(Vector3.zero, -dir * balanceGoalSize / 2);
    }

    private void DrawIdle(IdleAnimDetails details)
    {
        // Where is the raycast coming from 
        Gizmos.color = idleRaycastColor;
        Gizmos.DrawWireSphere(details.offsetToRaycast, idleRaycastSize);

        // Hit
        Gizmos.color = idleHitColor;

        RaycastHit hit;
        // Not changed by the gizmos matrix 
        Ray ray = new Ray(this.transform.position + this.transform.TransformDirection(details.offsetToRaycast), Vector3.down);


        if (Physics.Raycast(ray, out hit, idleAnimDetails.raycastRange, idleAnimDetails.layer))
        {
            Vector3 localPoint = this.transform.InverseTransformPoint(hit.point);
            Gizmos.DrawWireSphere(localPoint, idleHitSize);
            Gizmos.DrawLine(localPoint, details.offsetToRaycast);
        }
    }

    #endregion
}

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
    [Tooltip("Is essentially the speed that the animation plays in ratio to the distance travlled")]
    [SerializeField] float disToAngle;
    [SerializeField] int frameCount;

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
    [SerializeField] float keySize = 0.01f;
    [SerializeField] float detailSize = 0.002f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LegPair(front, frontAnimationDetails));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// The coroutine that controls a pair of legs 
    /// </summary>
    /// <returns></returns>
    private IEnumerator LegPair(LegSet pair, AnimationDetails details)
    {
        // The position that will be added to 
        Vector3 holdPos = pair.parentBone.position;
        float distanceTravelled = 0;
        
        // Should not be changed during play 
        // This is how much of a  0 to 1 scale each frame gets 
        float lerpPerFrame = 1.0f / (float)details.animationKeys.Count;

        // Caching
        int frameIndexHold = -1;
        List<Vector3> cacheLinePoints = new List<Vector3>();

        while (true)
        {
            
            // Adds distance from previous to new 
            float newDis = Vector3.Distance(pair.parentBone.position, holdPos);
            holdPos = pair.parentBone.position;

            distanceTravelled += newDis;
            pair.Rot += newDis * disToAngle;

            // Loops value if over 360 
            if(pair.Rot >= 360)
            {
                // Loop values 
                pair.Rot = 0;
                distanceTravelled = 0;
            }

            // Gets lerp that goes across whole animation 
            // Just takes the rotation and turns it into a 0 to 1 scale 
            float currentLerp = pair.Rot / 360.0f; 

            // Derrives from: 
            // index * lerpPerFrame <= currentLerp;
            int index = Mathf.FloorToInt(currentLerp / lerpPerFrame);
            print(lerpPerFrame);
            AnimationCurve curve = details.keyConnectionCurves[index];
            Vector3 previousPos = details.animationKeys[index];
            Vector3 nextPos;
            
           // Gets next position. If necessary loop 
           if (index + 1 < details.animationKeys.Count)
           {
               nextPos = details.animationKeys[index + 1];
           }
           else
           {
               nextPos = details.animationKeys[0];
           }

            pair.FootPosRight = previousPos;

             /*
             float lerpPerDetail = 1.0f / details.levelOfDetail[index];
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
             }

             // Used to locate position along the detail path 

             // Finds out what is the lerp that is currently between the current index point and its next destination 
             float detailLerp = Mathf.InverseLerp(index * lerpPerFrame, (index + 1) * lerpPerFrame, currentLerp);
             // Derrives from: 
             // index * lerpPer <= currentLerp;
             int detailIndex = Mathf.FloorToInt(detailLerp / lerpPerDetail);

             // Set feet position 
             pair.FootPosRight = Vector3.Lerp(cacheLinePoints[detailIndex], cacheLinePoints[detailIndex + 1], detailLerp);
             */


             yield return null;
        }
    }

    [System.Serializable]
    public class LegSet
    {
        [SerializeField] public Transform parentBone;
        [SerializeField] public Transform left;
        [SerializeField] public Transform right;

        // Avaliable for code use but not meant for editor
        public float Rot { get; set; }
        public Vector3 FootPosLeft { get; set; }
        public Vector3 FootPosRight { get; set; }
    }

    [System.Serializable]
    public class AnimationDetails
    {
        [SerializeField] public List<Vector3> animationKeys;
        [SerializeField] public List<AnimationCurve> keyConnectionCurves;
        [SerializeField] public List<int> levelOfDetail;
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

            if(i + 1 < details.animationKeys.Count)
            {
                // Line from current frame to next 
                DrawAnimationCurve(details, i, i + 1);
            }
            else
            {
                // Loops
                DrawAnimationCurve(details, i, 0);
            }
        }
        
    }

    /// <summary>
    /// Draws the animation curve between two of the animation keys 
    /// </summary>
    /// <param name="details"></param>
    /// <param name="currentIndex"></param>
    /// <param name="nextIndex"></param>
    private void DrawAnimationCurve(AnimationDetails details, int currentIndex, int nextIndex)
    {
        float lerp = 0;
        float changeInLerp = 1.0f / details.levelOfDetail[currentIndex];

        Vector3 currentPoint = details.animationKeys[currentIndex];
        Vector3 nextPoint = details.animationKeys[nextIndex];

        for (int j = 0; j < details.levelOfDetail[currentIndex]; j++)
        {
            // Curved points from one keyframe to the next 
            Vector3 beginline = Vector3.Slerp(nextPoint, currentPoint, details.keyConnectionCurves[currentIndex].Evaluate(lerp));
            Vector3 endLine = Vector3.Slerp(nextPoint, currentPoint, details.keyConnectionCurves[currentIndex].Evaluate(lerp + changeInLerp));

            Gizmos.DrawLine(beginline, endLine);
            Gizmos.DrawSphere(endLine, detailSize);

            lerp += changeInLerp;
        }
    }
}

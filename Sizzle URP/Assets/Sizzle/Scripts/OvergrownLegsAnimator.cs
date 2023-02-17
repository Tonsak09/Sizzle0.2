using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvergrownLegsAnimator : MonoBehaviour
{
    [Header("Transforms")]
    /*[SerializeField] Transform front;
    [SerializeField] Transform[] frontPair;
    [SerializeField] Vector3 frontOffsetCenter;
    [Space]
    [SerializeField] Transform back;
    [SerializeField] Transform[] backPair;
    [SerializeField] Vector3 backOffsetCenter;*/
    [SerializeField] LegSet front;
    [SerializeField] LegSet back;

    [Header("Animation")]
    [SerializeField] AnimationDetails frontAnimationDetails;

    [Header("Settings")]
    [Tooltip("Is essentially the speed that the animation plays in ratio to the distance travlled")]
    [SerializeField] float disToAngle;
    [SerializeField] int frameCount;

    [Header("Debug Gizmos")]
    [SerializeField] bool showGizmos;
    [SerializeField] Color wheelColor;
    [Tooltip("How far the wheels will be from the sides")]
    [SerializeField] float wheelSideOffset;
    [SerializeField] float wheelRadius;
    [SerializeField] Vector3 frontOffsetCenter;
    [Space]
    [SerializeField] Color animationColor;
    [SerializeField][Range(1, 30)] int animationcurveDetail;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LegPair(front));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// The coroutine that controls a pair of legs 
    /// </summary>
    /// <returns></returns>
    private IEnumerator LegPair(LegSet pair)
    {
        // The position that will be added to 
        Vector3 holdPos = pair.parentBone.position;
        float distanceTravelled = 0;
        

        while (true)
        {
            // Adds distance from previous to new 
            // TODO: Get vector between old and new. Use mag for distance and dot product with forward vector to see whether its moving with or against 
            float newDis = Vector3.Distance(pair.parentBone.position, holdPos);
            holdPos = pair.parentBone.position;

            distanceTravelled += newDis;
            pair.Rot += newDis * disToAngle;

            if(pair.Rot >= 360)
            {
                // Loop values 
                pair.Rot = 0;
                distanceTravelled = 0;
            }

            GetFrameFromWheel();
            


            yield return null;
        }
    }

    private void GetFrameFromWheel()
    {
        //print( (int)(frontRot / (360 / frameCount)));
    }

    [System.Serializable]
    public class LegSet
    {
        [SerializeField] public Transform parentBone;
        [SerializeField] public Transform left;
        [SerializeField] public Transform right;

        // Avaliable for code use but not meant for editor
        public float Rot { get; set; }
    }

    [System.Serializable]
    public class AnimationDetails
    {
        [SerializeField] public List<Vector3> animationKeys;
        [SerializeField] public List<AnimationCurve> keyConnectionCurves;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = wheelColor;
        

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(front.parentBone.position, front.parentBone.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;

        Vector3 pairCenter = frontOffsetCenter;
        Gizmos.DrawSphere(pairCenter, 0.01f);

        // Visualizes the wheels 
        DrawWheel(pairCenter + Vector3.right * wheelSideOffset, front.parentBone, front.Rot);
        DrawWheel(pairCenter - Vector3.right * wheelSideOffset, front.parentBone, front.Rot);

        // Draws out the animation curves and points 
        DrawAnimationPath(frontAnimationDetails);
    }

    private void DrawWheel(Vector3 center, Transform directionParent, float wheelRot)
    {
        if(!showGizmos)
        {
            return;
        }

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
            Gizmos.DrawSphere(details.animationKeys[i], 0.01f);

            if(i + 1 < details.animationKeys.Count)
            {
                float lerp = 0;
                float changeInLerp = 1.0f / animationcurveDetail;

                Vector3 current = details.animationKeys[i];
                Vector3 next = details.animationKeys[i + 1];

                for (int j = 0; j < animationcurveDetail; j++)
                {
                    Vector3 beginline = Vector3.Slerp(next, current, details.keyConnectionCurves[i].Evaluate(lerp));
                    Vector3 endLine = Vector3.Slerp(next, current, details.keyConnectionCurves[i].Evaluate(lerp + changeInLerp));
                    Gizmos.DrawLine(beginline, endLine);

                    Gizmos.DrawSphere(endLine, 0.002f);

                    lerp += changeInLerp;
                    print(endLine);
                }
            }
        }
    }
}

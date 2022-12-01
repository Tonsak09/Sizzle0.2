using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvergrownLegsAnimator : MonoBehaviour
{
    [Header("Transforms")]
    [SerializeField] Transform front;
    [SerializeField] Transform[] frontPair;
    [SerializeField] Vector3 frontOffsetCenter;
    [Space]
    [SerializeField] Transform back;
    [SerializeField] Transform[] backPair;
    [SerializeField] Vector3 backOffsetCenter;

    [Header("Settings")]
    [SerializeField] float disToAngle;
    [SerializeField] int frameCount;

    [Header("Debug Gizmos")]
    [SerializeField] Color wheelColor;
    [Tooltip("How far the wheels will be from the sides")]
    [SerializeField] float wheelSideOffset;
    [SerializeField] float wheelRadius;

    private float frontRot;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LegPair(front, true));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// The coroutine that controls a pair of legs 
    /// </summary>
    /// <returns></returns>
    private IEnumerator LegPair(Transform parent, bool isFront)
    {
        // The position that will be added to 
        Vector3 holdPos = parent.position;
        float distanceTravelled = 0;

        while (true)
        {
            // Adds distance from previous to new 
            // TODO: Get vector between old and new. Use mag for distance and dot product with forward vector to see whether its moving with or against 
            float newDis = Vector3.Distance(parent.position, holdPos);
            holdPos = parent.position;

            distanceTravelled += newDis;


            if(isFront)
            {
                frontRot += newDis * disToAngle;

                if(frontRot >= 360)
                {
                    // Loop values 
                    frontRot = 0;
                    distanceTravelled = 0;
                }

                GetFramFromWheel();
            }


            yield return null;
        }
    }

    private void GetFramFromWheel()
    {
        print( (int)(frontRot / (360 / frameCount)));
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = wheelColor;
        

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(front.position, front.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;

        Vector3 pairCenter = frontOffsetCenter;
        Gizmos.DrawSphere(pairCenter, 0.01f);

        DrawWheel(pairCenter + Vector3.right * wheelSideOffset, front, frontRot);
        DrawWheel(pairCenter - Vector3.right * wheelSideOffset, front, frontRot);
    }

    private void DrawWheel(Vector3 center, Transform directionParent, float wheelRot)
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
}

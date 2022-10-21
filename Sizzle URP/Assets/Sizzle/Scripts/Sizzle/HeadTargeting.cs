using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadTargeting : MonoBehaviour
{
    
    [SerializeField] Grab grab;
    [SerializeField] Transform compass;
    [SerializeField] Transform target;
    [SerializeField] Vector3 targetOffset;
    
    [SerializeField] float timeToLookAtPointsOfInterest;
    [SerializeField] float moveToInterestSpeed;
    [SerializeField] float recoveryFromInterestTime;

    private Coroutine lookAtInterestCoroutine;
    //private PlayerMovement pm;

    private Transform pointOfInterest;
    /// <summary>
    /// The Offset when Sizzle is not looking at a point of interest 
    /// </summary>
    private Vector3 headOffset;

    private bool targetOffseted;
    public float interestTimer;

    public Vector3 TargetPos { get { return target.position; } }
    public Transform PointOfInterest { get { return pointOfInterest; } set { pointOfInterest = value; } }
    public Vector3 HeadOffset { get { return headOffset; } set { headOffset = value; } }

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        interestTimer = timeToLookAtPointsOfInterest;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        // Only begins countdown if still 
        if(pm.moving || grab.candleHeld)
        {
            interestTimer = timeToLookAtPointsOfInterest;
        }
        else
        {
            interestTimer -= Time.deltaTime;
        }

        if (pointOfInterest == null)
        {
            // Normal position
            target.position = this.transform.position + (targetOffset + HeadOffset) + compass.transform.forward * 3;
        }
        else
        {
            // Once still 
            if (interestTimer <= 0)
            {
                // Set to point of interest if timer is below 0 and one is avaliable 
                target.position = pointOfInterest.position;
            }
            else
            {
                // Still look in default direction 
                target.position = this.transform.position + (targetOffset + HeadOffset) + compass.transform.forward * 3;
            }
        }
        */
    }

}

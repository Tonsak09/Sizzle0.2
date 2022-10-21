using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spearine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform rotationBone;
    [SerializeField] Vector3 boneForwardOffset;

    [Header("Ranges")]
    [SerializeField] float closeRange;
    [SerializeField] float midRange;
    [SerializeField] float farRange;

    [Header("Speeds")]
    [SerializeField] float curiousTurnSpeed;
    [SerializeField] float alertTurnSpeed;

    [Space]
    [SerializeField] float maxAngle;
    [SerializeField] AnimationCurve turnCurve;

    [Header("Alertness")]
    [SerializeField] float closeAlertRaise;
    [SerializeField] float midAlertRaise;
    [SerializeField] float farAlertRaise;

    [Header("Sounds")]
    [SerializeField] AudioClip alert;
    [SerializeField] AudioClip[] idles;

    private Transform player;
    // Target could be a player, but also any other charged entity 
    private Transform primaryTarget;


    // Used to calculate how aware spearine is to all
    // targets in its vicinity 
    private List<Transform> sparks;

    // If reaches 100 then Spearine is alert to target 
    [Range(0, 100)]
    private float alertness;

    private enum DistanceZone
    {
        close,
        mid,
        far,
        NotWithinRange
    }

    // Start is called before the first frame update
    void Start()
    {
        // Gets player rather than Sizzle because Sizzle is a folder that doesn't represent the tru position 
        player = GameObject.FindWithTag("Player").transform;

        // TESTING ONLY 
        //primaryTarget = player;
    }

    // Update is called once per frame
    void Update()
    {
        if(primaryTarget != null)
        {
            Vector3 targetVec = Vector3.ProjectOnPlane(primaryTarget.position - this.transform.position, Vector3.up).normalized;
            //targetVec += boneForwardOffset;

            float angleDifference = Vector3.Angle(this.transform.forward, targetVec);
            float lerp = angleDifference / maxAngle;

            float angleToTurn = turnCurve.Evaluate(lerp) * alertTurnSpeed;

            Vector3 newDir = Vector3.RotateTowards(rotationBone.transform.forward, targetVec, angleToTurn * Time.deltaTime, 0.0f);
            rotationBone.transform.rotation = Quaternion.LookRotation(newDir);
            //torque.Target = -targetVec;
        }
        else
        {
            UpdateAlertness();

            // Get target with largest alert 
            // Curious rotate towards that 
        }


        // Moving towards target but alerted towards player 
        if (primaryTarget != player && alertness >= 90)
        {
            primaryTarget = player;
        }

    }

    /// <summary>
    /// Gets the zone that the position is in 
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private DistanceZone GetZone(Vector3 pos)
    {
        float dis = (this.transform.position - pos).sqrMagnitude;

        // Checks for each range 
        if (dis <= Mathf.Pow(closeRange, 2))
        {
            return DistanceZone.close;
        }
        else if (dis <= Mathf.Pow(midRange, 2))
        {
            return DistanceZone.mid;
        }
        else if (dis <= Mathf.Pow(farRange, 2))
        {
            return DistanceZone.far;
        }
        else
        {
            return DistanceZone.NotWithinRange;
        }
    }

    /// <summary>
    /// Adds to each targets level of being alerted 
    /// </summary>
    private void UpdateAlertness()
    {
        DistanceZone zone = GetZone(player.position);

        switch(zone)
        {
            case DistanceZone.close:
                alertness += closeAlertRaise;
                break;
            case DistanceZone.mid:
                alertness += midAlertRaise;
                break;
            case DistanceZone.far:
                alertness += farAlertRaise;
                break;
            default:
                // Out of range 
                alertness = 0;
                break;
        }
        
    }

    /// <summary>
    /// Spearine attacks the player via animation 
    /// </summary>
    private void Attack()
    {
        // Lunges head towards target according to range 
    }

    /// <summary>
    /// Brings the player back and then resets the alertness
    /// </summary>
    private void ResetPosition()
    {

    }

    /// <summary>
    /// Whether or not the Spearine can see the target 
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool CanDetect(Vector3 pos)
    {
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, closeRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, midRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, farRange);
    }
}

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

    [Header("Attacking")]
    [SerializeField] Animator animator;
    [SerializeField] AnimationClip attackClip;

    [SerializeField] float minFXTime;
    [SerializeField] float maxFXTime;
    [SerializeField] ParticleSystem groundClashFX;
    [SerializeField] float groundHitRadius;

    [Header("Sounds")]
    [SerializeField] AudioClip alert;
    [SerializeField] AudioClip[] idles;


    private Transform player;
    // Target could be a player, but also any other charged entity 
    private Transform primaryTarget;
    private bool attacking;


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
    } // Test

    // Update is called once per frame
    void Update()
    {
        if(primaryTarget != null)
        {
            // Target has been decided 
            AimTowardsTarget();
            AttackWhenInRange();
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

    private void AimTowardsTarget()
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
                alertness += closeAlertRaise * Time.deltaTime;
                break;
            case DistanceZone.mid:
                alertness += midAlertRaise * Time.deltaTime;
                break;
            case DistanceZone.far:
                alertness += farAlertRaise * Time.deltaTime;
                break;
            default:
                // Out of range 
                alertness = 0;
                break;
        }
    }

    private void AttackWhenInRange()
    {
        if((this.transform.position - player.position).sqrMagnitude < Mathf.Pow(midRange, 2) && !attacking)
        {
            attacking = true;
            StartCoroutine(Attack());
        }
    }

    /// <summary>
    /// Spearine attacks the player via animation 
    /// </summary>
    private IEnumerator Attack()
    {
        // Lunges head towards target according to range 
        animator.SetBool("attacking", true);



        float timer = 0; 
        while(timer < attackClip.length)
        {
            // Attack Logic 
            RaycastHit hit;
            // Particles
            if ((timer >= minFXTime) && (timer <= maxFXTime))
            {
                //groundClashFX.Play();
                //ParticleSystem temp = Instantiate(groundClashFX, groundClashFX.transform.position, groundClashFX.transform.rotation);
                //temp.Play();
            }
            //print(timer);

            timer += Time.deltaTime;
            yield return null;
        }

        attacking = false;
        animator.SetBool("attacking", false);
        //groundClashFX.Stop();
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

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(groundClashFX.transform.position, groundHitRadius);
    }
}

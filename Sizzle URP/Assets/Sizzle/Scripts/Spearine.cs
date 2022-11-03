using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spearine : MonoBehaviour
{
    [Header("General")]
    [SerializeField] Transform rotationBone;
    [SerializeField] Transform spike;
    [SerializeField] LayerMask sizzleMask;

    [Header("Head Turning")]
    [SerializeField] Transform parentNeck;
    [SerializeField] Transform spearineNeck;
    [SerializeField] Transform sizzleBod;
    [SerializeField] float maxTurnAngle;
    [SerializeField] float neckTurnSpeed;

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

    [SerializeField] GameObject viewCam;

    [Header("Attacking")]
    [SerializeField] Vector3 hitCheckOffset;
    [SerializeField] float hitCheckRadius;
    [SerializeField] float attackCooldown;
    /*
[SerializeField] float minFXTime;
[SerializeField] float maxFXTime;
[SerializeField] ParticleSystem groundClashFX;
[SerializeField] float groundHitRadius;
*/
    [SerializeField] GameObject dangerDisplay;
    [SerializeField] List<Vector3> dangerPoints;

    [Header("Distaction")]
    [Tooltip("How long will Spearine be distracted by the charged object")]
    [SerializeField] float distractionTime;
    [Tooltip("After no longer being distracted how long can it be before Spearine can be distracted again")]
    [SerializeField] float distractionCoolDown;
    [Tooltip("How often to check for distraction")]
    [SerializeField] float checkTime;

    [Space]
    private Transform distractionRef;
    [SerializeField] float distractionCheckTimer;
    [SerializeField] float distractionCoolDownTimer;
    private bool distracted; 

    [Header("Sounds")]
    [SerializeField] AudioClip alertSound;
    [SerializeField] AudioClip[] idlesSounds;
    [SerializeField] float alertSoundDelay;

    [Header("Animation")]
    [SerializeField] Animator mainAnimator;
    [SerializeField] Animator headAnimator;
    [SerializeField] AnimationClip alertClip;
    [SerializeField] AnimationClip attackClip;

    [Header("Debug")]
    [SerializeField] bool showRange;
    [SerializeField] bool showHitSphere;

    private Transform player;
    private Transform primaryTarget; // Target could be a player, but also any other charged entity 
    public SpearineStates state;

    private CamManager cm;
    private SoundManager sm;

    // If reaches 100 then Spearine is alert to target 
    [Range(0, 100)]
    private float alertness;
    private Vector3 neckRotOffset;
    private Vector3 spearineRotOffset;

    private float holdTimeForAttack; // The time at which was last attacked 
    public bool canAttack;

    private Coroutine coAlertAndAttack;
    private Coroutine distractionCo;

    private enum DistanceZone
    {
        close,
        mid,
        far,
        NotWithinRange
    }

    public enum SpearineStates
    {
        passive,
        aggressive,
        distracted,
        attacking
    }

    // Start is called before the first frame update
    void Start()
    {
        // Gets player rather than Sizzle because Sizzle is a folder that doesn't represent the tru position 
        player = GameObject.FindWithTag("Player").transform;
        cm = GameObject.FindObjectOfType<CamManager>();
        sm = GameObject.FindObjectOfType<SoundManager>();

        primaryTarget = player;
        state = SpearineStates.passive;

        distractionCheckTimer = checkTime;
        distractionCoolDownTimer = distractionCoolDown;

        neckRotOffset = parentNeck.eulerAngles;
        spearineRotOffset = spearineNeck.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {

        

        //spearineNeck.transform.rotation = parentNeck.transform.rotation;
        //spearineNeck.transform.eulerAngles += neckRotOffset;
        switch(state)
        {
            case SpearineStates.passive:

                UpdateAlertness();

                AimTowardsPlayer();
                AimBone(parentNeck, sizzleBod, maxTurnAngle, neckTurnSpeed);
                spearineNeck.eulerAngles = spearineRotOffset + (parentNeck.eulerAngles - neckRotOffset);

                // Moving towards target but alerted towards player 
                if (alertness >= 90 && coAlertAndAttack == null)
                {
                    coAlertAndAttack = StartCoroutine(StartAlertPhase());
                }


                break;
            case SpearineStates.aggressive:

                Agressive();



                break;
            case SpearineStates.attacking: //spearineRig:Spine3_M
                
                break;
            case SpearineStates.distracted:

                // Checks if need to initialize 
                if (distractionCo == null)
                {
                    distractionCo = StartCoroutine(Distraction(distractionRef));
                }

                break;
        }
    }

    /// <summary>
    /// Logic that plays when Spearine is in an aggressive state 
    /// </summary>
    private void Agressive()
    {
        // Logic when not distracted 
        AimTowardsPlayer();
        AimBone(parentNeck, sizzleBod, maxTurnAngle, neckTurnSpeed);
        spearineNeck.eulerAngles = spearineRotOffset + (parentNeck.eulerAngles - neckRotOffset);

        AttackWhenInRange();

        // Can only be distracted once reaches 0 
        if (distractionCoolDownTimer <= 0)
        {
            // How often does Spearine check for a distraction 
            if (distractionCheckTimer <= 0)
            {
                // Check is a distraction is around
                ChargeObj[] distractions = GameObject.FindObjectsOfType<ChargeObj>();
                if (distractions.Length > 0)
                {

                    // Distraction has been found 
                    distractionRef = distractions[0].transform;
                    distracted = true;
                }

                // Reset timer 
                distractionCheckTimer = checkTime;
            }
            else
            {
                // Continues to countdown 
                distractionCheckTimer -= Time.deltaTime;
            }
        }
        else
        {
            distractionCoolDownTimer -= Time.deltaTime;
        }


        // Allows to attack Sizzle during these times 
        if(Time.realtimeSinceStartup - holdTimeForAttack >= attackCooldown)
        {
            canAttack = true;
        }
        else
        {
            // Band-aid solution that makes sure double attacks dont' happen 
            mainAnimator.SetBool("attacking", false);
        }
    }


    /// <summary>
    /// Directs the stem of Spearine towrads its target 
    /// </summary>
    private void AimTowardsPlayer()
    {
        AimTowardsTarget(player.transform.position);
    }

    /// <summary>
    /// Rotates the stem of the spearine towards the player
    /// </summary>
    /// <param name="target"></param>
    private void AimTowardsTarget(Vector3 target)
    {
        Vector3 targetVec = Vector3.ProjectOnPlane(target - this.transform.position, Vector3.up).normalized;
        //targetVec += boneForwardOffset;

        float angleDifference = Vector3.Angle(this.transform.forward, targetVec);
        float lerp = angleDifference / maxAngle;

        float angleToTurn = turnCurve.Evaluate(lerp) * alertTurnSpeed;

        Vector3 newDir = Vector3.RotateTowards(rotationBone.transform.forward, targetVec, angleToTurn * Time.deltaTime, 0.0f);
        rotationBone.transform.rotation = Quaternion.LookRotation(newDir);
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

        switch (zone)
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
    /// <summary>
    /// When the target is within range activate the attack coroutine 
    /// </summary>
    private void AttackWhenInRange()
    {
        if(!canAttack)
        {
            return;
        }

        if ((this.transform.position - player.position).sqrMagnitude < Mathf.Pow(midRange, 2) && coAlertAndAttack == null)
        {
            state = SpearineStates.attacking;
            canAttack = false;
            coAlertAndAttack = StartCoroutine(Attack());
        }
    }

    private bool IsHittingSizzle()
    {
        return Physics.CheckSphere(spike.transform.position + spike.TransformDirection(hitCheckOffset), hitCheckRadius, sizzleMask);
    }

    /// <summary>
    /// Adjusts the head to look towards the target position 
    /// </summary>
    private void AimBone(Transform bone, Transform target, float maxTurnAngle, float turnSpeed)
    {
        // Store the current head rotation since we will be resetting it
        Quaternion currentLocalRotation = bone.localRotation;
        // Reset the head rotation so our world to local space transformation will use the head's zero rotation. 
        // Note: Quaternion.Identity is the quaternion equivalent of "zero"
        bone.localRotation = Quaternion.identity;

        Vector3 targetWorldLookDir = target.position - bone.position;
        Vector3 targetLocalLookDir = bone.InverseTransformDirection(targetWorldLookDir);

        // Apply angle limit
        targetLocalLookDir = Vector3.RotateTowards(
          Vector3.forward,
          targetLocalLookDir,
          Mathf.Deg2Rad * maxTurnAngle, // Note we multiply by Mathf.Deg2Rad here to convert degrees to radians
          0 // We don't care about the length here, so we leave it at zero
        );

        // Get the local rotation by using LookRotation on a local directional vector
        Quaternion targetLocalRotation = Quaternion.LookRotation(targetLocalLookDir, Vector3.up);

        // Apply smoothing
        bone.localRotation = Quaternion.Slerp(
          currentLocalRotation,
          targetLocalRotation,
          1 - Mathf.Exp(-turnSpeed * Time.deltaTime)
        );

    }

    private IEnumerator StartAlertPhase()
    {
        mainAnimator.enabled = true;
        mainAnimator.SetBool("alert", true);
        
        cm.ChangeCam(viewCam);
        
        sm.PlaySoundFXAfterDelay(alertSound, this.transform.position, "SpearineAlert", alertSoundDelay);

        yield return new WaitForSeconds(alertClip.length);

        // Begins the attack phase 
        state = SpearineStates.aggressive;
        cm.ReturnToCommon();
        coAlertAndAttack = null;
    }

    /// <summary>
    /// Spearine attacks the player via animation 
    /// </summary>
    private IEnumerator Attack()
    {
        // Lunges head towards target according to range 
        mainAnimator.SetBool("attacking", true);
        mainAnimator.enabled = true;
        /* float timer = 0;
         while (timer < attackClip.length)
         {
             // Attack Logic 
             RaycastHit hit;
             // Particles
             *//*if ((timer >= minFXTime) && (timer <= maxFXTime))
             {
                 //groundClashFX.Play();
                 //ParticleSystem temp = Instantiate(groundClashFX, groundClashFX.transform.position, groundClashFX.transform.rotation);
                 //temp.Play();
             }
             //print(timer);*//*

             if (IsHittingSizzle())
             {
                 print("Hitting Sizzle");
                 LevelManager.Reload();
             }

             timer += Time.deltaTime;
             yield return null;
         }*/

        yield return new WaitForSeconds(attackClip.length); // Continues right before the end so it doesn't loop 

        // Sets back to aggressive state 
        state = SpearineStates.aggressive;
        holdTimeForAttack = Time.realtimeSinceStartup;
        
        
        coAlertAndAttack = null;
    }

    public void DisableAnimator()
    {
        mainAnimator.enabled = false;
    }

    private IEnumerator Distraction(Transform distaction)
    {
        float timer = distractionTime;

        Transform distractionHold = distaction;
        Vector3 pos = distractionHold.position;
        print("Distracted");
        while (timer >= 0)
        {
            AimTowardsTarget(pos);

            // Will continue to look at the last known position even if the distraction obj
            // has been destroyed 
            if(distractionHold != null)
            {
                // Sets new position to look at 
                pos = distractionHold.position;
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        distractionCoolDownTimer = distractionCoolDown;
        distracted = false;
        distractionCo = null;
    }

    private void OnDrawGizmos()
    {
        if (showRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(this.transform.position, closeRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(this.transform.position, midRange);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(this.transform.position, farRange);
        }

        if (showHitSphere)
        {
            if (IsHittingSizzle())
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.white;
            }
            Gizmos.DrawWireSphere(spike.transform.position + spike.TransformDirection(hitCheckOffset), hitCheckRadius);
        }

        for (int i = 0; i < dangerPoints.Count; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(this.transform.position + rotationBone.transform.TransformDirection(dangerPoints[i]), 0.1f);
        }

    }
}
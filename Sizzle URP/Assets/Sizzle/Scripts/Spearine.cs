using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spearine : MonoBehaviour
{
    [Header("General")]
    [SerializeField] Transform rotationBone;
    [SerializeField] Transform spike;
    [SerializeField] Transform trueNeck;
    [SerializeField] Transform headVisual;
    [SerializeField] LayerMask sizzleMask;

    [Header("Head Turning")]
    [SerializeField] Transform parentNeck;
    [SerializeField] Transform spearineNeck;
    [SerializeField] Transform sizzleBod;
    [SerializeField] Vector3 sizzlePointOffset;
    [SerializeField] float maxTurnAngle;
    [SerializeField] float neckTurnSpeed;

    [Header("Ranges")]
    [SerializeField] float closeRange;
    [SerializeField] float midRange;
    [SerializeField] float farRange;

    [Header("Speeds")]
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
    [SerializeField] GameObject dangerDisplay;
    [SerializeField] List<Vector3> dangerPoints;

    [Header("Distaction")]
    [SerializeField] Vector3 distractionPointOffset;
    [Tooltip("How long will Spearine be distracted by the charged object")]
    [SerializeField] float distractionTime;
    [Tooltip("After no longer being distracted how long can it be before Spearine can be distracted again")]
    [SerializeField] float distractionCoolDown;
    [SerializeField] float lingerTime;

    [SerializeField] float disCheckRange;
    [SerializeField] LayerMask disCheckLayers;

    [SerializeField] float distractionCoolDownTimer;
    private Transform distractionRef;

    [Header("Head Bobbing")]
    [SerializeField] float bobSpeed;
    [SerializeField] float bobMag;

    [Header("Effects")]
    [SerializeField] ParticleSystem questionFX;
    [SerializeField] ParticleSystem alarmFX;

    [Header("Sounds")]
    [SerializeField] AudioClip alertSound;
    [SerializeField] AudioClip[] idlesSounds;
    [SerializeField] float alertSoundDelay;

    [Header("Animation")]
    [SerializeField] Animator mainAnimator;
    [SerializeField] AnimationClip alertClip;
    [SerializeField] AnimationClip attackClip;

    [Space]
    [Tooltip("The bones that actually get rendered")]
    [SerializeField] List<Transform> visualBones;
    [Tooltip("The bones that are being animated")]
    [SerializeField] List<Transform> trueBones;

    [Space]
    [SerializeField] AnimationCurve animToLookLogicCurve;
    [SerializeField] float animToLookLogicSpeed;

    private float lerpAnimLookLogic;

    [Header("Debug")]
    [SerializeField] bool showRange;
    [SerializeField] bool showHitSphere;
    [SerializeField] bool showDistractionCheckRange;

    private Transform player;
    public SpearineStates state;

    private CamManager cm;
    private SoundManager sm;

    // If reaches 100 then Spearine is alert to target 
    [Range(0, 100)]
    private float alertness;
    private Vector3 neckRotOffset;
    private Vector3 spearineRotOffset;

    private float holdTimeForAttack; // The time at which was last attacked 
    private bool canAttack;

    private Coroutine animToLookLogicCo;
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

        state = SpearineStates.passive;

        distractionCoolDownTimer = distractionCoolDown;

        neckRotOffset = parentNeck.eulerAngles;
        spearineRotOffset = spearineNeck.eulerAngles;

        lerpAnimLookLogic = 1; // Set to look at player 
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();
        
        //spearineNeck.transform.rotation = parentNeck.transform.rotation;
        //spearineNeck.transform.eulerAngles += neckRotOffset;
        switch (state)
        {
            case SpearineStates.passive:

                UpdateAlertness();
                AimTowardsPlayer();
                // Moving towards target but alerted towards player 
                if (alertness >= 90 && coAlertAndAttack == null)
                {
                    coAlertAndAttack = StartCoroutine(StartAlertPhase());
                }

                AimBone(parentNeck, sizzleBod.position + sizzleBod.TransformDirection(sizzlePointOffset), maxTurnAngle, neckTurnSpeed);

                break;
            case SpearineStates.aggressive:

                Agressive();
                CheckForDistraction();
                AimBone(parentNeck, sizzleBod.position + sizzleBod.TransformDirection(sizzlePointOffset), maxTurnAngle, neckTurnSpeed);
                break;
            case SpearineStates.attacking:
                AimBone(parentNeck, sizzleBod.position + sizzleBod.TransformDirection(sizzlePointOffset), maxTurnAngle, neckTurnSpeed);;
                break;
            case SpearineStates.distracted:
                // Checks if need to initialize 
                if (distractionCo == null)
                {
                    questionFX.Play();
                    distractionCo = StartCoroutine(Distraction(distractionRef));
                }

                if(distractionRef != null)
                {
                    AimBone(parentNeck, distractionRef.position + distractionRef.TransformDirection(distractionPointOffset), maxTurnAngle, neckTurnSpeed);
                }

                break;
        }
    }

    /// <summary>
    /// Updates what is being hard animated and what is done procedurally
    /// </summary>
    private void UpdateAnimation()
    {
        
        headVisual.position = trueNeck.position;

        // Makes the visual either cloer to 
        // what is being animated or programmed 
        headVisual.rotation = Quaternion.Lerp(trueNeck.rotation, Quaternion.Euler(spearineRotOffset + (parentNeck.eulerAngles - neckRotOffset)), animToLookLogicCurve.Evaluate(lerpAnimLookLogic));

        // Apply bobbing 
        headVisual.rotation = Quaternion.Euler(headVisual.eulerAngles.x + Mathf.Lerp(0, Mathf.Sin(Time.time * bobSpeed) * bobMag, lerpAnimLookLogic), headVisual.eulerAngles.y, headVisual.eulerAngles.z);

        // Sets rest of bones after the root to the animation 
        // since parent bone is the only thing we need to govern 
        for (int i = 0; i < visualBones.Count; i++)
        {
            visualBones[i].localRotation = trueBones[i].localRotation;
        }
    }

    /// <summary>
    /// Logic that plays when Spearine is in an aggressive state 
    /// </summary>
    private void Agressive()
    {
        // Logic when not distracted 
        AimTowardsPlayer();
        AttackWhenInRange();

        


        // Allows to attack Sizzle during these times 
        if(Time.realtimeSinceStartup - holdTimeForAttack >= attackCooldown)
        {
            canAttack = true;
        }
        else
        {
            // Band-aid solution that makes sure double attacks dont' happen 
            //mainAnimator.SetBool("attacking", false);
        }
    }


    private void CheckForDistraction()
    {
        // Can only be distracted once reaches 0 
        if (distractionCoolDownTimer <= 0)
        {

            // Check is a distraction is around
            Collider[] distractionChecks = Physics.OverlapSphere(this.transform.position, disCheckRange, disCheckLayers);

            for (int i = 0; i < distractionChecks.Length; i++)
            {
                if(distractionChecks[i].GetComponent<ChargeObj>() != null)
                {
                    // Distraction has been found 
                    distractionRef = distractionChecks[i].transform;
                    state = SpearineStates.distracted;
                    break;
                }
            }
        }
        else
        {
            distractionCoolDownTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Directs the stem of Spearine towards its target 
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
        if(!canAttack || coAlertAndAttack != null)
        {
            return;
        }

        if ((this.transform.position - player.position).sqrMagnitude < Mathf.Pow(midRange, 2) && coAlertAndAttack == null)
        {
            // attackstate is now decided during animation
            //state = SpearineStates.attacking;
            canAttack = false;
            //ChangeToLookLogic(false);

            //alarmFX.Play();
            coAlertAndAttack = StartCoroutine(Attack());
        }
    }

    public bool IsHittingSizzle()
    {
        return Physics.CheckSphere(spike.transform.position + spike.TransformDirection(hitCheckOffset), hitCheckRadius, sizzleMask);
    }

    /// <summary>
    /// Adjusts the head to look towards the target position 
    /// </summary>
    private void AimBone(Transform bone, Transform target, float maxTurnAngle, float turnSpeed)
    {
        AimBone(bone, target.position, maxTurnAngle, turnSpeed);
    }

    private void AimBone(Transform bone, Vector3 target, float maxTurnAngle, float turnSpeed)
    {
        // Store the current head rotation since we will be resetting it
        Quaternion currentLocalRotation = bone.localRotation;
        // Reset the head rotation so our world to local space transformation will use the head's zero rotation. 
        // Note: Quaternion.Identity is the quaternion equivalent of "zero"
        bone.localRotation = Quaternion.identity;

        Vector3 targetWorldLookDir = target - bone.position;
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

    public void ChangeToLookLogic(bool toLook = true)
    {
        //StartCoroutine(AnimToLookLogicCo(toLook));
        if (animToLookLogicCo == null)
        {
            animToLookLogicCo = StartCoroutine(AnimToLookLogicCo(toLook));
        }
    }

    private IEnumerator StartAlertPhase()
    {
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

    private IEnumerator Distraction(Transform distaction)
    {
        float timer = distractionTime;

        Transform distractionHold = distaction;
        Vector3 pos = distractionHold.position;

        while (timer >= 0)
        {
            AimTowardsTarget(pos);

            // Will continue to look at the last known position even if the distraction obj
            // has been destroyed 
            if(distractionHold != null)
            {
                // Sets new position to look at 
                pos = distractionHold.position;

                // Aims the head towards the sparks 

            }
            else
            {
                yield return new WaitForSeconds(lingerTime);
                break;
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        // Reset variables 
        distractionCoolDownTimer = distractionCoolDown;

        // Set state 
        state = SpearineStates.aggressive;

        // Cleanup
        StopCoroutine(distractionCo);
        distractionCo = null;
    }

    private IEnumerator AnimToLookLogicCo(bool toLook = true)
    {
        print(toLook);
        if(toLook)
        {
            while (lerpAnimLookLogic < 1)
            {
                lerpAnimLookLogic += animToLookLogicSpeed * Time.deltaTime;
                yield return null;
            }

            lerpAnimLookLogic = 1;
        }
        else
        {
            while (lerpAnimLookLogic >= 0)
            {
                lerpAnimLookLogic -= animToLookLogicSpeed * Time.deltaTime;
                yield return null;
            }

            lerpAnimLookLogic = 0;
        }

        // Error can someonehow make this null????
        if(animToLookLogicCo != null)
        {
            StopCoroutine(animToLookLogicCo);
        }
        animToLookLogicCo = null;
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

        if(showDistractionCheckRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(this.transform.position, disCheckRange);
        }

        if(distractionRef != null)
        {
            Gizmos.DrawSphere(distractionRef.position, 0.1f);
        }
    }
}
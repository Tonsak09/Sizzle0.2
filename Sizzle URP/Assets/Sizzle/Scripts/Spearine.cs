using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spearine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform rotationBone;
    [SerializeField] Transform spike;
    [SerializeField] LayerMask sizzleMask;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] AnimationClip alertClip;
    [SerializeField] AnimationClip attackClip;

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
    [SerializeField] float minFXTime;
    [SerializeField] float maxFXTime;
    [SerializeField] ParticleSystem groundClashFX;
    [SerializeField] float groundHitRadius;

    [Header("Distaction")]
    [Tooltip("How long will Spearine be distracted by the charged object")]
    [SerializeField] float distractionTime;
    [Tooltip("After no longer being distracted how long can it be before Spearine can be distracted again")]
    [SerializeField] float distractionCoolDown;
    [Tooltip("How often to check for distraction")]
    [SerializeField] float checkTime;

    [Space]
    private Transform distractionRef;
    [SerializeField] float checkTimer;
    [SerializeField] float coolDownTimer;
    private bool distracted; 

    [Header("Sounds")]
    [SerializeField] AudioClip alertSound;
    [SerializeField] AudioClip[] idlesSounds;
    [SerializeField] float alertSoundDelay;

    [Header("Debug")]
    [SerializeField] bool showRange;
    [SerializeField] bool showHitSphere;

    private Transform player;
    private Transform primaryTarget; // Target could be a player, but also any other charged entity 

    private CamManager cm;
    private SoundManager sm;

    // If reaches 100 then Spearine is alert to target 
    [Range(0, 100)]
    private float alertness;
    private bool attacking;

    


    private Coroutine coAlertAndAttack;
    private Coroutine distractionCo;

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
        cm = GameObject.FindObjectOfType<CamManager>();
        sm = GameObject.FindObjectOfType<SoundManager>();


        checkTimer = checkTime;
        coolDownTimer = distractionCoolDown;
    }

    // Update is called once per frame
    void Update()
    {
        if (primaryTarget != null)
        {
            // Target has been decided 

            if(!distracted)
            {
                // Logic when not distracted 
                AimTowardsPlayer();
                AttackWhenInRange();

                // Can only be distracted once reaches 0 
                if(coolDownTimer <= 0)
                {
                    // How often does Spearine check for a distraction 
                    if (checkTimer <= 0)
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
                        checkTimer = checkTime;
                    }
                    else
                    {
                        // Continues to countdown 
                        checkTimer -= Time.deltaTime;
                    }
                }
                else
                {
                    coolDownTimer -= Time.deltaTime;
                }
                
            }
            else
            {
                // Checks if need to initialize 
                if(distractionCo == null)
                {
                    distractionCo = StartCoroutine(Distraction(distractionRef));
                }
            }

            
        }
        else
        {
            UpdateAlertness();

            // Moving towards target but alerted towards player 
            if (alertness >= 90 && coAlertAndAttack == null)
            {
                coAlertAndAttack = StartCoroutine(StartAlertPhase());
            }
        }

    }


    /// <summary>
    /// Directs the stem of Spearine towrads its target 
    /// </summary>
    private void AimTowardsPlayer()
    {
        AimTowardsTarget(player.transform.position);
    }

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
        if ((this.transform.position - player.position).sqrMagnitude < Mathf.Pow(midRange, 2) && !attacking && coAlertAndAttack == null)
        {
            attacking = true;
            coAlertAndAttack = StartCoroutine(Attack());
        }
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

    private bool IsHittingSizzle()
    {
        return Physics.CheckSphere(spike.transform.position + spike.TransformDirection(hitCheckOffset), hitCheckRadius, sizzleMask);
    }

    private IEnumerator StartAlertPhase()
    {
        animator.SetBool("alert", true);
        cm.ChangeCam(viewCam);

        sm.PlaySoundFXAfterDelay(alertSound, this.transform.position, "SpearineAlert", alertSoundDelay);

        float timer = alertClip.length;
        while (timer >= 0)
        {
            // Logic during alert 

            timer -= Time.deltaTime;
            yield return null;
        }

        // Begins the attack phase 
        primaryTarget = player;
        cm.ReturnToCommon();
        coAlertAndAttack = null;
    }

    /// <summary>
    /// Spearine attacks the player via animation 
    /// </summary>
    private IEnumerator Attack()
    {
        // Lunges head towards target according to range 
        animator.SetBool("attacking", true);

        float timer = 0;
        while (timer < attackClip.length)
        {
            // Attack Logic 
            RaycastHit hit;
            // Particles
            /*if ((timer >= minFXTime) && (timer <= maxFXTime))
            {
                //groundClashFX.Play();
                //ParticleSystem temp = Instantiate(groundClashFX, groundClashFX.transform.position, groundClashFX.transform.rotation);
                //temp.Play();
            }
            //print(timer);*/

            if (IsHittingSizzle())
            {
                print("Hitting Sizzle");
                LevelManager.Reload();
            }

            timer += Time.deltaTime;
            yield return null;
        }

        attacking = false;
        animator.SetBool("attacking", false);
        coAlertAndAttack = null;
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

        coolDownTimer = distractionCoolDown;
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

    }
}
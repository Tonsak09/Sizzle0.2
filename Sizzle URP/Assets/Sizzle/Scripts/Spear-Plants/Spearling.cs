using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spearling : MonoBehaviour
{
    [Header("Body Rotation")]
    [SerializeField] Transform rotationBone;
    [SerializeField] float alertTurnSpeed;
    [SerializeField] float maxAngle;
    [SerializeField] AnimationCurve turnCurve;

    [Header("Head Rotation")]
    [SerializeField] Transform headBone;
    [SerializeField] Transform neckLooker;
    [SerializeField] Vector3 playerPosOffset;
    [SerializeField] float maxTurnAngle;
    [SerializeField] float turnSpeed;
    [Space]
    [SerializeField] Vector3 distractionPosOffset;


    [Header("Attacking")]
    [SerializeField] Rigidbody[] flickBones;

    [SerializeField] Vector3 neckFlickPos;
    [SerializeField] float flickForce;
    [SerializeField] float flickTime;
    [SerializeField] Vector2 flickLingerTimeRand;
    [SerializeField] AnimationCurve flickCurve;


    [SerializeField] float attackCooldown;

    private Transform player;
    private float attackTimer;
    private float lerp; // lerp between animation and static position 
    public bool aware;

    private Vector3 neckRotOffset;
    private Vector3 spearineRotOffset;

    private Coroutine attackCo;
    private Coroutine distractionCo;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        neckRotOffset = neckLooker.eulerAngles;
        spearineRotOffset = headBone.eulerAngles;

        attackTimer = attackCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        if(aware)
        {

            // Make sure that head is not being taken
            // towards distraction 
            if(distractionCo == null)
            {
                AimTowardsTarget(player.transform.position);
                AimBone(neckLooker, player.transform.position + player.TransformDirection(playerPosOffset), maxTurnAngle, turnSpeed);
            }
            

            headBone.rotation = Quaternion.Euler(spearineRotOffset + (neckLooker.eulerAngles - neckRotOffset));

            // Attacks the player on a cooldown 
            if (attackTimer <= 0)
            {
                Attack();
                attackTimer = attackCooldown + Random.Range(flickLingerTimeRand.x, flickLingerTimeRand.y);
            }
            else
            {
                attackTimer -= Time.deltaTime;
            }
        }
    }

    private void Attack()
    {
        // Flicks the spearling forward 
        //flickBone.AddTorque(-flickForce * Vector3.right, ForceMode.Impulse);
        
        if(attackCo == null)
        {
            attackCo = StartCoroutine(FlickAttackCo());
        }
    }

    /// <summary>
    /// Rotates the stem of the spearine towards the player
    /// </summary>
    /// <param name="target"></param>
    private void AimTowardsTarget(Vector3 target)
    {
        Vector3 targetVec = Vector3.ProjectOnPlane(target - rotationBone.transform.position, Vector3.up).normalized;
        //targetVec += boneForwardOffset;

        float angleDifference = Vector3.Angle(rotationBone.transform.forward, targetVec);
        float lerp = angleDifference / maxAngle;

        float angleToTurn = turnCurve.Evaluate(lerp) * alertTurnSpeed;

        Vector3 newDir = Vector3.RotateTowards(rotationBone.transform.forward, targetVec, angleToTurn * Time.deltaTime, 0.0f);
        rotationBone.transform.rotation = Quaternion.LookRotation(newDir);
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
          Mathf.Deg2Rad * maxTurnAngle, // Note we multiply by Mathf.Deg2Rad here to convert dwd to radians
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

    public void SetDistraction(Transform point, float distractionMaxTime, float distractionHold)
    {
        if(distractionCo == null)
        {
            distractionCo = StartCoroutine(Distraction(point, distractionMaxTime, distractionHold));
        }
    }

    private IEnumerator Distraction(Transform point, float distractionMaxTime, float distractionHold)
    {
        float time = distractionMaxTime;

        while(time > 0)
        {
            // Make sure obj still exists 
            if(point == null)
            {
                break;
            }

            AimTowardsTarget(point.position);
            AimBone(neckLooker, point.position + point.TransformDirection(distractionPosOffset), maxTurnAngle, turnSpeed);


            yield return null;
        }

        yield return new WaitForSeconds(distractionHold);

        // Cleanup
        StopCoroutine(distractionCo);
        distractionCo = null;

    }

    private IEnumerator FlickAttackCo()
    {
        float timer = flickTime;

        while(timer >= 0)
        {
            for (int i = 0; i < flickBones.Length; i++)
            {
                flickBones[i].AddRelativeTorque(flickForce * flickCurve.Evaluate(1 - (timer / attackTimer)) * Time.deltaTime * Vector3.up, ForceMode.Force);
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        // Cleanup
        StopCoroutine(attackCo);
        attackCo = null;
    }

    private IEnumerator ChangeToStatic()
    {
        while (lerp <= 1)
        {
            lerp += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ChangeToLogic()
    {
        while (lerp <= 1)
        {
            lerp += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            aware = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            aware = false;
        }
    }
}

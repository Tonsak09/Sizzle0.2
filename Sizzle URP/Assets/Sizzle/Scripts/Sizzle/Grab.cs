using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grab : MonoBehaviour
{
    [Header("Neck")]
    [SerializeField] ConfigurableJoint neckJoint;

    [SerializeField] Vector3 neckDefaultRot;
    [SerializeField] Vector3 neckTargetRot;

    [SerializeField] AnimationCurve neckOpenAnimCurve;
    [SerializeField] float neckOpenSpeed;

    [SerializeField] AnimationCurve neckCloseAnimCurve;
    [SerializeField] float neckCloseSpeed;

    [Header("Jaw")]
    [SerializeField] ConfigurableJoint jawJoint;

    [SerializeField] Vector3 jawDefaultRot;
    [SerializeField] Vector3 jawTargetRot;

    [SerializeField] AnimationCurve jawOpenAnimCurve;
    [SerializeField] float JawOpenSpeed;

    [SerializeField] AnimationCurve jawCloseAnimCurve;
    [SerializeField] float JawCloseSpeed;

    [SerializeField] float closedJawLerp;

    [Header("Grabbing")]
    [Tooltip("The threshold the lerp must surpass before Sizzle can grab anything")]
    [SerializeField] Vector2 jawLerpRange;

    [SerializeField] Vector3 detectStartOffset;
    [SerializeField] Vector3 detectTargetOffset;
    [SerializeField] AnimationCurve detectOffsetAnimCurve;
    [SerializeField] float detectSpeed;

    [SerializeField] float detectStartSize;
    [SerializeField] float detectTargetSize;

    [SerializeField] LayerMask grabbable;

    [Header("Sounds")]
    [SerializeField] AudioClip biteSound;


    // used to pass the function from this class into the animation which is handeled by the bodyanimmanager
    // Needs to be done because lerp value constatly changes 
    private delegate void logic(float lerp); 
    private float detectLerp;
    private Transform heldItem;
    private const string ANIMKEY = "Head";
    private BodyAnimationManager animaManager;
    private SoundManager sm;

    private void Start()
    {
        animaManager = this.GetComponent<BodyAnimationManager>();

        sm = GameObject.FindObjectOfType<SoundManager>();
    }

    private void Update()
    {
        if(Input.GetMouseButton(1))
        {
            animaManager.TryAnimation(AnimateJaw(GrabLogic), ANIMKEY);
        }
    }

    private void GrabLogic(float jawLerp)
    {
        if (Input.GetMouseButton(1))
        {
            // Grabbing an item 
            if (heldItem == null)
            {
                // Makes sure it's within range
                if (jawLerp >= jawLerpRange.x && jawLerp <= jawLerpRange.y)
                {
                    Collider[] grabbables = Physics.OverlapSphere
                        (
                            neckJoint.transform.position +
                            neckJoint.transform.TransformDirection(Vector3.Lerp(detectStartOffset, detectTargetOffset, detectOffsetAnimCurve.Evaluate(detectLerp))),
                            Maths.Lerp(detectStartSize, detectTargetSize, detectLerp),
                            grabbable
                        );

                    if (grabbables.Length > 0)
                    {


                        heldItem = grabbables[0].transform;

                        // Sets held objects params so physics don't mess with one another 
                        heldItem.GetComponent<Grabbable>().SetGrabActive();
                        heldItem.GetComponent<Rigidbody>().isKinematic = true;
                        heldItem.GetComponent<Buoyancy>().enabled = false;

                        heldItem.transform.parent = jawJoint.transform;
                    }
                }
            }
            else
            {
                // Grabbed hold postion
                jawLerp = closedJawLerp;
                jawJoint.targetRotation = Quaternion.Euler(Vector3.Lerp(jawDefaultRot, jawTargetRot, jawOpenAnimCurve.Evaluate(jawLerp)));
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            // Grabbing an item 
            if (heldItem != null)
            {
                // Throwing an object 
                heldItem.parent = null;

                // Reactivates parts of obj so can act as normal 
                heldItem.GetComponent<Grabbable>().SetNonGrabActive();
                heldItem.GetComponent<Rigidbody>().isKinematic = false;
                heldItem.GetComponent<Buoyancy>().enabled = true;

                heldItem.eulerAngles = new Vector3(0, heldItem.eulerAngles.y, 0);

                heldItem = null;
            }
        }
    }

   /* private void AnimateJaw()
    {
        // On right click hold 
        if (Input.GetMouseButton(1))
        {
            // If it is possible to animate 
            if(animaManager.TryAnimation(AnimateJaw(), ANIMKEY))
            {
                
            }
        }
        else
        {

            // If it is possible to animate 
            if (animaManager.TryAnimation(AnimateJaw(), ANIMKEY))
            {

            }

            
        }
    }*/

    private IEnumerator AnimateJaw(logic jawLogic)
    {
        // The lerp between a mouth open and closed 
        float neckLerp = 0;
        float jawLerp = 0;


        while(Input.GetMouseButton(1))
        {

            // Continues unless fully open 
            if (neckLerp < 1)
            {
                neckLerp += neckOpenSpeed * Time.deltaTime;
            }
            if (jawLerp < 1)
            {
                jawLerp += JawOpenSpeed * Time.deltaTime;
            }
            if (detectLerp < 1)
            {
                detectLerp += detectSpeed * Time.deltaTime;
            }

            // Applys the rotation 
            neckJoint.targetRotation = Quaternion.Euler(Vector3.Lerp(neckDefaultRot, neckTargetRot, neckOpenAnimCurve.Evaluate(neckLerp)));
            jawJoint.targetRotation = Quaternion.Euler(Vector3.Lerp(jawDefaultRot, jawTargetRot, jawOpenAnimCurve.Evaluate(jawLerp)));

            jawLogic(jawLerp);

            yield return null;
        }

        while(!Input.GetMouseButton(1))
        {
            // Continues unless fully closed 
            if (neckLerp > 0)
            {
                neckLerp -= neckCloseSpeed * Time.deltaTime;
            }
            if (jawLerp > 0)
            {
                jawLerp -= JawCloseSpeed * Time.deltaTime;
            }
            if (detectLerp > 0)
            {
                detectLerp -= detectSpeed * Time.deltaTime;
            }

            // Applys the rotation 
            neckJoint.targetRotation = Quaternion.Euler(Vector3.Lerp(neckDefaultRot, neckTargetRot, neckCloseAnimCurve.Evaluate(neckLerp)));
            jawJoint.targetRotation = Quaternion.Euler(Vector3.Lerp(jawDefaultRot, jawTargetRot, jawCloseAnimCurve.Evaluate(jawLerp)));

            if(neckLerp <= 0 && jawLerp <= 0 && detectLerp <= 0)
            {
                //sm.PlaySoundFX(biteSound, neckJoint.targetPosition, "Bite");
                break;
            }

            jawLogic(jawLerp);

            yield return null;
        }

        animaManager.EndAnimation(ANIMKEY);
    }

    private void OnDrawGizmos()
    {
        /*if (jawLerp >= jawLerpRange.x && jawLerp <= jawLerpRange.y)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }*/

        Gizmos.DrawWireSphere(neckJoint.transform.position + 
            neckJoint.transform.TransformDirection(Vector3.Lerp(detectStartOffset, detectTargetOffset, detectOffsetAnimCurve.Evaluate(detectLerp))),
            Maths.Lerp(detectStartSize, detectTargetSize, detectLerp)
            );
    }
}

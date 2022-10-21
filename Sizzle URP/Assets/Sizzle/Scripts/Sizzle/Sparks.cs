using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sparks : MonoBehaviour
{
    [Header("Sparks")]
    [SerializeField] GameObject sparksFX;
    [SerializeField] Vector3 sparksOffsetFromNeck;
    [SerializeField] float delayToSpawnSparks;
    [Space]
    [SerializeField] Vector3 detectOffset;
    [SerializeField] float detectRadius;
    [SerializeField] LayerMask detectMask;
    [Space]
    [Tooltip("Sent out from Sizzle to check for Slime triggers")]
    [SerializeField] GameObject checkSphere;

    [Header("Neck")]
    [SerializeField] ConfigurableJoint neckJoint;

    [SerializeField] Vector3 neckDefaultRot;
    [SerializeField] Vector3 neckTargetRot;

    [SerializeField] AnimationCurve neckOpenAnimCurve;
    [SerializeField] AnimationCurve neckCloseAnimCurve;
    [SerializeField] float neckSpeed;


    [Header("Jaw")]
    [SerializeField] ConfigurableJoint jawJoint;

    [SerializeField] Vector3 jawDefaultRot;
    [SerializeField] Vector3 jawTargetRot;

    [SerializeField] AnimationCurve jawOpenAnimCurve;
    [SerializeField] AnimationCurve jawCloseAnimCurve;
    [SerializeField] float jawSpeed;

    [Header("Sounds")]
    [SerializeField] AudioClip sparkStartHiss;

    [SerializeField] float lerpCloseToPlayMouth;
    [SerializeField] AudioClip[] closeMouthNoises;

    private const string ANIMKEY = "Head";
    private BodyAnimationManager animaManager;
    private SoundManager sm;

    private List<Transform> sparksInWorld;

    public List<Transform> SparksInWorld { get { return sparksInWorld; } }

    private void Start()
    {
        animaManager = this.GetComponent<BodyAnimationManager>();
        sm = GameObject.FindObjectOfType<SoundManager>();
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            TryToSparks();
        }
    }

    /// <summary>
    /// Makes sure that there are no obstacles in front or other conditions
    /// that will inhibit Sizzle from creating sparks 
    /// </summary>
    private void TryToSparks()
    {
        // Tries to animate the sparks if possible 
        if(animaManager.TryAnimation(HeadAnimation(), ANIMKEY))
        {
            sm.PlaySoundFX(sparkStartHiss, neckJoint.targetPosition, "Hiss");
        }

        /*if (!Physics.CheckSphere(neckJoint.transform.position + neckJoint.transform.TransformDirection(detectOffset), detectRadius, detectMask))
        {
            animaManager.TryAnimateHead(HeadAnimation(), ANIMKEY);
        }*/
    }


    private IEnumerator HeadAnimation()
    {

        float neckLerp = 0;
        float jawLerp = 0;

        // Starts timer till spawning sparks 
        StartCoroutine(SpawnSparks());
        Instantiate(checkSphere, neckJoint.transform.position, neckJoint.transform.rotation);

        // Opening
        while (neckLerp < 1 && jawLerp < 1)
        {
            neckLerp = Mathf.Clamp01(neckLerp + neckSpeed * Time.deltaTime);
            jawLerp = Mathf.Clamp01(jawLerp + jawSpeed * Time.deltaTime);

            // Sets lerp to animation curve
            neckJoint.targetRotation = Quaternion.Euler(Vector3.Lerp(neckDefaultRot, neckTargetRot, neckOpenAnimCurve.Evaluate(neckLerp)));
            jawJoint.targetRotation = Quaternion.Euler(Vector3.Lerp(jawDefaultRot, jawTargetRot, jawOpenAnimCurve.Evaluate(jawLerp)));

            yield return null;
        }

        bool playedClose = false; // Whether the mouth close sound has been played 
        // Closing 
        while(neckLerp > 0 && jawLerp > 0)
        {
            neckLerp = Mathf.Clamp01(neckLerp - neckSpeed * Time.deltaTime);
            jawLerp = Mathf.Clamp01(jawLerp - jawSpeed * Time.deltaTime);

            // Sets lerp to animation curve
            neckJoint.targetRotation = Quaternion.Euler(Vector3.Lerp(neckDefaultRot, neckTargetRot, neckCloseAnimCurve.Evaluate(neckLerp)));
            jawJoint.targetRotation = Quaternion.Euler(Vector3.Lerp(jawDefaultRot, jawTargetRot, jawCloseAnimCurve.Evaluate(jawLerp)));

            if(!playedClose)
            {
                if(lerpCloseToPlayMouth > jawLerp)
                {
                    sm.PlaySoundFX(closeMouthNoises[Random.Range(0, closeMouthNoises.Length)], neckJoint.targetPosition, "Mouth");
                    playedClose = true;
                }
            }

            yield return null;
        }

        animaManager.EndAnimation(ANIMKEY);
    }

    private IEnumerator SpawnSparks()
    {
        yield return new WaitForSeconds(delayToSpawnSparks);
        Instantiate(sparksFX, neckJoint.transform.position + neckJoint.transform.TransformDirection(sparksOffsetFromNeck), neckJoint.transform.rotation);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(neckJoint.transform.position + neckJoint.transform.TransformDirection(detectOffset), detectRadius);
    }
}

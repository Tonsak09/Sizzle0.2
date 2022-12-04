using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceOnTrigger : MonoBehaviour
{
    [SerializeField] string tagToTrigger;

    [Header("Animation")]
    [Header("Translation")]
    [SerializeField] float startDelay;
    [SerializeField] AnimationCurve sinkCurve;
    [SerializeField] float sinkSpeed;
    [SerializeField] AnimationCurve riseCurve;
    [SerializeField] float riseSpeed;
    [SerializeField] Vector3 target;

    [Header("Rotation")]

    private Coroutine bounceCo;
    private Vector3 startHold;
    private Quaternion startRot;

    private void Awake()
    {
        startHold = this.transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void TryBounce()
    {
        if(bounceCo == null)
        {
            bounceCo = StartCoroutine(Bounce());
        }
    }

    private IEnumerator Bounce()
    {

        yield return new WaitForSeconds(startDelay);

        float lerp = 0; 
        while(lerp <= 1)
        {
            this.transform.position = Vector3.LerpUnclamped(startHold, startHold + this.transform.TransformDirection(target), sinkCurve.Evaluate(lerp));

            lerp += Time.deltaTime * sinkSpeed;
            yield return null;
        }

        // Position may not be the actual target 
        Vector3 midPos = this.transform.position;
        lerp = 0;
        while (lerp <= 1)
        {
            this.transform.position = Vector3.LerpUnclamped(midPos, startHold, riseCurve.Evaluate(lerp));

            lerp += Time.deltaTime * riseSpeed;
            yield return null;
        }

        bounceCo = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == tagToTrigger)
        {
            TryBounce();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(startHold, startHold + this.transform.TransformDirection(target));
        Gizmos.DrawWireSphere(startHold + this.transform.TransformDirection(target), 0.1f);
    }
}

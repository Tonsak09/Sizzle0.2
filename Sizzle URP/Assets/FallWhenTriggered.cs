using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallWhenTriggered : MonoBehaviour
{

    [SerializeField] Vector3 target;
    [SerializeField] AnimationCurve curve;

    [SerializeField] float speed;

    private Vector3 startPos;
    private bool fallen;

    private void Start()
    {
        startPos = this.transform.position;
        fallen = false;
    }

    private IEnumerator Fall()
    {
        float lerp = 0;

        while(lerp <= 1)
        {

            this.transform.position = Vector3.Lerp(startPos, startPos + target, curve.Evaluate(lerp));

            lerp += Time.deltaTime * speed;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!fallen && other.tag == "Player")
        {
            fallen = true;

            StartCoroutine(Fall());
        }
    }
}

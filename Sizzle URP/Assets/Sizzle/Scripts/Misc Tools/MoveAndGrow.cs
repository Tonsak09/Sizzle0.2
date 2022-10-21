using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAndGrow : MonoBehaviour
{
    [SerializeField] float lifeTime;
    [SerializeField] float speed;

    [SerializeField] Vector3 startSize;
    [SerializeField] Vector3 targetSize;
    [SerializeField] AnimationCurve growthCurve;

    private void Start()
    {
        Animate();
    }

    public void Animate()
    {
        StartCoroutine(EAnimate());
    }

    private IEnumerator EAnimate()
    {
        float lerp = 0;
        float timer = 0;

        while(lerp <= 1)
        {

            this.transform.localScale = Vector3.Lerp(startSize, targetSize, growthCurve.Evaluate(lerp));
            this.transform.position += this.transform.forward * speed * Time.deltaTime;

            lerp = timer / lifeTime;
            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(this.gameObject);
    }


    // TODO REMOVE THIS AS THE DESTROYER 
}

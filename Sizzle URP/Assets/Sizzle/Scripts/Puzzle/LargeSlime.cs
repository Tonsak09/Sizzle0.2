using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LargeSlime : MonoBehaviour
{
    [Header("Shrinking")]
    [SerializeField] float shrinkTime;
    [SerializeField] float shrinkPercentFinal;
    [SerializeField] AnimationCurve shrinkCurve;

    [Header("Spawning")]
    [SerializeField] GameObject normalSlime;
    [SerializeField] GameObject explodeFX;
    [Tooltip("x - min, y - max")]
    [SerializeField] Vector2 spawnDistance;
    
    // The scale this slime starts as 
    private Vector3 holdScale;
    private Vector3 scaleFinal { get { return holdScale * shrinkPercentFinal; } }

    private Coroutine PopCo;

    private void Start()
    {
        holdScale = this.transform.localScale;
    }

    public void Pop()
    {
        if(PopCo == null)
        {
            PopCo = StartCoroutine(ShrinkCo());
        }
    }

    private IEnumerator ShrinkCo()
    {
        float timer = 0;
        while(timer <= shrinkTime)
        {

            this.transform.localScale =  Vector3.Lerp(holdScale, scaleFinal, shrinkCurve.Evaluate(timer / shrinkTime));

            timer += Time.deltaTime;
            yield return null;
        }

        // Explode with smaller slimes 
        Instantiate(explodeFX, this.transform.position, Quaternion.identity);
        Destroy(this.gameObject);
        StopCoroutine(PopCo);
    }
}

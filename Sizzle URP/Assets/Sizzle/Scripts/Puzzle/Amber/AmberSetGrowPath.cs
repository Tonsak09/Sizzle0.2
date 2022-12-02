using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmberSetGrowPath : AmberSet
{
    [SerializeField] List<Transform> pathToGrow;
    [SerializeField] float pauseBetweenParts;
    [SerializeField] float growSpeed;
    [SerializeField] AnimationCurve YCurve;
    [SerializeField] AnimationCurve XZCurve;
    [Space]
    [SerializeField] Vector3 startScale;

    private Vector3[] targetScales;
    private bool started;

    private void Start()
    {
        targetScales = new Vector3[pathToGrow.Count];

        // Sets up scales of each target so they can be edited in editor without
        // needing to worry that they will need to be changed in code as well 
        for (int i = 0; i < targetScales.Length; i++)
        {
            targetScales[i] = pathToGrow[i].localScale;
            pathToGrow[i].localScale = startScale;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!started)
        {
            if(AllAmberUnlocked())
            {
                started = true;
                StartCoroutine(GrowPath());
            }
        }
    }

    /// <summary>
    /// Goes through each item to grow a path 
    /// </summary>
    /// <returns></returns>
    private IEnumerator GrowPath()
    {
        int index = 0;
        while(index < pathToGrow.Count)
        {
            StartCoroutine(GrowSingle(pathToGrow[index], targetScales[index]));
            index++;
            yield return new WaitForSeconds(pauseBetweenParts);
        }
    }

    /// <summary>
    /// Scales an indeividual item to a target size 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="targetScale"></param>
    /// <returns></returns>
    private IEnumerator GrowSingle(Transform target, Vector3 targetScale)
    {
        float lerp = 0;
        while(lerp <= 1)
        {
            target.localScale =
                new Vector3
                (
                    Mathf.LerpUnclamped(startScale.x, targetScale.x, XZCurve.Evaluate(lerp)),
                    Mathf.LerpUnclamped(startScale.y, targetScale.y, YCurve.Evaluate(lerp)),
                    Mathf.LerpUnclamped(startScale.y, targetScale.y, XZCurve.Evaluate(lerp))
                );


            lerp += Time.deltaTime * growSpeed;
            yield return null;
        }
    }
}

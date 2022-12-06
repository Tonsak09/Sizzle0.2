using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmberSetGrowPath : AmberSet
{
    [Header("Path")]
    [SerializeField] List<Transform> pathToGrow;
    [SerializeField] float pauseBetweenParts;
    [SerializeField] float growSpeed;
    [SerializeField] AnimationCurve YCurve;
    [SerializeField] AnimationCurve XZCurve;
    [Space]
    [SerializeField] Vector3 startScale;

    [Header("Audio")]
    [SerializeField] float minPitch;
    [SerializeField] float maxPitch;
    [SerializeField] float pitchVariation;
    [SerializeField] AnimationCurve pitchCurve;

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

            AudioSource audio = pathToGrow[index].GetComponent<AudioSource>();
            if (audio != null)
            {
                print("sound");
                float pitch = 1;//Mathf.Lerp(minPitch, maxPitch, pitchCurve.Evaluate(index / pathToGrow.Count));
                GameObject.FindObjectOfType<SoundManager>().PlaySoundFX(audio.clip, pathToGrow[index].position, "MUSH", pitch + Random.Range(-pitchVariation, pitchVariation), audio.volume, 99);
            }

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

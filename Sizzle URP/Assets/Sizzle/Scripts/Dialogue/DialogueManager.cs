using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] RectTransform display;
    [SerializeField] TextMeshProUGUI textMesh;

    [Header("Animation")]

    [SerializeField] Vector3 offsetPos;
    [SerializeField] AnimationCurve positionCurve;
    [SerializeField] float posLerpSpeed;
    [Space]
    [SerializeField] Vector3 startScale;
    [SerializeField] Vector3 targetedScale;
    [SerializeField] AnimationCurve scaleCurve;
    [SerializeField] float scaleLerpSpeed;

    [Header("General Settings")]
    [SerializeField] KeyCode nextKey;

    private bool moving;
    private Coroutine dialogueCoroutine;

    private Vector3 startPos;
    private Vector3 offsetedPos;
    private Vector3 offsetedScale;

    public bool Running { get { return dialogueCoroutine != null; } }

    private void Start()
    {
        display.gameObject.SetActive(true);

        startPos = display.position;
        offsetedPos = startPos + offsetPos;

        offsetedScale = targetedScale;

        StartCoroutine(Appear());

        textMesh.text = "";

        
    }

    private void Update()
    {
        if (Input.GetKeyDown(nextKey))
        {
            print("INPUT READ");
            StartCoroutine(Appear());
        }   
    }

    public void RunText(List<string> texts)
    {
        // Makes sure it doesn't overide 
        if(Running == false)
        {
            StartCoroutine(Appear());
            dialogueCoroutine = StartCoroutine(TextEnumerator(texts));
        }

    }

    private IEnumerator TextEnumerator(List<string> texts)
    {
        // Holds this position until moving has finished 
        while(moving)
        {
            yield return null;  
        }

        int current = 0;
        while(true)
        {
            textMesh.text = texts[current];

            if(Input.GetKeyDown(nextKey))
            {
                current++;

                // When to end the dialgue 
                if(current >= texts.Count)
                {
                    EndDialogue();
                    break;
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// End the current dialgoue if possible 
    /// </summary>
    public void EndDialogue()
    {
        if(dialogueCoroutine != null)
        {
            textMesh.text = "";
            StartCoroutine(Dissapear());
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }
    }

    private IEnumerator Appear()
    {
        moving = true;
        float posLerp = 0;
        float scaleLerp = 0;

        while (posLerp <= 1 || scaleLerp <= 1)
        {
            display.position = Vector3.Lerp(startPos, offsetedPos, positionCurve.Evaluate(posLerp));
            display.localScale = Vector3.Lerp(startScale, offsetedScale, scaleCurve.Evaluate(scaleLerp));

            posLerp += posLerpSpeed * Time.deltaTime;
            scaleLerp += scaleLerpSpeed * Time.deltaTime;

            yield return null;
        }

        moving = false;
    }
    private IEnumerator Dissapear()
    {
        moving = true;
        float lerp = 0;

        while (lerp <= 1)
        {
            //lerp += displaySpeed * Time.deltaTime;

            //display.position = Vector3.Lerp(onScreen.position, offScreen.position, lerp);

            yield return null;
        }

        moving = false;
    }


}

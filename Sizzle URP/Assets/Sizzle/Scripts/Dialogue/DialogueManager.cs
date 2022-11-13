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
    [Header("Appearing")]
    [SerializeField] Vector3 offsetPos;
    [SerializeField] AnimationCurve positionCurve;
    [SerializeField] float posLerpSpeed;
    [Space]
    [SerializeField] Vector3 startScale;
    [SerializeField] Vector3 targetedScale;
    [SerializeField] AnimationCurve scaleCurve;
    [SerializeField] float scaleLerpSpeed;
    [Header("Dissapearing")]
    [SerializeField] AnimationCurve dissapearCurve;
    [SerializeField] float dissapearSpeed;

    [Header("General Settings")]
    [SerializeField] KeyCode nextKey;
    [SerializeField] float textSpeed;

    private bool moving;
    private bool currentTextFinished;
    private Coroutine displayAppearanceCo;
    private Coroutine dialogueCoroutine;

    private Vector3 startPos;
    private Vector3 offsetedPos;
    private Vector3 offsetedScale;

    int index = 0;

    public bool Running { get { return dialogueCoroutine != null; } }

    private void Start()
    {
        display.gameObject.SetActive(true);

        startPos = display.position;
        offsetedPos = startPos + offsetPos;

        offsetedScale = targetedScale;

        currentTextFinished = true;
        textMesh.text = "";

        //displayAppearanceCo = StartCoroutine(Appear());
        //dialogueCoroutine = StartCoroutine(RunDialogue(sampleText));   
        //StartCoroutine(Dissapear());


        display.localScale = Vector3.zero;
    }

    private void Update()
    {

    }

    public void RunText(List<string> texts)
    {
        // Makes sure it doesn't overide 
        if(Running == false)
        {

            StartCoroutine(Appear());

            dialogueCoroutine = StartCoroutine(RunDialogue(texts));
        }

    }

    private IEnumerator RunDialogue(List<string> texts)
    {
        index = 0;
        while (index < texts.Count)
        {
            if(!currentTextFinished)
            {
                yield return null;
            }
            else
            {
                currentTextFinished = false;
                StartCoroutine(ProcessDialogue(texts[index], textSpeed));

            }
        }

        // End Text
        StartCoroutine(Dissapear());
    }

    /// <summary>
    /// Plays the dialogue to the display and includes effects
    /// </summary>
    /// <param name="dialogue"></param>
    private IEnumerator ProcessDialogue(string dialogue, float pauseTime)
    {
        string[] processed = dialogue.Split();

        for (int i = 0; i < processed.Length; i++)
        {
            // Check if event 
            if(processed[i][0] == '<')
            {
                // Play effect 
            }
            else
            {
                for (int j = 0; j < processed[i].Length; j++)
                {
                    textMesh.text += processed[i][j];
                    yield return new WaitForSeconds(pauseTime);
                }

                textMesh.text += " ";
            }
        }

        while(true)
        {
            if (Input.GetKeyDown(nextKey))
            {
                textMesh.text = "";
                index++;

                break;
            }
            yield return null;
        }

        currentTextFinished = true;
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
        Vector3 startScale = display.localScale;

        while (lerp <= 1)
        {
            lerp += dissapearSpeed * Time.deltaTime;

            display.localScale = Vector3.Lerp(Vector3.zero, startScale, dissapearCurve.Evaluate(lerp));

            yield return null;
        }

        moving = false;
        dialogueCoroutine = null;
    }


}

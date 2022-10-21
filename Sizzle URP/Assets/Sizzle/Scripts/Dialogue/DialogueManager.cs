using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] RectTransform display;
    [SerializeField] RectTransform offScreen;
    [SerializeField] RectTransform onScreen;
    [SerializeField] TextMeshProUGUI textMesh;

    [SerializeField] KeyCode nextKey;
    [SerializeField] float displaySpeed;

    private bool moving;
    private Coroutine dialogueCoroutine;

    public bool Running { get { return dialogueCoroutine != null; } }

    private void Start()
    {
        display.gameObject.SetActive(true);
        display.position = offScreen.position;

        textMesh.text = "";
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
        float lerp = 0;

        while (lerp <= 1)
        {
            lerp += displaySpeed * Time.deltaTime;

            display.position = Vector3.Lerp(offScreen.position, onScreen.position, lerp);

            yield return null;
        }

        moving = false;
    }
    private IEnumerator Dissapear()
    {
        print("test");

        moving = true;
        float lerp = 0;

        while (lerp <= 1)
        {
            lerp += displaySpeed * Time.deltaTime;

            display.position = Vector3.Lerp(onScreen.position, offScreen.position, lerp);

            yield return null;
        }

        moving = false;
    }


}

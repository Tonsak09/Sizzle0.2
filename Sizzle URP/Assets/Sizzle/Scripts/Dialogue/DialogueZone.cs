using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueZone : MonoBehaviour
{
    [SerializeField] List<string> text;

    private DialogueManager dm;

    private void Awake()
    {
        dm = GameManager.FindObjectOfType<DialogueManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            dm.RunText(text);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            //dm.EndDialogue();
        }
    }
}

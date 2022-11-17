using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonlitMeetingCinematic : MonoBehaviour
{
    [SerializeField] GameObject frontCam;
    [SerializeField] GameObject backCam;

    [SerializeField] Animator cinematicSizzleAnimator;

    [SerializeField] float startDelay;
    [SerializeField] float dialogueDelay;

    [SerializeField] List<string> snabianText;
    [SerializeField] int indexToSwapCam;

    private CamManager camManager;
    private Transitions transition;
    private DialogueManager dialogue;

    private GameObject sizzlePlayer;
    private bool activated;
    private bool camSwapped;
    
    // Start is called before the first frame update
    void Start()
    {
        camManager = GameObject.FindObjectOfType<CamManager>();
        transition = GameObject.FindObjectOfType<Transitions>();
        dialogue = GameObject.FindObjectOfType<DialogueManager>();

        sizzlePlayer = GameObject.FindGameObjectWithTag("Sizzle");

        activated = false;
        camSwapped = false;
    }

    private void Update()
    {
        if (!camSwapped && activated)
        {

            if (dialogue.Index == indexToSwapCam)
            {
                camSwapped = true;

                camManager.ChangeCam(backCam);
            }
        }
    }

    private void TryStartCinematic()
    {
        if(!activated)
        {
            activated = true;
            StartCoroutine(CinematicStart());
        }
        
    }

    private IEnumerator CinematicStart()
    {
        transition.TryBlackOut();

        yield return new WaitForSeconds(startDelay / 2);

        sizzlePlayer.SetActive(false);
        cinematicSizzleAnimator.gameObject.SetActive(true);
        // Change cam 
        camManager.ChangeCam(frontCam);

        yield return new WaitForSeconds(startDelay / 2);


        // Fade back in 
        transition.TryBlackIn();

        cinematicSizzleAnimator.SetBool("begin", true);

        yield return new WaitForSeconds(dialogueDelay);
        // Begin dialogue 
        dialogue.Apeear();
        dialogue.RunText(snabianText);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryStartCinematic();
    }
}

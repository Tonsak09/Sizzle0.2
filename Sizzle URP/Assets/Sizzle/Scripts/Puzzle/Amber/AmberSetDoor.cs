using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmberSetDoor : AmberSet
{

    [Header("Door")]
    [SerializeField] Transform door;
    [SerializeField] Vector3 targetOffset;
    [Space]
    [SerializeField] float startDelay;
    [SerializeField] float openSpeed;
    [SerializeField] AnimationCurve openCurve;
    [SerializeField] float closeSpeed;
    [SerializeField] AnimationCurve closeCurve;

    [Header("Cam")]
    [SerializeField] Transform targetCam;

    private Coroutine animCo;
    private Vector3 holdStart;
    private bool open;



    // Start is called before the first frame update
    void Start()
    {
        holdStart = door.position;
        open = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (AllAmberUnlocked())
        {
            print("All Unlocked");
            TryOpen();
        }
    }

    /// <summary>
    /// Attempts to open the door 
    /// </summary>
    public void TryOpen()
    {
        if(!open)
        {
            if(animCo == null)
            {
                animCo = StartCoroutine(OpenDoor());
            }
        }
    }

    /// <summary>
    /// Attempts to close the door 
    /// </summary>
    public void TryClose()
    {
        if (open)
        {
            if (animCo == null)
            {
                animCo = StartCoroutine(CloseDoor());
            }
        }
    }

    private IEnumerator OpenDoor()
    {
        yield return new WaitForSeconds(startDelay);

        float lerp = 0; 
        while(lerp <= 1)
        {
            door.transform.position = Vector3.Lerp(holdStart, holdStart + door.TransformDirection(targetOffset), openCurve.Evaluate(lerp));

            lerp += Time.deltaTime * openSpeed;
            yield return null;
        }

        open = true;
        animCo = null;
    }

    private IEnumerator CloseDoor()
    {
        float lerp = 1;
        while (lerp >= 0)
        {
            door.transform.position = Vector3.Lerp(holdStart, holdStart + door.TransformDirection(targetOffset), closeCurve.Evaluate(lerp));

            lerp -= Time.deltaTime * closeSpeed;
            yield return null;
        }

        open = false;
        animCo = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(door.position + door.TransformDirection(targetOffset), 0.1f);
    }

 
}

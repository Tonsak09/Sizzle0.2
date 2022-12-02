using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    private const string sizzleTag = "Player"; // Base joint has player tag which is the real collider
    private const string slimeTag = "Slime";

    private Transitions transition;

    // Start is called before the first frame update
    void Start()
    {
        transition = GameObject.FindObjectOfType<Transitions>();
    }

    private void OnTriggerEnter(Collider other)
    {
        string tag = other.gameObject.tag;

        switch (tag)
        {
            case sizzleTag:
                transition.ResetToCheckPoint();
                break;
            case slimeTag:
                other.GetComponent<Slime>().Pop();
                break;
        }
    }
}

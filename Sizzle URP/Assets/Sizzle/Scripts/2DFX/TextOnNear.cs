using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;

public class TextOnNear : MonoBehaviour
{
    private TextMeshPro tm;
    private Transform targetHold;

    private void Start()
    {
        tm = this.GetComponent<TextMeshPro>();

        tm.color = new Color(0.8f, 0.8f, 0.8f, 0.1f);

    }

    private void OnTriggerEnter(Collider other)
    {
        tm.color = new Color(1, 1, 1, 1);
        //cam.LookAt = this.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        tm.color = new Color(0.8f, 0.8f, 0.8f, 0.1f);
        //cam.LookAt = targetHold;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    [SerializeField] List<Collider> NonGrabbedColliders;
    [SerializeField] List<Collider> GrabbedColliders;

    public void SetNonGrabActive()
    {
        foreach (Collider item in GrabbedColliders)
        {
            item.GetComponent<Collider>().enabled = false;
        }

        foreach (Collider item in NonGrabbedColliders)
        {
            item.GetComponent<Collider>().enabled = true;
        }
    }

    public void SetGrabActive()
    {
        foreach (Collider item in NonGrabbedColliders)
        {
            item.GetComponent<Collider>().enabled = false;
        }

        foreach (Collider item in GrabbedColliders)
        {
            item.GetComponent<Collider>().enabled = true;
        }
    }
}

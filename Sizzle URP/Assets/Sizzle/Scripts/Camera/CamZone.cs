using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CamZone : MonoBehaviour
{
    [Tooltip("Parent if this cam zone is completely enveloped by the parent's zone")]
    [SerializeField] CamZone parent;
    [SerializeField] GameObject cam;

    private bool active = false;
    private CamManager manager;

    private void Start()
    {
        manager = GameObject.FindWithTag("Sizzle").GetComponent<CamManager>();
    }



    // This object can only interact with the Sizzle layer

    private void OnTriggerEnter(Collider other)
    {
        if(!active)
        {
            TrySwapCamera(manager);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(active)
        {
            // Only returns to common cam if the manager has not 
            // transistioned to another zone within this one 
            if (manager.Current == this.cam)
            {
                manager.ReturnToCommon();
                active = false;
            }
            else if (parent != null)
            {
                parent.TrySwapCamera(manager);
                active = false;
            }
        }
    }




    /// <summary>
    /// Tries to change the managers current camera 
    /// </summary>
    /// <param name="manager"></param>
    public void TrySwapCamera(CamManager manager)
    {
        manager.ChangeCam(cam);
        active = true;
    }
}

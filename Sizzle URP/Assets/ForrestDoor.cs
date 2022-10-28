using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForrestDoor : MonoBehaviour
{
    [SerializeField] Transform teleport;
    [SerializeField] Transform Sizzle;
    [SerializeField] float radius; 
    
    void Update()
    {
        EnterArea(Sizzle);
        if (Physics.SphereCast(p1, charCtrl.height / 2, transform.forward, out hit, 10))
        {

        }
    }

    private void EnterArea(Transform Sizzle) { Sizzle.position = teleport.position; }
 
}

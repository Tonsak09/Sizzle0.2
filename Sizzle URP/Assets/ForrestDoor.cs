using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForrestDoor : MonoBehaviour
{
    [SerializeField] Transform teleport;
    
    private void EnterArea(Transform Sizzle) { Sizzle.position = teleport.position; }
 
}

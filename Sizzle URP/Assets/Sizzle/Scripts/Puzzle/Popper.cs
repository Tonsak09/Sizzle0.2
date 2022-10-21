using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        LargeSlime ls = other.GetComponent<LargeSlime>();
        if (ls != null)
        {
            ls.Pop();
        }

    }
}

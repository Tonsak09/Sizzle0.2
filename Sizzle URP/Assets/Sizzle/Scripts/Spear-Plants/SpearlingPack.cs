using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearlingPack : MonoBehaviour
{
    [SerializeField] List<Spearling> spearlings;
    [Header("Distraction")]
    [SerializeField] float distractionCoolDown;
    [SerializeField] float disCheckRange;
    [SerializeField] LayerMask disCheckLayers;
    [Space]
    [SerializeField] float maxDistractionTime;
    [SerializeField] float distractionLingerTime;

    private float disTimer;
    private Transform distractionRef;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(distractionRef == null)
        {
            if (disTimer <= 0)
            {
                CheckForDistraction();
                disTimer = distractionCoolDown;
            }
            else
            {
                disTimer -= Time.deltaTime;
            }
        }
    }

    private void CheckForDistraction()
    {
        // Check is a distraction is around
        Collider[] distractionChecks = Physics.OverlapSphere(this.transform.position, disCheckRange, disCheckLayers);

        for (int i = 0; i < distractionChecks.Length; i++)
        {
            if (distractionChecks[i].GetComponent<ChargeObj>() != null)
            {
                // Distraction has been found 
                distractionRef = distractionChecks[i].transform;

                for (int j = 0; j < spearlings.Count; j++)
                {
                    spearlings[j].SetDistraction(distractionRef, maxDistractionTime, distractionLingerTime);
                }

                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(this.transform.position, disCheckRange);
    }
}

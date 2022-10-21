using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseCopyHeightToBuoyancy : PoseCopy
{
    [Space]
    [SerializeField] Buoyancy midBuoyancy;
    [SerializeField] float midDefaultHeight;
    private float midBuoyancyHeightHold;

    // Start is called before the first frame update
    void Start()
    {
        midBuoyancyHeightHold = midBuoyancy.Height;
    }

    /// <summary>
    /// This one doesn't actually use the quaternion, just changes the buoyancies height based on this items
    /// height 
    /// </summary>
    public override void UpdateTarget()
    {
        print(this.transform.position.y / midDefaultHeight);
        midBuoyancy.Height = midBuoyancyHeightHold * (this.transform.localPosition.y / midDefaultHeight);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizzleDataLoader : MonoBehaviour
{
    [Tooltip("If you want Sizzle to start at a different save point rather than what is saved ")]
    [SerializeField] SavePoint savePointOverride;
    [SerializeField] Transform baseJoint;
    // Start is called before the first frame update
    void Start()
    {
        if(savePointOverride == null)
        {
            SetSizzleToSaveTransform();
        }
        else
        {
            savePointOverride.SaveTransform();
            SetSizzleToSaveTransform();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetSizzleToSaveTransform()
    {
        baseJoint.transform.position = GameData.SizzleSavePos;
        baseJoint.transform.rotation = GameData.SizzleSaveOrientation;
    }
}

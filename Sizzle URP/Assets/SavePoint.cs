using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    /// <summary>
    /// Saves this objects transform as Sizzle's transform data in GameData
    /// </summary>
    public void SaveTransform()
    {
        GameData.SetSizzleSavePos(this.transform.position);
        GameData.SetSizzleSaveorientation(this.transform.rotation);
    }

    private void OnTriggerEnter(Collider other)
    {
        SaveTransform();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game singleton that holds general data that any class
/// can access
/// </summary>
public static class GameData
{
    private static Vector3 sizzleSavePos;
    private static Quaternion sizzleOrientation;

    public static Vector3 SizzleSavePos { get { return sizzleSavePos; } }
    public static Quaternion SizzleSaveOrientation { get { return sizzleOrientation; } }

    public static void SetSizzleSavePos(Vector3 pos)
    {
        sizzleSavePos = pos;
    }

    public static void SetSizzleSaveorientation(Quaternion rot)
    {
        sizzleOrientation = rot;
    }
}

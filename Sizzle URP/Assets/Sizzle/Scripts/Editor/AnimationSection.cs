using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Section", menuName = "Data/New Section", order = 1)]
public class AnimationSection : ScriptableObject
{
    public string key;
    List<Transform> bones;
}

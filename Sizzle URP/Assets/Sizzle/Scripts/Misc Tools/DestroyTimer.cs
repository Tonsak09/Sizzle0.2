using UnityEngine;
public class DestroyTimer : MonoBehaviour
{
    public float time;
    void Start()
    {
        Destroy(this.gameObject, time);
    }
}

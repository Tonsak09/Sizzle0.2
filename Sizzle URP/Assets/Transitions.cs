using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Transitions : MonoBehaviour
{
    [SerializeField] RectTransform border;
    [SerializeField] RectTransform blackOut;

    [SerializeField] float blackOutSpeed;
    [SerializeField] AnimationCurve blackOutCurve;
    [SerializeField] float blackInSpeed;
    [SerializeField] AnimationCurve blackInCurve;
    [SerializeField] Vector2 startWHB;
    [SerializeField] Vector2 targetWHB;

    private Coroutine animCo;

    // Start is called before the first frame update
    void Start()
    {
        print(blackOut.sizeDelta);
        TryBlackIn();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TryBlackOut()
    {
        if(animCo == null)
        {
            animCo = StartCoroutine(BlackOut());
        }
    }

    public void TryBlackIn()
    {
        if (animCo == null)
        {
            animCo = StartCoroutine(BlackIn());
        }
    }

    private IEnumerator BlackOut()
    {
        float lerp = 0; 

        while (lerp < 1)
        {
            blackOut.sizeDelta = Vector2.Lerp(startWHB, targetWHB, blackOutCurve.Evaluate(lerp));

            lerp += blackOutSpeed *Time.deltaTime;
            yield return null;
        }

        blackOut.sizeDelta = targetWHB;
        animCo = null;
    }

    private IEnumerator BlackIn()
    {
        float lerp = 0;

        while (lerp < 1)
        {
            blackOut.sizeDelta = Vector2.Lerp(targetWHB, startWHB, blackInCurve.Evaluate(lerp));

            lerp += blackInSpeed * Time.deltaTime;
            yield return null;
        }

        blackOut.sizeDelta = startWHB;
        animCo = null;
    }
}

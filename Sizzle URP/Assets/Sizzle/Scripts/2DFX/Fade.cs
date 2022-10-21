using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{

    public void FadeAndResetLevel()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        Image fade = this.GetComponent<Image>();
        float lerp = 1;
        while (lerp >= 0)
        {
            fade.color = Color.Lerp(Color.black, new Color(0, 0, 0, 0), lerp);

            lerp -= Time.deltaTime;
            yield return null;
        }
        LevelManager.Reload();
    }
}

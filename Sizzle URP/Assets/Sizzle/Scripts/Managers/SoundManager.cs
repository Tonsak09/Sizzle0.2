using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] GameObject soundSourceReference;

    private Dictionary<string, LinkedList<GameObject>> effects;
    public float soundMultiplier;

    public float SoundMultiplier { get { return soundMultiplier; } set { soundMultiplier = value; } }

    private void Start()
    {
        effects = new Dictionary<string, LinkedList<GameObject>>();

        SoundMultiplier = 1;
    }

    public void PlaySoundFX(AudioClip sound, Vector3 pos, string category, float pitch = 1, float volume = 1, int maxSounds = 3)
    {
        if(!effects.ContainsKey(category))
        {
            effects.Add(category, new LinkedList<GameObject>());
        }

        // Makes sure there is a limit to how much sound 
        if(effects[category].Count < maxSounds)
        {
            GameObject temp = Instantiate(soundSourceReference, pos, Quaternion.identity);

            AudioSource source = temp.GetComponent<AudioSource>();

            source.clip = sound;
            source.pitch = pitch;
            source.volume = volume * 0.5f * soundMultiplier;
            source.Play();

            effects[category].AddLast(temp);

            StartCoroutine(SoundCoroutine(temp, category, sound.length));
        }
    }

    public void PlaySoundFXAfterDelay(AudioClip sound, Vector3 pos, string category, float delay, float pitch = 1, float volume = 1)
    {
        GameObject temp = Instantiate(soundSourceReference, pos, Quaternion.identity);

        AudioSource source = temp.GetComponent<AudioSource>();

        source.clip = sound;
        source.pitch = pitch;
        source.volume = volume * 0.5f * soundMultiplier;
        source.Play();

        StartCoroutine(SoundDelayCoroutineSoundCoroutine(temp, category, sound.length, delay));
    }

    private IEnumerator SoundCoroutine(GameObject obj, string category, float time)
    {
        yield return new WaitForSeconds(time);
        if(effects.ContainsKey(category))
        {
            effects[category].Remove(obj);
        }
        Destroy(obj);
    }

    private IEnumerator SoundDelayCoroutineSoundCoroutine(GameObject obj, string category, float time, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(SoundCoroutine(obj, category, time));
    }
}

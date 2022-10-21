using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOnParticleDeath : MonoBehaviour
{

    [SerializeField] AudioClip[] OnBirthSounds;
    [SerializeField] AudioClip[] OnDeathSounds;

    [SerializeField] float volume;
    [SerializeField] int maxNumBirth;
    [SerializeField] int maxNumDeath;


    private AudioClip onBirthSound { get { if (OnBirthSounds.Length == 0) { return null; } return OnBirthSounds[Random.Range(0, OnBirthSounds.Length)]; } }
    private AudioClip onDeathSound { get { if (OnDeathSounds.Length == 0) { return null; } return OnDeathSounds[Random.Range(0, OnDeathSounds.Length)]; } }

    [SerializeField] float pitchMax;
    [SerializeField] float pitchMin;

    [SerializeField] string category;

    private ParticleSystem ps;
    private SoundManager sm;
    private int numbOfParticles;

    private void Start()
    {
        ps = this.GetComponent<ParticleSystem>();
        sm = GameObject.FindObjectOfType<SoundManager>();
    }

    private void Update()
    {
        int count = ps.particleCount;

        if (count < numbOfParticles && onDeathSound != null)
        { //particle has died
            sm.PlaySoundFX(onDeathSound, this.transform.position, category, Random.Range(pitchMin, pitchMax), volume, maxNumDeath);
        }
        else if (count > numbOfParticles && onBirthSound != null)
        { //particle has been born
            sm.PlaySoundFX(onBirthSound, this.transform.position, category, Random.Range(pitchMin, pitchMax), volume, maxNumBirth);
        }
        numbOfParticles = count;
    }
}

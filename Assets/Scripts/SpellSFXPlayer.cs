using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellSFXPlayer : MonoBehaviour
{
    private AudioSource audio;
    public AudioClip audioControlSpell;
    public AudioClip audioSlowDownSpell;
    public AudioClip audioVacuumSpell;

    void Awake()
    {
        audio = GetComponent<AudioSource>();
    }

    public void PlayControlSpell() {
        audio.PlayOneShot(audioControlSpell);
    }

    public void PlaySlowDownSpell() {
        audio.PlayOneShot(audioSlowDownSpell);
    }

    public void PlayVacuumSpell() {
        audio.PlayOneShot(audioVacuumSpell);
    }
}

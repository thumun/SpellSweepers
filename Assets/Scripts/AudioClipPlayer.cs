using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioClipPlayer : MonoBehaviour
{
    private AudioSource audio;

    public AudioClip audioClip;

    public bool loopAudio = false;

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
        audio.clip = audioClip;
        audio.loop = loopAudio;

        if (loopAudio) {
            audio.Play();
        }
    }

    public void PlayClip() {
        audio.PlayOneShot(audioClip);
    }

    public void StopClip() {
        audio.Stop();
    }

    public void ChangeVolume(float newVol)
    {
        AudioListener.volume = newVol;
    }
}

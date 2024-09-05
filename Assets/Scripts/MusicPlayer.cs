using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer player;

    AudioSource audioSource;

    public AudioClip[] ac;

    void Awake()
    {
        if (player == null)
        {
            player = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (player != this)
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float startVolume = audioSource.volume;

        yield return new WaitForSeconds(1.0f);

        while (audioSource.volume < 1.0f)
        {
            audioSource.volume += Time.deltaTime / 3.0f;
        }

        audioSource.volume = 1.0f;
    }

    public void playAudio(string audioName)
    {
        switch (audioName)
        {
            case "boxBreak":
                audioSource.PlayOneShot(ac[0]);
                break;
            case "checkout":
                audioSource.PlayOneShot(ac[1]);
                break;
            case "collect":
                audioSource.PlayOneShot(ac[2]);
                break;
            case "enemyDie":
                audioSource.PlayOneShot(ac[3]);
                break;
            case "powerUp":
                audioSource.PlayOneShot(ac[4]);
                break;
            case "pushing":
                audioSource.PlayOneShot(ac[5]);
                break;

            default:
                Debug.LogWarning("Music String Not Found");
                break;
        }
    }
}

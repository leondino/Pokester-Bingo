using Coffee.UIExtensions;
using System.Collections.Generic;
using UnityEngine;

public class RoundCelebration : MonoBehaviour
{
    [SerializeField]
    private List<AudioClip> correctSounds = new List<AudioClip>();
    [SerializeField]
    private List<AudioClip> wrongSounds = new List<AudioClip>();

    private AudioSource celebrationAudio;
    private UIParticle particles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        celebrationAudio = GetComponent<AudioSource>();

        particles = GetComponent<UIParticle>();
    }

    public void CorrectCelebration()
    {
        // Play a random correct sound
        int randomIndex = Random.Range(0, correctSounds.Count);
        AudioClip selectedClip = correctSounds[randomIndex];
        celebrationAudio.clip = selectedClip;
        celebrationAudio.Play();

        particles.Play();
    }

    public void WrongCelebration()
    {
        // Play a random wrong sound
        int randomIndex = Random.Range(0, wrongSounds.Count);
        AudioClip selectedClip = wrongSounds[randomIndex];
        celebrationAudio.clip = selectedClip;
        celebrationAudio.Play();
    }
}

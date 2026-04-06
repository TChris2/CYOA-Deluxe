using System.Collections;
using UnityEngine;

// Controls the popup's sfx of the letter in the finale unlock screen
public class LetterUnlockScreen : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] sfx;
    [SerializeField]
    private ParticleSystem particleExplode;
    [SerializeField]
    private float sfxDelay;
    [SerializeField]
    private Canvas letterIcon;
    Animator animator;
    AudioSource audioSource;
    ParticleSystemForceField forceField;

    void Awake()
    {
        animator = GetComponentInParent<Animator>();
        audioSource = GetComponent<AudioSource>();
        forceField = GetComponentInChildren<ParticleSystemForceField>();
    }
    
    // Plays the sfx after the object is enabled
    void OnEnable()
    {
        StartCoroutine(PlaySFX());
    }

    // Plays the sfx animation
    IEnumerator PlaySFX()
    {
        forceField.enabled = true;
        letterIcon.overrideSorting = true;

        yield return new WaitForSeconds(.2f);

        // Plays charging sfx
        audioSource.clip = sfx[0];
        audioSource.Play();

        yield return new WaitForSeconds(particleExplode.main.startDelay.constant + sfxDelay);

        // Plays explosion sfx
        audioSource.clip = sfx[1];
        audioSource.Play();
        // Disables force field to prevent it from interferring with explosion particles
        forceField.enabled = false;

        yield return new WaitForSeconds(.1f);

        // Puts letter icon above particle layer when playing popup animation
        letterIcon.sortingOrder = 2;

        // Plays popup animation
        animator.Play("Letter Popup");

        yield return new WaitForSeconds(particleExplode.main.duration + particleExplode.main.startLifetime.constantMax);
        
        // Sets it back to the regular layer after animation plays
        letterIcon.overrideSorting = false;
    }
}

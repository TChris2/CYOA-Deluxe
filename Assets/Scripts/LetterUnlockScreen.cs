using System.Collections;
using UnityEngine;

public class LetterUnlockScreen : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] sfx;
    [SerializeField]
    private ParticleSystem particleExplode;
    [SerializeField]
    private float sfxDelay;
    [SerializeField]
    private Canvas canvas;
    
    void OnEnable()
    {
        StartCoroutine(PlaySFX());
    }

    IEnumerator PlaySFX()
    {
        AudioSource audioSource = GetComponent<AudioSource>();

        yield return new WaitForSeconds(.2f);

        audioSource.clip = sfx[0];
        audioSource.Play();

        yield return new WaitForSeconds(particleExplode.main.startDelay.constant + sfxDelay);

        audioSource.clip = sfx[1];
        audioSource.Play();
        GetComponentInChildren<ParticleSystemForceField>().enabled = false;

        yield return new WaitForSeconds(.1f);
        canvas.sortingOrder = 2;

        Animator animator = GetComponentInParent<Animator>();
        animator.Play("Letter Popup");

        yield return new WaitForSeconds(particleExplode.main.duration + particleExplode.main.startLifetime.constantMax);
        
        canvas.sortingOrder = 1;
    }
}

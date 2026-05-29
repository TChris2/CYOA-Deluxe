using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System;

/// <summary>
/// Handles transitions
/// </summary>
public class TransitionManager : MonoBehaviour
{
    private const float fadeStartDelayDefault = 0f;
    public float fadeStartDelay = 0f;
    private const float fadeDurationDefault = 1.5f;
    public float fadeDuration = 1.5f;
    private const float actionDelayDefault = 0f;
    public float actionDelay = 0f;
    public bool isTransitioning;
    private int _fadeAmt = Shader.PropertyToID("_FadeAmt");
    private int _UseShutters = Shader.PropertyToID("_UseShutters");
    private int _UseRadialWipe = Shader.PropertyToID("_UseRadialWipe");
    private int _UsePlainBlack = Shader.PropertyToID("_UsePlainBlack");
    private int? _lastEffect;
    private Image _image;
    private Material _material;
    public Action onTransition;

    void Start()
    {
        _image = GetComponent<Image>();
        _image.enabled = false;
        Material mat = _image.material;
        _image.material = new Material(mat);
        _material = _image.material;

        _lastEffect = _UsePlainBlack;
    }

    /// <summary>
    /// Fades out transitions
    /// </summary>
    public void FadeOut(FadeType fadeType)
    {
        ChangeFadeEffect(fadeType);
        StartFadeOut();
    }

    /// <summary>
    /// Fades in transitions
    /// </summary>
    public void FadeIn(FadeType fadeType)
    {
        ChangeFadeEffect(fadeType);
        StartFadeIn();
    }

    /// <summary>
    /// Updates fade effect
    /// </summary>
    void ChangeFadeEffect(FadeType fadeType)
    {
        if(_lastEffect.HasValue)
        {
            _material.SetFloat(_lastEffect.Value, 0f);
        }
        
        switch (fadeType)
        {
            case FadeType.Shutters:
                SwitchEffect(_UseShutters);
                break;
            case FadeType.RadialWipe:
                SwitchEffect(_UseRadialWipe);
                break;
            case FadeType.PlainBlack:
                SwitchEffect(_UsePlainBlack);
                break;
        }
    }   

    /// <summary>
    /// Switches effect
    /// </summary>
    void SwitchEffect(int effectToTurnOn)
    {
        _material.SetFloat(effectToTurnOn, 0f);

        _lastEffect = effectToTurnOn;
    }
    
    /// <summary>
    /// Starts fade out
    /// </summary>
    void StartFadeOut()
    {
        _material.SetFloat(_fadeAmt, 0f);

        StartCoroutine(HandleFade(1f, 0f));
    }

    /// <summary>
    /// Starts fade in
    /// </summary>
    void StartFadeIn()
    {
        _material.SetFloat(_fadeAmt, 1f);

        StartCoroutine(HandleFade(0f, 1f));
    }

    /// <summary>
    /// Handles transition
    /// </summary>
    IEnumerator HandleFade(float targetAmt, float startAmt)
    {
        _image.enabled = true;
        isTransitioning = true;

        float elaspedTime = 0f;

        // Debug.Log($"TransitionManager: fadeStartDelay {fadeStartDelay}");

        float lerpedAmt = Mathf.Lerp(startAmt, targetAmt, (elaspedTime / fadeDuration));
        _material.SetFloat(_fadeAmt, lerpedAmt);
        if (fadeStartDelay > 0)
            yield return new WaitForSecondsRealtime(fadeStartDelay);

        while (elaspedTime < fadeDuration)
        {
            elaspedTime += Time.unscaledDeltaTime;

            lerpedAmt = Mathf.Lerp(startAmt, targetAmt, (elaspedTime / fadeDuration));
            _material.SetFloat(_fadeAmt, lerpedAmt);
            // Debug.Log($"TransitionManager: elaspedTime {elaspedTime} lerpedAmt {lerpedAmt}");
            yield return null;
        }

        _material.SetFloat(_fadeAmt, targetAmt);

        // Debug.Log($"TransitionManager: actionDelay {actionDelay}");

        if (actionDelay > 0)
            yield return new WaitForSecondsRealtime(actionDelay);

        // Resets back to defaults
        fadeStartDelay = fadeStartDelayDefault;
        fadeDuration = fadeDurationDefault;
        actionDelay = actionDelayDefault;

        if (onTransition != null)
        {
            onTransition?.Invoke();
            onTransition = null;
        }
    }

    /// <summary>
    /// Ends transition
    /// </summary>
    public void EndTransition()
    {
        _image.enabled = false;
        isTransitioning = false;
    }

    /// <summary>
    /// Changes scenes
    /// </summary>
    public void ChangeScene(string nextScene)
    {
        SceneManager.LoadScene(nextScene);
    }
}

public enum FadeType
{
    Shutters,
    RadialWipe,
    PlainBlack
}

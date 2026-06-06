using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Light))]
public class FlashlightController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.6f;
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float flickerStrength = 0.08f;

    private Light _light;
    private InputAction _toggleAction;
    private bool _isOn = true;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _light = GetComponent<Light>();
        maxIntensity = _light.intensity;

        _toggleAction = new InputAction(binding: "<Keyboard>/f");
        _toggleAction.performed += _ => Toggle();
    }

    private void OnEnable() => _toggleAction.Enable();
    private void OnDisable() => _toggleAction.Disable();

    private void Toggle()
    {
        _isOn = !_isOn;

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(_isOn ? FadeIn() : FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float start = _light.intensity;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            float curve = 1f - (t * t);
            float flicker = 1f + Random.Range(-flickerStrength, flickerStrength);
            _light.intensity = start * curve * flicker;
            yield return null;
        }

        _light.intensity = 0f;
    }

    private IEnumerator FadeIn()
    {
        float start = _light.intensity;
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            // curva que sobe rápido no início e estabiliza no final
            float curve = 1f - (1f - t) * (1f - t);
            float flicker = 1f + Random.Range(-flickerStrength * 0.5f, flickerStrength * 0.5f);
            _light.intensity = Mathf.Lerp(start, maxIntensity, curve) * flicker;
            yield return null;
        }

        _light.intensity = maxIntensity;
    }
}

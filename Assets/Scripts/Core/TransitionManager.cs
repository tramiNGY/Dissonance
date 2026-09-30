using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Manage Scene transition effects.
/// </summary>
public class TransitionManager : MonoBehaviour
{
    // Inspector
    [SerializeField] private float _fadeSpeed = 1f; // Alpha transition speed (1 = 1 second from 0 to 1).

    // Fields
    private Image _fadeImage;
    private float _currentAlpha;
    private bool _fadeInProgress;
    private bool _fadingIn;

    // Unity Lifecycle
    void Awake()
    {
        _fadeImage = GetComponentInChildren<Image>();
    }

    void Update()
    {
        // Check if a fade is in progress.
        if (!_fadeInProgress)
        {
            return;
        }
        // Fade-In
        if (_fadingIn)
        {
            _currentAlpha += _fadeSpeed * Time.deltaTime; // Time.deltaTime permits to base fadeSpeed on actual time instead of framerate.
        }
        // Fade-Out
        else
        {
            _currentAlpha -= _fadeSpeed * Time.deltaTime;
        }
        _currentAlpha = Mathf.Clamp(_currentAlpha, 0, 1); // Keep Alpha between 0 and 1 if the increment or decrement exceeds value range.
        SetAlpha(_currentAlpha);
        
        // Fade End
        if (_currentAlpha == 0 || _currentAlpha == 1)
        {
            _fadeInProgress = false;
        }
    }

    // Methods
    /// <summary>
    /// Set alpha value of fadeImage Color.
    /// </summary>
    /// <param name="alpha">color transparency parameter: fully transparent 0, opaque: 1.</param>
    private void SetAlpha(float alpha)
    {
        Color currentColor = _fadeImage.color; // color is a struct, cannot modify directly its group of values (r, g, b, a), this creates a copy.
        currentColor.a = alpha; // Modify alpha value of the copy.
        _fadeImage.color = currentColor; // Assign modified color to fadeImage.
    }

    /// <summary>
    /// Start Fade-in transition (transparent to opaque).
    /// </summary>
    public void StartFadeIn()
    {
        Debug.Log("StartFade-In called");
        _currentAlpha = 0;
        SetAlpha(_currentAlpha);
        _fadingIn = true;
        _fadeInProgress = true;
    }

    /// <summary>
    /// Start Fade-out transition (opaque to transparent).
    /// </summary>
    public void StartFadeOut()
    {
        Debug.Log("StartFade-Out called");
        _currentAlpha = 1;
        SetAlpha(_currentAlpha);
        _fadingIn = false;
        _fadeInProgress = true;
    }
}

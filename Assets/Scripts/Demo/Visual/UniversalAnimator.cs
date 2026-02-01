using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UniversalAnimator : MonoBehaviour
{
    // Animate ANYTHING using this script

    private SpriteRenderer _spriteRenderer;
    public SpriteRenderer SpriteRenderer => _spriteRenderer;

    private Image _image;
    public Image Image => _image;

    private TMP_Text _text;
    public TMP_Text Text => _text;

    private RectTransform _rect;
    public RectTransform Rect => _rect;

    private const string GlitchCharacters = "!@#$%^&*()_+-=[]{}|;':\",./<>?ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    [SerializeField] private ParticleSystem[] _particles;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _image = GetComponent<Image>();
        _text = GetComponent<TMP_Text>();
        _rect = GetComponent<RectTransform>();
    }

    public void PlayParticles()
    {
        if (_particles != null)
        {
            foreach (ParticleSystem particle in _particles)
            {
                particle.Play();
            }
        }
    }

    // ================= POSITION ANIMATIONS =================

    public void Animate(Vector2 endPosition, float speed)
    {
        StartCoroutine(TransformPosition(endPosition, speed));
    }

    public void Animate(Vector2 endPosition, float speed, bool isNeedToDestroyAtEnd)
    {
        if (!isNeedToDestroyAtEnd) StartCoroutine(TransformPosition(endPosition, speed));
        else StartCoroutine(TransformPositionAndDestroy(endPosition, speed));
    }

    public void AnimateWithOffset(Vector2 offset, float speed, bool isNeedToDestroyAtEnd)
    {
        Vector2 endPosition = (Vector2)transform.position + offset;
        Animate(endPosition, speed, isNeedToDestroyAtEnd);
    }

    public IEnumerator TransformPosition(Vector2 endPosition, float speed)
    {
        Vector2 startPosition = transform.position;
        float distance = Vector2.Distance(startPosition, endPosition);
        float duration = distance / speed;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = t * t;
            transform.position = Vector2.Lerp(startPosition, endPosition, easedT);
            yield return null;
        }

        transform.position = endPosition;
    }

    public IEnumerator TransformPositionAndDestroy(Vector2 endPosition, float speed)
    {
        yield return TransformPosition(endPosition, speed);
        Destroy(gameObject);
    }

    public void Animate(Vector3 endPosition, float speed)
    {
        StartCoroutine(TransformPosition(endPosition, speed));
    }

    public void Animate(Vector3 endPosition, float speed, bool isNeedToDestroyAtEnd)
    {
        if (!isNeedToDestroyAtEnd) StartCoroutine(TransformPosition(endPosition, speed));
        else StartCoroutine(TransformPositionAndDestroy(endPosition, speed));
    }

    public void AnimateWithOffset(Vector3 offset, float speed, bool isNeedToDestroyAtEnd)
    {
        Vector3 endPosition = transform.position + offset;
        Animate(endPosition, speed, isNeedToDestroyAtEnd);
    }

    public IEnumerator TransformPosition(Vector3 endPosition, float speed)
    {
        Vector3 startPosition = transform.position;
        float distance = Vector3.Distance(startPosition, endPosition);
        float duration = distance / speed;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = t * t;
            transform.position = Vector3.Lerp(startPosition, endPosition, easedT);
            yield return null;
        }

        transform.position = endPosition;
    }

    public IEnumerator TransformPositionAndDestroy(Vector3 endPosition, float speed)
    {
        yield return TransformPosition(endPosition, speed);
        Destroy(gameObject);
    }

    // ================= SPRITE SIZE ANIMATIONS =================

    public void AnimateSpriteSize(Vector2 targetSize, float duration, Action onComplete = null)
    {
        StartCoroutine(AnimateSpriteSizeCoroutine(_spriteRenderer, targetSize, duration, onComplete));
    }

    public void AnimateSpriteSize(Vector2 startSize, Vector2 targetSize, float duration, Action onComplete = null)
    {
        _spriteRenderer.size = startSize;
        StartCoroutine(AnimateSpriteSizeCoroutine(_spriteRenderer, targetSize, duration, onComplete));
    }

    private IEnumerator AnimateSpriteSizeCoroutine(SpriteRenderer sr, Vector2 targetSize, float duration, Action onComplete)
    {
        Vector2 startSize = sr.size;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = EaseOutQuad(t);
            sr.size = Vector2.Lerp(startSize, targetSize, easedT);
            yield return null;
        }

        sr.size = targetSize;
        onComplete?.Invoke();
    }

    public void AnimateSpriteSizeWithOvershoot(Vector2 targetSize, float duration, float overshootFactor, Action onComplete = null)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        StartCoroutine(AnimateSpriteSizeWithOvershootCoroutine(sr, targetSize, duration, overshootFactor, 0f, onComplete));
    }

    public void AnimateSpriteSizeWithOvershoot(Vector2 startSize, Vector2 targetSize, float duration, float overshootFactor, Action onComplete = null)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.size = startSize;
        StartCoroutine(AnimateSpriteSizeWithOvershootCoroutine(sr, targetSize, duration, overshootFactor, 0f, onComplete));
    }

    public void AnimateSpriteSizeWithOvershoot(Vector2 startSize, Vector2 targetSize, float duration, float overshootFactor, float timeBeforeAnimation, Action onComplete = null)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.size = startSize;
        StartCoroutine(AnimateSpriteSizeWithOvershootCoroutine(sr, targetSize, duration, overshootFactor, timeBeforeAnimation, onComplete));
    }

    private IEnumerator AnimateSpriteSizeWithOvershootCoroutine(SpriteRenderer sr, Vector2 targetSize, float duration, float overshootFactor, float timeBeforeAnimation, Action onComplete)
    {
        Vector2 startSize = sr.size;
        float elapsedTime = 0f;

        if (timeBeforeAnimation > 0) yield return new WaitForSeconds(timeBeforeAnimation);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = EaseOutBack(t, overshootFactor);
            sr.size = Vector2.LerpUnclamped(startSize, targetSize, easedT);
            yield return null;
        }

        sr.size = targetSize;
        onComplete?.Invoke();
    }

    // ================= IMAGE SIZE ANIMATIONS (UI) =================

    public void AnimateImageSize(Vector2 targetSize, float duration, Action onComplete = null)
    {
        StartCoroutine(AnimateImageSizeCoroutine(targetSize, duration, onComplete));
    }

    public void AnimateImageSize(Vector2 startSize, Vector2 targetSize, float duration, Action onComplete = null)
    {
        _rect.sizeDelta = startSize;
        StartCoroutine(AnimateImageSizeCoroutine(targetSize, duration, onComplete));
    }

    private IEnumerator AnimateImageSizeCoroutine(Vector2 targetSize, float duration, Action onComplete)
    {
        Vector2 startSize = _rect.sizeDelta;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = EaseOutQuad(t);
            _rect.sizeDelta = Vector2.Lerp(startSize, targetSize, easedT);
            yield return null;
        }

        _rect.sizeDelta = targetSize;
        onComplete?.Invoke();
    }

    public void AnimateImageSizeWithOvershoot(Vector2 targetSize, float duration, float overshootFactor = 1.2f, Action onComplete = null)
    {
        StartCoroutine(AnimateImageSizeWithOvershootCoroutine(targetSize, duration, overshootFactor, 0f, onComplete));
    }

    public void AnimateImageSizeWithOvershoot(Vector2 startSize, Vector2 targetSize, float duration, float overshootFactor, Action onComplete = null)
    {
        _rect.sizeDelta = startSize;
        StartCoroutine(AnimateImageSizeWithOvershootCoroutine(targetSize, duration, overshootFactor, 0f, onComplete));
    }

    public void AnimateImageSizeWithOvershoot(Vector2 startSize, Vector2 targetSize, float duration, float overshootFactor, float timeBeforeAnimation, Action onComplete = null)
    {
        _rect.sizeDelta = startSize;
        StartCoroutine(AnimateImageSizeWithOvershootCoroutine(targetSize, duration, overshootFactor, timeBeforeAnimation, onComplete));
    }

    private IEnumerator AnimateImageSizeWithOvershootCoroutine(Vector2 targetSize, float duration, float overshootFactor, float timeBeforeAnimation, Action onComplete)
    {
        Vector2 startSize = _rect.sizeDelta;
        float elapsedTime = 0f;

        if (timeBeforeAnimation > 0) yield return new WaitForSeconds(timeBeforeAnimation);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = EaseOutBack(t, overshootFactor);
            _rect.sizeDelta = Vector2.LerpUnclamped(startSize, targetSize, easedT);
            yield return null;
        }

        _rect.sizeDelta = targetSize;
        onComplete?.Invoke();
    }

    // ================= IMAGE FILL ANIMATIONS =================

    public void AnimateImageFill(float targetFill, float duration, Action onComplete = null)
    {
        StartCoroutine(AnimateImageFillCoroutine(targetFill, duration, onComplete));
    }

    public void AnimateImageFill(float startFill, float targetFill, float duration, Action onComplete = null)
    {
        _image.fillAmount = startFill;
        StartCoroutine(AnimateImageFillCoroutine(targetFill, duration, onComplete));
    }

    private IEnumerator AnimateImageFillCoroutine(float targetFill, float duration, Action onComplete)
    {
        float startFill = _image.fillAmount;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = EaseOutQuad(t);
            _image.fillAmount = Mathf.Lerp(startFill, targetFill, easedT);
            yield return null;
        }

        _image.fillAmount = targetFill;
        onComplete?.Invoke();
    }

    // ================= SCALE ANIMATIONS =================

    public void AnimateScale(Vector3 targetScale, float duration, Action onComplete = null)
    {
        StartCoroutine(AnimateScaleCoroutine(targetScale, duration, onComplete));
    }

    public void AnimateScale(Vector3 startScale, Vector3 targetScale, float duration, Action onComplete = null)
    {
        transform.localScale = startScale;
        StartCoroutine(AnimateScaleCoroutine(targetScale, duration, onComplete));
    }

    public void AnimateScaleWithOvershoot(Vector3 targetScale, float duration, float overshootFactor, Action onComplete = null)
    {
        StartCoroutine(AnimateScaleWithOvershootCoroutine(targetScale, duration, overshootFactor, onComplete));
    }

    public void AnimateScaleWithOvershoot(Vector3 startScale, Vector3 targetScale, float duration, float overshootFactor, Action onComplete = null)
    {
        transform.localScale = startScale;
        StartCoroutine(AnimateScaleWithOvershootCoroutine(targetScale, duration, overshootFactor, onComplete));
    }

    private IEnumerator AnimateScaleCoroutine(Vector3 targetScale, float duration, Action onComplete)
    {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = EaseOutQuad(t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, easedT);
            yield return null;
        }

        transform.localScale = targetScale;
        onComplete?.Invoke();
    }

    private IEnumerator AnimateScaleWithOvershootCoroutine(Vector3 targetScale, float duration, float overshootFactor, Action onComplete)
    {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = EaseOutBack(t, overshootFactor);
            transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, easedT);
            yield return null;
        }

        transform.localScale = targetScale;
        onComplete?.Invoke();
    }

    // ================= POSITION SETTERS =================

    public void SetPosition(Vector2 position)
    {
        transform.position = position;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    // ================= COLOR INTERPOLATION =================

    public void InterpolateColor(Color endColor, float duration, Action onComplete = null)
    {
        StartCoroutine(ColorInterpolationCoroutine(endColor, duration, onComplete));
    }

    public void InterpolateColor(Color startColor, Color endColor, float duration, Action onComplete = null)
    {
        SetColor(startColor);
        StartCoroutine(ColorInterpolationCoroutine(endColor, duration, onComplete));
    }

    public void InterpolateColorWithEasing(Color endColor, float duration, EaseType easeType, Action onComplete = null)
    {
        StartCoroutine(ColorInterpolationWithEasingCoroutine(endColor, duration, easeType, onComplete));
    }

    public void InterpolateColorWithEasing(Color startColor, Color endColor, float duration, EaseType easeType, Action onComplete = null)
    {
        SetColor(startColor);
        StartCoroutine(ColorInterpolationWithEasingCoroutine(endColor, duration, easeType, onComplete));
    }

    private IEnumerator ColorInterpolationCoroutine(Color endColor, float duration, Action onComplete)
    {
        Color startColor = GetCurrentColor();
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            SetColor(Color.Lerp(startColor, endColor, t));
            yield return null;
        }

        SetColor(endColor);
        onComplete?.Invoke();
    }

    private IEnumerator ColorInterpolationWithEasingCoroutine(Color endColor, float duration, EaseType easeType, Action onComplete)
    {
        Color startColor = GetCurrentColor();
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = ApplyEasing(t, easeType);
            SetColor(Color.Lerp(startColor, endColor, easedT));
            yield return null;
        }

        SetColor(endColor);
        onComplete?.Invoke();
    }

    // ================= ALPHA/FADE ANIMATIONS =================

    public void FadeIn(float duration, Action onComplete = null)
    {
        Color current = GetCurrentColor();
        Color startColor = new Color(current.r, current.g, current.b, 0f);
        Color endColor = new Color(current.r, current.g, current.b, 1f);
        SetColor(startColor);
        StartCoroutine(ColorInterpolationCoroutine(endColor, duration, onComplete));
    }

    public void FadeOut(float duration, Action onComplete = null)
    {
        Color current = GetCurrentColor();
        Color endColor = new Color(current.r, current.g, current.b, 0f);
        StartCoroutine(ColorInterpolationCoroutine(endColor, duration, onComplete));
    }

    public void FadeTo(float targetAlpha, float duration, Action onComplete = null)
    {
        Color current = GetCurrentColor();
        Color endColor = new Color(current.r, current.g, current.b, targetAlpha);
        StartCoroutine(ColorInterpolationCoroutine(endColor, duration, onComplete));
    }

    public void FadeInOut(float fadeInDuration, float holdDuration, float fadeOutDuration, Action onComplete = null)
    {
        StartCoroutine(FadeInOutCoroutine(fadeInDuration, holdDuration, fadeOutDuration, onComplete));
    }

    private IEnumerator FadeInOutCoroutine(float fadeInDuration, float holdDuration, float fadeOutDuration, Action onComplete)
    {
        Color current = GetCurrentColor();
        Color transparent = new Color(current.r, current.g, current.b, 0f);
        Color opaque = new Color(current.r, current.g, current.b, 1f);

        SetColor(transparent);

        yield return ColorInterpolationCoroutine(opaque, fadeInDuration, null);
        yield return new WaitForSeconds(holdDuration);
        yield return ColorInterpolationCoroutine(transparent, fadeOutDuration, null);

        onComplete?.Invoke();
    }

    // ================= PULSE/BLINK ANIMATIONS =================

    public void PulseColor(Color pulseColor, float duration, int pulseCount = 1, Action onComplete = null)
    {
        StartCoroutine(PulseColorCoroutine(pulseColor, duration, pulseCount, onComplete));
    }

    private IEnumerator PulseColorCoroutine(Color pulseColor, float duration, int pulseCount, Action onComplete)
    {
        Color originalColor = GetCurrentColor();
        float pulseDuration = duration / (pulseCount * 2);

        for (int i = 0; i < pulseCount; i++)
        {
            yield return ColorInterpolationCoroutine(pulseColor, pulseDuration, null);
            yield return ColorInterpolationCoroutine(originalColor, pulseDuration, null);
        }

        onComplete?.Invoke();
    }

    public void BlinkAlpha(float minAlpha, float maxAlpha, float blinkSpeed, float duration, Action onComplete = null)
    {
        StartCoroutine(BlinkAlphaCoroutine(minAlpha, maxAlpha, blinkSpeed, duration, onComplete));
    }

    private IEnumerator BlinkAlphaCoroutine(float minAlpha, float maxAlpha, float blinkSpeed, float duration, Action onComplete)
    {
        float elapsedTime = 0f;
        Color baseColor = GetCurrentColor();

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(elapsedTime * blinkSpeed * Mathf.PI * 2) + 1f) / 2f);
            SetColor(new Color(baseColor.r, baseColor.g, baseColor.b, alpha));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        SetColor(new Color(baseColor.r, baseColor.g, baseColor.b, maxAlpha));
        onComplete?.Invoke();
    }

    // ================= COLOR HELPERS =================

    private Color GetCurrentColor()
    {
        if (_spriteRenderer != null) return _spriteRenderer.color;
        if (_image != null) return _image.color;
        if (_text != null) return _text.color;
        return Color.white;
    }

    private void SetColor(Color color)
    {
        if (_spriteRenderer != null) _spriteRenderer.color = color;
        if (_image != null) _image.color = color;
        if (_text != null) _text.color = color;
    }

    public void SetAlpha(float alpha)
    {
        Color current = GetCurrentColor();
        SetColor(new Color(current.r, current.g, current.b, alpha));
    }

    // ================= TEXT ANIMATIONS =================

    public void AppearingText(string endText, float duration)
    {
        StartCoroutine(AppearingTextCoroutine(endText, duration));
    }

    private IEnumerator AppearingTextCoroutine(string endText, float duration)
    {
        _text.text = "";
        int length = endText.Length;
        float timePerCharacter = duration / length;

        for (int i = 0; i < length; i++)
        {
            _text.text += endText[i];
            yield return new WaitForSeconds(timePerCharacter);
        }
    }

    public void TextTypingAnimation(string text, float timePerCharacter)
    {
        StartCoroutine(TextTypingAnimationCoroutine(text, timePerCharacter));
    }

    private IEnumerator TextTypingAnimationCoroutine(string text, float timePerCharacter)
    {
        _text.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            _text.text += c;

            if (char.IsWhiteSpace(c) || char.IsPunctuation(c))
                continue;

            yield return new WaitForSeconds(timePerCharacter);
        }
    }

    public void GlitchTextAnimation(string text, float duration)
    {
        StartCoroutine(GlitchTextAnimationCoroutine(text, duration));
    }

    private IEnumerator GlitchTextAnimationCoroutine(string text, float duration)
    {
        int length = text.Length;
        bool[] revealed = new bool[length];
        int revealedCount = 0;
        float revealInterval = duration / length;
        float nextRevealTime = 0f;
        float elapsedTime = 0f;
        StringBuilder sb = new StringBuilder(length);

        while (revealedCount < length)
        {
            elapsedTime += Time.deltaTime;

            while (nextRevealTime <= elapsedTime && revealedCount < length)
            {
                int index = GetRandomUnrevealedIndex(revealed);
                revealed[index] = true;
                revealedCount++;
                nextRevealTime += revealInterval;
            }

            sb.Clear();
            for (int i = 0; i < length; i++)
            {
                if (revealed[i] || char.IsWhiteSpace(text[i]))
                    sb.Append(text[i]);
                else
                    sb.Append(GlitchCharacters[Random.Range(0, GlitchCharacters.Length)]);
            }

            _text.text = sb.ToString();
            yield return null;
        }

        _text.text = text;
    }

    private int GetRandomUnrevealedIndex(bool[] revealed)
    {
        int count = 0;
        for (int i = 0; i < revealed.Length; i++)
            if (!revealed[i]) count++;

        int target = Random.Range(0, count);
        int current = 0;

        for (int i = 0; i < revealed.Length; i++)
        {
            if (!revealed[i])
            {
                if (current == target) return i;
                current++;
            }
        }

        return 0;
    }

    public void StyledTypingAnimation(string richText, float timePerCharacter)
    {
        StartCoroutine(StyledTypingCoroutine(richText, timePerCharacter));
    }

    private IEnumerator StyledTypingCoroutine(string richText, float timePerCharacter)
    {
        _text.text = richText;
        _text.ForceMeshUpdate();

        TMP_TextInfo textInfo = _text.textInfo;
        int totalVisibleCharacters = textInfo.characterCount;

        _text.maxVisibleCharacters = 0;

        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            _text.maxVisibleCharacters = i;

            if (i < totalVisibleCharacters)
            {
                char c = textInfo.characterInfo[i].character;
                if (!char.IsWhiteSpace(c) && !char.IsPunctuation(c))
                    yield return new WaitForSeconds(timePerCharacter);
            }
        }
    }

    // ================= EASING FUNCTIONS =================

    public enum EaseType
    {
        Linear,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseOutBack,
        EaseOutElastic,
        EaseOutBounce
    }

    private float ApplyEasing(float t, EaseType easeType)
    {
        return easeType switch
        {
            EaseType.Linear => t,
            EaseType.EaseInQuad => t * t,
            EaseType.EaseOutQuad => EaseOutQuad(t),
            EaseType.EaseInOutQuad => EaseInOutQuad(t),
            EaseType.EaseInCubic => t * t * t,
            EaseType.EaseOutCubic => EaseOutCubic(t),
            EaseType.EaseInOutCubic => EaseInOutCubic(t),
            EaseType.EaseOutBack => EaseOutBack(t, 1.70158f),
            EaseType.EaseOutElastic => EaseOutElastic(t),
            EaseType.EaseOutBounce => EaseOutBounce(t),
            _ => t
        };
    }

    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

    private float EaseOutBack(float t, float overshoot)
    {
        float c1 = overshoot;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private float EaseOutElastic(float t)
    {
        const float c4 = (2f * Mathf.PI) / 3f;
        return t == 0f ? 0f : t == 1f ? 1f : Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }

    private float EaseOutBounce(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (t < 1f / d1)
            return n1 * t * t;
        else if (t < 2f / d1)
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        else if (t < 2.5f / d1)
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        else
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
    }
}
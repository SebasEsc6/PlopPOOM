using UnityEngine;
using PrimeTween;

[RequireComponent(typeof(RectTransform))]
public class UIPrimeTweenAnimator : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RectTransform targetRect;
    private CanvasGroup canvasGroup;

    [Header("Configuración de Animaciones")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private Ease easeTypeEnter = Ease.OutBack;
    [SerializeField] private Ease easeTypeExit = Ease.InBack;

    [Header("Tipo de Animación")]
    [SerializeField] private AnimationType animationType = AnimationType.Fade;

    [Header("Opciones de Animación")]
    [SerializeField] private float fadeFrom = 0f;
    [SerializeField] private float fadeTo = 1f;
    [SerializeField] private float scaleFrom = 0.8f;
    [SerializeField] private float scaleTo = 1f;
    [SerializeField] private float slideDistance = 100f;
    [SerializeField] private SlideDirection slideDirection = SlideDirection.Bottom;

    private Vector2 originalPosition;
    private Sequence currentSequence;
    private bool initialized = false;

    // Enumeraciones para configurar el tipo de animación
    public enum AnimationType
    {
        Fade,
        Scale,
        Slide,
        FadeAndScale,
        FadeAndSlide
    }
    //Establece las direcciones para las animaciones de tipo Slide
    public enum SlideDirection
    {
        Top,
        Right,
        Bottom,
        Left
    }

    // Delegado para eventos de animación
    public delegate void AnimationEvent();
    public event AnimationEvent OnEnterAnimationComplete;
    public event AnimationEvent OnExitAnimationComplete;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (initialized) return;

        // Asignar el RectTransform si no está asignado
        if (targetRect == null)
            targetRect = GetComponent<RectTransform>();

        // Buscar o agregar el CanvasGroup para animaciones de fade
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Guardar la posición original
        originalPosition = targetRect.anchoredPosition;

        initialized = true;
    }

    private void OnDisable()
    {
        // Asegurarse de detener cualquier animación en curso al desactivar
        if (currentSequence.isAlive)
            currentSequence.Stop();
    }

    public void Exit()
    {
        Application.Quit();
    }


    // Prepara y ejecuta la animación de entrada
    public void PlayEnterAnimation(System.Action callback = null)
    {
        Initialize();
        // Preparar para animación
        PrepareForEntryAnimation();

        // Reproducir animación de entrada
        PlayEntryAnimationInternal(() =>
        {
            OnEnterAnimationComplete?.Invoke();
            if (callback != null) callback();
        });
    }

    /// Ejecuta la animación de salida
    public void PlayExitAnimation(System.Action callback = null)
    {
        Initialize();

        // Reproducir animación de salida
        PlayExitAnimationInternal(() =>
        {
            OnExitAnimationComplete?.Invoke();
            if (callback != null) callback();
        });
    }

    private void PrepareForEntryAnimation()
    {
        // Configurar estado inicial según el tipo de animación
        switch (animationType)
        {
            case AnimationType.Fade:
                canvasGroup.alpha = fadeFrom;
                break;

            case AnimationType.Scale:
                targetRect.localScale = new Vector3(scaleFrom, scaleFrom, 1f);
                break;

            case AnimationType.Slide:
                targetRect.anchoredPosition = GetSlideStartPosition();
                break;

            case AnimationType.FadeAndScale:
                canvasGroup.alpha = fadeFrom;
                targetRect.localScale = new Vector3(scaleFrom, scaleFrom, 1f);
                break;

            case AnimationType.FadeAndSlide:
                canvasGroup.alpha = fadeFrom;
                targetRect.anchoredPosition = GetSlideStartPosition();
                break;
        }
    }

    private Vector2 GetSlideStartPosition()
    {
        // Calcular posición inicial para animación de deslizamiento
        Vector2 offset = Vector2.zero;

        switch (slideDirection)
        {
            case SlideDirection.Top:
                offset = new Vector2(0, slideDistance);
                break;
            case SlideDirection.Right:
                offset = new Vector2(slideDistance, 0);
                break;
            case SlideDirection.Bottom:
                offset = new Vector2(0, -slideDistance);
                break;
            case SlideDirection.Left:
                offset = new Vector2(-slideDistance, 0);
                break;
        }

        return originalPosition + offset;
    }

    private Vector2 GetSlideEndPosition()
    {
        // Para la animación de salida, se invierte la dirección
        Vector2 offset = Vector2.zero;

        switch (slideDirection)
        {
            case SlideDirection.Top:
                offset = new Vector2(0, -slideDistance);
                break;
            case SlideDirection.Right:
                offset = new Vector2(-slideDistance, 0);
                break;
            case SlideDirection.Bottom:
                offset = new Vector2(0, slideDistance);
                break;
            case SlideDirection.Left:
                offset = new Vector2(slideDistance, 0);
                break;
        }

        return originalPosition + offset;
    }

    private void PlayEntryAnimationInternal(System.Action onComplete = null)
    {
        // Detener animación previa si existe
        if (currentSequence.isAlive)
            currentSequence.Stop();

        // Crear secuencia según el tipo de animación seleccionado
        currentSequence = Sequence.Create();

        switch (animationType)
        {
            case AnimationType.Fade:
                currentSequence.Group(Tween.Alpha(canvasGroup, fadeTo, animationDuration, easeTypeEnter));
                break;

            case AnimationType.Scale:
                currentSequence.Group(Tween.Scale(targetRect, new Vector3(scaleTo, scaleTo, 1f), animationDuration, easeTypeEnter));
                break;

            case AnimationType.Slide:
                currentSequence.Group(Tween.Position(targetRect, originalPosition, animationDuration, easeTypeEnter));
                break;

            case AnimationType.FadeAndScale:
                currentSequence.Group(Tween.Alpha(canvasGroup, fadeTo, animationDuration, easeTypeEnter))
                              .Group(Tween.Scale(targetRect, new Vector3(scaleTo, scaleTo, 1f), animationDuration, easeTypeEnter));
                break;

            case AnimationType.FadeAndSlide:
                currentSequence.Group(Tween.Alpha(canvasGroup, fadeTo, animationDuration, easeTypeEnter))
                              .Group(Tween.Position(targetRect, originalPosition, animationDuration, easeTypeEnter));
                break;
        }

        // Añadir callback al finalizar
        if (onComplete != null)
            currentSequence.OnComplete(onComplete);
    }

    private void PlayExitAnimationInternal(System.Action onComplete = null)
    {
        // Detener animación previa si existe
        if (currentSequence.isAlive)
            currentSequence.Stop();

        // Crear secuencia según el tipo de animación seleccionado
        currentSequence = Sequence.Create();

        switch (animationType)
        {
            case AnimationType.Fade:
                currentSequence.Group(Tween.Alpha(canvasGroup, fadeFrom, animationDuration, easeTypeExit));
                break;

            case AnimationType.Scale:
                currentSequence.Group(Tween.Scale(targetRect, new Vector3(scaleFrom, scaleFrom, 1f), animationDuration, easeTypeExit));
                break;

            case AnimationType.Slide:
                currentSequence.Group(Tween.Position(targetRect, GetSlideEndPosition(), animationDuration, easeTypeExit));
                break;

            case AnimationType.FadeAndScale:
                currentSequence.Group(Tween.Alpha(canvasGroup, fadeFrom, animationDuration, easeTypeExit))
                              .Group(Tween.Scale(targetRect, new Vector3(scaleFrom, scaleFrom, 1f), animationDuration, easeTypeExit));
                break;

            case AnimationType.FadeAndSlide:
                currentSequence.Group(Tween.Alpha(canvasGroup, fadeFrom, animationDuration, easeTypeExit))
                              .Group(Tween.Position(targetRect, GetSlideEndPosition(), animationDuration, easeTypeExit));
                break;
        }

        // Añadir callback para cuando termine la animación
        currentSequence.OnComplete(() =>
        {
            // Restaurar la posición original
            targetRect.anchoredPosition = originalPosition;

            // Invocar callback si existe
            if (onComplete != null) onComplete.Invoke();
        });
    }

    // Método para restaurar la posición original del panel en caso de problemas
    public void ResetPosition()
    {
        Initialize();
        targetRect.anchoredPosition = originalPosition;
        targetRect.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
    }

    // Configurar propiedades de animación en tiempo de ejecución
    public void SetAnimationProperties(AnimationType type, float duration, Ease enterEase, Ease exitEase)
    {
        animationType = type;
        animationDuration = duration;
        easeTypeEnter = enterEase;
        easeTypeExit = exitEase;
    }

    /// Configurar opciones de deslizamiento
    public void SetSlideOptions(SlideDirection direction, float distance)
    {
        slideDirection = direction;
        slideDistance = distance;
    }

    /// Configurar opciones de escala
    public void SetScaleOptions(float from, float to)
    {
        scaleFrom = from;
        scaleTo = to;
    }

    /// Configurar opciones de transparencia
    public void SetFadeOptions(float from, float to)
    {
        fadeFrom = from;
        fadeTo = to;
    }
}
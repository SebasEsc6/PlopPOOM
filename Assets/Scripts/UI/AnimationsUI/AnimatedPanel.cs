using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PrimeTween;

[RequireComponent(typeof(UIPrimeTweenAnimator))]
public class AnimatedPanel : MonoBehaviour
{
    [SerializeField] private bool autoActivateOnStart = true;

    [Header("Background Options")]
    [SerializeField] private bool useBackgroundPanel = true;
    [SerializeField] private GameObject backgroundPanel;

    private UIPrimeTweenAnimator animator;

    private void Awake()
    {
        animator = GetComponent<UIPrimeTweenAnimator>();
    }

    private void OnEnable()
    {
        if (autoActivateOnStart)
        {
            // Solo activar el panel de fondo si está configurado para usarlo
            if (useBackgroundPanel && backgroundPanel != null)
            {
                backgroundPanel.SetActive(true);
            }

            // Reproducir animación de entrada cuando el panel se active
            animator.PlayEnterAnimation(() =>
            {
                // Solo desactivar el panel de fondo si está configurado para usarlo
                if (useBackgroundPanel && backgroundPanel != null)
                {
                    backgroundPanel.SetActive(false);
                }
            });
        }
        else
        {
            // Asegurarse de que el panel de fondo esté desactivado si no se autoinicia
            if (useBackgroundPanel && backgroundPanel != null)
            {
                backgroundPanel.SetActive(false);
            }
        }
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
        // La animación de entrada se reproducirá automáticamente en OnEnable
    }

    public void HidePanel()
    {
        // Reproducir animación de salida y desactivar al terminar
        animator.PlayExitAnimation(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void GoSceneAnim(string scene)
    {
        if (useBackgroundPanel)
        {
            backgroundPanel.SetActive(true);
        }
        // Reproducir animación de salida y desactivar al terminar
        animator.PlayExitAnimation(() =>
        {
            SceneManager.LoadScene(scene);
        });
    }
    public void ReloadSceneAnim()
    {
        if (useBackgroundPanel)
        {
            backgroundPanel.SetActive(true);
        }
        // Reproducir animación de salida y desactivar al terminar
        animator.PlayExitAnimation(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }

}
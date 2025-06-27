using UnityEngine;
using PrimeTween;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;

public class TitleAnim : MonoBehaviour
{
    [SerializeField] private AnimatedPanel TitlePanel;
    [SerializeField] private AnimatedPanel MenuPanel;

    void OnEnable()
    {
        // Suscribe al evento global del sistema de entrada
        InputSystem.onEvent += OnInputEvent;
    }

    void OnDisable()
    {
        // Limpia al desactivar
        InputSystem.onEvent -= OnInputEvent;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
            return;

        foreach (var control in device.allControls)
        {
            if (control is ButtonControl button)
            {
                if (button.ReadValueFromEvent(eventPtr) > 0)
                {
                    // Si el botón es presionado, iniciamos la animación
                    ListenKey();                    
                    break; 
                }
            }
        }
    }
    private void ListenKey()
    {
        TitlePanel.HidePanel();
        Tween.Delay(1f, () =>
        {
            MenuPanel.ShowPanel();
        });
    }
}

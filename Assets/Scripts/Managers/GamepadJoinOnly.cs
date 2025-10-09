using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(PlayerInputManager))]
public class GamepadJoinOnly : MonoBehaviour
{
    private PlayerInputManager _pim;

    // Track which devices ya están emparejados
    private readonly HashSet<InputDevice> _pairedDevices = new();

    // Para evitar doble lectura la misma frame
    private float _nextScanTime;

    private void Awake()
    {
        _pim = GetComponent<PlayerInputManager>();
        // Join manual: lo disparamos nosotros cuando un mando pulsa algo
        _pim.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
    }

    private void Update()
    {
        // Pequeño throttle para no spamear joins (opcional)
        if (Time.unscaledTime < _nextScanTime) return;
        _nextScanTime = Time.unscaledTime + 0.02f;

        // Recorre todos los gamepads conectados
        foreach (var gp in Gamepad.all)
        {
            if (gp == null || _pairedDevices.Contains(gp)) continue;

            if (WasAnyPressed(gp))
            {
                // Busca un scheme compatible con este dispositivo en el prefab
                string scheme = FindSchemeForDevice(gp);

                // Unir jugador emparejando ESTE gamepad
                // Version-agnostic: usa JoinPlayer(controlScheme, pairWithDevice)
                _pim.JoinPlayer(controlScheme: scheme, pairWithDevice: gp);

                _pairedDevices.Add(gp);

                // Si alcanzamos el límite del PlayerInputManager, deja de escanear
                if (_pim.playerCount >= _pim.maxPlayerCount) return;
            }
        }
    }

    // Check rápido: ¿algún botón fue presionado esta frame?
    private static bool WasAnyPressed(Gamepad gp)
    {
        // Evita stick/axis ruidosos; solo botones
        foreach (var c in gp.allControls)
        {
            if (c is ButtonControl b && b.wasPressedThisFrame)
                return true;
        }
        return false;
    }

    // Encuentra un control scheme del asset del prefab que soporte este device
    private string FindSchemeForDevice(InputDevice dev)
    {
        var pi = _pim.playerPrefab != null ? _pim.playerPrefab.GetComponent<PlayerInput>() : null;
        var actions = pi != null ? pi.actions : null;
        if (actions == null) return null; // no forces a scheme

        foreach (var cs in actions.controlSchemes)
        {
            if (cs.SupportsDevice(dev))
                return cs.name; // devuelve el nombre (string) para APIs antiguas
        }
        return null;
    }

    // Útil si implementas “player left”: te permite liberar el mando
    public void UnmarkDevice(InputDevice device)
    {
        _pairedDevices.Remove(device);
    }
}

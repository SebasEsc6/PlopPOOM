using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Simple Behaviour that manages the UI.
/// If you want to modify this Script please copy it into your own project and add it to your Player Prefab.
/// </summary>
public class UIManager : MonoBehaviour
{
    ConnectionManager m_ConnectionManager;
    VisualElement m_Root;
    TextField m_PlayerNameField;
    TextField m_SessionNameField;
    VisualElement m_LoginUI;
    VisualElement m_InGameUI;
    Button m_LoginButton;

    void Start()
    {
        var doc = GetComponent<UIDocument>();
        m_ConnectionManager = GetComponent<ConnectionManager>();

        m_ConnectionManager.OnConnected += ShowInGameUI;
        m_ConnectionManager.OnDisconnected += ShowLoginUI;

        m_Root = doc.rootVisualElement;
        m_LoginUI = m_Root.Q<VisualElement>("login");
        m_InGameUI = m_Root.Q<VisualElement>("ingame");
        m_PlayerNameField = m_Root.Q<TextField>("player-name");
        m_SessionNameField = m_Root.Q<TextField>("session-name");
        m_LoginButton = m_Root.Q<Button>("login-button");
        m_LoginButton.clicked += async () =>
        {
            await m_ConnectionManager.CreateOrJoinSessionAsync(m_SessionNameField.value, m_PlayerNameField.value);
        };

        m_Root.Q<Button>("logout-button").clicked += m_ConnectionManager.Disconnect;
    }

    void ShowLoginUI()
    {
        m_LoginUI.style.display = DisplayStyle.Flex;
        m_InGameUI.style.display = DisplayStyle.None;
    }

    void ShowInGameUI()
    {
        m_LoginUI.style.display = DisplayStyle.None;
        m_InGameUI.style.display = DisplayStyle.Flex;
    }
}

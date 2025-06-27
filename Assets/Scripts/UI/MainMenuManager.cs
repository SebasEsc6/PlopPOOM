using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button createBtn;
    [SerializeField] private Button quickBtn;
    [SerializeField] private Button joinBtn;
    [SerializeField] private TMP_InputField codeInput;

    private void Start()
    {
        createBtn.onClick.AddListener(() =>
            GameNetwork.Instance.CreateAndHostLobby()
        );
        quickBtn.onClick.AddListener(() =>
            GameNetwork.Instance.QuickJoinLobby()
        );
        joinBtn.onClick.AddListener(() =>
            GameNetwork.Instance.JoinByCodeLobby(codeInput.text)
        );
    }
}

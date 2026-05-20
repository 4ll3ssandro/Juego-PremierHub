using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private GameObject errorText;

    private TMP_InputField activeInput;

    private void Start()
    {
        activeInput = usernameInput;

        usernameInput.onSelect.AddListener(_ => OpenKeyboard(usernameInput));
        passwordInput.onSelect.AddListener(_ => OpenKeyboard(passwordInput));
    }

    private void OpenKeyboard(TMP_InputField input)
    {
        activeInput = input;

        if (NonNativeKeyboard.Instance == null)
        {
            Debug.LogWarning("No NonNativeKeyboard instance was found in the scene.");
            return;
        }

        NonNativeKeyboard.Instance.InputField = activeInput;
        activeInput.text = string.Empty;
        NonNativeKeyboard.Instance.PresentKeyboard(string.Empty);
    }

    public void Login()
    {
        bool usernameEmpty = string.IsNullOrWhiteSpace(usernameInput.text);
        bool passwordEmpty = string.IsNullOrWhiteSpace(passwordInput.text);

        if (usernameEmpty || passwordEmpty)
        {
            errorText.SetActive(true);
            return;
        }

        SceneManager.LoadScene("SampleScene");
    }
}

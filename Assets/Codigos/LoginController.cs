using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Text.RegularExpressions;

public class LoginController : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private GameObject errorText;

    private TMP_InputField activeInput;
    private TMP_Text errorLabel;
    private Button loginButton;
    private Coroutine loginRoutine;

    private static readonly Regex EmailRegex = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private void Start()
    {
        activeInput = usernameInput;
        errorLabel = errorText != null ? errorText.GetComponent<TMP_Text>() : null;
        GameObject loginButtonObject = GameObject.Find("LoginButton");
        loginButton = loginButtonObject != null ? loginButtonObject.GetComponent<Button>() : null;

        usernameInput.onSelect.AddListener(_ => OpenKeyboard(usernameInput));
        passwordInput.onSelect.AddListener(_ => OpenKeyboard(passwordInput));

        HideError();
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
        if (loginRoutine != null)
        {
            return;
        }

        string correo = usernameInput.text.Trim();
        string contrasena = passwordInput.text;

        bool usernameEmpty = string.IsNullOrWhiteSpace(correo);
        bool passwordEmpty = string.IsNullOrWhiteSpace(contrasena);

        if (usernameEmpty || passwordEmpty)
        {
            ShowError("Ingresa correo y contrasena");
            return;
        }

        if (!EmailRegex.IsMatch(correo))
        {
            ShowError("Ingresa un correo valido");
            return;
        }

        loginRoutine = StartCoroutine(LoginRoutine(correo, contrasena));
    }

    private IEnumerator LoginRoutine(string correo, string contrasena)
    {
        SetLoginEnabled(false);
        ShowError("Conectando...");

        PremierHubApiClient.LoginResult result = null;
        yield return PremierHubApiClient.Login(correo, contrasena, loginResult => result = loginResult);

        loginRoutine = null;
        SetLoginEnabled(true);

        if (result == null || !result.Success)
        {
            ShowError(result != null && !string.IsNullOrWhiteSpace(result.Error)
                ? result.Error
                : "Credenciales incorrectas");
            yield break;
        }

        PremierHubSession.StartSession(result.UserId, result.SessionCookie);
        SceneManager.LoadScene("Menu");
    }

    private void ShowError(string message)
    {
        if (errorLabel != null)
        {
            errorLabel.text = message;
        }

        if (errorText != null)
        {
            errorText.SetActive(true);
        }
    }

    private void HideError()
    {
        if (errorText != null)
        {
            errorText.SetActive(false);
        }
    }

    private void SetLoginEnabled(bool enabled)
    {
        if (loginButton != null)
        {
            loginButton.interactable = enabled;
        }
    }
}

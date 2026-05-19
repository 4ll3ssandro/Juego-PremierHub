using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private GameObject errorText;

    private TouchScreenKeyboard keyboard;
    private TMP_InputField activeInput;

    private void Start()
    {
        activeInput = usernameInput;

        usernameInput.onSelect.AddListener(_ => OpenKeyboard(usernameInput, false));
        passwordInput.onSelect.AddListener(_ => OpenKeyboard(passwordInput, true));
    }

    private void Update()
    {
        if (keyboard != null && activeInput != null)
        {
            activeInput.text = keyboard.text;
        }
    }

    private void OpenKeyboard(TMP_InputField input, bool isPassword)
    {
        activeInput = input;

        keyboard = TouchScreenKeyboard.Open(
            input.text,
            isPassword ? TouchScreenKeyboardType.Default : TouchScreenKeyboardType.Default,
            false,
            false,
            isPassword
        );
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
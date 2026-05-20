using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class ShowKeyboard : MonoBehaviour

{
    private TMP_InputField inputField;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputField = GetComponent<TMP_InputField>();

        if (inputField == null)
        {
            Debug.LogWarning($"{nameof(ShowKeyboard)} requires a TMP_InputField on the same GameObject.", this);
            return;
        }

        inputField.onSelect.AddListener(_ => OpenKeyboard());
    }

    // Update is called once per frame
    public void OpenKeyboard()
    {
        if (NonNativeKeyboard.Instance == null)
        {
            Debug.LogWarning("No NonNativeKeyboard instance was found in the scene.", this);
            return;
        }

        NonNativeKeyboard.Instance.InputField = inputField;
        inputField.text = string.Empty;
        NonNativeKeyboard.Instance.PresentKeyboard(string.Empty);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class RestartGame : MonoBehaviour
{
    private InputDevice leftController;
    private bool wasPressedLastFrame;

    private void Start()
    {
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    }

    private void Update()
    {
        if (!leftController.isValid)
        {
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        }

        bool isPressed =
            leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool yButtonPressed)
            && yButtonPressed;

        if (isPressed && !wasPressedLastFrame)
        {
            RestartScene();
        }

        wasPressedLastFrame = isPressed;
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuGameSelection : MonoBehaviour
{
    [SerializeField] private string goalKeeperSceneName = "SampleScene";

    private Button goalKeeperButton;

    private void Awake()
    {
        goalKeeperButton = GetComponent<Button>();
        goalKeeperButton.onClick.AddListener(LoadGoalKeeperScene);
    }

    private void OnDestroy()
    {
        if (goalKeeperButton != null)
        {
            goalKeeperButton.onClick.RemoveListener(LoadGoalKeeperScene);
        }
    }

    public void LoadGoalKeeperScene()
    {
        SceneManager.LoadScene(goalKeeperSceneName);
    }
}

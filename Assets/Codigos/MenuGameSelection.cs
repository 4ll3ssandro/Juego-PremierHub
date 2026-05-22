using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuGameSelection : MonoBehaviour
{
    [SerializeField] private string goalKeeperSceneName = "SampleScene";
    [SerializeField] private TMP_Text pointsText;

    private Button goalKeeperButton;
    private Coroutine pointsRoutine;

    private void Awake()
    {
        goalKeeperButton = GetComponent<Button>();
        goalKeeperButton.onClick.AddListener(LoadGoalKeeperScene);
    }

    private void Start()
    {
        EnsurePointsText();
        RefreshPoints();
    }

    private void OnDestroy()
    {
        if (goalKeeperButton != null)
        {
            goalKeeperButton.onClick.RemoveListener(LoadGoalKeeperScene);
        }
    }

    public void RefreshPoints()
    {
        if (pointsRoutine != null)
        {
            StopCoroutine(pointsRoutine);
        }

        pointsRoutine = StartCoroutine(RefreshPointsRoutine());
    }

    private IEnumerator RefreshPointsRoutine()
    {
        SetPointsText("Puntos: ...");

        PremierHubApiClient.PointsResult result = null;
        yield return PremierHubApiClient.GetCurrentUserPoints(pointsResult => result = pointsResult);

        pointsRoutine = null;

        if (result == null || !result.Success)
        {
            SetPointsText("Puntos: --");
            Debug.LogWarning(result != null && !string.IsNullOrWhiteSpace(result.Error)
                ? $"No se pudieron consultar los puntos: {result.Error}"
                : "No se pudieron consultar los puntos.");
            yield break;
        }

        SetPointsText("Puntos: " + result.Dinero);
    }

    public void LoadGoalKeeperScene()
    {
        SceneManager.LoadScene(goalKeeperSceneName);
    }

    private void EnsurePointsText()
    {
        if (pointsText != null)
        {
            return;
        }

        GameObject existingTextObject = GameObject.Find("UserPointsText");
        if (existingTextObject != null)
        {
            pointsText = existingTextObject.GetComponent<TMP_Text>();
            if (pointsText != null)
            {
                return;
            }
        }

        Debug.LogWarning("No se encontro UserPointsText ni hay pointsText asignado en el inspector.");
    }

    private void SetPointsText(string text)
    {
        if (pointsText != null)
        {
            pointsText.text = text;
        }
    }
}

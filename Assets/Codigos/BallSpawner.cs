using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BallSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private BoxCollider spawnZone;
    [SerializeField] private BoxCollider targetZone;
    [SerializeField] private float launchSpeed = 10f;
    [SerializeField] private int ballsPerRound = 10;
    [SerializeField] private float roundDuration = 60f;
    [SerializeField] private float minSecondsBetweenSpawns = 2f;
    [SerializeField] private float maxBallLifetime = 8f;
    [SerializeField] private TMP_Text ballsRemainingText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private GameObject[] rightHandRayObjects;

    private int ballsRemaining;
    private int ballsSpawned;
    private int activeBalls;
    private int saves;
    private int currentStreak;
    private int bestStreak;
    private bool gameOverShown;
    private bool savesSubmitted;
    private GameObject activeBall;
    private Coroutine activeBallCleanupCoroutine;
    private TMP_Text attemptsSummaryText;
    private TMP_Text totalPointsText;
    private TMP_Text pointsDeltaText;
    private TMP_Text bestStreakText;
    private TMP_Text precisionText;

    private void Start()
    {
        ballsRemaining = ballsPerRound;
        EnsureBallsRemainingText();
        EnsureGameOverPanel();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (ballsRemainingText != null)
        {
            ballsRemainingText.gameObject.SetActive(true);
        }

        SetRightHandRayActive(false);
        UpdateBallsRemainingText();
        StartCoroutine(SpawnRound());
    }

    private IEnumerator SpawnRound()
    {
        float[] spawnTimes = GetRandomSpawnTimes();
        float elapsedTime = 0f;

        for (int i = 0; i < spawnTimes.Length; i++)
        {
            float waitTime = spawnTimes[i] - elapsedTime;

            if (waitTime > 0f)
            {
                yield return new WaitForSeconds(waitTime);
            }

            ClearActiveBallAsMiss();
            SpawnBall();
            ballsRemaining--;
            UpdateBallsRemainingText();
            elapsedTime = spawnTimes[i];
        }
    }

    private void SpawnBall()
    {
        ballsSpawned++;
        activeBalls++;

        Vector3 spawnPosition = GetRandomPointInZone(spawnZone);
        Vector3 targetPosition = GetRandomPointInZone(targetZone);

        GameObject newBall = Instantiate(
            ballPrefab,
            spawnPosition,
            Quaternion.identity
        );

        activeBall = newBall;

        Rigidbody rb = newBall.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;

            Vector3 direction = (targetPosition - spawnPosition).normalized;
            rb.linearVelocity = direction * launchSpeed;
        }

        if (activeBallCleanupCoroutine != null)
        {
            StopCoroutine(activeBallCleanupCoroutine);
        }

        activeBallCleanupCoroutine = StartCoroutine(CleanupActiveBallAfterDelay(newBall, maxBallLifetime));
    }

    public bool RegisterSave()
    {
        return RegisterBallResolved(true);
    }

    public bool RegisterMiss()
    {
        return RegisterBallResolved(false);
    }

    private bool RegisterBallResolved(bool wasSaved)
    {
        if (activeBalls <= 0)
        {
            return false;
        }

        if (wasSaved)
        {
            saves++;
            currentStreak++;
            bestStreak = Mathf.Max(bestStreak, currentStreak);
        }
        else
        {
            currentStreak = 0;
        }

        activeBalls--;

        if (activeBallCleanupCoroutine != null)
        {
            StopCoroutine(activeBallCleanupCoroutine);
            activeBallCleanupCoroutine = null;
        }

        activeBall = null;

        if (!gameOverShown && ballsSpawned >= ballsPerRound && activeBalls <= 0)
        {
            ShowGameOver();
        }

        return true;
    }

    private IEnumerator CleanupActiveBallAfterDelay(GameObject ball, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (ball != null && ball == activeBall)
        {
            ClearActiveBallAsMiss();
        }
    }

    private void ClearActiveBallAsMiss()
    {
        if (activeBall == null)
        {
            return;
        }

        GameObject ballToDestroy = activeBall;
        RegisterBallResolved(false);

        if (ballToDestroy != null)
        {
            ballToDestroy.SetActive(false);
            Destroy(ballToDestroy);
        }
    }

    private void ShowGameOver()
    {
        gameOverShown = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (ballsRemainingText != null)
        {
            ballsRemainingText.gameObject.SetActive(false);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = saves.ToString();
        }

        UpdateGameOverStats();
        SubmitSavesToProfile();
        SetRightHandRayActive(true);
    }

    private void SubmitSavesToProfile()
    {
        if (savesSubmitted)
        {
            return;
        }

        savesSubmitted = true;
        StartCoroutine(SubmitSavesRoutine());
    }

    private IEnumerator SubmitSavesRoutine()
    {
        PremierHubApiClient.SavesResult result = null;
        yield return PremierHubApiClient.SubmitSaves(saves, savesResult => result = savesResult);

        if (result == null || !result.Success)
        {
            Debug.LogWarning(result != null && !string.IsNullOrWhiteSpace(result.Error)
                ? $"No se pudieron guardar los puntos por atajadas: {result.Error}"
                : "No se pudieron guardar los puntos por atajadas.");

            if (totalPointsText != null)
            {
                totalPointsText.text = "--";
            }

            if (pointsDeltaText != null)
            {
                pointsDeltaText.text = "Puntos no guardados";
            }

            yield break;
        }

        if (totalPointsText != null)
        {
            totalPointsText.text = FormatNumber(result.Dinero);
        }

        if (pointsDeltaText != null)
        {
            pointsDeltaText.text = "+" + FormatNumber(result.PointsEarned) + " pts esta ronda";
        }

        Debug.Log($"Puntos por atajadas guardados. Atajadas: {saves}. Puntos: {result.PointsEarned}. Saldo: {result.Dinero}");
    }

    private Vector3 GetRandomPointInZone(BoxCollider zone)
    {
        Bounds bounds = zone.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);

        return new Vector3(randomX, randomY, randomZ);
    }

    private float[] GetRandomSpawnTimes()
    {
        float[] randomOffsets = new float[ballsPerRound];
        float availableRandomTime = roundDuration - minSecondsBetweenSpawns * (ballsPerRound - 1);

        if (availableRandomTime < 0f)
        {
            availableRandomTime = 0f;
        }

        for (int i = 0; i < randomOffsets.Length; i++)
        {
            randomOffsets[i] = Random.Range(0f, availableRandomTime);
        }

        System.Array.Sort(randomOffsets);

        for (int i = 0; i < randomOffsets.Length; i++)
        {
            randomOffsets[i] += minSecondsBetweenSpawns * i;
        }

        return randomOffsets;
    }

    private void UpdateBallsRemainingText()
    {
        if (ballsRemainingText != null)
        {
            ballsRemainingText.text = "Balones restantes: " + ballsRemaining;
        }
    }

    private void EnsureBallsRemainingText()
    {
        if (ballsRemainingText != null)
        {
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            return;
        }

        GameObject textObject = new GameObject("BallsRemainingText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(canvas.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(6f, 100f);
        textRect.sizeDelta = new Vector2(500f, 50f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = 36f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;

        ballsRemainingText = text;
    }

    private void EnsureGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            ConfigureGameOverPanel(gameOverPanel);
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            return;
        }

        GameObject panel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0.04f, 0.08f, 0.15f, 0.92f);

        gameOverPanel = panel;
        ConfigureGameOverPanel(panel);
    }

    private void ConfigureGameOverPanel(GameObject panel)
    {
        RectTransform panelRect = panel.GetComponent<RectTransform>();

        if (panelRect != null)
        {
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = Vector2.zero;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }

        Image panelImage = panel.GetComponent<Image>();

        if (panelImage != null)
        {
            panelImage.color = new Color(0.04f, 0.08f, 0.15f, 0.92f);
        }

        for (int i = panel.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(panel.transform.GetChild(i).gameObject);
        }

        AddDivider(panel.transform, "LeftDivider", new Vector2(-165f, 0f), new Vector2(1.5f, 265f));
        AddDivider(panel.transform, "RightDivider", new Vector2(170f, 0f), new Vector2(1.5f, 265f));

        AddLabel(panel.transform, "AtajadasLabel", "ATAJADAS", new Vector2(-330f, 126f), new Vector2(230f, 28f), 18f, new Color(1f, 0.12f, 0.45f, 1f));
        finalScoreText = AddLabel(panel.transform, "FinalScoreText", "0", new Vector2(-330f, 42f), new Vector2(250f, 128f), 116f, Color.white);
        finalScoreText.fontStyle = FontStyles.Bold;

        attemptsSummaryText = AddLabel(panel.transform, "AttemptsSummaryText", "", new Vector2(-330f, -54f), new Vector2(270f, 34f), 20f, new Color(0.72f, 0.76f, 0.86f, 1f));
        attemptsSummaryText.characterSpacing = 8f;

        AddLabel(panel.transform, "TotalPointsLabel", "PUNTOS TOTALES", new Vector2(0f, 124f), new Vector2(250f, 28f), 18f, new Color(0.72f, 0.76f, 0.86f, 1f));
        totalPointsText = AddLabel(panel.transform, "TotalPointsText", "...", new Vector2(0f, 58f), new Vector2(270f, 78f), 64f, Color.white);
        totalPointsText.fontStyle = FontStyles.Bold;
        pointsDeltaText = AddLabel(panel.transform, "PointsDeltaText", "Guardando puntos...", new Vector2(0f, 2f), new Vector2(220f, 32f), 19f, new Color(0.26f, 1f, 0.59f, 1f));
        pointsDeltaText.fontStyle = FontStyles.Bold;

        AddLabel(panel.transform, "BestStreakLabel", "MEJOR RACHA", new Vector2(0f, -50f), new Vector2(230f, 28f), 18f, new Color(0.72f, 0.76f, 0.86f, 1f));
        bestStreakText = AddLabel(panel.transform, "BestStreakText", "0 SEGUIDAS", new Vector2(0f, -88f), new Vector2(230f, 42f), 29f, Color.white);
        bestStreakText.fontStyle = FontStyles.Bold;

        AddLabel(panel.transform, "PrecisionLabel", "PRECISIÓN", new Vector2(335f, 124f), new Vector2(220f, 28f), 18f, new Color(0.72f, 0.76f, 0.86f, 1f));
        precisionText = AddLabel(panel.transform, "PrecisionText", "0%", new Vector2(335f, 72f), new Vector2(220f, 64f), 52f, Color.white);
        precisionText.fontStyle = FontStyles.Bold;

        AddResultsButton(panel.transform, "RestartButton", "REINICIAR", new Vector2(335f, -62f), RestartCurrentScene);
        AddResultsButton(panel.transform, "MenuButton", "MENÚ", new Vector2(335f, -128f), ReturnToMenu);
        UpdateGameOverStats();
    }

    private TextMeshProUGUI AddLabel(Transform parent, string name, string text, Vector2 position, Vector2 size, float fontSize, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.enableAutoSizing = true;
        label.fontSizeMin = Mathf.Min(14f, fontSize);
        label.fontSizeMax = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
        label.raycastTarget = false;

        return label;
    }

    private void AddDivider(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject divider = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        divider.transform.SetParent(parent, false);

        RectTransform rect = divider.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = divider.GetComponent<Image>();
        image.color = new Color(0.46f, 0.53f, 0.66f, 0.25f);
    }

    private void AddResultsButton(Transform parent, string name, string text, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(230f, 56f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = name == "MenuButton"
            ? new Color(0.52f, 0.12f, 0.34f, 1f)
            : new Color(0.2f, 0.52f, 0.95f, 1f);

        Button restartButton = buttonObject.GetComponent<Button>();

        if (action != null)
        {
            restartButton.onClick.AddListener(action);
        }

        TextMeshProUGUI buttonText = AddLabel(buttonObject.transform, "Text (TMP)", text, Vector2.zero, buttonRect.sizeDelta, 22f, Color.white);
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.characterSpacing = 7f;
    }

    private void RestartCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    private void UpdateGameOverStats()
    {
        int precision = ballsPerRound > 0 ? Mathf.RoundToInt((float)saves / ballsPerRound * 100f) : 0;

        if (finalScoreText != null)
        {
            finalScoreText.text = saves.ToString();
        }

        if (attemptsSummaryText != null)
        {
            attemptsSummaryText.text = "DE " + ballsPerRound + " INTENTOS - " + precision + "% PRECISIÓN";
        }

        if (bestStreakText != null)
        {
            bestStreakText.text = bestStreak + " SEGUIDAS";
        }

        if (precisionText != null)
        {
            precisionText.text = precision + "%";
        }
    }

    private string FormatNumber(int value)
    {
        return value.ToString("N0").Replace(",", " ");
    }

    private void SetRightHandRayActive(bool isActive)
    {
        EnsureRightHandRayObjects();

        foreach (GameObject rayObject in rightHandRayObjects)
        {
            if (rayObject != null)
            {
                rayObject.SetActive(isActive);
            }
        }
    }

    private void EnsureRightHandRayObjects()
    {
        if (rightHandRayObjects != null && rightHandRayObjects.Length > 0)
        {
            return;
        }

        GameObject rightHand = GameObject.Find("RightHand");

        if (rightHand == null)
        {
            return;
        }

        Transform rayTransform = FindChildRecursive(rightHand.transform, "Ray Interactor");

        if (rayTransform != null)
        {
            rightHandRayObjects = new[] { rayTransform.gameObject };
        }
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform match = FindChildRecursive(child, childName);

            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}

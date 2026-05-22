using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

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
    private bool gameOverShown;
    private GameObject activeBall;
    private Coroutine activeBallCleanupCoroutine;

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
            finalScoreText.text = "Atajaste " + saves + " de " + ballsPerRound + " balones";
        }

        SetRightHandRayActive(true);
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
        if (gameOverPanel != null && finalScoreText != null)
        {
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
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(650f, 260f);

        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.82f);

        GameObject scoreTextObject = new GameObject("FinalScoreText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        scoreTextObject.transform.SetParent(panel.transform, false);

        RectTransform scoreRect = scoreTextObject.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 0.5f);
        scoreRect.anchorMax = new Vector2(0.5f, 0.5f);
        scoreRect.anchoredPosition = new Vector2(0f, 55f);
        scoreRect.sizeDelta = new Vector2(580f, 80f);

        TextMeshProUGUI scoreText = scoreTextObject.GetComponent<TextMeshProUGUI>();
        scoreText.text = "Atajaste 0 de " + ballsPerRound + " balones";
        scoreText.fontSize = 42f;
        scoreText.alignment = TextAlignmentOptions.Center;
        scoreText.color = Color.white;

        GameObject buttonObject = new GameObject("RestartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(panel.transform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, -60f);
        buttonRect.sizeDelta = new Vector2(330f, 70f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.52f, 0.95f, 1f);

        Button restartButton = buttonObject.GetComponent<Button>();
        RestartGame restartGame = FindFirstObjectByType<RestartGame>();

        if (restartGame != null)
        {
            restartButton.onClick.AddListener(restartGame.RestartScene);
        }

        GameObject buttonTextObject = new GameObject("Text (TMP)", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        buttonTextObject.transform.SetParent(buttonObject.transform, false);

        RectTransform buttonTextRect = buttonTextObject.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI buttonText = buttonTextObject.GetComponent<TextMeshProUGUI>();
        buttonText.text = "Volver a iniciar";
        buttonText.fontSize = 30f;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;

        gameOverPanel = panel;
        finalScoreText = scoreText;
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

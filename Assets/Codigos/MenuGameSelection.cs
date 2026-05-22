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
    [SerializeField] private string defaultPlayerName = "Jugador PremierHUB";

    private Button goalKeeperButton;
    private Coroutine pointsRoutine;
    private TMP_Text playerNameText;
    private TMP_Text playerInitialsText;
    private static Sprite roundedSprite;

    private void Awake()
    {
        goalKeeperButton = GetComponent<Button>();
        goalKeeperButton.onClick.AddListener(LoadGoalKeeperScene);
    }

    private void Start()
    {
        BuildMenu();
        SetPlayerName(string.IsNullOrWhiteSpace(PremierHubSession.DisplayName)
            ? defaultPlayerName
            : PremierHubSession.DisplayName);
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
        SetPointsText("...");

        PremierHubApiClient.PointsResult result = null;
        yield return PremierHubApiClient.GetCurrentUserPoints(pointsResult => result = pointsResult);

        pointsRoutine = null;

        if (result == null || !result.Success)
        {
            SetPointsText("--");
            Debug.LogWarning(result != null && !string.IsNullOrWhiteSpace(result.Error)
                ? $"No se pudieron consultar los puntos: {result.Error}"
                : "No se pudieron consultar los puntos.");
            yield break;
        }

        if (!string.IsNullOrWhiteSpace(result.DisplayName))
        {
            SetPlayerName(result.DisplayName);
        }

        SetPointsText(FormatNumber(result.Dinero));
    }

    public void LoadGoalKeeperScene()
    {
        SceneManager.LoadScene(goalKeeperSceneName);
    }

    private void BuildMenu()
    {
        Transform panel = transform.parent;

        if (panel == null)
        {
            return;
        }

        ConfigurePanel(panel);

        for (int i = panel.childCount - 1; i >= 0; i--)
        {
            Transform child = panel.GetChild(i);

            if (child != transform)
            {
                Destroy(child.gameObject);
            }
        }

        GameObject background = AddPanel(panel, "DarkMenuBackground", Vector2.zero, new Vector2(1250f, 500f), new Color(0.04f, 0.08f, 0.15f, 1f), false, false);
        background.transform.SetAsFirstSibling();

        AddPanel(panel, "SoftBottomGlow", new Vector2(0f, -224f), new Vector2(1250f, 76f), new Color(0.05f, 0.13f, 0.23f, 0.5f), false, false);
        AddPanel(panel, "CenterDivider", new Vector2(0f, -18f), new Vector2(2f, 315f), new Color(0.22f, 0.3f, 0.43f, 0.5f), false, false);

        BuildHeader(panel);
        BuildGoalKeeperSide(panel);
        BuildRefereeSide(panel);
        ConfigurePlayButton();
    }

    private void ConfigurePanel(Transform panel)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();

        if (rect == null)
        {
            return;
        }

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.anchoredPosition = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panel.GetComponent<Image>();

        if (image != null)
        {
            image.color = new Color(0.04f, 0.08f, 0.15f, 1f);
        }
    }

    private void BuildHeader(Transform panel)
    {
        AddPanel(panel, "BrandPill", new Vector2(-535f, 208f), new Vector2(122f, 32f), new Color(0.35f, 0.04f, 0.2f, 1f), true, false);
        AddLabel(panel, "BrandText", "PREMIERHUB", new Vector2(-535f, 208f), new Vector2(104f, 22f), 11f, new Color(1f, 0.08f, 0.36f, 1f), FontStyles.Bold, TextAlignmentOptions.Center, 4f);
        AddLabel(panel, "ModeHeader", "ELIGE TU MODO DE JUEGO", new Vector2(-286f, 208f), new Vector2(330f, 22f), 12f, new Color(0.66f, 0.71f, 0.8f, 1f), FontStyles.Bold, TextAlignmentOptions.Left, 5f);

        GameObject profilePill = AddPanel(panel, "ProfilePill", new Vector2(420f, 208f), new Vector2(300f, 48f), new Color(0.1f, 0.15f, 0.24f, 0.95f), true, false);
        AddPanel(profilePill.transform, "Avatar", new Vector2(-126f, 0f), new Vector2(31f, 31f), new Color(0.45f, 0.54f, 0.67f, 1f), true, false);
        playerInitialsText = AddLabel(profilePill.transform, "PlayerInitialsText", "JP", new Vector2(-126f, 0f), new Vector2(30f, 22f), 10f, Color.white, FontStyles.Bold, TextAlignmentOptions.Center, 0f);
        playerNameText = AddLabel(profilePill.transform, "PlayerNameText", defaultPlayerName, new Vector2(-41f, 7f), new Vector2(132f, 16f), 9f, Color.white, FontStyles.Bold, TextAlignmentOptions.Left, 0f);
        AddLabel(profilePill.transform, "AccountText", "Cuenta PremierHUB", new Vector2(-36f, -8f), new Vector2(142f, 14f), 8f, new Color(0.62f, 0.67f, 0.76f, 1f), FontStyles.Normal, TextAlignmentOptions.Left, 0f);
        AddPanel(profilePill.transform, "ProfileDivider", new Vector2(45f, 0f), new Vector2(1f, 28f), new Color(0.33f, 0.4f, 0.52f, 0.85f), false, false);
        AddLabel(profilePill.transform, "PointsTitle", "TUS PUNTOS", new Vector2(98f, 8f), new Vector2(78f, 13f), 8f, new Color(0.75f, 0.78f, 0.84f, 1f), FontStyles.Bold, TextAlignmentOptions.Left, 4f);
        pointsText = AddLabel(profilePill.transform, "UserPointsText", "...", new Vector2(97f, -9f), new Vector2(82f, 20f), 16f, new Color(1f, 0.08f, 0.36f, 1f), FontStyles.Bold, TextAlignmentOptions.Left, 4f);
    }

    private void BuildGoalKeeperSide(Transform panel)
    {
        AddLabel(panel, "GoalKeeperMode", "MODO 01", new Vector2(-535f, 116f), new Vector2(78f, 18f), 11f, new Color(1f, 0.08f, 0.36f, 1f), FontStyles.Bold, TextAlignmentOptions.Left, 5f);
        AddPanel(panel, "GoalKeeperLine", new Vector2(-295f, 116f), new Vector2(430f, 1.5f), new Color(1f, 0.08f, 0.36f, 0.72f), false, false);
        AddLabel(panel, "GoalKeeperTitle", "Modo Portero", new Vector2(-375f, 55f), new Vector2(420f, 64f), 42f, Color.white, FontStyles.Bold, TextAlignmentOptions.Left, 0f);
    }

    private void BuildRefereeSide(Transform panel)
    {
        AddLabel(panel, "RefereeMode", "MODO 02", new Vector2(84f, 116f), new Vector2(78f, 18f), 11f, new Color(0.46f, 0.53f, 0.66f, 1f), FontStyles.Bold, TextAlignmentOptions.Left, 5f);
        AddPanel(panel, "RefereeLine", new Vector2(318f, 116f), new Vector2(430f, 1.5f), new Color(0.26f, 0.34f, 0.48f, 0.72f), false, false);
        AddLabel(panel, "RefereeLockedTop", "EN OBRA", new Vector2(526f, 116f), new Vector2(86f, 18f), 10f, new Color(0.65f, 0.7f, 0.8f, 1f), FontStyles.Bold, TextAlignmentOptions.Right, 4f);

        AddLabel(panel, "RefereeTitle", "Modo Arbitro", new Vector2(238f, 55f), new Vector2(420f, 64f), 42f, new Color(0.76f, 0.79f, 0.84f, 1f), FontStyles.Bold, TextAlignmentOptions.Left, 0f);

        GameObject disabledButton = AddPanel(panel, "RefereeDisabledButton", new Vector2(158f, -122f), new Vector2(225f, 52f), new Color(0.08f, 0.13f, 0.22f, 0.95f), true, false);
        AddLabel(disabledButton.transform, "RefereeButtonText", "PROXIMAMENTE", Vector2.zero, new Vector2(188f, 22f), 12f, new Color(0.48f, 0.55f, 0.68f, 1f), FontStyles.Bold, TextAlignmentOptions.Center, 5f);
    }

    private void SetPointsText(string text)
    {
        if (pointsText != null)
        {
            pointsText.text = text;
        }
    }

    private void SetPlayerName(string displayName)
    {
        displayName = GetDisplayNameOrFallback(displayName);

        if (playerNameText != null)
        {
            playerNameText.text = displayName;
        }

        if (playerInitialsText != null)
        {
            playerInitialsText.text = GetInitials(displayName);
        }
    }

    private string GetDisplayNameOrFallback(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return defaultPlayerName;
        }

        string trimmedName = displayName.Trim();

        if (trimmedName.Length <= 2 && int.TryParse(trimmedName, out _))
        {
            return defaultPlayerName;
        }

        return trimmedName;
    }

    private void ConfigurePlayButton()
    {
        RectTransform rect = GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(-446f, -122f);
        rect.sizeDelta = new Vector2(210f, 52f);

        Image image = GetComponent<Image>();

        if (image == null)
        {
            image = gameObject.AddComponent<Image>();
        }

        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color(1f, 0.04f, 0.33f, 1f);
        image.raycastTarget = true;
        goalKeeperButton.targetGraphic = image;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        AddLabel(transform, "PlayButtonText", "JUGAR AHORA  ->", Vector2.zero, new Vector2(180f, 26f), 14f, Color.white, FontStyles.Bold, TextAlignmentOptions.Center, 4f);
        transform.SetAsLastSibling();
    }

    private GameObject AddPanel(Transform parent, string name, Vector2 position, Vector2 size, Color color, bool rounded, bool raycastTarget)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = panel.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = raycastTarget;

        if (rounded)
        {
            image.sprite = GetRoundedSprite();
            image.type = Image.Type.Sliced;
        }

        return panel;
    }

    private TMP_Text AddLabel(Transform parent, string name, string text, Vector2 position, Vector2 size, float fontSize, Color color, FontStyles fontStyle, TextAlignmentOptions alignment, float characterSpacing)
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
        label.fontSizeMin = Mathf.Min(8f, fontSize);
        label.fontSizeMax = fontSize;
        label.color = color;
        label.fontStyle = fontStyle;
        label.alignment = alignment;
        label.characterSpacing = characterSpacing;
        label.raycastTarget = false;

        return label;
    }

    private Sprite GetRoundedSprite()
    {
        if (roundedSprite != null)
        {
            return roundedSprite;
        }

        const int size = 64;
        const float radius = 18f;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nearestX = Mathf.Clamp(x, radius, size - radius);
                float nearestY = Mathf.Clamp(y, radius, size - radius);
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(nearestX, nearestY));
                float alpha = distance <= radius ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        roundedSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(18f, 18f, 18f, 18f));
        return roundedSprite;
    }

    private string FormatNumber(int value)
    {
        return value.ToString("N0").Replace(",", " ");
    }

    private string GetInitials(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return "JP";
        }

        string[] parts = displayName.Trim().Split(' ');
        string initials = parts[0].Substring(0, 1);

        if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
        {
            initials += parts[1].Substring(0, 1);
        }

        return initials.ToUpperInvariant();
    }
}

public static class PremierHubSession
{
    public static bool IsLoggedIn { get; private set; }
    public static int UserId { get; private set; }
    public static string SessionCookie { get; private set; }
    public static string DisplayName { get; private set; }

    public static void StartSession(int userId, string sessionCookie)
    {
        UserId = userId;
        SessionCookie = sessionCookie;
        DisplayName = string.Empty;
        IsLoggedIn = true;
    }

    public static void StartSession(int userId, string sessionCookie, string displayName)
    {
        UserId = userId;
        SessionCookie = sessionCookie;
        DisplayName = displayName;
        IsLoggedIn = true;
    }

    public static void Clear()
    {
        UserId = 0;
        SessionCookie = string.Empty;
        DisplayName = string.Empty;
        IsLoggedIn = false;
    }
}

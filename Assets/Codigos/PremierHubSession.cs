public static class PremierHubSession
{
    public static bool IsLoggedIn { get; private set; }
    public static int UserId { get; private set; }
    public static string SessionCookie { get; private set; }

    public static void StartSession(int userId, string sessionCookie)
    {
        UserId = userId;
        SessionCookie = sessionCookie;
        IsLoggedIn = true;
    }

    public static void Clear()
    {
        UserId = 0;
        SessionCookie = string.Empty;
        IsLoggedIn = false;
    }
}

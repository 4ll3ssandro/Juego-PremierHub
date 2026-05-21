using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class PremierHubApiClient
{
    private const string ApiUrlKey = "PREMIERHUB_API_URL";
    private const string DefaultApiUrl = "http://localhost:4000";
    private const int RequestTimeoutSeconds = 10;
    private static string baseUrl;

    [Serializable]
    private class LoginRequest
    {
        public string correo;
        public string contrasena;
    }

    [Serializable]
    public class LoginResponse
    {
        public bool success;
        public UserData user;
        public string error;
    }

    [Serializable]
    public class UserData
    {
        public int id_usuario;
    }

    public class LoginResult
    {
        public bool Success;
        public string Error;
        public int UserId;
        public string SessionCookie;
    }

    public static IEnumerator Login(string correo, string contrasena, Action<LoginResult> onComplete)
    {
        yield return EnsureConfigLoaded();

        string url = CombineUrl(baseUrl, "/api/auth/login");
        string payload = JsonUtility.ToJson(new LoginRequest
        {
            correo = correo,
            contrasena = contrasena
        });

        using UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.timeout = RequestTimeoutSeconds;
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log($"PremierHub login request: {url}");

        if (!string.IsNullOrEmpty(PremierHubSession.SessionCookie))
        {
            request.SetRequestHeader("Cookie", PremierHubSession.SessionCookie);
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.DataProcessingError)
        {
            Debug.LogWarning($"PremierHub login connection failed: {request.error}");
            onComplete?.Invoke(new LoginResult
            {
                Success = false,
                Error = "No se pudo conectar con el servidor"
            });
            yield break;
        }

        LoginResponse response = TryParseLoginResponse(request.downloadHandler.text);
        string responseError = response != null && !string.IsNullOrWhiteSpace(response.error)
            ? response.error
            : "Credenciales incorrectas";

        if (request.result == UnityWebRequest.Result.ProtocolError || response == null || !response.success)
        {
            Debug.LogWarning($"PremierHub login failed. Code: {request.responseCode}. Body: {request.downloadHandler.text}");
            onComplete?.Invoke(new LoginResult
            {
                Success = false,
                Error = responseError
            });
            yield break;
        }

        string cookie = ExtractSessionCookie(request.GetResponseHeader("Set-Cookie"));
        Debug.Log($"PremierHub login success. UserId: {(response.user != null ? response.user.id_usuario : 0)}");
        onComplete?.Invoke(new LoginResult
        {
            Success = true,
            UserId = response.user != null ? response.user.id_usuario : 0,
            SessionCookie = cookie
        });
    }

    private static IEnumerator EnsureConfigLoaded()
    {
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            yield break;
        }

        string envPath = Path.Combine(Application.streamingAssetsPath, ".env");

        if (envPath.Contains("://") || envPath.Contains("jar:"))
        {
            using UnityWebRequest request = UnityWebRequest.Get(envPath);
            yield return request.SendWebRequest();

            baseUrl = request.result == UnityWebRequest.Result.Success
                ? ParseApiUrl(request.downloadHandler.text)
                : DefaultApiUrl;
        }
        else if (File.Exists(envPath))
        {
            baseUrl = ParseApiUrl(File.ReadAllText(envPath));
        }
        else
        {
            baseUrl = DefaultApiUrl;
        }

        baseUrl = baseUrl.Trim().TrimEnd('/');
        Debug.Log($"PremierHub API URL: {baseUrl}");

        if (baseUrl.Contains("localhost") || baseUrl.Contains("127.0.0.1"))
        {
            Debug.LogWarning("PREMIERHUB_API_URL points to a local backend. Use the public backend URL for standalone builds.");
        }
    }

    private static string ParseApiUrl(string envText)
    {
        if (string.IsNullOrWhiteSpace(envText))
        {
            return DefaultApiUrl;
        }

        string[] lines = envText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (line.StartsWith("#", StringComparison.Ordinal) || !line.StartsWith(ApiUrlKey, StringComparison.Ordinal))
            {
                continue;
            }

            int separatorIndex = line.IndexOf('=');
            if (separatorIndex < 0)
            {
                continue;
            }

            string value = line.Substring(separatorIndex + 1).Trim().Trim('"');
            return string.IsNullOrWhiteSpace(value) ? DefaultApiUrl : value;
        }

        return DefaultApiUrl;
    }

    private static LoginResponse TryParseLoginResponse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonUtility.FromJson<LoginResponse>(json);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static string ExtractSessionCookie(string setCookieHeader)
    {
        if (string.IsNullOrWhiteSpace(setCookieHeader))
        {
            return string.Empty;
        }

        const string cookieName = "ph_session=";
        int start = setCookieHeader.IndexOf(cookieName, StringComparison.Ordinal);
        if (start < 0)
        {
            return string.Empty;
        }

        int end = setCookieHeader.IndexOf(';', start);
        return end < 0
            ? setCookieHeader.Substring(start).Trim()
            : setCookieHeader.Substring(start, end - start).Trim();
    }

    private static string CombineUrl(string root, string path)
    {
        return root.TrimEnd('/') + "/" + path.TrimStart('/');
    }
}

using System.Collections;
using UnityEngine;

public class SpotifyAuthManager : MonoBehaviour
{
    private const string clientId = "442d078301b0487e97a4b7d7bb9d0f33";
    private const string redirectUri = "myunityapp://callback";
    private const string scope = "user-read-private user-read-email";
    private string accessToken = "";

    /// <summary>
    /// Initiates the Spotify authentication process.
    /// </summary>
    public void AuthenticateWithSpotify()
    {
        string authUrl = $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=token&redirect_uri={redirectUri}&scope={scope}";
        Application.OpenURL(authUrl);
    }

    /// <summary>
    /// Sets the access token after redirection from Spotify.
    /// </summary>
    public void SetAccessToken(string url)
    {
        // Extract the access token from the URL
        if (url.Contains("access_token="))
        {
            string token = url.Split(new string[] { "access_token=" }, System.StringSplitOptions.None)[1];
            token = token.Split('&')[0];
            accessToken = token;
            Debug.Log("Spotify Access Token set: " + accessToken);
            StartCoroutine(FetchSpotifyUserProfile());
        }
    }

    private IEnumerator FetchSpotifyUserProfile()
    {
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get("https://api.spotify.com/v1/me"))
        {
            www.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return www.SendWebRequest();

            if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching Spotify user profile: " + www.error);
            }
            else
            {
                Debug.Log("Spotify User Profile: " + www.downloadHandler.text);
            }
        }
    }
}
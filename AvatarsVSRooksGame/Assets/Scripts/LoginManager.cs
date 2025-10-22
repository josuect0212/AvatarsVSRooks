using UnityEngine;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Auth;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text errorText;
    private FirebaseAuth auth;

    void Start()
    {
        StartCoroutine(WaitForFirebaseInitialization());
    }

    private System.Collections.IEnumerator WaitForFirebaseInitialization()
    {
        while (!FirebaseInitializer.IsFirebaseInitialized)
        {
            Debug.Log("Waiting for Firebase to initialize...");
            yield return null;
        }

        auth = FirebaseAuth.DefaultInstance;
        errorText.text = "";
        Debug.Log("Firebase Auth initialized.");
    }

    /// <summary>
    /// Validates the user credentials.
    /// </summary>
    public void OnSubmitButtonPressed()
    {
        if (auth == null)
        {
            Debug.LogError("Firebase Auth is not initialized.");
            errorText.text = "Error! Firebase not initialized.";
            return;
        }

        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
        {
            errorText.text = "Error! Please fill in all fields.";
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(username, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("User signed in successfully.");
                SceneManager.LoadScene("LoggedIn");
            }
            else
            {
                Debug.LogError($"Sign-in failed: {task.Exception}");
                errorText.text = "Error! Invalid username or password.";
                clearInputFields();
            }
        });
    }

    private void clearInputFields()
    {
        usernameInput.text = "";
        passwordInput.text = "";
    }
}
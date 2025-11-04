using UnityEngine;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Auth;

public class RegisterManager : MonoBehaviour
{

    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text errorText;
    private FirebaseAuth auth;

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

    void Start()
    {
        StartCoroutine(WaitForFirebaseInitialization());
    }
    /// <summary>
    /// Registers the new user.
    /// </summary>
    public void OnSubmitReg()
    {

        string username = usernameInput.text;
        string password = passwordInput.text;
        //this is for an Ideal confirm password field
        //string auxPassword = auxPasswordInput.text;

        if (auth == null)
        {
            Debug.LogError("Firebase Auth is not initialized.");
            errorText.text = "Error! Firebase not initialized.";
            return;
        }

        if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
        {
            errorText.text = "Error! Please fill in all fields.";
            return;
        }

    }

    void Update()
    {
        
    }
}

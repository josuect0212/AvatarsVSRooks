using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class RegisterManager : MonoBehaviour
{
    [Header("Inputs (assign in Inspector)")]
    [SerializeField] private TMP_InputField inputEmail;
    [SerializeField] private TMP_InputField inputUsername;
    [SerializeField] private TMP_InputField inputPassword;
    [SerializeField] private TMP_Dropdown dropdownRole;

    [Header("UI (assign in Inspector)")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private UnityEngine.UI.Button buttonRegister;

    private FirebaseAuth auth;
    private DatabaseReference dbRef;

    private void Start()
    {
        statusText.text = "";
        StartCoroutine(WaitForFirebaseInitialization());
    }

    private IEnumerator WaitForFirebaseInitialization()
    {
        while (!FirebaseInitializer.IsFirebaseInitialized)
        {
            yield return null;
        }

        auth = FirebaseAuth.DefaultInstance;

        FirebaseDatabase db = FirebaseDatabase.GetInstance(
            FirebaseInitializer.app,
            "https://avatarsvsrooks-default-rtdb.firebaseio.com/"
        );
        dbRef = db.RootReference;

        Debug.Log("Firebase inicializado en RegisterManager");
    }

    public void OnRegisterButtonPressed()
    {
        statusText.text = "";

        if (auth == null)
        {
            statusText.text = "Firebase no inicializado";
            return;
        }

        string email = inputEmail.text.Trim();
        string username = inputUsername.text.Trim();
        string password = inputPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Completa todos los campos.";
            return;
        }

        if (password.Length < 6)
        {
            statusText.text = "La contraseña debe ser de 6+ caracteres.";
            return;
        }

        buttonRegister.interactable = false;
        CheckUsernameAvailability(username, email, password);
    }

    private void CheckUsernameAvailability(string username, string email, string password)
    {
        dbRef.Child("users").OrderByChild("username").EqualTo(username).GetValueAsync()
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
                statusText.text = "Error en la base de datos.";
                buttonRegister.interactable = true;
                return;
            }

            if (task.Result.Exists)
            {
                statusText.text = "Nombre de usuario no disponible.";
                buttonRegister.interactable = true;
                return;
            }

            CreateAuthUser(username, email, password);
        });
    }

    private void CreateAuthUser(string username, string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password)
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log(task.Exception);
                statusText.text = TranslateAuthError(task.Exception);
                buttonRegister.interactable = true;
                return;
            }

            FirebaseUser newUser = task.Result.User;
            Debug.Log("Usuario creado: " + newUser.UserId);

            SaveUserData(newUser.UserId, username, email);
        });
    }

    private void SaveUserData(string uid, string username, string email)
    {
        string role = dropdownRole.value == 1 ? "admin" : "player";
        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var userData = new Dictionary<string, object>()
        {
            { "uid", uid },
            { "email", email },
            { "username", username },
            { "role", role },
            { "createdAt", createdAt }
        };

        dbRef.Child("users").Child(uid).SetValueAsync(userData)
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
                statusText.text = "Error guardando datos.";
                buttonRegister.interactable = true;
                return;
            }

            statusText.text = "Registro exitoso 🎉";
            Debug.Log("Registro completo");

            // Cambia la escena si quieres
            // SceneManager.LoadScene("Login");
        });
    }

    private string TranslateAuthError(AggregateException ex)
    {
        string message = "Error al registrar.";

        var flat = ex.Flatten();
        if (flat.InnerExceptions.Count == 0) return message;

        var inner = flat.InnerExceptions[0];
        if (inner is Firebase.FirebaseException fbEx)
        {
            var errorCode = (AuthError)fbEx.ErrorCode;
            switch (errorCode)
            {
                case AuthError.EmailAlreadyInUse: message = "Este email ya está registrado."; break;
                case AuthError.InvalidEmail: message = "Correo inválido."; break;
                case AuthError.WeakPassword: message = "Contraseña débil."; break;
                default: message = fbEx.Message; break;
            }
        }
        return message;
    }
//verificar si el usuario desea cancelar el registro
    public void OnCancelPressed()
    {
        SceneManager.LoadScene("Login");
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class RegisterManager : MonoBehaviour
{
    [Header("Inputs (assign in Inspector)")]
    public TMP_InputField inputEmail;
    public TMP_InputField inputUsername;
    public TMP_InputField inputPassword;
    public TMP_Dropdown dropdownRole; // 0 = Jugador, 1 = Administrador

    [Header("UI")]
    public TMPro.TextMeshProUGUI statusText;
    public GameObject buttonRegister; // assign the Button GameObject

    // Firebase refs
    private FirebaseAuth auth;
    private DatabaseReference dbRef;

    void Start()
    {
        SetStatus("");
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("Firebase inicializado correctamente.");
            }
            else
            {
                Debug.LogError($"Dependencias Firebase faltantes: {status}");
                SetStatus("Error inicializando Firebase. Revisa la consola.");
            }
        });
    }

    public void OnRegisterButtonPressed()
    {
        SetStatus("");
        string email = inputEmail.text.Trim();
        string username = inputUsername.text.Trim();
        string password = inputPassword.text;

        // Validaciones simples
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetStatus("Rellena todos los campos.");
            return;
        }

        if (password.Length < 6)
        {
            SetStatus("La contraseña debe tener al menos 6 caracteres.");
            return;
        }

        // bloquear botón para evitar múltiples clicks
        var btn = buttonRegister.GetComponent<UnityEngine.UI.Button>();
        if (btn != null) btn.interactable = false;

        // comprobar username único en DB, luego crear usuario
        CheckUsernameAndRegister(username, email, password);
    }

    void CheckUsernameAndRegister(string username, string email, string password)
    {
        // Query: users where username == provided username
        Query usernameQuery = FirebaseDatabase.DefaultInstance
            .GetReference("users")
            .OrderByChild("username")
            .EqualTo(username);

        usernameQuery.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error consultando username: " + task.Exception);
                SetStatus("Error en la base de datos. Intenta luego.");
                UnlockRegisterButton();
                return;
            }

            var snapshot = task.Result;
            if (snapshot.Exists)
            {
                SetStatus("El usuario ya existe. Elige otro.");
                UnlockRegisterButton();
            }
            else
            {
                // username libre -> crear en Auth
                CreateAuthUser(email, password);
            }
        });
    }

    void CreateAuthUser(string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Registro cancelado.");
                SetStatus("Registro cancelado.");
                UnlockRegisterButton();
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("Error al crear usuario: " + task.Exception);
                string msg = ParseFirebaseAuthError(task.Exception);
                SetStatus(msg);
                UnlockRegisterButton();
                return;
            }

            FirebaseUser newUser = task.Result.User;
            Debug.LogFormat("Usuario creado: {0} ({1})", newUser.Email, newUser.UserId);

            // guardar datos en Realtime DB
            SaveAdditionalUserData(newUser);
        });
    }

    void SaveAdditionalUserData(FirebaseUser firebaseUser)
    {
        string uid = firebaseUser.UserId;
        string username = inputUsername.text.Trim();
        string email = inputEmail.text.Trim();
        string role = dropdownRole.value == 1 ? "admin" : "player";
        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Dictionary<string, object> userData = new Dictionary<string, object>()
        {
            {"uid", uid},
            {"email", email},
            {"username", username},
            {"role", role},
            {"createdAt", createdAt}
        };

        dbRef.Child("users").Child(uid).SetValueAsync(userData).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al guardar datos: " + task.Exception);
                SetStatus("Error guardando datos. El usuario fue creado en Auth, pero falló la DB.");
                UnlockRegisterButton();
            }
            else
            {
                SetStatus("Registro exitoso.");
                Debug.Log("Datos del usuario guardados en Realtime DB.");
                // Opcional: redirigir a Login o Main Scene
                // SceneManager.LoadScene("Login");
            }
        });
    }

    string ParseFirebaseAuthError(System.AggregateException exception)
    {
        string msg = "Error al registrar.";
        if (exception == null) return msg;

        var flat = exception.Flatten();
        if (flat.InnerExceptions.Count > 0)
        {
            var inner = flat.InnerExceptions[0];
            if (inner is FirebaseException fbEx)
            {
                var code = (AuthError)fbEx.ErrorCode;
                switch (code)
                {
                    case AuthError.EmailAlreadyInUse:
                        msg = "El correo ya está registrado.";
                        break;
                    case AuthError.InvalidEmail:
                        msg = "Correo inválido.";
                        break;
                    case AuthError.WeakPassword:
                        msg = "Contraseña débil.";
                        break;
                    default:
                        msg = fbEx.Message;
                        break;
                }
            }
            else
            {
                msg = inner.Message;
            }
        }
        return msg;
    }

    void UnlockRegisterButton()
    {
        var btn = buttonRegister.GetComponent<UnityEngine.UI.Button>();
        if (btn != null) btn.interactable = true;
    }

    void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    // Botón Cancel: vuelve a la escena Login (si la tienes)
    public void OnCancelPressed()
    {
        // SceneManager.LoadScene("Login");
    }
}

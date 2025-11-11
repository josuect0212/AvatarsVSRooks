using System;
<<<<<<< HEAD
=======
using System.Collections;
>>>>>>> main
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
<<<<<<< HEAD
    public TMP_InputField inputEmail;
    public TMP_InputField inputUsername;
    public TMP_InputField inputPassword;
    public TMP_Dropdown dropdownRole; // 0 = Jugador, 1 = Administrador

    [Header("UI")]
    public TMPro.TextMeshProUGUI statusText;
    public GameObject buttonRegister; // assign the Button GameObject
=======
    [SerializeField] private TMP_InputField inputEmail;
    [SerializeField] private TMP_InputField inputUsername;
    [SerializeField] private TMP_InputField inputPassword;
    [SerializeField] private TMP_Dropdown dropdownRole; // 0 = Jugador, 1 = Administrador

    [Header("UI")]
    [SerializeField] private TMP_Text statusText; // usa TMP_Text (coherente con LoginManager)
    [SerializeField] private GameObject buttonRegister; // asigna el GameObject del botón
>>>>>>> main

    // Firebase refs
    private FirebaseAuth auth;
    private DatabaseReference dbRef;

    void Start()
    {
<<<<<<< HEAD
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
=======
        statusText.text = "";
        StartCoroutine(WaitForFirebaseInitialization());
    }

    private IEnumerator WaitForFirebaseInitialization()
    {
        while (!FirebaseInitializer.IsFirebaseInitialized)
        {
            Debug.Log("Esperando inicialización de Firebase...");
            yield return null;
        }

        auth = FirebaseAuth.DefaultInstance;

        // 🔧 Solución: inicializar la base con URL manual
        var app = FirebaseApp.DefaultInstance;
        FirebaseDatabase db = FirebaseDatabase.GetInstance(app, "https://avatarsvsrooks-default-rtdb.firebaseio.com/");
        dbRef = db.RootReference;

        Debug.Log("Firebase inicializado en RegisterManager.");
>>>>>>> main
    }

    public void OnRegisterButtonPressed()
    {
<<<<<<< HEAD
        SetStatus("");
=======
        statusText.text = "";

        if (auth == null)
        {
            Debug.LogError("Firebase Auth no inicializado.");
            statusText.text = "Error: Firebase no inicializado.";
            return;
        }

>>>>>>> main
        string email = inputEmail.text.Trim();
        string username = inputUsername.text.Trim();
        string password = inputPassword.text;

        // Validaciones simples
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
<<<<<<< HEAD
            SetStatus("Rellena todos los campos.");
=======
            statusText.text = "Rellena todos los campos.";
>>>>>>> main
            return;
        }

        if (password.Length < 6)
        {
<<<<<<< HEAD
            SetStatus("La contraseña debe tener al menos 6 caracteres.");
=======
            statusText.text = "La contraseña debe tener al menos 6 caracteres.";
>>>>>>> main
            return;
        }

        // bloquear botón para evitar múltiples clicks
<<<<<<< HEAD
        var btn = buttonRegister.GetComponent<UnityEngine.UI.Button>();
        if (btn != null) btn.interactable = false;
=======
        var btnComp = buttonRegister.GetComponent<UnityEngine.UI.Button>();
        if (btnComp != null) btnComp.interactable = false;
>>>>>>> main

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
<<<<<<< HEAD
                SetStatus("Error en la base de datos. Intenta luego.");
=======
                statusText.text = "Error en la base de datos. Intenta luego.";
>>>>>>> main
                UnlockRegisterButton();
                return;
            }

<<<<<<< HEAD
            var snapshot = task.Result;
            if (snapshot.Exists)
            {
                SetStatus("El usuario ya existe. Elige otro.");
=======
            DataSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                statusText.text = "El usuario ya existe. Elige otro.";
>>>>>>> main
                UnlockRegisterButton();
            }
            else
            {
                // username libre -> crear en Auth
<<<<<<< HEAD
                CreateAuthUser(email, password);
=======
                CreateAuthUser(username, email, password);
>>>>>>> main
            }
        });
    }

<<<<<<< HEAD
    void CreateAuthUser(string email, string password)
=======
    void CreateAuthUser(string username, string email, string password)
>>>>>>> main
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Registro cancelado.");
<<<<<<< HEAD
                SetStatus("Registro cancelado.");
=======
                statusText.text = "Registro cancelado.";
>>>>>>> main
                UnlockRegisterButton();
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("Error al crear usuario: " + task.Exception);
                string msg = ParseFirebaseAuthError(task.Exception);
<<<<<<< HEAD
                SetStatus(msg);
=======
                statusText.text = msg;
                UnlockRegisterButton();
                return;
            }

            // 1) Intentar obtener el usuario desde auth.CurrentUser (recomendado)
            FirebaseUser newUser = auth.CurrentUser;

            // 2) Si es null, intentar usar reflexión para sacar Result / User
            if (newUser == null)
            {
                try
                {
                    var taskType = task.GetType();
                    var resultProp = taskType.GetProperty("Result");
                    if (resultProp != null)
                    {
                        object resultValue = resultProp.GetValue(task);
                        if (resultValue != null)
                        {
                            // Si resultValue tiene propiedad "User" (AuthResult.User)
                            var userProp = resultValue.GetType().GetProperty("User");
                            if (userProp != null)
                            {
                                object userObj = userProp.GetValue(resultValue);
                                if (userObj is FirebaseUser fu)
                                {
                                    newUser = fu;
                                }
                            }

                            // Si resultValue es directamente un FirebaseUser
                            if (newUser == null && resultValue is FirebaseUser fu2)
                            {
                                newUser = fu2;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Reflexión: no se pudo extraer usuario desde task.Result: " + ex.Message);
                }
            }

            if (newUser == null)
            {
                Debug.LogError("No se pudo obtener el usuario creado (newUser == null).");
                statusText.text = "Error inesperado al crear usuario.";
>>>>>>> main
                UnlockRegisterButton();
                return;
            }

<<<<<<< HEAD
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
=======
            Debug.LogFormat("Usuario creado: {0} ({1})", newUser.Email, newUser.UserId);

            // guardar datos en Realtime DB
            SaveAdditionalUserData(newUser, username, email);
        });
    }

    void SaveAdditionalUserData(FirebaseUser firebaseUser, string username, string email)
    {
        string uid = firebaseUser.UserId;
        string role = dropdownRole != null && dropdownRole.value == 1 ? "admin" : "player";
>>>>>>> main
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
<<<<<<< HEAD
                SetStatus("Error guardando datos. El usuario fue creado en Auth, pero falló la DB.");
=======
                statusText.text = "Error guardando datos. El usuario fue creado en Auth, pero falló la DB.";
>>>>>>> main
                UnlockRegisterButton();
            }
            else
            {
<<<<<<< HEAD
                SetStatus("Registro exitoso.");
                Debug.Log("Datos del usuario guardados en Realtime DB.");
                // Opcional: redirigir a Login o Main Scene
=======
                statusText.text = "Registro exitoso.";
                Debug.Log("Datos del usuario guardados en Realtime DB.");
                // Opcional: redirigir a Login o a la escena principal
>>>>>>> main
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

<<<<<<< HEAD
    void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    // Botón Cancel: vuelve a la escena Login (si la tienes)
    public void OnCancelPressed()
    {
        // SceneManager.LoadScene("Login");
=======
    // Botón Cancel: vuelve a la escena Login (si la tienes)
    public void OnCancelPressed()
    {
        SceneManager.LoadScene("Login");
>>>>>>> main
    }
}

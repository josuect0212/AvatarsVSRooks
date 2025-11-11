<<<<<<< Updated upstream
=======
using System;
using System.Collections;
using System.Collections.Generic;
>>>>>>> Stashed changes
using UnityEngine;

public class RegisterManager : MonoBehaviour
{
<<<<<<< Updated upstream
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
=======
    [Header("Inputs (assign in Inspector)")]
    [SerializeField] private TMP_InputField inputEmail;
    [SerializeField] private TMP_InputField inputUsername;
    [SerializeField] private TMP_InputField inputPassword;
    [SerializeField] private TMP_Dropdown dropdownRole; // 0 = Jugador, 1 = Administrador

    [Header("UI")]
    [SerializeField] private TMP_Text statusText; // usa TMP_Text (coherente con LoginManager)
    [SerializeField] private GameObject buttonRegister; // asigna el GameObject del botón

    // Firebase refs
    private FirebaseAuth auth;
    private DatabaseReference dbRef;

    void Start()
    {
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
    }

    public void OnRegisterButtonPressed()
    {
        statusText.text = "";

        if (auth == null)
        {
            Debug.LogError("Firebase Auth no inicializado.");
            statusText.text = "Error: Firebase no inicializado.";
            return;
        }

        string email = inputEmail.text.Trim();
        string username = inputUsername.text.Trim();
        string password = inputPassword.text;

        // Validaciones simples
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Rellena todos los campos.";
            return;
        }

        if (password.Length < 6)
        {
            statusText.text = "La contraseña debe tener al menos 6 caracteres.";
            return;
        }

        // bloquear botón para evitar múltiples clicks
        var btnComp = buttonRegister.GetComponent<UnityEngine.UI.Button>();
        if (btnComp != null) btnComp.interactable = false;

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
                statusText.text = "Error en la base de datos. Intenta luego.";
                UnlockRegisterButton();
                return;
            }

            DataSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                statusText.text = "El usuario ya existe. Elige otro.";
                UnlockRegisterButton();
            }
            else
            {
                // username libre -> crear en Auth
                CreateAuthUser(username, email, password);
            }
        });
    }

    void CreateAuthUser(string username, string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Registro cancelado.");
                statusText.text = "Registro cancelado.";
                UnlockRegisterButton();
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("Error al crear usuario: " + task.Exception);
                string msg = ParseFirebaseAuthError(task.Exception);
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
                UnlockRegisterButton();
                return;
            }

            Debug.LogFormat("Usuario creado: {0} ({1})", newUser.Email, newUser.UserId);

            // guardar datos en Realtime DB
            SaveAdditionalUserData(newUser, username, email);
        });
    }

    void SaveAdditionalUserData(FirebaseUser firebaseUser, string username, string email)
    {
        string uid = firebaseUser.UserId;
        string role = dropdownRole != null && dropdownRole.value == 1 ? "admin" : "player";
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
                statusText.text = "Error guardando datos. El usuario fue creado en Auth, pero falló la DB.";
                UnlockRegisterButton();
            }
            else
            {
                statusText.text = "Registro exitoso.";
                Debug.Log("Datos del usuario guardados en Realtime DB.");
                // Opcional: redirigir a Login o a la escena principal
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

    // Botón Cancel: vuelve a la escena Login (si la tienes)
    public void OnCancelPressed()
    {
        SceneManager.LoadScene("Login");
>>>>>>> Stashed changes
    }
}

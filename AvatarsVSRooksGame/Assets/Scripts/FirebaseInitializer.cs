using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Database;

public class FirebaseInitializer : MonoBehaviour
{
    public static bool IsFirebaseInitialized { get; private set; } = false;
    public static FirebaseApp app;

    void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;

                
                app.Options.DatabaseUrl = new System.Uri("https://avatarsvsrooks-default-rtdb.firebaseio.com/");

                IsFirebaseInitialized = true;
                Debug.Log("Firebase is ready to use with Realtime Database.");
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }
}

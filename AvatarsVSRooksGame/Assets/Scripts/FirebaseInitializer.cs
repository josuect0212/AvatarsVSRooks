using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;

public class FirebaseInitializer : MonoBehaviour
{
    public static bool IsFirebaseInitialized { get; private set; } = false;

    void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                IsFirebaseInitialized = true;
                Debug.Log("Firebase is ready to use.");
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }
}

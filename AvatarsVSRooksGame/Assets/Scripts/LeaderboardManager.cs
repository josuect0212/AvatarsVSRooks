using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    private FirebaseAuth auth;
    private DatabaseReference dbRef;
    private bool firebaseReady = false;

    public event Action OnFirebaseReady;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);
        InitializeFirebase();
    }

    public bool IsReady() => firebaseReady;

    private void InitializeFirebase()
    {
        Debug.Log("LeaderboardManager: Inicializando Firebase...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var result = task.Result;

            if (result != DependencyStatus.Available)
            {
                Debug.LogError("Firebase dependencias NO listas: " + result);
                return;
            }

            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            dbRef = FirebaseDatabase.GetInstance(app,
                "https://avatarsvsrooks-default-rtdb.firebaseio.com/")
                .RootReference;

            firebaseReady = true;
            Debug.Log("✔ Firebase listo para LeaderboardManager");

            OnFirebaseReady?.Invoke();
        });
    }

    [Serializable]
    public class LeaderEntry
    {
        public string uid;
        public string username;
        public double totalTime;
        public long timestamp;
    }

    public void SubmitTotalTime(double totalTime, Action<bool, string> callback = null)
    {
        if (!firebaseReady)
        {
            callback?.Invoke(false, "Firebase no listo");
            return;
        }

        var user = auth.CurrentUser;
        if (user == null)
        {
            auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(loginTask =>
            {
                if (!loginTask.IsFaulted && loginTask.IsCompleted)
                    SubmitTotalTime(totalTime, callback);
                else
                    callback?.Invoke(false, "Error auth");
            });
            return;
        }

        string uid = user.UserId;

        dbRef.Child("leaderboard").Child(uid).GetValueAsync().ContinueWithOnMainThread(readTask =>
        {
            bool shouldWrite = true;
            if (readTask.Result.Exists)
            {
                try
                {
                    double prev = Convert.ToDouble(readTask.Result.Child("totalTime").Value);
                    if (prev <= totalTime) shouldWrite = false;
                }
                catch { }
            }

            if (!shouldWrite)
            {
                callback?.Invoke(true, "Ya tienes mejor tiempo");
                return;
            }

            dbRef.Child("users").Child(uid).Child("username").GetValueAsync().ContinueWithOnMainThread(nameTask =>
            {
                string username = nameTask.Result.Exists ?
                    nameTask.Result.Value.ToString() :
                    (user.DisplayName ?? "Player");

                var entry = new Dictionary<string, object>()
                {
                    {"uid", uid},
                    {"username", username},
                    {"totalTime", totalTime},
                    {"timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds()}
                };

                dbRef.Child("leaderboard").Child(uid).SetValueAsync(entry)
                .ContinueWithOnMainThread(writeTask =>
                {
                    if (!writeTask.IsFaulted)
                        callback?.Invoke(true, "Guardado");
                    else
                        callback?.Invoke(false, "Error guardando");
                });
            });
        });
    }

    public void GetTopN(int n, Action<List<LeaderEntry>> callback)
    {
        if (!firebaseReady)
        {
            callback?.Invoke(new List<LeaderEntry>());
            return;
        }

        Query q = dbRef.Child("leaderboard")
                       .OrderByChild("totalTime")
                       .LimitToFirst(n);

        q.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            var list = new List<LeaderEntry>();

            if (task.Result == null)
            {
                callback?.Invoke(list);
                return;
            }

            foreach (var child in task.Result.Children)
            {
                try
                {
                    list.Add(new LeaderEntry()
                    {
                        uid = child.Key,
                        username = child.Child("username").Value.ToString(),
                        totalTime = Convert.ToDouble(child.Child("totalTime").Value),
                        timestamp = Convert.ToInt64(child.Child("timestamp").Value)
                    });
                }
                catch { }
            }

            callback?.Invoke(list);
        });
    }
}

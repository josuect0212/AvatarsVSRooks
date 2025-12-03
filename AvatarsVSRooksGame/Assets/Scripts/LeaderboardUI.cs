using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    public Transform contentParent;
    public GameObject rowPrefab;
    public int topN = 10;
    public TMP_Text statusText;

    private void OnEnable()
    {
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.OnFirebaseReady += OnFirebaseReady;
        }

        TryRefresh();
    }

    private void OnDisable()
    {
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.OnFirebaseReady -= OnFirebaseReady;
        }
    }

    private void OnFirebaseReady()
    {
        Debug.Log("LeaderboardUI: Firebase listo → Refresh automático");
        Refresh();
    }

    private void TryRefresh()
    {
        if (LeaderboardManager.Instance == null)
        {
            statusText.text = "Esperando sistema...";
            return;
        }

        if (!LeaderboardManager.Instance.IsReady())
        {
            statusText.text = "Conectando...";
            return;
        }

        Refresh();
    }

    public void Refresh()
    {
        if (contentParent == null || rowPrefab == null)
        {
            Debug.LogError("LeaderboardUI: falta asignar contentParent o rowPrefab.");
            return;
        }

        statusText.text = "Cargando...";

        foreach (Transform t in contentParent) Destroy(t.gameObject);

        LeaderboardManager.Instance.GetTopN(topN, (list) =>
        {
            if (list == null || list.Count == 0)
            {
                statusText.text = "Sin datos aún.";
                return;
            }

            statusText.text = "";
            int rank = 1;
            foreach (var e in list)
            {
                GameObject go = Instantiate(rowPrefab, contentParent);
                var txt = go.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.text = $"#{rank}  {e.username}   {e.totalTime:F2}s";
                }
                rank++;
            }
        });
    }
}

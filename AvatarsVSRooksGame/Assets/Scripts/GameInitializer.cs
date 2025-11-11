using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string loginSceneName = "Login";
    
    [Header("Theme Manager Prefab (Optional)")]
    [SerializeField] private GameObject themeManagerPrefab;

    private void Awake()
    {
        // Asegurar que el ThemeManager existe
        EnsureThemeManager();
        
        // Cargar la primera escena (Login)
        LoadFirstScene();
    }

    private void EnsureThemeManager()
    {
        if (ThemeManager.Instance == null)
        {
            GameObject themeManagerObj;
            
            if (themeManagerPrefab != null)
            {
                // Si hay un prefab asignado, instanciarlo
                themeManagerObj = Instantiate(themeManagerPrefab);
            }
            else
            {
                // Crear uno nuevo si no hay prefab
                themeManagerObj = new GameObject("ThemeManager");
                themeManagerObj.AddComponent<ThemeManager>();
            }
            
            DontDestroyOnLoad(themeManagerObj);
            Debug.Log("ThemeManager creado e inicializado");
        }
    }

    private void LoadFirstScene()
    {
        if (!string.IsNullOrEmpty(loginSceneName))
        {
            SceneManager.LoadScene(loginSceneName);
        }
    }
}
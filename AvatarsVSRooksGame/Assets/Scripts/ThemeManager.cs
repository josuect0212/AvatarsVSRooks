using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }

    [Header("Theme Settings")]
    public Color lightBackgroundColor = new Color(0.95f, 0.95f, 0.95f);
    public Color darkBackgroundColor = new Color(0.15f, 0.15f, 0.15f);
    public Color lightTextColor = Color.black;
    public Color darkTextColor = Color.white;

    [Header("Favorite Colors")]
    public Color[] availableColors = new Color[]
    {
        new Color(0.2f, 0.6f, 1f),    // Azul
        new Color(1f, 0.3f, 0.3f),    // Rojo
        new Color(0.3f, 0.8f, 0.3f),  // Verde
        new Color(1f, 0.7f, 0.2f),    // Naranja
        new Color(0.7f, 0.3f, 0.9f)   // Morado
    };

    private bool isDarkMode = false;
    private int currentColorIndex = 0;
    private List<Image> backgrounds = new List<Image>();
    private List<Text> texts = new List<Text>();
    private List<Button> buttons = new List<Button>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPreferences();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Limpiar referencias de la escena anterior
        CleanupNullReferences();
        // Aplicar tema a la nueva escena
        ApplyTheme();
    }

    private void CleanupNullReferences()
    {
        backgrounds.RemoveAll(item => item == null);
        texts.RemoveAll(item => item == null);
        buttons.RemoveAll(item => item == null);
    }

    // Registrar elementos UI
    public void RegisterBackground(Image background)
    {
        if (!backgrounds.Contains(background))
        {
            backgrounds.Add(background);
            background.color = isDarkMode ? darkBackgroundColor : lightBackgroundColor;
        }
    }

    public void RegisterText(Text text)
    {
        if (!texts.Contains(text))
        {
            texts.Add(text);
            text.color = isDarkMode ? darkTextColor : lightTextColor;
        }
    }

    public void RegisterButton(Button button)
    {
        if (!buttons.Contains(button))
        {
            buttons.Add(button);
            ColorBlock colors = button.colors;
            colors.normalColor = availableColors[currentColorIndex];
            colors.highlightedColor = availableColors[currentColorIndex] * 1.2f;
            colors.pressedColor = availableColors[currentColorIndex] * 0.8f;
            button.colors = colors;
        }
    }

    // Cambiar entre modo claro y oscuro
    public void ToggleTheme()
    {
        isDarkMode = !isDarkMode;
        ApplyTheme();
        SavePreferences();
    }

    // Cambiar color favorito
    public void SetFavoriteColor(int colorIndex)
    {
        if (colorIndex >= 0 && colorIndex < availableColors.Length)
        {
            currentColorIndex = colorIndex;
            ApplyButtonColors();
            SavePreferences();
        }
    }

    public void NextFavoriteColor()
    {
        currentColorIndex = (currentColorIndex + 1) % availableColors.Length;
        ApplyButtonColors();
        SavePreferences();
    }

    // Establecer color personalizado desde gradiente
    public void SetCustomFavoriteColor(Color customColor)
    {
        // Guardar el color personalizado en el primer slot o crear uno nuevo
        if (availableColors.Length > 0)
        {
            availableColors[0] = customColor;
            currentColorIndex = 0;
        }
        ApplyButtonColors();
        SaveCustomColor(customColor);
    }

    private void SaveCustomColor(Color color)
    {
        PlayerPrefs.SetFloat("CustomColorR", color.r);
        PlayerPrefs.SetFloat("CustomColorG", color.g);
        PlayerPrefs.SetFloat("CustomColorB", color.b);
        PlayerPrefs.SetFloat("CustomColorA", color.a);
        PlayerPrefs.SetInt("HasCustomColor", 1);
        PlayerPrefs.Save();
    }

    private void LoadCustomColor()
    {
        if (PlayerPrefs.GetInt("HasCustomColor", 0) == 1)
        {
            float r = PlayerPrefs.GetFloat("CustomColorR", 1f);
            float g = PlayerPrefs.GetFloat("CustomColorG", 1f);
            float b = PlayerPrefs.GetFloat("CustomColorB", 1f);
            float a = PlayerPrefs.GetFloat("CustomColorA", 1f);
            
            if (availableColors.Length > 0)
            {
                availableColors[0] = new Color(r, g, b, a);
            }
        }
    }

    // Aplicar tema completo
    private void ApplyTheme()
    {
        ApplyBackgroundColors();
        ApplyTextColors();
        ApplyButtonColors();
    }

    private void ApplyBackgroundColors()
    {
        Color bgColor = isDarkMode ? darkBackgroundColor : lightBackgroundColor;
        foreach (var bg in backgrounds)
        {
            if (bg != null)
                bg.color = bgColor;
        }
    }

    private void ApplyTextColors()
    {
        Color textColor = isDarkMode ? darkTextColor : lightTextColor;
        foreach (var text in texts)
        {
            if (text != null)
                text.color = textColor;
        }
    }

    private void ApplyButtonColors()
    {
        Color favoriteColor = availableColors[currentColorIndex];
        foreach (var button in buttons)
        {
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = favoriteColor;
                colors.highlightedColor = favoriteColor * 1.2f;
                colors.pressedColor = favoriteColor * 0.8f;
                colors.selectedColor = favoriteColor * 1.1f;
                button.colors = colors;
            }
        }
    }

    // Guardar y cargar preferencias
    private void SavePreferences()
    {
        PlayerPrefs.SetInt("DarkMode", isDarkMode ? 1 : 0);
        PlayerPrefs.SetInt("FavoriteColorIndex", currentColorIndex);
        PlayerPrefs.Save();
    }

    private void LoadPreferences()
    {
        isDarkMode = PlayerPrefs.GetInt("DarkMode", 0) == 1;
        currentColorIndex = PlayerPrefs.GetInt("FavoriteColorIndex", 0);
        LoadCustomColor();
    }

    // Getters
    public bool IsDarkMode() => isDarkMode;
    public Color GetCurrentFavoriteColor() => availableColors[currentColorIndex];
    public int GetCurrentColorIndex() => currentColorIndex;
}
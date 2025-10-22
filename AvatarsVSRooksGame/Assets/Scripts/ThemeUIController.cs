using UnityEngine;
using UnityEngine.UI;

// Script para controlar la UI del selector de temas
public class ThemeUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button toggleThemeButton;
    [SerializeField] private Text themeButtonText;
    [SerializeField] private Button[] colorButtons;

    private void Start()
    {
        SetupButtons();
        UpdateThemeButtonText();
    }

    private void SetupButtons()
    {
        if (toggleThemeButton != null)
        {
            toggleThemeButton.onClick.AddListener(OnToggleTheme);
        }

        for (int i = 0; i < colorButtons.Length; i++)
        {
            int colorIndex = i; // Capturar índice para el closure
            if (colorButtons[i] != null)
            {
                // Asignar color al botón
                if (i < ThemeManager.Instance.availableColors.Length)
                {
                    ColorBlock colors = colorButtons[i].colors;
                    Color color = ThemeManager.Instance.availableColors[i];
                    colors.normalColor = color;
                    colors.highlightedColor = color * 1.2f;
                    colors.pressedColor = color * 0.8f;
                    colorButtons[i].colors = colors;
                }

                colorButtons[i].onClick.AddListener(() => OnColorSelected(colorIndex));
            }
        }
    }

    private void OnToggleTheme()
    {
        ThemeManager.Instance.ToggleTheme();
        UpdateThemeButtonText();
    }

    private void OnColorSelected(int colorIndex)
    {
        ThemeManager.Instance.SetFavoriteColor(colorIndex);
        Debug.Log($"Color seleccionado: {colorIndex}");
    }

    private void UpdateThemeButtonText()
    {
        if (themeButtonText != null && ThemeManager.Instance != null)
        {
            themeButtonText.text = ThemeManager.Instance.IsDarkMode() ? "Modo Claro" : "Modo Oscuro";
        }
    }
}
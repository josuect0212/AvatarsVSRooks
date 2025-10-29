using UnityEngine;
using UnityEngine.UI;

public class ThemeToggleButton : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private Text buttonText; // Opcional: cambiar texto

    [Header("Textos (opcional)")]
    [SerializeField] private string lightModeText = "Modo Oscuro";
    [SerializeField] private string darkModeText = "Modo Claro";

    private void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleTheme);
        }

        UpdateButtonText();
    }

    private void ToggleTheme()
    {
        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.ToggleTheme();
            UpdateButtonText();
        }
        else
        {
            Debug.LogWarning("ThemeManager no encontrado");
        }
    }

    private void UpdateButtonText()
    {
        if (buttonText != null && ThemeManager.Instance != null)
        {
            buttonText.text = ThemeManager.Instance.IsDarkMode() ? darkModeText : lightModeText;
        }
    }
}
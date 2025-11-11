using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Script para registrar automáticamente elementos UI con el ThemeManager
public class UIElement : MonoBehaviour
{
    public enum ElementType
    {
        Background,
        Text,
        Button
    }

    [SerializeField] private ElementType elementType;

    private void Start()
    {
        RegisterWithThemeManager();
    }

    private void OnDestroy()
    {
        // Las referencias se limpian automáticamente en ThemeManager
    }

    private void RegisterWithThemeManager()
    {
        // Si no existe ThemeManager, crearlo automáticamente
        if (ThemeManager.Instance == null)
        {
            Debug.LogWarning("ThemeManager no encontrado. Creando uno automáticamente...");
            GameObject themeManagerObj = new GameObject("ThemeManager");
            themeManagerObj.AddComponent<ThemeManager>();
        }

        // Esperar un frame para asegurar que el ThemeManager se inicialice
        StartCoroutine(RegisterAfterInit());
    }

    private IEnumerator RegisterAfterInit()
    {
        yield return null; // Esperar un frame

        switch (elementType)
        {
            case ElementType.Background:
                Image image = GetComponent<Image>();
                if (image != null)
                    ThemeManager.Instance.RegisterBackground(image);
                break;

            case ElementType.Text:
                Text text = GetComponent<Text>();
                if (text != null)
                    ThemeManager.Instance.RegisterText(text);
                break;

            case ElementType.Button:
                Button button = GetComponent<Button>();
                if (button != null)
                    ThemeManager.Instance.RegisterButton(button);
                break;
        }
    }
}
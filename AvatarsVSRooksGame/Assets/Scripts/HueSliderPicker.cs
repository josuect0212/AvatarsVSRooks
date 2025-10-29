using UnityEngine;
using UnityEngine.UI;

// Opción más simple: Slider de Hue (matiz) con saturación y brillo fijos
public class HueSliderPicker : MonoBehaviour
{
[Header("Referencias UI")]
[SerializeField] private Slider hueSlider;
[SerializeField] private Image previewColor;
[SerializeField] private Image sliderBackground; // Imagen de fondo con gradiente
[SerializeField] private Button applyButton;

    [Header("Configuración")]
    [SerializeField] private float saturation = 1f; // 0-1
    [SerializeField] private float brightness = 1f; // 0-1

    private Color selectedColor = Color.red;

    private void Start()
    {
        if (hueSlider != null)
        {
            hueSlider.minValue = 0f;
            hueSlider.maxValue = 1f;
            hueSlider.onValueChanged.AddListener(OnHueChanged);
            
            // Colorear la barra del slider con gradiente
            CreateHueGradient();
        }

        if (applyButton != null)
        {
            applyButton.onClick.AddListener(ApplyColor);
        }

        // Inicializar con color actual
        InitializeFromCurrentColor();
    }

    private void InitializeFromCurrentColor()
    {
        if (ThemeManager.Instance != null)
        {
            Color currentColor = ThemeManager.Instance.GetCurrentFavoriteColor();
            float h, s, v;
            Color.RGBToHSV(currentColor, out h, out s, out v);
            
            if (hueSlider != null)
            {
                hueSlider.value = h;
            }
        }
    }

    private void CreateHueGradient()
    {
        if (sliderBackground == null) return;

        // Crear textura de gradiente horizontal
        int width = 360;
        int height = 1;
        Texture2D texture = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            float hue = (float)x / width;
            Color color = Color.HSVToRGB(hue, 1f, 1f);
            texture.SetPixel(x, 0, color);
        }

        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        // Crear sprite del gradiente
        Sprite gradientSprite = Sprite.Create(
            texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f)
        );

        sliderBackground.sprite = gradientSprite;
}

    private void OnHueChanged(float hue)
    {
        selectedColor = Color.HSVToRGB(hue, saturation, brightness);
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (previewColor != null)
        {
            previewColor.color = selectedColor;
        }
    }

    private void ApplyColor()
    {
        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.SetCustomFavoriteColor(selectedColor);
            Debug.Log($"Color aplicado: {selectedColor}");
        }
    }

    public Color GetSelectedColor()
    {
        return selectedColor;
    }
}
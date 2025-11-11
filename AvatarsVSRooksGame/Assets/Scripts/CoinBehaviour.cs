using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CoinBehavior : MonoBehaviour, IPointerClickHandler
{
    [Header("Valores de Monedas")]
    [SerializeField] private int coinValue = 25; // 25, 50 o 100
    
    [Header("Efectos Visuales")]
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatAmount = 15f; // En píxeles para UI
    [SerializeField] private bool enableFloat = true;
    
    [Header("Duración")]
    [SerializeField] private float lifetime = 10f; // Tiempo antes de desaparecer
    [SerializeField] private bool hasLifetime = true;
    [SerializeField] private float blinkStartTime = 7f; // Comenzar a parpadear a los 7 segundos
    [SerializeField] private float blinkSpeed = 5f;
    
    [Header("Animación de Recolección")]
    [SerializeField] private float collectAnimationDuration = 0.5f;
    [SerializeField] private float collectScaleMultiplier = 1.5f;
    
    private RookContainer container;
    private CoinSpawner spawner;
    private RectTransform rectTransform;
    private Image image;
    private Vector2 startPosition;
    private float timeAlive = 0f;
    private bool isCollecting = false;
    private CanvasGroup canvasGroup;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        
        // Agregar CanvasGroup si no existe (para fade)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    void Start()
    {
        startPosition = rectTransform.anchoredPosition;
        
        if (hasLifetime)
        {
            StartCoroutine(LifetimeRoutine());
        }
        
        //Debug.Log($"Moneda de {coinValue} creada en posición {startPosition}");
    }
    
    void Update()
    {
        if (isCollecting) return;
        
        timeAlive += Time.deltaTime;
        
        // Efecto de flotación (movimiento arriba y abajo)
        if (enableFloat)
        {
            float offsetY = Mathf.Sin(timeAlive * floatSpeed) * floatAmount;
            rectTransform.anchoredPosition = startPosition + new Vector2(0, offsetY);
        }
        
        // Efecto de parpadeo cuando está cerca de desaparecer
        if (hasLifetime && timeAlive > blinkStartTime)
        {
            float alpha = Mathf.Abs(Mathf.Sin(timeAlive * blinkSpeed));
            canvasGroup.alpha = Mathf.Lerp(0.3f, 1f, alpha);
        }
    }
    
    public void SetContainer(RookContainer rookContainer, CoinSpawner spawnerRef)
    {
        container = rookContainer;
        spawner = spawnerRef;
    }
    
    public int GetValue()
    {
        return coinValue;
    }
    
    IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        DestroyCoin(false);
    }
    
    // Implementación de IPointerClickHandler para detectar clics en UI
    public void OnPointerClick(PointerEventData eventData)
    {
        Collect();
    }
    
    // Método para recolectar la moneda
    public void Collect()
    {
        if (isCollecting) return;
        
        isCollecting = true;
        
        // Agregar el valor al CoinManager
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AddCoins(coinValue);
            //Debug.Log($"Moneda de {coinValue} recolectada! Total: {CoinManager.Instance.GetTotalCoins()}");
        }
        else
        {
            //Debug.LogWarning("CoinManager.Instance es null!");
        }
        
        // Iniciar animación de recolección
        StartCoroutine(CollectAnimation());
    }
    
    void DestroyCoin(bool collected)
    {
        // Liberar la casilla en el spawner
        if (spawner != null && container != null)
        {
            spawner.FreeContainer(container);
        }
        
        if (!collected)
        {
            Destroy(gameObject);
        }
    }
    
    IEnumerator CollectAnimation()
    {
        float elapsed = 0f;
        Vector3 startScale = rectTransform.localScale;
        Vector3 targetScale = startScale * collectScaleMultiplier;
        Vector2 startPos = rectTransform.anchoredPosition;
        
        // Posición objetivo (arriba)
        Vector2 targetPos = startPos + new Vector2(0, 200f);
        
        while (elapsed < collectAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / collectAnimationDuration;
            
            // Curva de animación más suave
            float smoothProgress = 1f - Mathf.Pow(1f - progress, 3f);
            
            // Escala: crece y luego se reduce
            float scaleProgress = Mathf.Sin(progress * Mathf.PI);
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, scaleProgress);
            
            // Movimiento hacia arriba con curva
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothProgress);
            
            // Fade out
            canvasGroup.alpha = 1f - progress;
            
            yield return null;
        }
        
        DestroyCoin(true);
        Destroy(gameObject);
    }
}
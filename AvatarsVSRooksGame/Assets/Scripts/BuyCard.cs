using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuyCard : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Rook Type")]
    public RookType rookType;
    
    [Header("Referencias")]
    public GameObject rook_Drag;
    public GameObject rook_Game;
    public Canvas canvas;
    public GameManager gameManager;
    
    [Header("Sistema de Monedas")]
    [SerializeField] private int cardCost = 50; // Costo de la carta
    [SerializeField] private Text costText; // Para mostrar el costo (opcional)
    [SerializeField] private Image cardImage; // Para feedback visual
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color cannotAffordColor = Color.gray;
    
    private GameObject rookDragInstance;
    private bool canAfford = true;

    void Start()
    {
        gameManager = GameManager.instance;
        SetupCardCost(); // Configurar costo según el tipo de rook
        UpdateCardVisual();
        
        // Mostrar el costo si hay un Text asignado
        if (costText != null)
        {
            costText.text = cardCost.ToString();
        }
    }

    void Update()
    {
        // Actualizar si el jugador puede comprar la carta
        bool previousCanAfford = canAfford;
        canAfford = CoinManager.Instance != null && CoinManager.Instance.CanAfford(cardCost);
        
        // Solo actualizar visual si cambió el estado
        if (previousCanAfford != canAfford)
        {
            UpdateCardVisual();
        }
    }
    
    void SetupCardCost()
    {
        switch (rookType)
        {
            case RookType.Sand:
                cardCost = 50;
                break;
            case RookType.Rock:
                cardCost = 100;
                break;
            case RookType.Fire:
                cardCost = 150;
                break;
            case RookType.Water:
                cardCost = 150;
                break;
        }
        
        if (costText != null)
        {
            costText.text = cardCost.ToString();
        }
    }
    
    void UpdateCardVisual()
    {
        if (cardImage != null)
        {
            cardImage.color = canAfford ? normalColor : cannotAffordColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Verificar si el jugador tiene suficientes monedas
        if (!canAfford)
        {
            Debug.Log($"No tienes suficientes monedas. Necesitas {cardCost}, tienes {CoinManager.Instance?.GetTotalCoins()}");
            StartCoroutine(ShakeCard());
            return;
        }
        
        // Crear instancia de arrastre
        rookDragInstance = Instantiate(rook_Drag, canvas.transform);
        rookDragInstance.transform.position = Input.mousePosition;
        
        // Configurar el tipo de rook en la instancia
        var rookController = rookDragInstance.GetComponent<RookController>();
        if (rookController != null)
        {
            rookController.rookType = rookType;
        }
        
        rookDragInstance.GetComponent<RookDrag>().card = this;
        gameManager.draggingRook = rookDragInstance;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rookDragInstance != null)
        {
            rookDragInstance.transform.position = Input.mousePosition;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (rookDragInstance == null) return;
        
        // Intentar colocar la torre
        bool placed = gameManager.PlaceRook();
        
        // Si se colocó exitosamente, cobrar las monedas
        if (placed && CoinManager.Instance != null)
        {
            if (CoinManager.Instance.SpendCoins(cardCost))
            {
                //Debug.Log($"Torre comprada por {cardCost} monedas. Monedas restantes: {CoinManager.Instance.GetTotalCoins()}");
            }
        }
        
        gameManager.draggingRook = null;
        Destroy(rookDragInstance);
    }
    
    // Efecto visual cuando no se puede comprar
    System.Collections.IEnumerator ShakeCard()
    {
        Vector3 originalPosition = transform.localPosition;
        float shakeDuration = 0.3f;
        float shakeMagnitude = 10f;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float x = originalPosition.x + Random.Range(-1f, 1f) * shakeMagnitude;
            transform.localPosition = new Vector3(x, originalPosition.y, originalPosition.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localPosition = originalPosition;
    }
    
    // Método público para cambiar el costo (útil para upgrades)
    public void SetCost(int newCost)
    {
        cardCost = newCost;
        if (costText != null)
        {
            costText.text = cardCost.ToString();
        }
        UpdateCardVisual();
    }
    
    public int GetCost()
    {
        return cardCost;
    }
}
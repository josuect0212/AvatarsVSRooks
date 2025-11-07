using UnityEngine;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;
    
    [Header("Sistema de Monedas")]
    [SerializeField] private int totalCoins = 100; // Monedas iniciales
    
    [Header("UI (Opcional)")]
    [SerializeField] private Text coinText; // Para UI Text
    [SerializeField] private TMPro.TextMeshProUGUI coinTextTMP; // Para TextMeshPro
    
    [Header("Sonidos (Opcional)")]
    [SerializeField] private AudioClip coinCollectSound;
    private AudioSource audioSource;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Configurar AudioSource si hay sonido
        if (coinCollectSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    void Start()
    {
        UpdateCoinDisplay();
    }
    
    public void AddCoins(int amount)
    {
        totalCoins += amount;
        UpdateCoinDisplay();
        
        // Reproducir sonido
        if (audioSource != null && coinCollectSound != null)
        {
            audioSource.PlayOneShot(coinCollectSound);
        }
    }
    
    public bool SpendCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            totalCoins -= amount;
            UpdateCoinDisplay();
            return true;
        }
        
        Debug.Log("No tienes suficientes monedas!");
        return false;
    }
    
    public int GetTotalCoins()
    {
        return totalCoins;
    }
    
    public bool CanAfford(int amount)
    {
        return totalCoins >= amount;
    }
    
    void UpdateCoinDisplay()
    {
        // Actualizar UI Text si está asignado
        if (coinText != null)
        {
            coinText.text = totalCoins.ToString();
        }
        
        // Actualizar TextMeshPro si está asignado
        if (coinTextTMP != null)
        {
            coinTextTMP.text = totalCoins.ToString();
        }
    }
    
    // Método para restablecer monedas (útil para reiniciar nivel)
    public void ResetCoins(int startAmount = 100)
    {
        totalCoins = startAmount;
        UpdateCoinDisplay();
    }
}
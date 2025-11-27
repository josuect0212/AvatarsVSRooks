using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RookController : MonoBehaviour
{
    [Header("Rook Type")]
    public RookType rookType;
    
    [Header("Combat")]
    public GameObject fire;
    public List<GameObject> avatars;
    public GameObject toAttack;
    public float attackCooldown = 4f;
    private float attackTime;
    public int damage;
    public int maxHealth;
    public int currentHealth;
    public bool isAttacking;
    public RookContainer currentRookContainer;
    
    [Header("Health Bar")]
    public GameObject healthBarPrefab; // Prefab del Canvas con la barra de vida
    private GameObject healthBarInstance;
    private Image healthBarFill;
    public Vector3 healthBarOffset = new Vector3(0, 30f, 0); // Offset en píxeles UI

    private void Start()
    {
        ApplyRookStats();
        currentHealth = maxHealth;
        SetupHealthBar();
    }

    private void Update()
    {
        avatars.RemoveAll(a => a == null);

        if (toAttack == null || !avatars.Contains(toAttack))
        {
            toAttack = avatars.Count > 0 ? avatars[0] : null;
        }

        if (toAttack != null)
        {
            if (Time.time >= attackTime)
            {
                GameObject fireInstance = Instantiate(fire, transform);
                fireInstance.GetComponent<Fire>().damage = damage;
                attackTime = Time.time + attackCooldown;
            }
        }
        
        // Actualizar posición de la barra de vida
        UpdateHealthBarPosition();
    }

    private void ApplyRookStats()
    {
        attackCooldown = 4f;
        
        switch (rookType)
        {
            case RookType.Sand:
                damage = 2;
                maxHealth = 3;
                break;
                
            case RookType.Rock:
                damage = 4;
                maxHealth = 14;
                break;
                
            case RookType.Fire:
                damage = 8;
                maxHealth = 16;
                break;
                
            case RookType.Water:
                damage = 8;
                maxHealth = 16;
                break;
        }
    }

    void SetupHealthBar()
    {
        if (healthBarPrefab == null) 
        {
            Debug.LogWarning($"[{rookType}] No hay healthBarPrefab asignado");
            return;
        }
        
        // Instanciar la barra de vida como hijo del mismo Canvas que el Rook
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, parentCanvas.transform);
        }
        else
        {
            healthBarInstance = Instantiate(healthBarPrefab);
        }
        
        // Asegurar que la escala sea correcta
        healthBarInstance.transform.localScale = Vector3.one;
        
        // Buscar la imagen "Fill" en los hijos
        Transform fillTransform = healthBarInstance.transform.Find("AvatarHealthBar/Fill Area/Fill");
        if (fillTransform != null)
        {
            healthBarFill = fillTransform.GetComponent<Image>();
        }
        
        // Si no lo encontramos con el path exacto, buscar por nombre
        if (healthBarFill == null)
        {
            Image[] allImages = healthBarInstance.GetComponentsInChildren<Image>();
            foreach (Image img in allImages)
            {
                if (img.gameObject.name == "Fill")
                {
                    healthBarFill = img;
                    break;
                }
            }
        }
        
        if (healthBarFill != null)
        {
            // Configurar la imagen para usar Fill
            healthBarFill.type = Image.Type.Filled;
            healthBarFill.fillMethod = Image.FillMethod.Horizontal;
            healthBarFill.fillOrigin = 0; // Izquierda a derecha
            healthBarFill.fillAmount = 1f; // 100% al inicio
            
            Debug.Log($"[{rookType}] ✅ Barra de vida configurada");
        }
        else
        {
            Debug.LogError($"[{rookType}] ❌ No se encontró Image 'Fill' en el prefab");
        }
    }

    void UpdateHealthBarPosition()
    {
        if (healthBarInstance == null) return;
        
        // La barra de vida sigue la posición del Rook con un offset
        healthBarInstance.transform.position = transform.position + healthBarOffset;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        
        // Actualizar barra de vida usando fillAmount
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / (float)maxHealth;
        }
        
        Debug.Log($"[{rookType}] 💔 -{damageAmount} HP ({currentHealth}/{maxHealth})");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"[{rookType}] 💀 Destruido");
        
        // Destruir la barra de vida
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
        
        // Liberar el contenedor
        if (currentRookContainer != null)
        {
            currentRookContainer.filled = false; 
        }
        
        Destroy(this.gameObject);
    }
}
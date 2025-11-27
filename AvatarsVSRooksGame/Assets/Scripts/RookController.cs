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
    public Slider healthBar;
    public Vector3 healthBarOffset = new Vector3(0, 1, 0);
    
    private Canvas healthBarCanvas;

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
        if (healthBarCanvas != null)
        {
            healthBarCanvas.transform.position = transform.position + healthBarOffset;
        }
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
        if (healthBar == null) return;
        
        // Crear canvas para la barra de vida
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(transform);
        healthBarCanvas = canvasObj.AddComponent<Canvas>();
        healthBarCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;
        
        // Configurar el slider
        healthBar.transform.SetParent(canvasObj.transform);
        RectTransform rect = healthBar.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(1, 0.2f);
        rect.localScale = Vector3.one * 0.01f;
        
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        
        // Actualizar barra de vida
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
        
        if (currentHealth <= 0)
        {
            if (currentRookContainer != null)
            {
                currentRookContainer.filled = false; 
            }
            Destroy(this.gameObject);          
        }
    }
}
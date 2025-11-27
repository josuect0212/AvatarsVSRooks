using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarController : MonoBehaviour
{
    [Header("Avatar Type")]
    public AvatarType avatarType;
    
    [Header("Stats")]
    public int maxHealth;
    public int currentHealth;
    public int damage;
    public float attackCooldown;
    public float movementSpeed = 0.5f;
    public bool isRangedAttack;
    public float attackRange = 3f;
    
    [Header("Ranged Attack Settings")]
    [Tooltip("Para unidades a rango: tolerancia horizontal para considerar 'misma columna' (en píxeles)")]
    public float columnTolerance = 30f;
    [Tooltip("Para unidades a rango: si es true, siguen moviéndose mientras atacan")]
    public bool moveWhileAttacking = false;
    
    [Header("Ranged Attack")]
    public GameObject projectilePrefab;
    
    [Header("Health Bar")]
    public GameObject healthBarPrefab; // Prefab del Canvas con la barra de vida
    private GameObject healthBarInstance;
    private Image healthBarFill; // Usamos Image en vez de Slider
    public Vector3 healthBarOffset = new Vector3(0, 30f, 0); // Offset en píxeles UI
    
    [Header("Coin Reward")]
    public int coinReward = 75;
    
    private bool isStopped;
    private float attackTimer;
    private RookController targetRook;
    private Canvas parentCanvas;

    void Start()
    {
        ApplyAvatarStats();
        currentHealth = maxHealth;
        SetupHealthBar();
        
        // Buscar el Canvas padre para instanciar proyectiles
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            parentCanvas = FindObjectOfType<Canvas>();
        }
        
        Debug.Log($"[{gameObject.name}] Iniciado - Tipo: {avatarType}, HP: {maxHealth}, Daño: {damage}, Ranged: {isRangedAttack}");
    }

    void Update()
    {
        // ============ VERIFICAR SI EL OBJETIVO SIGUE VIVO ============
        // Usar el operador == de Unity que detecta objetos destruidos
        bool targetDestroyed = false;
        if (!ReferenceEquals(targetRook, null))
        {
            // targetRook tiene una referencia, pero ¿el objeto fue destruido?
            if (targetRook == null) // El operador == de Unity retorna true si fue destruido
            {
                targetDestroyed = true;
            }
        }
        
        if (targetDestroyed)
        {
            Debug.Log($"[{avatarType}] 💀 Objetivo destruido, continuando movimiento...");
            targetRook = null;
            isStopped = false;
        }
        
        // ============ BUSCAR OBJETIVOS ============
        if (isRangedAttack)
        {
            SearchForRangedTarget();
        }
        else
        {
            // Las unidades melee: si no hay objetivo, moverse y buscar
            if (targetRook == null)
            {
                isStopped = false;
            }
            
            if (!isStopped)
            {
                SearchForMeleeTarget();
            }
        }
        
        // ============ MOVIMIENTO ============
        if (!isStopped)
        {
            transform.position += Vector3.up * movementSpeed * Time.deltaTime;
        }
        
        // ============ ATAQUE ============
        if (targetRook != null)
        {
            float distance = Vector3.Distance(transform.position, targetRook.transform.position);
            
            if (distance > attackRange)
            {
                Debug.Log($"[{avatarType}] 📏 Objetivo fuera de rango");
                targetRook = null;
                isStopped = false;
            }
            else
            {
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    Attack();
                    attackTimer = attackCooldown;
                }
            }
        }
        
        UpdateHealthBarPosition();
    }

    void ApplyAvatarStats()
    {
        switch (avatarType)
        {
            case AvatarType.Archer: // Avatar Flechador
                maxHealth = 5;
                damage = 2;
                attackCooldown = 10f; // Cada 10 segundos
                movementSpeed = 8.33f; // 100 píxeles / 12 segundos
                isRangedAttack = true;
                attackRange = 2000f;
                columnTolerance = 30f;
                moveWhileAttacking = false;
                break;
                
            case AvatarType.ShieldBearer: // Avatar Escudero
                maxHealth = 10;
                damage = 3;
                attackCooldown = 15f; // Cada 15 segundos
                movementSpeed = 10f; // 100 píxeles / 10 segundos
                isRangedAttack = true;
                attackRange = 2000f;
                columnTolerance = 30f;
                moveWhileAttacking = false;
                break;
                
            case AvatarType.Lumberjack: // Avatar Leñador
                maxHealth = 20;
                damage = 9;
                attackCooldown = 5f; // Cada 5 segundos (si hay torre enfrente)
                movementSpeed = 7.69f; // 100 píxeles / 13 segundos
                isRangedAttack = false;
                attackRange = 60f;
                break;
                
            case AvatarType.Cannibal: // Avatar Caníbal
                maxHealth = 25;
                damage = 12;
                attackCooldown = 3f; // Cada 3 segundos (si hay torre enfrente)
                movementSpeed = 7.14f; // 100 píxeles / 14 segundos
                isRangedAttack = false;
                attackRange = 60f;
                break;
        }
    }

    /// <summary>
    /// Busca el mejor objetivo para unidades a distancia.
    /// SOLO ataca Rooks que están directamente al frente (misma columna X).
    /// </summary>
    void SearchForRangedTarget()
    {
        // Si ya tenemos un objetivo válido, verificar que sigue en la misma columna y en rango
        if (targetRook != null)
        {
            float distanceX = Mathf.Abs(transform.position.x - targetRook.transform.position.x);
            float distanceY = transform.position.y - targetRook.transform.position.y; // Negativo si está arriba
            
            // Verificar que sigue al frente (misma columna) y en rango
            if (distanceX <= columnTolerance && distanceY < 0 && Mathf.Abs(distanceY) <= attackRange)
            {
                return; // Mantener objetivo actual
            }
            else
            {
                targetRook = null;
            }
        }
        
        // Buscar todos los Rooks en la escena
        RookController[] allRooks = FindObjectsOfType<RookController>();
        
        RookController bestTarget = null;
        float bestDistance = float.MaxValue;
        
        foreach (RookController rook in allRooks)
        {
            if (rook == null || !rook.gameObject.activeInHierarchy) continue;
            
            // Calcular distancia en X (horizontal) y Y (vertical)
            float distanceX = Mathf.Abs(transform.position.x - rook.transform.position.x);
            float distanceY = rook.transform.position.y - transform.position.y; // Positivo si el Rook está arriba
            
            // SOLO atacar si:
            // 1. Está en la misma columna (distancia X menor a la tolerancia)
            // 2. Está AL FRENTE (arriba del avatar, Y positivo)
            // 3. Está dentro del rango de ataque
            if (distanceX <= columnTolerance && distanceY > 0 && distanceY <= attackRange)
            {
                if (distanceY < bestDistance)
                {
                    bestDistance = distanceY;
                    bestTarget = rook;
                }
            }
        }
        
        // Si encontramos un objetivo
        if (bestTarget != null)
        {
            targetRook = bestTarget;
            attackTimer = 0.1f;
            isStopped = !moveWhileAttacking;
            Debug.Log($"[{avatarType}] 🎯 Objetivo al frente: {targetRook.rookType} a distancia {bestDistance:F0}");
        }
        else
        {
            targetRook = null;
            isStopped = false;
        }
    }

    /// <summary>
    /// Busca objetivos para unidades melee.
    /// Solo busca en un cono frontal (hacia arriba).
    /// </summary>
    void SearchForMeleeTarget()
    {
        RookController[] allRooks = FindObjectsOfType<RookController>();
        
        foreach (RookController rook in allRooks)
        {
            if (rook == null || !rook.gameObject.activeInHierarchy) continue;
            
            float distance = Vector3.Distance(transform.position, rook.transform.position);
            
            if (distance <= attackRange)
            {
                // Para melee, verificar que el Rook está adelante (arriba en la pantalla)
                Vector3 dirToRook = (rook.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(Vector3.up, dirToRook);
                
                // Solo atacar si está en un cono de 120° al frente
                if (angle <= 60f)
                {
                    targetRook = rook;
                    isStopped = true;
                    attackTimer = 0.1f;
                    
                    Debug.Log($"[{avatarType}] 🎯 Objetivo melee: {rook.rookType}");
                    return;
                }
            }
        }
    }

    void SetupHealthBar()
    {
        if (healthBarPrefab == null) 
        {
            Debug.LogWarning($"[{avatarType}] No hay healthBarPrefab asignado");
            return;
        }
        
        // Instanciar la barra de vida como hijo del mismo Canvas que el avatar
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, parentCanvas.transform);
        }
        else
        {
            healthBarInstance = Instantiate(healthBarPrefab);
        }
        
        // Asegurar que la escala sea correcta (el prefab tiene escala 0)
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
            
            Debug.Log($"[{avatarType}] ✅ Barra de vida configurada con Image.fillAmount");
        }
        else
        {
            Debug.LogError($"[{avatarType}] ❌ No se encontró Image 'Fill' en el prefab");
        }
    }

    void UpdateHealthBarPosition()
    {
        if (healthBarInstance == null) return;
        
        // La barra de vida sigue la posición del avatar con un offset
        healthBarInstance.transform.position = transform.position + healthBarOffset;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Solo para melee - las unidades a rango usan SearchForRangedTarget
        if (!isRangedAttack && collision.gameObject.layer == 10)
        {
            RookController rook = collision.GetComponent<RookController>();
            if (rook == null) rook = collision.GetComponentInParent<RookController>();
            
            if (rook != null && targetRook == null)
            {
                targetRook = rook;
                isStopped = true;
                attackTimer = 0.1f;
                
                Debug.Log($"[{avatarType}] 🎯 Colisión melee: {rook.rookType}");
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (!isRangedAttack && collision.gameObject.layer == 10)
        {
            RookController rook = collision.GetComponent<RookController>();
            if (rook == null) rook = collision.GetComponentInParent<RookController>();
            
            if (rook != null && rook == targetRook)
            {
                targetRook = null;
                isStopped = false;
            }
        }
    }

    void Attack()
    {
        // Doble verificación de que el objetivo existe
        if (targetRook == null || !targetRook.gameObject.activeInHierarchy)
        {
            Debug.Log($"[{avatarType}] ⚠️ Objetivo ya no existe");
            targetRook = null;
            isStopped = false;
            return;
        }
        
        if (isRangedAttack)
        {
            // ATAQUE A DISTANCIA
            if (projectilePrefab != null)
            {
                // Instanciar el proyectil como hijo del Canvas
                Transform spawnParent = parentCanvas != null ? parentCanvas.transform : transform.parent;
                
                GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity, spawnParent);
                
                // Mantener Z
                Vector3 pos = projectile.transform.position;
                pos.z = transform.position.z;
                projectile.transform.position = pos;
                
                AvatarProjectile projScript = projectile.GetComponent<AvatarProjectile>();
                if (projScript != null)
                {
                    projScript.Initialize(damage, targetRook.transform.position);
                    Debug.Log($"[{avatarType}] 🏹 Disparando a {targetRook.rookType}");
                }
                else
                {
                    Destroy(projectile);
                    targetRook.TakeDamage(damage);
                }
            }
            else
            {
                // Sin prefab, hacer daño directo
                targetRook.TakeDamage(damage);
            }
        }
        else
        {
            // ATAQUE MELEE
            targetRook.TakeDamage(damage);
            Debug.Log($"[{avatarType}] ⚔️ Golpe melee: {damage} daño");
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        
        // Actualizar barra de vida usando fillAmount
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / (float)maxHealth;
        }
        
        Debug.Log($"[{avatarType}] 💔 -{damageAmount} HP ({currentHealth}/{maxHealth})");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"[{avatarType}] 💀 Murió");
        
        // Destruir la barra de vida
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
        
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AddCoins(coinReward);
        }
        
        if (transform.parent != null)
        {
            SpawnPoint sp = transform.parent.GetComponent<SpawnPoint>();
            if (sp != null && sp.avatars != null)
            {
                sp.avatars.Remove(this.gameObject);
            }
        }
        
        AvatarSpawner spawner = FindObjectOfType<AvatarSpawner>();
        if (spawner != null)
        {
            spawner.OnAvatarDestroyed(gameObject);
        }
        
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        AvatarSpawner spawner = FindObjectOfType<AvatarSpawner>();
        if (spawner != null)
        {
            spawner.OnAvatarDestroyed(gameObject);
        }
    }

    // DEBUG: Visualizar en el editor
    private void OnDrawGizmosSelected()
    {
        // Dibujar rango de ataque
        Gizmos.color = isRangedAttack ? Color.cyan : Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Para melee, dibujar cono de detección
        if (!isRangedAttack)
        {
            Gizmos.color = Color.yellow;
            Vector3 leftDir = Quaternion.Euler(0, 0, 60) * Vector3.up;
            Vector3 rightDir = Quaternion.Euler(0, 0, -60) * Vector3.up;
            
            Gizmos.DrawLine(transform.position, transform.position + leftDir * attackRange);
            Gizmos.DrawLine(transform.position, transform.position + rightDir * attackRange);
        }
        
        // Dibujar línea al objetivo
        if (targetRook != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetRook.transform.position);
        }
    }
}
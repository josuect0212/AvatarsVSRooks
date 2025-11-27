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
    public Slider healthBar;
    public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);
    
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
        // IMPORTANTE: En Unity, cuando un objeto se destruye, la referencia no es null inmediatamente
        // Hay que usar el operador == que Unity sobrecarga para detectar objetos destruidos
        if (targetRook != null && (targetRook == null || targetRook.gameObject == null || !targetRook.gameObject.activeInHierarchy))
        {
            Debug.Log($"[{avatarType}] 💀 Objetivo destruido, buscando nuevo...");
            targetRook = null;
            isStopped = false;
        }
        
        // ============ BUSCAR OBJETIVOS ============
        // Las unidades a rango SIEMPRE buscan objetivos, estén paradas o no
        if (isRangedAttack)
        {
            SearchForRangedTarget();
        }
        else if (!isStopped)
        {
            // Las unidades melee solo buscan mientras se mueven
            SearchForMeleeTarget();
        }
        
        // ============ MOVIMIENTO ============
        // Moverse si no está detenido, O si es unidad a rango con moveWhileAttacking
        if (!isStopped || (isRangedAttack && moveWhileAttacking && targetRook != null))
        {
            transform.position += Vector3.up * movementSpeed * Time.deltaTime;
        }
        
        // ============ ATAQUE ============
        if (targetRook != null)
        {
            // Verificar distancia
            float distance = Vector3.Distance(transform.position, targetRook.transform.position);
            
            if (distance > attackRange)
            {
                // Objetivo fuera de rango
                Debug.Log($"[{avatarType}] 📏 Objetivo fuera de rango ({distance:F0} > {attackRange})");
                targetRook = null;
                isStopped = false;
            }
            else
            {
                // Atacar cuando el cooldown termine
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
            case AvatarType.Archer:
                maxHealth = 5;
                damage = 2;
                attackCooldown = 2f;
                movementSpeed = 12.0f;
                isRangedAttack = true;
                attackRange = 2000f;
                columnTolerance = 30f;
                moveWhileAttacking = false;
                break;
                
            case AvatarType.ShieldBearer:
                maxHealth = 10;
                damage = 3;
                attackCooldown = 3f;
                movementSpeed = 10.0f;
                isRangedAttack = true;
                attackRange = 2000f;
                columnTolerance = 30f;
                moveWhileAttacking = false;
                break;
                
            case AvatarType.Lumberjack:
                maxHealth = 20;
                damage = 9;
                attackCooldown = 1.5f;
                movementSpeed = 13.0f;
                isRangedAttack = false;
                attackRange = 60f; // Rango corto para melee
                break;
                
            case AvatarType.Cannibal:
                maxHealth = 25;
                damage = 12;
                attackCooldown = 1f;
                movementSpeed = 14.0f;
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
        if (healthBar == null) return;
        
        healthBar.minValue = 0;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        healthBar.wholeNumbers = true;
        healthBar.gameObject.SetActive(true);
    }

    void UpdateHealthBarPosition()
    {
        if (healthBar == null || Camera.main == null) return;
        
        Vector3 worldPos = transform.position + healthBarOffset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        
        if (screenPos.z > 0)
        {
            healthBar.transform.position = screenPos;
        }
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
        
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
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
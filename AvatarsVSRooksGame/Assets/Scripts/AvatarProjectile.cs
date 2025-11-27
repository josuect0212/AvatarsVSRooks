using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Proyectil para avatares que funciona en un Canvas UI.
/// Debe ser instanciado como hijo de un Canvas.
/// </summary>
public class AvatarProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 800f; // Velocidad en píxeles UI por segundo
    public float lifetime = 5f;
    
    [Header("Visual")]
    public Color projectileColor = Color.yellow;
    public Vector2 size = new Vector2(20f, 20f); // Tamaño en píxeles UI
    
    private int damage;
    private Vector3 targetPosition;
    private Vector3 direction;
    private bool initialized = false;
    private float timeAlive = 0f;
    
    private RectTransform rectTransform;
    private Image image;
    private Canvas parentCanvas;

    void Awake()
    {
        SetupUIComponents();
    }

    void SetupUIComponents()
    {
        // Obtener o crear RectTransform
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            // Si no tiene RectTransform, necesitamos añadirlo
            // Pero primero verificamos si ya es un GameObject UI
            gameObject.AddComponent<RectTransform>();
            rectTransform = GetComponent<RectTransform>();
        }
        
        // Configurar tamaño
        rectTransform.sizeDelta = size;
        
        // Obtener o crear Image
        image = GetComponent<Image>();
        if (image == null)
        {
            image = gameObject.AddComponent<Image>();
        }
        
        // Crear sprite circular para el Image
        image.sprite = CreateCircleSprite();
        image.color = projectileColor;
        image.raycastTarget = false; // No bloquear clicks
        
        // Buscar el Canvas padre
        parentCanvas = GetComponentInParent<Canvas>();
        
        // Configurar collider para detección (si no existe)
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;
        collider.size = size;
        
        // Rigidbody2D para detección de colisiones
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.isKinematic = true;
        
        Debug.Log($"🎨 Proyectil UI configurado - Tamaño: {size}, Color: {projectileColor}");
    }

    Sprite CreateCircleSprite()
    {
        int textureSize = 64;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        texture.filterMode = FilterMode.Bilinear;
        
        Color[] pixels = new Color[textureSize * textureSize];
        Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
        float radius = textureSize / 2f - 2f;
        
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius)
                {
                    // Crear un gradiente suave desde el centro
                    float alpha = 1f - (distance / radius) * 0.3f;
                    pixels[y * textureSize + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    pixels[y * textureSize + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
    }

    public void Initialize(int damageAmount, Vector3 target)
    {
        damage = damageAmount;
        targetPosition = target;
        
        // Calcular dirección
        direction = (target - transform.position).normalized;
        
        initialized = true;
        
        Debug.Log($"🏹 Proyectil UI inicializado:");
        Debug.Log($"   Posición: {transform.position}");
        Debug.Log($"   Objetivo: {target}");
        Debug.Log($"   Dirección: {direction}");
        Debug.Log($"   Daño: {damage}");
    }

    void Update()
    {
        if (!initialized)
        {
            Debug.LogWarning("⚠️ Proyectil sin inicializar - destruyendo");
            Destroy(gameObject);
            return;
        }
        
        timeAlive += Time.deltaTime;
        
        if (timeAlive > lifetime)
        {
            Debug.Log($"⏰ Proyectil destruido por tiempo");
            Destroy(gameObject);
            return;
        }
        
        // Movimiento
        Vector3 movement = direction * speed * Time.deltaTime;
        transform.position += movement;
        
        // Rotación visual hacia la dirección
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }
        
        // Verificar distancia al objetivo (backup por si no hay colisión)
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        if (distanceToTarget < 15f) // 15 píxeles de tolerancia
        {
            // Buscar Rook en esa posición
            TryHitRookAtPosition(targetPosition);
        }
    }

    void TryHitRookAtPosition(Vector3 position)
    {
        // Buscar colliders cercanos
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, 30f);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject.layer == 10) // Layer Rooks
            {
                RookController rook = hit.GetComponent<RookController>();
                if (rook == null) rook = hit.GetComponentInParent<RookController>();
                
                if (rook != null)
                {
                    Debug.Log($"💥 Impacto por proximidad en {rook.rookType}");
                    rook.TakeDamage(damage);
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"🎯 Colisión detectada: {collision.gameObject.name} (Layer: {collision.gameObject.layer})");
        
        if (collision.gameObject.layer == 10) // Layer Rooks
        {
            RookController rook = collision.GetComponent<RookController>();
            if (rook == null) rook = collision.GetComponentInParent<RookController>();
            if (rook == null) rook = collision.GetComponentInChildren<RookController>();
            
            if (rook != null)
            {
                Debug.Log($"💥 ¡IMPACTO! {damage} daño a {rook.rookType}");
                rook.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }

    void OnDestroy()
    {
        Debug.Log($"💀 Proyectil destruido en {transform.position}");
    }
}
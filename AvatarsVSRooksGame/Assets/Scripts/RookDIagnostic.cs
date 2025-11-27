using UnityEngine;

// SCRIPT TEMPORAL PARA DIAGNÓSTICO
// Añádelo temporalmente a tus prefabs de Rook para verificar su configuración
public class RookDiagnostic : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"========== DIAGNÓSTICO DE ROOK: {gameObject.name} ==========");
        
        // Verificar Layer
        Debug.Log($"Layer: {gameObject.layer} ({LayerMask.LayerToName(gameObject.layer)})");
        if (gameObject.layer != 10)
        {
            Debug.LogError($"❌ LAYER INCORRECTO! Debe ser 10 (Rooks), es {gameObject.layer}");
        }
        else
        {
            Debug.Log($"✅ Layer correcto (10)");
        }
        
        // Verificar Colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        Debug.Log($"Colliders encontrados: {colliders.Length}");
        
        foreach (Collider2D col in colliders)
        {
            Debug.Log($"  - Tipo: {col.GetType().Name}");
            Debug.Log($"    IsTrigger: {col.isTrigger}");
            Debug.Log($"    Enabled: {col.enabled}");
            Debug.Log($"    Bounds: {col.bounds}");
            
            if (!col.isTrigger)
            {
                Debug.LogWarning($"⚠️ Collider NO es trigger! Los proyectiles necesitan triggers");
            }
        }
        
        // Verificar RookController
        RookController controller = GetComponent<RookController>();
        if (controller == null)
        {
            Debug.LogError($"❌ NO tiene RookController component!");
        }
        else
        {
            Debug.Log($"✅ RookController encontrado: {controller.rookType}");
        }
        
        // Verificar hijos también
        Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();
        if (childColliders.Length > colliders.Length)
        {
            Debug.Log($"Colliders en hijos: {childColliders.Length - colliders.Length}");
            foreach (Collider2D col in childColliders)
            {
                if (col.gameObject != gameObject)
                {
                    Debug.Log($"  - Hijo: {col.gameObject.name}, Layer: {col.gameObject.layer}, IsTrigger: {col.isTrigger}");
                }
            }
        }
        
        Debug.Log($"========== FIN DIAGNÓSTICO ==========\n");
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"🎯 Rook {gameObject.name} detectó colisión con: {collision.gameObject.name} (Layer: {collision.gameObject.layer})");
    }
}
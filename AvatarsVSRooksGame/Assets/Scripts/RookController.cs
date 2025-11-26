using System.Collections.Generic;
using UnityEngine;

public class RookController : MonoBehaviour
{
    [Header("Rook Type")]
    public RookType rookType;
    
    [Header("Combat")]
    public GameObject fire;
    public List<GameObject> avatars;
    public GameObject toAttack;
    public float attackCooldown = 4f; // Todas atacan cada 4 segundos
    private float attackTime;
    public int damage;
    public int health;
    public bool isAttacking;
    public RookContainer currentRookContainer;

    private void Start()
    {
        ApplyRookStats();
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
    }

    private void ApplyRookStats()
    {
        // Frecuencia de ataque es siempre 4 segundos
        attackCooldown = 4f;
        
        switch (rookType)
        {
            case RookType.Sand:
                damage = 2;
                health = 3;
                break;
                
            case RookType.Rock:
                damage = 4;
                health = 14;
                break;
                
            case RookType.Fire:
                damage = 8;
                health = 16;
                break;
                
            case RookType.Water:
                damage = 8;
                health = 16;
                break;
        }
    }

    public void TakeDamage(int damage)
    {
        if (health - damage <= 0)
        {
            if (currentRookContainer != null)
            {
                currentRookContainer.filled = false; 
            }
            Destroy(this.gameObject);          
        }
        else
        {
            health -= damage;
        }
    }
}
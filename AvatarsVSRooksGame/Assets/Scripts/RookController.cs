using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RookController : MonoBehaviour
{
    public GameObject fire;
    public List<GameObject> avatars;
    public GameObject toAttack;
    public float attackCooldown;
    private float attackTime;
    public int damage;
    public int health;
    public bool isAttacking;
    public RookContainer currentRookContainer;

    private void Update()
   {
        // Clean nulls from avatar list (if any were destroyed)
        avatars.RemoveAll(a => a == null);

        // Reset attack target if needed
        if (toAttack == null || !avatars.Contains(toAttack))
        {
            toAttack = avatars.Count > 0 ? avatars[0] : null;
        }

        // If there’s a valid target, shoot periodically
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

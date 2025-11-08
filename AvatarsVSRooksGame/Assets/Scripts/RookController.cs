using System.Collections.Generic;
using UnityEngine;

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

    private void Update()
    {
        if (avatars.Count > 0 && !isAttacking)
        {
            isAttacking = true;
            toAttack = avatars[0];
        }
        else if (avatars.Count == 0 && isAttacking)
        {
            isAttacking = false;
        }

        if (toAttack != null)
        {
            if (attackTime <= Time.time)
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
            Destroy(this.gameObject);
        }
        else
        {
            health -= damage;
        }
    }
}

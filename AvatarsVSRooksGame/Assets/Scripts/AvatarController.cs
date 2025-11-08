using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AvatarController : MonoBehaviour
{
    public int health;
    public int damage;
    public float attackCooldown;
    public float movementSpeed;
    private bool isStopped;

    void Update()
    {
        if (!isStopped)
        {
            transform.Translate(Vector3.up * movementSpeed * Time.deltaTime);
        }
        
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 10)
        {
            StartCoroutine(Attack(collision));
            isStopped = true;
        }
    }

    IEnumerator Attack(Collider2D collision)
    {
        if (collision == null)
        {
            isStopped = false;
        }
        else
        {
            collision.gameObject.GetComponent<RookController>().TakeDamage(damage);
            yield return new WaitForSeconds(attackCooldown);
            StartCoroutine(Attack(collision));
        }

    }

    public void TakeDamage(int damage)
    {
        if (health - damage <= 0)
        {
            transform.parent.GetComponent<SpawnPoint>().avatars.Remove(this.gameObject);
            Destroy(this.gameObject);
        }
        else
        {
            health -= damage;
        }
    }
}

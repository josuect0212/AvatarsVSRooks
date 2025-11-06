using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarController : MonoBehaviour
{
    public int Health;
    public int Damage;
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
            isStopped = true;
        }
    }

    public void TakeDamage(int damage)
    {
        if (Health - damage <= 0)
        {
            transform.parent.GetComponent<SpawnPoint>().avatars.Remove(this.gameObject);
            Destroy(this.gameObject);
        }
        else
        {
            Health -= damage;
        }
    }
}

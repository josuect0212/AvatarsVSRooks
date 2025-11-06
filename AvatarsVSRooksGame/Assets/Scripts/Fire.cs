using UnityEngine;

public class Fire : MonoBehaviour
{
    public float movementSpeed;
    public int damage;

    void Update()
    {
        transform.Translate(Vector3.right * movementSpeed * Time.deltaTime);
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 11)
        {
            collision.gameObject.GetComponent<AvatarController>().TakeDamage(damage);
            Destroy(this.gameObject);
        }
    }
}

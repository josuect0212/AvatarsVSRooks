using System;
using UnityEngine;

public class DeathHitBox : MonoBehaviour
{
    private GameManager gm;
    void Start()
    {
        gm = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("hiiiiit");
        AvatarController avatar = collision.GetComponent<AvatarController>();
        if (avatar!=null)
        {
            Debug.Log("Game Lost");
            gm.gameWon = false;
        }
    }
}

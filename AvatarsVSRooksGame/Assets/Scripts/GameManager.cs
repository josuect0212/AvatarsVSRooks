using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public GameObject draggingRook;
    public GameObject currentContainer;
    public bool? gameWon = null;
    public static GameManager instance;

    private void Awake() { 
        instance = this;
    }

    public bool PlaceRook()
    {
        if (draggingRook != null && currentContainer != null)
        {
            RookContainer container = currentContainer.GetComponent<RookContainer>();

            if (container != null && !container.filled)
            {
                GameObject objectGame = Instantiate(
                    draggingRook.GetComponent<RookDrag>().card.rook_Game,
                    currentContainer.transform
                );

                RookController rookController = objectGame.GetComponent<RookController>();
                rookController.avatars = container.spawnPoint.avatars;
                rookController.currentRookContainer = container;

                container.filled = true;

                return true; // Torre colocada exitosamente
            }
        }

        return false; // No se pudo colocar
    }
    public void CheckWinCondition()
    {
        AvatarSpawner spawner = FindObjectOfType<AvatarSpawner>();
        if (spawner != null && spawner.AllAvatarsDefeated())
        {
            gameWon = true;
            Debug.Log("You won!");
        }
    }

    public void LoseGame()
    {
        gameWon = false;
        Debug.Log("You lost!");
    }
    
}

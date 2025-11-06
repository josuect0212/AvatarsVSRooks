using UnityEngine;

public class GameManager : MonoBehaviour
{

    public GameObject draggingRook;
    public GameObject currentContainer;

    public static GameManager instance;

    private void Awake() { 
        instance = this;
    }

    public void PlaceRook()
    {
        if (draggingRook != null && currentContainer != null)
        {
            GameObject objectGame = Instantiate(draggingRook.GetComponent<RookDrag>().card.rook_Game, currentContainer.transform);
            objectGame.GetComponent<RookController>().avatars = currentContainer.GetComponent<RookContainer>().spawnPoint.avatars;
            currentContainer.GetComponent<RookContainer>().filled = true;

        }
    }
}

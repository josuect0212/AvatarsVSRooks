using UnityEngine;

public class GameManager : MonoBehaviour
{

    public GameObject draggingRook;
    public GameObject currentContainer;

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
            
            objectGame.GetComponent<RookController>().avatars = 
                container.spawnPoint.avatars;
            
            container.filled = true;
            
            return true; // Torre colocada exitosamente
        }
    }
    
    return false; // No se pudo colocar
}
}

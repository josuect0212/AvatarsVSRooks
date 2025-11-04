using UnityEngine;
using UnityEngine.UI;

public class RookContainer : MonoBehaviour
{
    public bool filled;
    public GameManager gameManager;
    public Image backgroundImage;

    void Start() {
        gameManager = GameManager.instance;
    }

    public void OnTriggerEnter2D(Collider2D collision) {

        if (gameManager.draggingRook != null && filled == false) {
            gameManager.currentContainer = this.gameObject;
            backgroundImage.enabled = true;
        }
    }

    public void OnTriggerExit2D(Collider2D collision){
        gameManager.currentContainer = null;
        backgroundImage.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

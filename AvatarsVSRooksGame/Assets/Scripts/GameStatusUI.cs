using UnityEngine;
using UnityEngine.UI;

public class GameStatusUI : MonoBehaviour
{
    public Image winImage;
    public Image loseImage;

    private GameManager gm;

    void Start()
    {
        gm = GameManager.instance;
        winImage.gameObject.SetActive(false);
        loseImage.gameObject.SetActive(false);
    }

    void Update()
    {
        if (gm == null) return;

        switch (gm.gameWon){
            case null:
                winImage.gameObject.SetActive(false);
                loseImage.gameObject.SetActive(false);
                break;

            case true:
                winImage.gameObject.SetActive(true);
                loseImage.gameObject.SetActive(false);
                break;

            case false:
                winImage.gameObject.SetActive(false);
                loseImage.gameObject.SetActive(true);
                break;
        }
    }
}

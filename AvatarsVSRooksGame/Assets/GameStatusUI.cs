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

        if (gm.gameWon == null)
        {
            winImage.gameObject.SetActive(false);
            loseImage.gameObject.SetActive(false);
        }
        else if (gm.gameWon == true)
        {
            winImage.gameObject.SetActive(true);
            loseImage.gameObject.SetActive(false);
        }
        else if (gm.gameWon == false)
        {
            winImage.gameObject.SetActive(false);
            loseImage.gameObject.SetActive(true);
        }
    }
}

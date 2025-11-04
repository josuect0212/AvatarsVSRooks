using UnityEngine;
using UnityEngine.EventSystems;

public class BuyCard : MonoBehaviour, IDragHandler,IPointerDownHandler,IPointerUpHandler
{
    public GameObject rook_Drag;
    public GameObject rook_Game;
    public Canvas canvas;
    public GameObject rookDragInstance;
    public GameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDrag(PointerEventData eventData) {
        rookDragInstance.transform.position = Input.mousePosition;
    }

    public void OnPointerDown(PointerEventData eventData) {
        rookDragInstance = Instantiate(rook_Drag, canvas.transform);
        rookDragInstance.transform.position = Input.mousePosition;
        rookDragInstance.GetComponent<RookDrag>().card = this;
        gameManager.draggingRook = rookDragInstance;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        gameManager.PlaceRook();
        gameManager.draggingRook = null;
        Destroy(rookDragInstance);
        
    }
}

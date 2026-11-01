using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class TileButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, HideInInspector] private TileBase tile;
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI textMeshPro;

    public void SetTileBase(TileBase tile)
    {
        this.tile = tile;
        transform.GetChild(0).GetComponent<Image>().sprite = tile switch
        {
            Tile t => t.sprite,
            AnimatedTile t => t.m_AnimatedSprites[0],
            _ => null
        };
    }

    public TextMeshProUGUI ActivateTextMeshPro()
    {
        panel.SetActive(true);
        return textMeshPro;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * 1.2f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }

    public void OnClick()
    {
        LevelEditorManager.Instance.SetCurrentTile(tile);
    }
}
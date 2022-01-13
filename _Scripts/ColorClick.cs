using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class ColorClick : MonoBehaviour, IPointerClickHandler
{
    public Image[] images;

    private Image image;
    private Outline outline;
    private Color color;

    public void OnPointerClick(PointerEventData eventData)
    {
        image = GetComponent<Image>();
        outline = GetComponent<Outline>();

        //Set another colors unselected
        foreach (var im in images)
            im.GetComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0f);

        LobbyManager.S.color = image.color;
        color = outline.effectColor;
        color.a = 1f;
        outline.effectColor = color;
    }
}

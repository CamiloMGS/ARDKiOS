using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizePanelUI : MonoBehaviour
{
    [SerializeField] private RectTransform scanArea;
    [SerializeField] private RectTransform panel;
    void Start()
    {
        Vector2 newAnchoredPosition = new Vector2((Screen.width-scanArea.rect.width)/2, (Screen.height - scanArea.rect.height)/2);
        panel.offsetMax = new Vector2(newAnchoredPosition.x, newAnchoredPosition.y - (scanArea.position.y -Screen.height/2));//right-top
        panel.offsetMin = new Vector2(-newAnchoredPosition.x, -newAnchoredPosition.y - (scanArea.position.y - Screen.height / 2));//left-bottom
    }

}

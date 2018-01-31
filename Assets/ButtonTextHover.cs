using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonTextHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private Text text;
    public Color onExit;
    public Color onHover;
    public float transitionDuration;

    private void Start() {
        text = transform.GetChild(0).GetComponent<Text>();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        text.CrossFadeColor(onHover, transitionDuration, true, true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        text.CrossFadeColor(onExit, transitionDuration, true, true);
    }
}

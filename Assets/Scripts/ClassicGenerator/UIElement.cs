using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// !!!!!!Elements holding this script must have the anchors set (left, right, top, bottom) and NOT width, height!!!!
/// </summary>
public class UIElement : MonoBehaviour
{
    protected List<GameObject> objects;

    protected RectTransform Container;

    protected float ContainerWidth;
    protected float ContainerHeight;

    public void Awake()
    {
        objects = new List<GameObject>();
        Container = gameObject.GetComponent<RectTransform>();
    }

    public void Start()
    {
        ContainerWidth = Container.rect.width;
        ContainerHeight = Container.rect.height;
        OnStart();
    }

    protected virtual void OnStart() { }

    /// <summary>
    /// Add a panel element. xStart, xEnd, yStart, yEnd are percentage values (between 0 and 1).
    /// </summary>
    protected RectTransform AddPanel(string name, Color backgroundColor, float xStart, float yStart, float xEnd, float yEnd, RectTransform parent, Sprite shape = null)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        Image image = panel.AddComponent<Image>();
        image.color = backgroundColor;
        image.raycastTarget = false;
        if (shape != null) image.sprite = shape;

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(0, 0);
        rectTransform.anchorMin = new Vector2(xStart, 1 - yEnd);
        rectTransform.anchorMax = new Vector2(xEnd, 1 - yStart);
        objects.Add(panel);

        return rectTransform;
    }


    /// <summary>
    /// Add a text element. xStart, xEnd, yStart, yEnd are percentage values (between 0 and 1).
    /// </summary>
    protected GameObject AddText(string content, int fontSize, Color fontColor, FontStyle fontStyle, float xStart, float yStart, float xEnd, float yEnd, RectTransform parent, TextAnchor textAnchor = TextAnchor.MiddleCenter)
    {
        GameObject textElement = new GameObject(content);
        textElement.transform.SetParent(parent, false);
        Text text = textElement.AddComponent<Text>();
        text.text = content;
        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        text.font = ArialFont;
        text.material = ArialFont.material;
        text.fontStyle = fontStyle;
        text.color = fontColor;
        text.fontSize = fontSize;
        text.alignment = textAnchor;
        text.raycastTarget = false;

        RectTransform textRect = textElement.GetComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(0, 0);
        textRect.sizeDelta = new Vector2(0, 0);
        textRect.anchorMin = new Vector2(xStart, 1 - yEnd);
        textRect.anchorMax = new Vector2(xEnd, 1 - yStart);
        objects.Add(textElement);

        return textElement;
    }

    /// <summary>
    /// Destroys all elements in this UI Element.
    /// </summary>
    protected virtual void Clear()
    {
        foreach (GameObject go in objects) GameObject.Destroy(go);
        objects.Clear();
    }

    /// <summary>
    /// Sets the background color of the panel.
    /// </summary>
    protected void SetBackgroundColor(Color color)
    {
        gameObject.GetComponent<Image>().color = color;
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GlassButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector] public Color normalFill, hoverFill;
    [HideInInspector] public Color normalBorder, hoverBorder;
    [HideInInspector] public Color normalText, hoverText;
    [HideInInspector] public float hoverScale = 1.06f;

    private Image background;
    private Outline border;
    private TMP_Text label;

    private Color targetFill, targetBorder, targetText;
    private float targetScale = 1f;
    private bool isHover;

    void Awake()
    {
        background = GetComponent<Image>();
        border = GetComponent<Outline>();
        label = GetComponentInChildren<TMP_Text>();
        SetNormal();
    }

    void OnEnable()
    {
        transform.localScale = Vector3.one;
    }

    void Update()
    {
        float t = Time.unscaledDeltaTime * 10f;

        if (background != null) background.color = Color.Lerp(background.color, targetFill, t);
        if (border != null) border.effectColor = Color.Lerp(border.effectColor, targetBorder, t);
        if (label != null) label.color = Color.Lerp(label.color, targetText, t);

        float s = Mathf.Lerp(transform.localScale.x, targetScale, t);
        transform.localScale = new Vector3(s, s, 1f);
    }

    private void SetNormal()
    {
        targetFill = normalFill;
        targetBorder = normalBorder;
        targetText = normalText;
        targetScale = 1f;
    }

    private void SetHover()
    {
        targetFill = hoverFill;
        targetBorder = hoverBorder;
        targetText = hoverText;
        targetScale = hoverScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;
        SetHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;
        SetNormal();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = 0.97f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = isHover ? hoverScale : 1f;
    }
}
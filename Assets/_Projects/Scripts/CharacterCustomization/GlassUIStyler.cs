using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GlassUIStyler : MonoBehaviour
{
    [Header("Палитра (меняйте здесь)")]
    public Color normalFill = new Color(1f, 1f, 1f, 0.10f);
    public Color hoverFill = new Color(1f, 1f, 1f, 0.20f);
    public Color normalBorder = new Color(1f, 1f, 1f, 0.35f);
    public Color hoverBorder = new Color(1f, 0.79f, 0.42f, 1f);
    public Color normalText = new Color(0.95f, 0.95f, 0.95f, 1f);
    public Color hoverText = new Color(1f, 0.87f, 0.54f, 1f);
    public Color shineColor = new Color(1f, 1f, 1f, 0.08f);

    [Header("Главная кнопка (SAVE)")]
public Button primaryButton;
public Color primaryBorder = new Color(0.85f, 0.64f, 0.25f, 1f);

    [ContextMenu("Apply Glass Style")]
    public void ApplyStyle()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            StyleButton(button);
        }

        TMP_InputField[] fields = GetComponentsInChildren<TMP_InputField>(true);
        foreach (TMP_InputField field in fields)
        {
            StyleInputField(field);
        }
if (primaryButton != null)
{
    Outline primaryOutline = primaryButton.GetComponent<Outline>();
    primaryOutline.effectColor = primaryBorder;

    GlassButton primaryGlass = primaryButton.GetComponent<GlassButton>();
    primaryGlass.normalBorder = primaryBorder;
}

#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        Debug.Log("Glass style applied to " + buttons.Length + " buttons and " + fields.Length + " input fields.");
    }

    private void StyleButton(Button button)
    {
        button.transition = Selectable.Transition.None;

        Image image = button.GetComponent<Image>();
        image.color = normalFill;

        Outline outline = button.GetComponent<Outline>();
        if (outline == null) outline = button.gameObject.AddComponent<Outline>();
        outline.effectColor = normalBorder;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.color = normalText;
            label.fontStyle = FontStyles.Bold;
        }

        AddShine(button.transform, image);

        GlassButton glass = button.GetComponent<GlassButton>();
        if (glass == null) glass = button.gameObject.AddComponent<GlassButton>();
        glass.normalFill = normalFill;
        glass.hoverFill = hoverFill;
        glass.normalBorder = normalBorder;
        glass.hoverBorder = hoverBorder;
        glass.normalText = normalText;
        glass.hoverText = hoverText;
    }

    private void StyleInputField(TMP_InputField field)
    {
        Image image = field.GetComponent<Image>();
        image.color = normalFill;

        Outline outline = field.GetComponent<Outline>();
        if (outline == null) outline = field.gameObject.AddComponent<Outline>();
        outline.effectColor = normalBorder;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        if (field.textComponent != null) field.textComponent.color = normalText;

        TMP_Text placeholder = field.placeholder as TMP_Text;
        if (placeholder != null)
        {
            Color c = normalText;
            c.a = 0.5f;
            placeholder.color = c;
        }

        AddShine(field.transform, image);
    }

    private void AddShine(Transform parent, Image baseImage)
    {
        if (parent.Find("GlassShine") != null) return;

        GameObject shineObject = new GameObject("GlassShine", typeof(RectTransform), typeof(Image));
        shineObject.transform.SetParent(parent, false);
        shineObject.transform.SetAsFirstSibling();

        RectTransform rect = shineObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = new Vector2(3f, 0f);
        rect.offsetMax = new Vector2(-3f, -3f);

        Image shine = shineObject.GetComponent<Image>();
        shine.sprite = baseImage.sprite;
        shine.type = baseImage.type;
        shine.color = shineColor;
        shine.raycastTarget = false;
    }
}
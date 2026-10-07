using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FloodFillLight : MonoBehaviour
{
    [Header("Настройки заливки для всех букв контейнера")]
    public Color fillColor = Color.white;
    public float fillSpeed = 5f;

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            TMP_Text tmp = child.GetComponent<TMP_Text>();
            if (tmp == null)
                continue;

            Graphic graphic = child.GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.raycastTarget = true;
                ExpandRaycastArea(graphic, tmp);
            }

            LetterHoverFill hover = child.GetComponent<LetterHoverFill>();
            if (hover == null)
                hover = child.gameObject.AddComponent<LetterHoverFill>();

            hover.fillColor = fillColor;
            hover.fillSpeed = fillSpeed;
        }
    }

    // Расширяет зону обнаружения мыши (raycast), НЕ трогая размер/позицию
    // RectTransform и, соответственно, не сдвигая сам текст ни на пиксель.
    private void ExpandRaycastArea(Graphic graphic, TMP_Text tmp)
    {
        tmp.ForceMeshUpdate();
        Bounds bounds = tmp.textBounds;

        RectTransform rect = graphic.rectTransform;
        Rect rectArea = rect.rect;

        float extraWidth = Mathf.Max(0f, bounds.size.x - rectArea.width);
        float extraHeight = Mathf.Max(0f, bounds.size.y - rectArea.height);

        float horizontal = -extraWidth / 2f;
        float vertical = -extraHeight / 2f;

        // Left, Bottom, Right, Top — отрицательные значения расширяют зону наружу.
        graphic.raycastPadding = new Vector4(horizontal, vertical, horizontal, vertical);
    }

    [RequireComponent(typeof(TMP_Text))]
    private class LetterHoverFill : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Color fillColor = Color.white;
        public float fillSpeed = 5f;

        private static readonly int FaceColorID = Shader.PropertyToID("_FaceColor");

        private TMP_Text tmpText;
        private Material materialInstance;
        private Coroutine fillRoutine;

        private void Awake()
        {
            tmpText = GetComponent<TMP_Text>();
            materialInstance = tmpText.fontMaterial;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            StartFillTo(1f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StartFillTo(0f);
        }

        private void StartFillTo(float targetAlpha)
        {
            if (fillRoutine != null)
                StopCoroutine(fillRoutine);

            fillRoutine = StartCoroutine(FillRoutine(targetAlpha));
        }

        private IEnumerator FillRoutine(float targetAlpha)
        {
            Color current = materialInstance.GetColor(FaceColorID);
            float startAlpha = current.a;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime * fillSpeed;

                Color c = fillColor;
                c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
                materialInstance.SetColor(FaceColorID, c);

                yield return null;
            }

            Color final = fillColor;
            final.a = targetAlpha;
            materialInstance.SetColor(FaceColorID, final);
        }
    }
}
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ButtonAnimation : MonoBehaviour
{
    [Header("Цвет текста")]
    [SerializeField] Color textNormalColor = Color.black;
    [SerializeField] Color textHighlightColor = Color.white;

    [Header("Волна по буквам")]
    [SerializeField] float delayBetweenLetters = 0.08f;
    [SerializeField] float letterFadeDuration = 0.2f;

    [Header("Цвет фона кнопки")]
    [SerializeField] Color bgNormalColor = new Color(1f, 1f, 1f, 0.08f);
    [SerializeField] Color bgHighlightColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] float bgFadeDuration = 0.15f;

    [Header("Нажатие кнопки")]
    [SerializeField] Color bgPressedColor = new Color(0.05f, 0.05f, 0.05f, 1f);
    [SerializeField] float pressedScale = 0.96f;
    [SerializeField] float pressFadeDuration = 0.08f;

    class ButtonRefs
    {
        public Image background;
        public TMP_Text text;
        public RectTransform rect;
        public Coroutine waveRoutine;
        public Coroutine bgRoutine;
        public Coroutine pressRoutine;
        public List<Coroutine> activeLetterFades = new();
        public bool isHovering;
    }

    List<ButtonRefs> buttons = new();

    void Initialize()
    {
        foreach (Image bg in GetComponentsInChildren<Image>())
        {
            TMP_Text text = bg.GetComponentInChildren<TMP_Text>();
            if (text == null) continue;

            var btn = new ButtonRefs { background = bg, text = text, rect = bg.GetComponent<RectTransform>() };
            buttons.Add(btn);
            SetupHover(bg.gameObject, btn);
        }
    }

    void Start()
    {
        Initialize();
    }

    void OnDisable()
    {
        foreach (var btn in buttons)
        {
            if (btn == null || btn.background == null) continue;

            if (btn.waveRoutine != null) StopCoroutine(btn.waveRoutine);
            if (btn.bgRoutine != null) StopCoroutine(btn.bgRoutine);
            if (btn.pressRoutine != null) StopCoroutine(btn.pressRoutine);
            foreach (var r in btn.activeLetterFades)
            {
                if (r != null) StopCoroutine(r);
            }
            btn.activeLetterFades.Clear();

            btn.background.color = bgNormalColor;
            if (btn.rect != null) btn.rect.localScale = Vector3.one;
            SetAllLettersColor(btn.text, textNormalColor);
            btn.isHovering = false;
        }
    }

    void SetupHover(GameObject hoverTarget, ButtonRefs btn)
    {
        SetAllLettersColor(btn.text, textNormalColor);
        btn.background.color = bgNormalColor;

        EventTrigger trigger = hoverTarget.GetComponent<EventTrigger>();
        if (trigger == null) trigger = hoverTarget.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => { btn.isHovering = true; StartHover(btn, true); });
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => { btn.isHovering = false; StartHover(btn, false); });
        trigger.triggers.Add(exitEntry);

        EventTrigger.Entry downEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        downEntry.callback.AddListener((data) => StartPress(btn, true));
        trigger.triggers.Add(downEntry);

        EventTrigger.Entry upEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        upEntry.callback.AddListener((data) => StartPress(btn, false));
        trigger.triggers.Add(upEntry);
    }

    void StartHover(ButtonRefs btn, bool toHighlight)
    {
        StartWave(btn, toHighlight);
        StartBgFade(btn, toHighlight);
    }

    void StartWave(ButtonRefs btn, bool toHighlight)
    {
        if (btn.waveRoutine != null) StopCoroutine(btn.waveRoutine);

        foreach (var r in btn.activeLetterFades)
        {
            if (r != null) StopCoroutine(r);
        }
        btn.activeLetterFades.Clear();

        btn.waveRoutine = StartCoroutine(Wave(btn, toHighlight));
    }

    IEnumerator Wave(ButtonRefs btn, bool toHighlight)
    {
        TMP_Text text = btn.text;
        Color target = toHighlight ? textHighlightColor : textNormalColor;
        int charCount = text.textInfo.characterCount;

        for (int i = 0; i < charCount; i++)
        {
            if (!text.textInfo.characterInfo[i].isVisible) continue;

            Coroutine r = StartCoroutine(FadeLetter(text, i, target));
            btn.activeLetterFades.Add(r);

            yield return new WaitForSeconds(delayBetweenLetters);
        }
    }

    IEnumerator FadeLetter(TMP_Text text, int charIndex, Color target)
    {
        var info = text.textInfo.characterInfo[charIndex];
        int matIndex = info.materialReferenceIndex;
        int vertIndex = info.vertexIndex;

        Color start = text.textInfo.meshInfo[matIndex].colors32[vertIndex];
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / letterFadeDuration;
            Color current = Color.Lerp(start, target, t);

            Color32[] colors = text.textInfo.meshInfo[matIndex].colors32;
            colors[vertIndex + 0] = current;
            colors[vertIndex + 1] = current;
            colors[vertIndex + 2] = current;
            colors[vertIndex + 3] = current;

            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            yield return null;
        }
    }

    void SetAllLettersColor(TMP_Text text, Color color)
    {
        text.ForceMeshUpdate();

        for (int i = 0; i < text.textInfo.characterCount; i++)
        {
            var info = text.textInfo.characterInfo[i];
            if (!info.isVisible) continue;

            int matIndex = info.materialReferenceIndex;
            int vertIndex = info.vertexIndex;
            Color32[] colors = text.textInfo.meshInfo[matIndex].colors32;

            colors[vertIndex + 0] = color;
            colors[vertIndex + 1] = color;
            colors[vertIndex + 2] = color;
            colors[vertIndex + 3] = color;
        }

        text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    void StartBgFade(ButtonRefs btn, bool toHighlight)
    {
        if (btn.bgRoutine != null) StopCoroutine(btn.bgRoutine);
        btn.bgRoutine = StartCoroutine(FadeBackground(btn, toHighlight ? bgHighlightColor : bgNormalColor, bgFadeDuration));
    }

    IEnumerator FadeBackground(ButtonRefs btn, Color target, float duration)
    {
        Color start = btn.background.color;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            btn.background.color = Color.Lerp(start, target, t);
            yield return null;
        }
        btn.background.color = target;
    }


    void StartPress(ButtonRefs btn, bool isDown)
    {
        if (btn.pressRoutine != null) StopCoroutine(btn.pressRoutine);
        btn.pressRoutine = StartCoroutine(PressEffect(btn, isDown));
    }

    IEnumerator PressEffect(ButtonRefs btn, bool isDown)
    {
        Color colorTarget = isDown ? bgPressedColor : (btn.isHovering ? bgHighlightColor : bgNormalColor);
        float scaleTarget = isDown ? pressedScale : 1f;

        Color colorStart = btn.background.color;
        Vector3 scaleStart = btn.rect.localScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / pressFadeDuration;
            btn.background.color = Color.Lerp(colorStart, colorTarget, t);
            btn.rect.localScale = Vector3.Lerp(scaleStart, Vector3.one * scaleTarget, t);
            yield return null;
        }

        btn.background.color = colorTarget;
        btn.rect.localScale = Vector3.one * scaleTarget;
    }
}
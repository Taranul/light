using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ButtonHoverContainer : MonoBehaviour
{
    [Header("Перетащи сюда все объекты-кнопки (заполнится автоматически)")]
    List<TMP_Text> buttonTexts = new();

    [Header("Цвета текста")]
    [SerializeField] Color normalColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] Color highlightColor = Color.white;

    [Header("Задержка старта между буквами, сек")]
    [SerializeField] float delayBetweenLetters = 0.03f;

    [Header("Длительность перехода одной буквы, сек")]
    [SerializeField] float letterFadeDuration = 0.15f;

    Dictionary<TMP_Text, Coroutine> activeSweep = new();
    Dictionary<TMP_Text, List<Coroutine>> activeLetterFades = new();

    void Initialize()
    {
        foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>())
        {
            buttonTexts.Add(text);
            activeLetterFades[text] = new List<Coroutine>();
        }
    }

    void Start()
    {
        Initialize();

        foreach (var text in buttonTexts)
        {
            SetupHover(text);
        }
    }

    void SetupHover(TMP_Text text)
    {
        text.color = normalColor;

        GameObject targetObject = text.GetComponentInParent<UnityEngine.UI.Image>()?.gameObject ?? text.gameObject;

        EventTrigger trigger = targetObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = targetObject.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => StartSweep(text, highlightColor));
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => StartSweep(text, normalColor));
        trigger.triggers.Add(exitEntry);
    }


    void StartSweep(TMP_Text text, Color targetColor)
    {
        if (activeSweep.TryGetValue(text, out Coroutine sweepRoutine) && sweepRoutine != null)
            StopCoroutine(sweepRoutine);

        foreach (var letterRoutine in activeLetterFades[text])
        {
            if (letterRoutine != null)
                StopCoroutine(letterRoutine);
        }
        activeLetterFades[text].Clear();

        Coroutine routine = StartCoroutine(Sweep(text, targetColor));
        activeSweep[text] = routine;
    }

    IEnumerator Sweep(TMP_Text text, Color targetColor)
    {
        text.ForceMeshUpdate();
        int charCount = text.textInfo.characterCount;

        for (int i = charCount - 1; i >= 0; i--)
        {
            if (!text.textInfo.characterInfo[i].isVisible) continue;

            Coroutine letterRoutine = StartCoroutine(FadeLetter(text, i, targetColor));
            activeLetterFades[text].Add(letterRoutine);

            yield return new WaitForSeconds(delayBetweenLetters);
        }
    }

    IEnumerator FadeLetter(TMP_Text text, int charIndex, Color targetColor)
    {
        TMP_TextInfo textInfo = text.textInfo;
        int matIndex = textInfo.characterInfo[charIndex].materialReferenceIndex;
        int vertIndex = textInfo.characterInfo[charIndex].vertexIndex;
        Color32[] vertexColors = textInfo.meshInfo[matIndex].colors32;

        Color startColor = vertexColors[vertIndex];
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / letterFadeDuration;
            Color current = Color.Lerp(startColor, targetColor, t);

            vertexColors = text.textInfo.meshInfo[matIndex].colors32;
            vertexColors[vertIndex + 0] = current;
            vertexColors[vertIndex + 1] = current;
            vertexColors[vertIndex + 2] = current;
            vertexColors[vertIndex + 3] = current;

            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            yield return null;
        }
    }
}
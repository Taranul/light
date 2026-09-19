using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    GameObject menuButtonsContainer;
    GameObject settingsPanel;
    GameObject languagePanel;

    Button btnSettings;
    Button btnExit;

    Button btnLanguage;
    Button btnBack;

    Button btnEnglish;
    Button btnRussian;
    Button btnLanguageBack;

    void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        menuButtonsContainer = transform.Find("MenuButtonsContainer")?.gameObject;
        settingsPanel = transform.Find("SettingsPanel")?.gameObject;
        languagePanel = transform.Find("LanguagePanel")?.gameObject;

        if (menuButtonsContainer == null) Debug.LogError("MenuManager: не найден MenuButtonsContainer");
        if (settingsPanel == null) Debug.LogError("MenuManager: не найден SettingsPanel");
        if (languagePanel == null) Debug.LogError("MenuManager: не найден LanguagePanel");
        if (menuButtonsContainer == null || settingsPanel == null || languagePanel == null) return;

        btnSettings = FindButton(menuButtonsContainer.transform, "Button_Settings");
        btnExit = FindButton(menuButtonsContainer.transform, "Button_Exit");

        btnLanguage = FindButton(settingsPanel.transform, "Button_Language");
        btnBack = FindButton(settingsPanel.transform, "Button_Back");

        btnEnglish = FindButton(languagePanel.transform, "En_Language_Button");
        btnRussian = FindButton(languagePanel.transform, "Ru_Language_Button");
        btnLanguageBack = FindButton(languagePanel.transform, "Back_Language_Button");

        btnSettings?.onClick.AddListener(OpenSettings);
        btnExit?.onClick.AddListener(QuitGame);
        btnBack?.onClick.AddListener(CloseSettings);
        btnLanguage?.onClick.AddListener(OpenLanguagePanel);
        btnEnglish?.onClick.AddListener(() => SetLanguage("en"));
        btnRussian?.onClick.AddListener(() => SetLanguage("ru"));
        btnLanguageBack?.onClick.AddListener(CloseLanguagePanel);

        settingsPanel.SetActive(false);
        languagePanel.SetActive(false);
    }

    Button FindButton(Transform parent, string childName)
    {
        Transform t = parent.Find(childName);
        if (t == null)
        {
            Debug.LogError($"MenuManager: не найден объект '{childName}' внутри '{parent.name}'");
            return null;
        }
        Button b = t.GetComponent<Button>();
        if (b == null) Debug.LogError($"MenuManager: на объекте '{childName}' нет компонента Button");
        return b;
    }

    void OpenSettings()
    {
        StartCoroutine(SwitchPanels(menuButtonsContainer, settingsPanel, true));
    }

    void CloseSettings()
    {
        StartCoroutine(SwitchPanels(settingsPanel, menuButtonsContainer, false));
    }

    void OpenLanguagePanel()
    {
        StartCoroutine(SwitchPanels(settingsPanel, languagePanel, true));
    }

    void CloseLanguagePanel()
    {
        StartCoroutine(SwitchPanels(languagePanel, settingsPanel, false));
    }

    IEnumerator SwitchPanels(GameObject hide, GameObject show, bool isForward)
    {
        Debug.Log($"SwitchPanels: {hide.name} -> {show.name}, isForward={isForward}");
        EventSystem.current.SetSelectedGameObject(null);

        MenuButtonsAnimator hideAnimator = hide.GetComponent<MenuButtonsAnimator>();
        if (hideAnimator != null)
        {
            bool animationDone = false;
            hideAnimator.PlayOut(() => animationDone = true, isForward);

            float safetyTimer = 0f;
            const float maxWait = 2f;
            while (!animationDone && safetyTimer < maxWait)
            {
                safetyTimer += Time.deltaTime;
                yield return null;
            }

            if (!animationDone)
                Debug.LogWarning($"MenuManager: анимация ухода у '{hide.name}' не завершилась вовремя, переключаю панель принудительно.");
        }
        else
        {
            Debug.Log($"SwitchPanels: у {hide.name} нет MenuButtonsAnimator, жду 1 кадр");
            yield return null;
        }

        Debug.Log($"SwitchPanels: выключаю {hide.name}, включаю {show.name}");
        hide.SetActive(false);

        MenuButtonsAnimator showAnimator = show.GetComponent<MenuButtonsAnimator>();
        showAnimator?.SetNextDirection(isForward);

        show.SetActive(true);
    }

    void SetLanguage(string code)
    {
        PlayerPrefs.SetString("Language", code);
        Debug.Log("Язык установлен: " + code);
        CloseLanguagePanel();
    }

    void QuitGame()
    {
        Application.Quit();
    }
}
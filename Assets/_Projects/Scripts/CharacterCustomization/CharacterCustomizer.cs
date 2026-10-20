using UnityEngine;
using System.Collections;

public class CharacterCustomizer : MonoBehaviour
{
    public SpriteRenderer headRenderer;
    public Sprite[] headSprites;
    private int currentHeadIndex = 0;

    public SpriteRenderer bodyRenderer;
    public Sprite[] bodySprites;
    private int currentBodyIndex = 0;

    public SpriteRenderer legsRenderer;
    public Sprite[] legsSprites;
    private int currentLegsIndex = 0;

    public GameObject headGlow;
    public GameObject bodyGlow;
    public GameObject legsGlow;

    private Coroutine activeGlowCoroutine;

    void Start()
    {
        LoadCustomization();

        headRenderer.sprite = headSprites[currentHeadIndex];
        bodyRenderer.sprite = bodySprites[currentBodyIndex];
        legsRenderer.sprite = legsSprites[currentLegsIndex];
    }

    public void NextHead()
    {
        currentHeadIndex = currentHeadIndex + 1;

        if (currentHeadIndex >= headSprites.Length) {
            currentHeadIndex = 0;
        }

        headRenderer.sprite = headSprites[currentHeadIndex];
        activeGlowCoroutine = StartCoroutine(ShowGlowTemporarily(headGlow));
    }

    public void PreviousHead()
    {
        currentHeadIndex = currentHeadIndex - 1;
        if (currentHeadIndex < 0) {
            currentHeadIndex = headSprites.Length - 1;
        }
        headRenderer.sprite = headSprites[currentHeadIndex];
        activeGlowCoroutine = StartCoroutine(ShowGlowTemporarily(headGlow));
    }

    public void NextBody()
    {
        currentBodyIndex = currentBodyIndex + 1;

        if (currentBodyIndex >= bodySprites.Length) {
            currentBodyIndex = 0;
        }

        bodyRenderer.sprite = bodySprites[currentBodyIndex];
        activeGlowCoroutine = StartCoroutine(ShowGlowTemporarily(bodyGlow));
    }

    public void PreviousBody()
    {
        currentBodyIndex = currentBodyIndex - 1;
        if (currentBodyIndex < 0) {
            currentBodyIndex = bodySprites.Length - 1;
        }
        bodyRenderer.sprite = bodySprites[currentBodyIndex];
        activeGlowCoroutine = StartCoroutine(ShowGlowTemporarily(bodyGlow));
    }

    public void NextLegs()
    {
        currentLegsIndex = currentLegsIndex + 1;

        if (currentLegsIndex >= legsSprites.Length) {
            currentLegsIndex = 0;
        }

        legsRenderer.sprite = legsSprites[currentLegsIndex];
        activeGlowCoroutine = StartCoroutine(ShowGlowTemporarily(legsGlow));
    }

    public void PreviousLegs()
    {
        currentLegsIndex = currentLegsIndex - 1;
        if (currentLegsIndex < 0) {
            currentLegsIndex = legsSprites.Length - 1;
        }
        legsRenderer.sprite = legsSprites[currentLegsIndex];
        activeGlowCoroutine = StartCoroutine(ShowGlowTemporarily(legsGlow));
    }

    private IEnumerator ShowGlowTemporarily(GameObject glowObject)
    {
        if (activeGlowCoroutine != null)
        {
            StopCoroutine(activeGlowCoroutine);
        }

        headGlow.SetActive(false);
        bodyGlow.SetActive(false);
        legsGlow.SetActive(false);

        glowObject.SetActive(true);
        yield return new WaitForSeconds(1.2f);
        glowObject.SetActive(false);
    }
    
     public void SaveCustomization()
    {
        PlayerPrefs.SetInt("SavedHeadIndex", currentHeadIndex);
        PlayerPrefs.SetInt("SavedBodyIndex", currentBodyIndex);
        PlayerPrefs.SetInt("SavedLegsIndex", currentLegsIndex);
        PlayerPrefs.Save();

        Debug.Log("Character customization saved!");
    }

    private void LoadCustomization()
    {
        currentHeadIndex = PlayerPrefs.GetInt("SavedHeadIndex", 0);
        currentBodyIndex = PlayerPrefs.GetInt("SavedBodyIndex", 0);
        currentLegsIndex = PlayerPrefs.GetInt("SavedLegsIndex", 0);
    }
}
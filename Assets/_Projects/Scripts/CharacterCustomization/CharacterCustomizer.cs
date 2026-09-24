using UnityEngine;

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

    void Start()
    {
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
    }

    public void PreviousHead()
    {
        currentHeadIndex = currentHeadIndex - 1;
        if (currentHeadIndex < 0) {
            currentHeadIndex = headSprites.Length - 1;
        }
        headRenderer.sprite = headSprites[currentHeadIndex];
    }

    public void NextBody()
    {
        currentBodyIndex = currentBodyIndex + 1;

        if (currentBodyIndex >= bodySprites.Length) {
            currentBodyIndex = 0;
        }

        bodyRenderer.sprite = bodySprites[currentBodyIndex];
    }

    public void PreviousBody()
    {
        currentBodyIndex = currentBodyIndex - 1;
        if (currentBodyIndex < 0) {
            currentBodyIndex = bodySprites.Length - 1;
        }
        bodyRenderer.sprite = bodySprites[currentBodyIndex];
    }

    public void NextLegs()
    {
        currentLegsIndex = currentLegsIndex + 1;

        if (currentLegsIndex >= legsSprites.Length) {
            currentLegsIndex = 0;
        }

        legsRenderer.sprite = legsSprites[currentLegsIndex];
    }

    public void PreviousLegs()
    {
        currentLegsIndex = currentLegsIndex - 1;
        if (currentLegsIndex < 0) {
            currentLegsIndex = legsSprites.Length - 1;
        }
        legsRenderer.sprite = legsSprites[currentLegsIndex];
    }
}
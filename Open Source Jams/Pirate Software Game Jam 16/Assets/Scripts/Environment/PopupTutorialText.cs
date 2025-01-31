using UnityEngine;
using TMPro;
using System.Collections;

public class PopupTutorialText : MonoBehaviour
{
    private PlayerMovement playerMovement => PlayerMovement.Instance;
    [Header("Configuration")]
    [TextArea(3, 10)]
    public string[] tutorialMessages;

    [Header("References")]
    public GameObject tutorialUI;
    public TextMeshProUGUI tutorialText;

    private Coroutine activeCoroutine;
    private bool isTutorialActive;
    private bool isAnimating;
    private int currentMessageIndex;
    private bool WaitingForAdvanceInput;
    private bool hasCompleted;
    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || hasCompleted || isTutorialActive) return;

        StartTutorialSequence();
    }

    private void StartTutorialSequence()
    {
        tutorialUI.SetActive(true);
        isTutorialActive = true;
        currentMessageIndex = 0;

        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);
        playerMovement.LockPlayerInput(true);

        activeCoroutine = StartCoroutine(DisplayTutorialMessages());
    }

    private IEnumerator DisplayTutorialMessages()
    {
        if (triggerCollider != null)
            triggerCollider.enabled = false;

        while (currentMessageIndex < tutorialMessages.Length)
        {
            if (!string.IsNullOrWhiteSpace(tutorialMessages[currentMessageIndex]))
            {
                // Start typewriter effect
                Coroutine typing = StartCoroutine(TypewriterEffect.TypeText(tutorialText, tutorialMessages[currentMessageIndex], 0.05f));
                isAnimating = true;
                yield return new WaitForSeconds(0.1f);

                // Wait for animation or skip input
                yield return new WaitUntil(() => Input.anyKeyDown || typing == null);
                if (typing != null) StopCoroutine(typing);
                tutorialText.text = tutorialMessages[currentMessageIndex];

                // Wait for advance input after animation completes
                yield return new WaitForSeconds(0.1f);
                yield return new WaitUntil(() => Input.anyKeyDown);

                // Clear text and move to next message
                tutorialText.text = "";
                currentMessageIndex++;
            }
            else
            {
                currentMessageIndex++;
            }
        }

        CompleteTutorial();
    }

    private void CompleteTutorial()
    {
        tutorialUI.SetActive(false);
        hasCompleted = true;
        isTutorialActive = false;
        tutorialText.text = "";

        playerMovement.LockPlayerInput(false);
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class BeNormalMinigame : MonoBehaviour
{
    public static BeNormalMinigame Instance;
    private GuardManager guardManager => GuardManager.Instance;
    public GameObject dialogueParent;

    [Header("Dialogue Configuration")]
    public List<DialogueTree> possibleTrees;
    private DialogueTree currentTree;
    private int currentNodeIndex = 0;

    [Header("UI References")]
    public TextMeshProUGUI guardText;
    public List<Button> responseButtons;

    [Header("System References")]
    public PlayerMovement playerMovement => PlayerMovement.Instance;
    public Guard possessedGuard;
    public Guard talkingGuard;
    public AudioManager audioManager;

    private void Awake() => Instance = this;

    public void StartDialogue()
    {
        if (dialogueParent.activeSelf) return;

        dialogueParent.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        FreezeActors();

        if (possibleTrees.Count == 0)
        {
            Debug.LogError("No dialogue trees assigned!");
            return;
        }

        currentTree = possibleTrees[Random.Range(0, possibleTrees.Count)];
        currentNodeIndex = 0;
        DisplayCurrentNode();

        talkingGuard.currentstate = Guard.AlertState.Idle;
        talkingGuard.agent.isStopped = true;
    }
    private void DisplayCurrentNode()
    {
        if (currentNodeIndex < 0 || currentNodeIndex >= currentTree.nodes.Count)
        {
            EndDialogue(true);
            return;
        }

        var currentNode = currentTree.nodes[currentNodeIndex];
        guardText.text = currentNode.npcLine;
        audioManager.PlaySFX(audioManager.Chatter_Effect_v2);

        for (int i = 0; i < responseButtons.Count; i++)
        {
            var currentButton = responseButtons[i];
            bool hasResponse = i < currentNode.responses.Count;
            currentButton.gameObject.SetActive(hasResponse);

            if (hasResponse)
            {
                SetupButton(currentButton, currentNode.responses[i], i);
            }
            else
            {
                currentButton.onClick.RemoveAllListeners();
            }
        }
    }

    private void SetupButton(Button button, DialogueResponse response, int index)
    {
        // Set button text
        Transform textTransform = button.transform.Find("Text");
        if (textTransform != null)
        {
            TextMeshProUGUI buttonText = textTransform.GetComponent<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = response.text;
        }

        // Clear and set click handler
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => HandleResponse(response, index));
    }

    private void HandleResponse(DialogueResponse response, int responseIndex)
    {
        if (!response.isGoodResponse)
        {
            // Immediate failure on wrong answer
            GameOver();
            return;
        }

        // Move to next node if response was good
        currentNodeIndex = response.nextNodeIndex;

        if (currentNodeIndex == -1)
        {
            SuccessfulDialogue();
        }
        else
        {
            DisplayCurrentNode();
        }
    }
    private void SuccessfulDialogue()
    {
        Debug.Log("Dialogue completed successfully!");

        // Explicitly reset both guards' navigation
        if (talkingGuard != null)
        {
            talkingGuard.agent.isStopped = false;
            talkingGuard.currentstate = Guard.AlertState.Idle;
        }

        EndDialogue(false);
        StartCoroutine(DelayedUnfreeze());
    }

    private IEnumerator DelayedUnfreeze()
    {
        yield return new WaitForSeconds(0.5f);
        UnfreezeActors();
    }
    private void GameOver()
    {
        Debug.Log("Game Over - Wrong response!");
        playerMovement.StopPossessingGuard();
        talkingGuard.ChasePlayer();
        EndDialogue(true);
    }

    private void EndDialogue(bool badDialogue)
    {
        dialogueParent.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Additional agent cleanup
        if (talkingGuard != null)
        {
            talkingGuard.agent.isStopped = false;
            talkingGuard.agent.ResetPath();
        }

        UnfreezeActors();

        talkingGuard.StartDialogueCooldown();
        possessedGuard.StartDialogueCooldown();
    }

    private void UnfreezeActors()
    {
        guardManager.UnfreezeGuards("guardnonekeycard");
        guardManager.UnfreezeGuards("guardkeycard");

        if (playerMovement != null)
        {
            playerMovement.isFrozen = false;
            playerMovement.rb.isKinematic = false;
        }
    }

    private void FreezeActors()
    {
        guardManager.FreezeGuards("guardnonekeycard");
        guardManager.FreezeGuards("guardkeycard");
    }
}
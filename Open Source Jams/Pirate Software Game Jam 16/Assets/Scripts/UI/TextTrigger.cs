using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(BoxCollider))]
public class TextTrigger : MonoBehaviour
{
    private LorePieces lorePieces => LorePieces.Instance;
    private GameObject textObject;
    private TextMeshProUGUI tmpText;
    public float displayTime = 3f;
    public string displayText = "Hello, World!";

    private Coroutine hideCoroutine;

    private void Start()
    {
        textObject = lorePieces.LoreParent;
        tmpText = lorePieces.tmpText;

        if (textObject != null)
        {
            textObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Show the text object
            if (textObject != null)
            {
                textObject.SetActive(true);
            }

            // Set the text
            if (tmpText != null)
            {
                Coroutine typing = StartCoroutine(TypewriterEffect.TypeText(tmpText, displayText, 0.05f));
            }

            // Start the hide timer
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }
            // hideCoroutine = StartCoroutine(HideAfterDelay());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Hide immediately when player exits
            if (textObject != null)
            {
                textObject.SetActive(false);
            }

            // Stop the coroutine if it's running
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }
        }
    }

    private void OnDestroy()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        if (textObject != null)
        {
            textObject.SetActive(false);
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);
        
        if (textObject != null)
        {
            textObject.SetActive(false);
        }
    }

    // Ensure the collider is set to be a trigger
    private void OnValidate()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }
}
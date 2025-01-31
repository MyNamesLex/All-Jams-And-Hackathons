using System.Collections;
using TMPro;
using UnityEngine;

public static class TypewriterEffect
{
    /// <summary>
    /// Start typing effect on a TextMeshProUGUI component,
    /// including a custom image as a cursor via an inline sprite.
    /// </summary>
    /// <param name="caller">MonoBehaviour for starting the coroutine (e.g. 'this').</param>
    /// <param name="textMeshPro">The TextMeshProUGUI to animate.</param>
    /// <param name="fullText">The full text to reveal.</param>
    /// <param name="typeSpeed">Delay in seconds between each character.</param>
    public static float StartTyping(
        MonoBehaviour caller,
        TextMeshProUGUI textMeshPro,
        string fullText,
        float typeSpeed = 0.05f)
    {
        if (caller == null || textMeshPro == null || string.IsNullOrEmpty(fullText))
        {
            return 0;
        }
        else
        {
            caller.StartCoroutine(TypeText(textMeshPro, fullText, typeSpeed));
            return fullText.Length * typeSpeed;
        }
    }

    public static void StopTyping(MonoBehaviour caller)
    {
        if (caller != null) caller.StopAllCoroutines();
    }

    public static IEnumerator TypeText(TextMeshProUGUI textMeshPro, string fullText, float typeSpeed)
    {
        textMeshPro.text = "";
        for (int i = 0; i < fullText.Length; i++)
        {
            textMeshPro.text = fullText.Substring(0, i + 1) + "|";
            yield return new WaitForSeconds(typeSpeed);
        }
    }
}

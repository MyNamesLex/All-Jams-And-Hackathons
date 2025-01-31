using UnityEngine;
using TMPro;

public class LorePieces : MonoBehaviour
{
    public static LorePieces Instance;

    public GameObject LoreParent;
    public TextMeshProUGUI tmpText;

    private void Awake()
    {
        Instance = this;
    }
}

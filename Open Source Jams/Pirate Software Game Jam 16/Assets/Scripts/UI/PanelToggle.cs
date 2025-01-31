using UnityEngine;
using TMPro;

public class PanelToggle : MonoBehaviour
{
    public GameObject Panel;
    public TextMeshProUGUI SettingsButtonText;

    public void OnClick()
    {
        if (Panel.gameObject.activeSelf)
        {
            SettingsButtonText.text = "Settings";
            Panel.SetActive(false);
            return;
        }
        else if (Panel.gameObject.activeSelf == false)
        {
            SettingsButtonText.text = "Hide Settings";
            Panel.SetActive(true);
            return;
        }
    }
}

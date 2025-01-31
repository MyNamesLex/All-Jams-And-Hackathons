using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class SceneChangeTrigger : MonoBehaviour
{
    public string sceneName;

    private void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogError("Scene name not specified in SceneChangeTrigger!");
            }
        }
    }

    private void OnValidate()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }
}
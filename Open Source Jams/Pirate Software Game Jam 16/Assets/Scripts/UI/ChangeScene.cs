using UnityEngine;

public class ChangeScene : MonoBehaviour
{
    public void ChangeToScene(string sceneName)
    {
        Debug.Log("Changing to scene: " + sceneName);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}

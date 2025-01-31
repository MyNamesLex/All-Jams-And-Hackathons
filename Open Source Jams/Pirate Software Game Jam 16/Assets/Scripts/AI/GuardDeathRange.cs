using UnityEngine;
using UnityEngine.SceneManagement;

public class GuardDeathRange : MonoBehaviour
{
    PlayerMovement playerMovement => PlayerMovement.Instance;
    public GridManager gridManager => GridManager.Instance;
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            if (playerMovement.isPossessing == false && playerMovement.isDismountingGuard == false && gameObject.GetComponentInParent<Guard>().currentstate == Guard.AlertState.ChasingPlayer)
            {
                if (gridManager == null)
                {
                    Debug.Log("grid manager null, reloading current scene");
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
                else
                {
                    gridManager.LoadGridDataAsset(gridManager.activeGridDataAsset);
                }
            }
        }
    }
}

using UnityEngine;

public class TPFailsafe : MonoBehaviour
{
    GridManager GridManager => GridManager.Instance;

    public void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            col.gameObject.transform.position = GridManager.activeGridDataAsset.SpawnLocation;
        }
    }
}

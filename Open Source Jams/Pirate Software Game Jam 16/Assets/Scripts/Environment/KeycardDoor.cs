using UnityEngine;
using UnityEngine.AI;

public class KeycardDoor : MonoBehaviour
{
    private NavMeshObstacle nmo;
    public BoxCollider BoxCollider;
    private PlayerMovement pm;
    public GameObject KeyPad;
    public Material KeyPadEnabled;

    public void Start()
    {
        pm = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        nmo = GetComponent<NavMeshObstacle>();
        BoxCollider = GetComponent<BoxCollider>();
    }

    public void OnTriggerEnter(Collider col)
    {
        if (col.GetComponentInParent<Guard>() != null && col.GetComponentInParent<Guard>().gameObject.CompareTag("GuardKeyCard")
            && col.name == "KeyCardRange" && col.GetComponentInParent<Guard>().currentstate == Guard.AlertState.isPossessed)
        {
            pm.StopPossessingGuard();
            BoxCollider.enabled = false;
            nmo.carving = false;

            if (KeyPad != null)
            {
                KeyPad.GetComponent<MeshRenderer>().material = KeyPadEnabled;
            }

            Destroy(this.gameObject);
        }
    }
}

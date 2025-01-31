using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

public class RammableWall : MonoBehaviour
{
    public AudioManager AudioManager => AudioManager.Instance;
    private NavMeshObstacle nmo;

    [Header("Bring Attention To Wall")]
    private GuardManager gm;
    public float AttractionDistance;

    [Header("VFX")]
    public VisualEffect RamVFX;
    public float RamVFXDuration;

    [Tooltip("RB Add force amount to be applied to the rammablewall")]
    public float RamForce;

    public void Start()
    {
        nmo = GetComponent<NavMeshObstacle>();
        gm = GameObject.FindGameObjectWithTag("Managers").GetComponent<GuardManager>();
    }

    public void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            PlayerMovement pm = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
            if (pm.isRamming)
            {
                AudioManager.PlaySFX(AudioManager.wall_crash);

                gm.AttractGuards(AttractionDistance, transform);

                GetComponent<Rigidbody>().isKinematic = false;
                GetComponent<Rigidbody>().AddForce(pm.transform.forward * RamForce);

                BoxCollider[] bc = gameObject.GetComponents<BoxCollider>();
                foreach (BoxCollider b in bc)
                {
                    b.enabled = false;
                }

                nmo.carving = false;

                StartCoroutine(VFXTimer());
                StartCoroutine(DecreasePlayerSpeed(pm));
            }
        }

        if (col.GetComponentInParent<Guard>() != null && col.gameObject.GetComponentInParent<Guard>().gameObject.tag.Contains("Guard")
        && GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().GuardBeingPossessed ==
        col.gameObject.GetComponentInParent<Guard>().gameObject && col.name == "RamRange")
        {
            PlayerMovement pm = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
            if (pm.isRamming)
            {
                AudioManager.PlaySFX(AudioManager.wall_crash);

                pm.StopPossessingGuard();
                gm.AttractGuards(AttractionDistance, transform);

                GetComponent<Rigidbody>().isKinematic = false;
                GetComponent<Rigidbody>().AddForce(pm.transform.forward * RamForce);

                BoxCollider[] bc = gameObject.GetComponents<BoxCollider>();
                foreach (BoxCollider b in bc)
                {
                    b.enabled = false;
                }

                nmo.carving = false;

                StartCoroutine(VFXTimer());
            }
        }
    }

    public IEnumerator VFXTimer()
    {
        yield return new WaitForSeconds(RamVFXDuration);
    }

    public IEnumerator DecreasePlayerSpeed(PlayerMovement pm)
    {
        pm.Speed /= 2;
        yield return new WaitForSeconds(2);
        pm.Speed *= 2;
        Destroy(gameObject);
    }
}

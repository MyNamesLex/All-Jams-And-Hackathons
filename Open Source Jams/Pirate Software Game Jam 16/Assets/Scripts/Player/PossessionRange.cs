using UnityEngine;

public class PossessionRange : MonoBehaviour
{
    public PlayerMovement pm;
    private bool inrange;
    private Collider col1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = pm.transform.position;
        if (inrange && col1 != null)
        {
            pm.PotentialGuardBeingPossessed = col1.transform.GetComponentInParent<Guard>().gameObject;
            pm.CanBeInGremlinMode = true;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (pm.isPossessing == false)
        {
            Debug.Log(col.gameObject.name);
            if (col.tag.Contains("Guard") && col.GetComponentInParent<Guard>().currentstate != Guard.AlertState.ChasingPlayer)
            {
                Debug.Log("guard in range");
                col1 = col;
                pm.CanBeInGremlinMode = true;
                inrange = true;
            }

            if (col.tag.Contains("Guard") && col.GetComponentInParent<Guard>().currentstate == Guard.AlertState.ChasingPlayer)
            {
                Debug.Log("guard chasing in range, no possess");
                pm.CanBeInGremlinMode = false;
                inrange = false;
            }
        }
        else
        {
            Debug.Log("player possessing and guard in range, no possess");
            pm.CanBeInGremlinMode = false;
            inrange = false;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.tag.Contains("Guard"))
        {
            inrange = false;
            col1 = null;
            pm.CanBeInGremlinMode = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardManager : MonoBehaviour
{
    public static GuardManager Instance;
    public Guard[] NoneKeyCardGuards;
    public Guard[] KeyCardGuards;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GameObject[] glist = GameObject.FindGameObjectsWithTag("GuardNoneKeyCard");

        for (int i = 0; i < glist.Length; i++)
        {
            NoneKeyCardGuards[i] = glist[i].GetComponent<Guard>();
        }

        GameObject[] glist2 = GameObject.FindGameObjectsWithTag("GuardKeyCard");

        for (int i = 0; i < glist2.Length; i++)
        {
            KeyCardGuards[i] = glist2[i].GetComponent<Guard>();
        }
    }

    public void FreezeGuards(string GuardType)
    {
        List<Guard> guards = new List<Guard>();
        guards = new List<Guard>(FindObjectsByType<Guard>(FindObjectsSortMode.None));

        foreach (Guard g in guards)
        {
            if (g != null)
            {
                g.agent.isStopped = true;
            }
        }
    }

    public void UnfreezeGuards(string GuardType)
    {
        List<Guard> guards = new List<Guard>();
        guards = new List<Guard>(FindObjectsByType<Guard>(FindObjectsSortMode.None));

        foreach (Guard g in guards)
        {
            if (g != null)
            {
                g.agent.isStopped = false;
            }
        }
    }

    public void AttractGuards(float Distance, Transform ObjectToGoTo)
    {
        foreach (Guard g in NoneKeyCardGuards)
        {
            if (g != null)
            {
                float d = Vector3.Distance(ObjectToGoTo.position, g.transform.position);
                if (d < Distance)
                {
                    g.agent.SetDestination(ObjectToGoTo.transform.position);
                }
            }
        }

        foreach (Guard g in KeyCardGuards)
        {
            if (g != null)
            {
                float d = Vector3.Distance(ObjectToGoTo.position, g.transform.position);
                if (d < Distance)
                {
                    g.agent.SetDestination(ObjectToGoTo.transform.position);
                }
            }
        }

    }
}

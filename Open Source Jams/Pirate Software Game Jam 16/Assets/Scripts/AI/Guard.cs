using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;
using System.Collections;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Guard))]
public class GuardEditor : Editor
{
    private Guard guard;

    void OnEnable()
    {
        guard = (Guard)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (!guard.PointToPointGuard) return;

        GUILayout.Space(10);
        GUILayout.Label("Path Editing Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Current Position"))
        {
            AddNewPointAtCurrentPosition();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Remove Last Point"))
        {
            RemoveLastPoint();
        }

        if (GUILayout.Button("Clear All Points"))
        {
            ClearAllPoints();
        }
        GUILayout.EndHorizontal();
    }

    private void AddNewPointAtCurrentPosition()
    {
        guard.Points.Add(new Guard.PointToPointGuardObject(
            guard.transform.localPosition,
            false,
            0
        ));
    }


    private void RemoveLastPoint()
    {
        if (guard.Points.Count > 0)
        {
            guard.Points.RemoveAt(guard.Points.Count - 1);
        }
    }

    private void ClearAllPoints()
    {
        guard.Points.Clear();
    }
}
#endif

public class Guard : MonoBehaviour
{
    private PlayerMovement PlayerMovement => PlayerMovement.Instance;
    private BeNormalMinigame BeNormalMinigame => BeNormalMinigame.Instance;
    public NavMeshAgent agent;
    public Transform Eyes;
    public Transform player;
    private float OGMemory;

    [Header("VFX")]
    public VisualEffect AlertVFX;
    public VisualEffect PossessionVFX;
    public int segments = 50;
    public float MeshReduceRadius = 10f;
    public MeshFilter visionMeshFilter;
    private Mesh visionMesh;

    [Header("Audio")]
    AudioManager AudioManager => AudioManager.Instance;

    [Header("AI")]
    [Range(0, 360)]

    public float FOV;
    public float WallStareDetectionRange;
    public float viewRadius;
    public float updateTime = 0.2f;
    public float Increment = 0f;
    public LayerMask targetMask;
    public LayerMask obstructionMask;
    public float range;
    [Tooltip("Make an invis transform in mid of map and assign it to this")]
    public Transform CentrePoint;
    [Tooltip("How long the guard will remember the player exists, when the memory float time runs out, it will revert back to roaming")]
    public float Memory;
    public Transform PlayerPossessionLoc;
    public Rigidbody rb;
    public enum AlertState { Idle, Suspicious, Chasing, Possessed, Roaming, ChasingPlayer, HeardSomethingSus, isPossessed, isDizzy }
    public AlertState currentstate;
    [Tooltip("Guard loop, goes to and from one point to another point, PointToPointGuard")]
    public bool PointToPointGuard;
    public List<PointToPointGuardObject> Points = new List<PointToPointGuardObject>();
    public int CurrentPoint = 0;

    private float throwawaystoretimeuntilcontinue;

    [Header("Heavy Guard, Cant Be Hijacked")]
    public bool HeavyGuard;

    [Header("Static Guard, Doesnt Move Unless To Chase, After Chase, Goes Back To Spot Guard Started In")]
    public bool StaticGuard;
    public bool WillRotate180;
    public bool IsOnDialogueCooldown;
    public float TimeUntilRotate;
    private float OGTimeUntilRotate;
    private bool Flipped = false;
    private Vector3 Point;
    private Quaternion Rot;

    [Header("Keycard Guard")]
    [Tooltip("Keycard Guard Has, will be ignored if tag doesnt say guard has a keycard, needs to match the value EXACTLY of what we want this guard to open")]
    public Color KeyCard;

    void Start()
    {
        if (PlayerMovement.Instance != null)
        {
            player = PlayerMovement.Instance.transform;
        }

        agent = GetComponent<NavMeshAgent>();
        OGMemory = Memory;
        rb = GetComponent<Rigidbody>();

        visionMesh = new Mesh();
        visionMeshFilter.mesh = visionMesh;

        if (PointToPointGuard)
        {
            agent.SetDestination(Points[CurrentPoint].position);
        }

        if (StaticGuard)
        {
            Point = transform.position;
            Rot = transform.rotation;
            if (WillRotate180)
            {
                OGTimeUntilRotate = TimeUntilRotate;
            }
            agent.SetDestination(Point);
        }

        if (StaticGuard == false && PointToPointGuard == false)
        {
            if (RandomPoint(CentrePoint.position, range, out Vector3 point))
            {
                agent.SetDestination(point);
            }
        }
    }
    [System.Obsolete]
    void Update()
    {
        // update the vision loop every 0.2 seconds
        Increment += Time.deltaTime;
        if (Increment >= updateTime)
        {
            VisionLoop();
        }
    }

    [System.Obsolete]
    private void VisionLoop()
    {
        if (agent.isStopped || currentstate == AlertState.Possessed)
        {
            UpdateVisionConeMesh();
            return;
        }
        CheckFOV();

        if (currentstate == AlertState.isPossessed && PlayerMovement.isFrozen)
        {
            agent.isStopped = true;
            rb.linearVelocity = Vector3.zero;
            return;
        }

        if (currentstate != AlertState.isPossessed)
        {
            if (currentstate == AlertState.ChasingPlayer)
            {
                ChasePlayer();
                UpdateAlertStateVFX(AlertState.Chasing);

                Memory -= Time.deltaTime;

                if (Memory <= 0)
                {
                    LostPlayer();
                    currentstate = AlertState.Idle;
                    UpdateAlertStateVFX(AlertState.Idle);
                }
            }
            else if (agent.remainingDistance <= 0.1f && PointToPointGuard == false && StaticGuard == false)
            {
                if (RandomPoint(CentrePoint.position, range, out Vector3 point))
                {
                    currentstate = AlertState.Roaming;
                    Debug.DrawRay(point, Vector3.up, Color.red, 1.0f);
                    agent.SetDestination(point);
                }
            }
            else if (PointToPointGuard)
            {
                PointToPointFunc();
            }
            else if (StaticGuard)
            {
                StaticGuardFunc();
            }
            else
            {
                UpdateAlertStateVFX(AlertState.Idle);
            }
        }
        else
        {
            UpdateAlertStateVFX(AlertState.Possessed);
            agent.isStopped = true;
        }
    }

    public bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, range, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        Debug.LogError("random point return false");
        result = Vector3.zero;
        return false;
    }

    [System.Obsolete]
    private void PointToPointFunc()
    {
        //Debug.Log($"Agent State: Stopped: {agent.isStopped}, Remaining Distance: {agent.remainingDistance}, Has Path: {agent.hasPath}, Path Status: {agent.pathStatus}");
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            CurrentPoint++;

            if (CurrentPoint >= Points.Count)
            {
                CurrentPoint = 0;
            }
            if (GameObject.FindGameObjectWithTag("GridManager") != null)
            {
                if (GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>().activeGridDataAsset.name == "Level5")
                {
                    Vector3 offset = Points[CurrentPoint].position + new Vector3(17.7243385f, 0.511333168f, 6.1608119f);
                    agent.SetDestination(offset);
                }
                else
                {
                    agent.SetDestination(Points[CurrentPoint].position);
                }
            }
            else
            {
                agent.SetDestination(Points[CurrentPoint].position);
            }
        }

        if (agent.isStopped)
        {
            Debug.Log("Has Stopped");

            CurrentPoint++;

            if (CurrentPoint >= Points.Count)
            {
                CurrentPoint = 0;
            }

            if (GameObject.FindGameObjectWithTag("GridManager") != null)
            {
                if (GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>().activeGridDataAsset.name == "Level5")
                {
                    Vector3 offset = Points[CurrentPoint].position + new Vector3(17.7243385f, 0.511333168f, 6.1608119f);
                    agent.SetDestination(offset);
                }
                else
                {
                    agent.SetDestination(Points[CurrentPoint].position);
                }
            }
            else
            {
                agent.SetDestination(Points[CurrentPoint].position);
            }
        }

        if (agent.remainingDistance <= 0.1f)
        {
            if (CurrentPoint > Points.Count - 1)
            {
                CurrentPoint = 0;
            }

            if (Points[CurrentPoint].position == null)
            {
                Debug.LogError(Points[CurrentPoint].name + " is null, not going anywhere");
            }

            if (Vector3.Distance(agent.destination, Points[CurrentPoint].position) <= 0.1f)
            {
                if (Points[CurrentPoint].Stop)
                {
                    if (throwawaystoretimeuntilcontinue == 0)
                    {
                        throwawaystoretimeuntilcontinue = Points[CurrentPoint].TimeUntilContinue;
                    }
                    else
                    {

                        throwawaystoretimeuntilcontinue -= Time.deltaTime;
                    }

                    //Debug.Log(throwawaystoretimeuntilcontinue);

                    if (throwawaystoretimeuntilcontinue < 0)
                    {
                        CurrentPoint++;

                        if (CurrentPoint >= Points.Count)
                        {
                            CurrentPoint = 0;
                        }

                        agent.SetDestination(Points[CurrentPoint].position);
                        throwawaystoretimeuntilcontinue = 0;
                    }
                }
                else
                {
                    CurrentPoint++;

                    if (CurrentPoint >= Points.Count)
                    {
                        CurrentPoint = 0;
                    }

                    if (GameObject.FindGameObjectWithTag("GridManager") != null)
                    {
                        if (GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>().activeGridDataAsset.name == "Level5")
                        {
                            Vector3 offset = Points[CurrentPoint].position + new Vector3(17.7243385f, 0.511333168f, 6.1608119f);
                            agent.SetDestination(offset);
                        }
                        else
                        {
                            agent.SetDestination(Points[CurrentPoint].position);
                        }
                    }
                    else
                    {
                        agent.SetDestination(Points[CurrentPoint].position);
                    }
                }
            }
        }
        //failsafe
        if (agent.remainingDistance == 0f)
        {
            CurrentPoint++;

            if (CurrentPoint >= Points.Count)
            {
                CurrentPoint = 0;
            }
            if (GameObject.FindGameObjectWithTag("GridManager") != null)
            {
                if (GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>().activeGridDataAsset.name == "Level5")
                {
                    Vector3 offset = Points[CurrentPoint].position + new Vector3(17.7243385f, 0.511333168f, 6.1608119f);
                    agent.SetDestination(offset);
                }
                else
                {
                    agent.SetDestination(Points[CurrentPoint].position);
                }
            }
            else
            {
                agent.SetDestination(Points[CurrentPoint].position);
            }
        }
    }

    public void StaticGuardFunc()
    {
        if (agent.remainingDistance <= 0.1f)
        {
            if (Vector3.Distance(agent.destination, Point) <= 1f && transform.rotation != Rot && Flipped == false)
            {
                transform.rotation = Rot;
            }
        }

        if (WillRotate180)
        {
            TimeUntilRotate -= Time.deltaTime;
            if (TimeUntilRotate <= 0)
            {
                TimeUntilRotate = OGTimeUntilRotate;

                if (Flipped)
                {
                    transform.rotation *= Quaternion.AngleAxis(180, Vector3.up);
                    return;
                }
                else
                {
                    transform.rotation *= Quaternion.AngleAxis(-180, Vector3.up);
                }

                Flipped = !Flipped;
            }
        }
    }

    public void ChasePlayer()
    {
        currentstate = AlertState.ChasingPlayer;
        agent.SetDestination(player.transform.position);
    }

    public void LostPlayer()
    {
        currentstate = AlertState.ChasingPlayer;
        Memory = OGMemory;
        Debug.Log("lost player");

        Vector3 point;
        if (RandomPoint(CentrePoint.position, range, out point) && PointToPointGuard == false && StaticGuard == false)
        {
            Debug.DrawRay(point, Vector3.up, Color.red, 1.0f);
            agent.SetDestination(point);
        }

        if (PointToPointGuard)
        {
            CurrentPoint = 0;
            agent.SetDestination(Points[CurrentPoint].position);
        }

        if (StaticGuard)
        {
            agent.SetDestination(Point);
        }
    }

    [System.Obsolete]
    private void CheckFOV()
    {
        UpdateVisionConeMesh();

        if (player == null) return;
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Transform target = playerObj.transform;
        if (playerMovement == null) return;

        // Check distance
        float distanceToTarget = Vector3.Distance(Eyes.position, target.position);
        if (distanceToTarget > viewRadius) return;

        // Check FOV angle
        Vector3 dirToTarget = (target.position - Eyes.position).normalized;
        if (Vector3.Angle(Eyes.forward, dirToTarget) >= FOV / 2) return;

        // Check obstruction
        if (Physics.Raycast(Eyes.position, transform.forward, distanceToTarget, obstructionMask))
        {
            Debug.Log("obstruction");
            if (PointToPointGuard)
            {
                Debug.Log("ptp obstruction");
                CurrentPoint = 0;
                agent.SetDestination(Points[CurrentPoint].position);
            }
            if (StaticGuard == false && RandomPoint(CentrePoint.position, range, out Vector3 point))
            {
                Debug.Log("roaming guard obstruction");
                agent.SetDestination(point);
            }
        }

        // Check if player is possessing and can trigger minigame
        if (playerMovement.isPossessing)
        {
            if (playerMovement.CanTriggerBeNormalMinigame && IsOnDialogueCooldown == false && BeNormalMinigame.Instance.dialogueParent.activeSelf == false)
            {
                StartBeNormalMinigame(playerMovement.PossessedGuard);
            }
            else
            {
                Debug.Log("Player hidden, possessing guard");
            }
        }
        else
        {
            currentstate = AlertState.ChasingPlayer;
            Debug.DrawRay(Eyes.position, dirToTarget * distanceToTarget, Color.green);
        }

        Debug.DrawRay(Eyes.position, transform.forward * WallStareDetectionRange, Color.red);
        if (Physics.Raycast(Eyes.position, transform.forward, WallStareDetectionRange, obstructionMask) && agent.remainingDistance <= 0.1f)
        {
            Debug.Log("obstruction");
            if (PointToPointGuard)
            {
                Debug.Log("ptp obstruction");
                CurrentPoint++;
                if (CurrentPoint >= Points.Count)
                {
                    CurrentPoint = 0;
                }
                agent.SetDestination(Points[CurrentPoint].position);
            }
            if (StaticGuard == false && RandomPoint(CentrePoint.position, range, out Vector3 point))
            {
                Debug.Log("roaming guard obstruction");
                agent.SetDestination(point);
            }
        }
        // no target found
        //ChasingPlayer = false;
    }

    public void StartDialogueCooldown()
    {
        StartCoroutine(DialogueCooldown());
    }

    private IEnumerator DialogueCooldown()
    {
        IsOnDialogueCooldown = true;
        yield return new WaitForSeconds(8f);
        IsOnDialogueCooldown = false;
    }

    private void UpdateVisionConeMesh()
    {
        if (player == null) return;
        if (Vector3.Distance(transform.position, player.position) > MeshReduceRadius)

            visionMesh.Clear();

        int OptimizedSegments;
        if (Vector3.Distance(transform.position, player.position) > MeshReduceRadius)
        {
            OptimizedSegments = segments / 2;
        }
        else
        {
            OptimizedSegments = segments;
        }

        Vector3[] vertices = new Vector3[OptimizedSegments + 1];
        int[] triangles = new int[OptimizedSegments * 3];

        vertices[0] = Vector3.zero;

        float halfFOV = FOV * 0.5f;
        float angleStep = FOV / OptimizedSegments;

        for (int i = 0; i < OptimizedSegments; i++)
        {
            float currentAngle = -halfFOV + i * angleStep;

            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward;

            float distance = viewRadius;
            if (Physics.Raycast(transform.position, transform.TransformDirection(dir),
                                out RaycastHit hit, viewRadius, obstructionMask))
            {
                distance = hit.distance;
            }

            Vector3 localDir = dir * distance;
            vertices[i + 1] = localDir;

            if (i < OptimizedSegments - 1)
            {
                int triIndex = i * 3;
                triangles[triIndex] = 0;
                triangles[triIndex + 1] = i + 1;
                triangles[triIndex + 2] = i + 2;
            }
        }

        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals();
    }

    public void StartBeNormalMinigame(Guard otherGuard)
    {
        BeNormalMinigame.talkingGuard = otherGuard;
        BeNormalMinigame.possessedGuard = this;

        BeNormalMinigame.StartDialogue();
    }

    public void UpdateAlertStateVFX(AlertState state)
    {
        //REFACTOR TO BE NON VFX BASED WELGL DOESNT LIKE VFX :c
        switch (state)
        {
            case AlertState.Idle:
                //AlertVFX.SetInt("Alert State", 2);
                //if (AlertVFX.gameObject.activeSelf == true) AlertVFX.gameObject.SetActive(false);
                break;
            case AlertState.Suspicious:
                //AlertVFX.SetInt("Alert State", 0);
                //if (AlertVFX.gameObject.activeSelf == false) AlertVFX.gameObject.SetActive(true);
                break;
            case AlertState.Chasing:
                AudioManager.PlaySFX(AudioManager.Alert);
                //AlertVFX.SetInt("Alert State", 1);
                //if (AlertVFX.gameObject.activeSelf == false) AlertVFX.gameObject.SetActive(true);
                break;
            case AlertState.Possessed:
                AudioManager.PlaySFX(AudioManager.possess_v4);
                //AlertVFX.SetInt("Alert State", 2);
                //if (AlertVFX.gameObject.activeSelf == true) AlertVFX.gameObject.SetActive(false);
                break;
        }
        /*
        if (state == AlertState.Possessed && PossessionVFX.gameObject.activeSelf == false)
        {
            PossessionVFX.gameObject.SetActive(true);
        }
        else if (state != AlertState.Possessed && PossessionVFX.gameObject.activeSelf == true)
        {
            PossessionVFX.gameObject.SetActive(false);
        }
        */
    }

    [System.Serializable]
    public class PointToPointGuardObject
    {
        public string name;
        public Vector3 position;
        public bool Stop;
        public float TimeUntilContinue;
        public PointToPointGuardObject(Vector3 pos, bool stop = false, float timeUntilContinue = 0)
        {
            position = pos;
            Stop = stop;
            TimeUntilContinue = timeUntilContinue;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;
    [Header("VFX")]
    public VisualEffect SprintPoof;

    [Header("Movement")]
    public Rigidbody rb;
    private const float YMinSpriteRot = -80.0f;
    private const float SpriteRotSpeed = 500f;
    private const float YMaxSpriteRot = 80.0f;
    private float Horizontal;
    private float Vertical;

    [Header("Camera")]
    public CameraMovement cam;

    [Header("Audio")]
    public AudioManager am;

    [Header("Ramming")]
    public float RamTimer;
    private bool LockRam;
    private float OGRamTimer;
    public float RamCooldownFloat;

    [Header("Possession")]
    public GameObject PotentialGuardBeingPossessed;
    public GameObject GuardBeingPossessed;
    public Guard PossessedGuard;
    public LayerMask PossessionMask;

    public Image PossessionMeter;

    public float PossessionDistance;
    public float PossessionTime;
    private float OGPossessionTime;
    private float OGPossessionDistance;
    private float slowFactor = 2.05f;

    [Tooltip("Adds Onto The Time Of TimeUntilGuardMovesAgain, once that timer is done, this timer starts")]
    public float TimeUntilCanPossessGuardAgain;
    public float TimeUntilGuardMovesAgain;
    public UnityEngine.UI.Image TimeUntilCanPossessGuardAgainImg;

    [Header("Pause Menu")]
    public GameObject PauseMenuParent;

    [Header("Managers")]
    public GuardManager guardManager;
    [Header("Be Normal Minigame")]
    public BeNormalMinigame bnm;
    public float MinigameEnabledTimer;
    private float MinigameTimeMax;

    [Header("Speed")]
    public float Speed;
    public float SneakSpeed;
    public float SprintSpeed;
    public float RamSpeed;

    [Header("States")]
    public bool isSneaking;
    public bool isWalking;
    public bool isPossessing;
    public bool isSprinting;
    public bool CanBeInGremlinMode;
    public bool LockedOutOfPossessingAGuard;
    public bool isPaused;
    public bool isRamming;
    public bool isFrozen;
    public bool isInputLocked;
    public bool isDismountingGuard;
    public bool CanTriggerBeNormalMinigame;

    [Header("Keycodes")]
    public KeyCode SneakKey;
    public KeyCode SprintKey;
    public KeyCode GremlinKey;
    public KeyCode PauseKey;
    public KeyCode RamKey;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = GetComponent<Rigidbody>();

        OGRamTimer = RamTimer;
        OGPossessionTime = PossessionTime;
        OGPossessionDistance = PossessionDistance;
        MinigameTimeMax = MinigameEnabledTimer;
    }

    private void Update()
    {
        if (isInputLocked)
        {
            ResetPlayerState();
            return;
        }

        Controls();

        if (isRamming && LockRam == false)
        {
            RamTimer -= Time.deltaTime;

            if (RamTimer <= 0)
            {
                LockRam = true;
                isRamming = false;
                StartCoroutine(RamCooldown());
            }
        }

        if (CanTriggerBeNormalMinigame == false)
        {
            MinigameEnabledTimer -= Time.deltaTime;

            if (MinigameEnabledTimer <= 0)
            {
                MinigameEnabledTimer = MinigameTimeMax;
                CanTriggerBeNormalMinigame = true;
            }
        }

        // player

        if (isSneaking)
        {
            Horizontal = Input.GetAxisRaw("Horizontal") * SneakSpeed;
            Vertical = Input.GetAxisRaw("Vertical") * SneakSpeed;
        }
        if (isWalking)
        {
            Horizontal = Input.GetAxisRaw("Horizontal") * Speed;
            Vertical = Input.GetAxisRaw("Vertical") * Speed;
        }
        if (isSprinting)
        {
            Horizontal = Input.GetAxisRaw("Horizontal") * SprintSpeed;
            Vertical = Input.GetAxisRaw("Vertical") * SprintSpeed;
        }
        if (isRamming)
        {
            Horizontal = Input.GetAxisRaw("Horizontal") * RamSpeed;
            Vertical = Input.GetAxisRaw("Vertical") * RamSpeed;
        }

        Vector3 Movement = cam.transform.right * Horizontal + cam.transform.forward * Vertical;
        Movement.y = 0f;

        Vector2 v2 = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
        Vector3 ClampedSpeed = new Vector3(0, 0, 0);

        // States
        if (isSneaking)
        {
            ClampedSpeed = Vector3.ClampMagnitude(v2, SneakSpeed);
        }
        if (isWalking)
        {
            ClampedSpeed = Vector3.ClampMagnitude(v2, Speed);
        }
        if (isSprinting)
        {
            ClampedSpeed = Vector3.ClampMagnitude(v2, SprintSpeed);
        }
        if (isRamming)
        {
            ClampedSpeed = Vector3.ClampMagnitude(v2, RamSpeed);
        }

        if (isPossessing && isFrozen == false)
        {
            PossessedGuard.rb.linearVelocity = new Vector3(ClampedSpeed.x, rb.linearVelocity.y, ClampedSpeed.y);

            PossessedGuard.rb.AddForce(Movement * Speed, ForceMode.Impulse);

            if (Movement.magnitude > 0.1f)
            {
                if (Movement.magnitude > 0.1f)
                {
                    Vector3 dir = Movement.normalized;

                    Quaternion qrot = Quaternion.LookRotation(dir);

                    Vector3 rot = qrot.eulerAngles;

                    if (rot.y > 180f)
                    {
                        rot.y -= 360f;
                    }

                    if (Mathf.Approximately(rot.y, 90f))
                    {
                        rot.y = YMaxSpriteRot;
                    }

                    if (Mathf.Approximately(rot.y, -90f))
                    {
                        rot.y = YMinSpriteRot;
                    }

                    qrot = Quaternion.Euler(rot);

                    PossessedGuard.transform.rotation = Quaternion.RotateTowards(PossessedGuard.transform.rotation, qrot, SpriteRotSpeed * Time.deltaTime);
                }

                PossessionDistance -= Movement.magnitude;

                float distance = Mathf.Clamp(PossessionDistance / OGPossessionDistance, 0f, 1f);
                float time = Mathf.Clamp(PossessionTime / OGPossessionTime, 0f, 1f);

                float fill = Mathf.Lerp(distance, time, 0.5f);

                PossessionMeter.fillAmount = Mathf.Lerp(PossessionMeter.fillAmount, fill, slowFactor);

                //Debug.Log(PossessionMeter.fillAmount);

                if (PossessionMeter.fillAmount <= 0)
                {
                    StopPossessingGuard();
                }
            }
        }
        else if (isFrozen == false)
        {
            rb.linearVelocity = new Vector3(ClampedSpeed.x, rb.linearVelocity.y, ClampedSpeed.y);

            rb.AddForce(Movement * Speed, ForceMode.Impulse);

            if (Movement.magnitude > 0.1f)
            {
                Vector3 dir = Movement.normalized;

                Quaternion qrot = Quaternion.LookRotation(dir);

                Vector3 rot = qrot.eulerAngles;

                if (rot.y > 180f)
                {
                    rot.y -= 360f;
                }

                if (Mathf.Approximately(rot.y, 90f))
                {
                    rot.y = YMaxSpriteRot;
                }

                if (Mathf.Approximately(rot.y, -90f))
                {
                    rot.y = YMinSpriteRot;
                }

                qrot = Quaternion.Euler(rot);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, qrot, SpriteRotSpeed * Time.deltaTime);
            }
        }

        // possession

        if (isPossessing)
        {
            transform.position = PossessedGuard.PlayerPossessionLoc.position;

            if (isFrozen == false)
            {
                PossessionTime -= Time.deltaTime;
            }
        }

        // pause

        if (isPaused)
        {
            guardManager.FreezeGuards("guardkeycard");
            guardManager.FreezeGuards("guardnonekeycard");
        }

        if (LockedOutOfPossessingAGuard)
        {
            TimeUntilCanPossessGuardAgainImg.gameObject.SetActive(true);

            float t = 7f;
            TimeUntilCanPossessGuardAgainImg.fillAmount -= Time.deltaTime / t;

            if (TimeUntilCanPossessGuardAgainImg.fillAmount <= 0)
            {
                TimeUntilCanPossessGuardAgainImg.gameObject.SetActive(false);
                TimeUntilCanPossessGuardAgainImg.fillAmount = 1;
                am.PlaySFX(am.Cooldown);
                LockedOutOfPossessingAGuard = false;
            }
        }
    }

    public IEnumerator RamCooldown()
    {
        yield return new WaitForSeconds(RamCooldownFloat);
        LockRam = false;
    }

    public void Controls()
    {
        if (Input.GetKeyDown(SneakKey))
        {
            isSneaking = true;
            isWalking = false;
        }
        if (Input.GetKeyUp(SneakKey))
        {
            isSneaking = false;
            isWalking = true;
        }

        if (Input.GetKeyDown(SprintKey))
        {
            isSprinting = true;
            isWalking = false;

            SprintPoof.SendEvent("Sprint");
        }
        if (Input.GetKeyUp(SprintKey))
        {
            isSprinting = false;
            isWalking = true;
            SprintPoof.SendEvent("Walk");
        }

        if (Input.GetKeyDown(GremlinKey) && CanBeInGremlinMode && LockedOutOfPossessingAGuard == false)
        {
            PossessGuard();
        }

        else if (Input.GetKeyDown(GremlinKey) && isPossessing)
        {
            StopPossessingGuard();
        }

        if (Input.GetKeyDown(PauseKey) && isPaused == false)
        {
            isPaused = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            PauseMenuParent.SetActive(true);
            rb.isKinematic = true;
        }
        else if (Input.GetKeyDown(PauseKey) && isPaused == true)
        {
            isPaused = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            PauseMenuParent.SetActive(false);
            rb.isKinematic = false;

            guardManager.UnfreezeGuards("guardkeycard");
            guardManager.UnfreezeGuards("guardnonekeycard");
        }

        if (Input.GetKeyDown(RamKey))
        {
            isRamming = true;
        }
        if (Input.GetKeyUp(RamKey))
        {
            isRamming = false;
        }
    }

    public void PossessGuard()
    {
        GuardBeingPossessed = PotentialGuardBeingPossessed;
        PotentialGuardBeingPossessed = null;
        PossessedGuard = GuardBeingPossessed.GetComponent<Guard>();

        if (PossessedGuard.HeavyGuard)
        {
            PossessedGuard.ChasePlayer();
            Debug.Log("Heavy Guard, cant be hijacked");
            return;
        }


        isPossessing = true;
        PossessedGuard.currentstate = Guard.AlertState.isPossessed;

        CanBeInGremlinMode = false;

        rb.useGravity = false;
        rb.isKinematic = true;
        transform.position = PossessedGuard.PlayerPossessionLoc.position;
    }

    public void StopPossessingGuard()
    {
        am.PlaySFX(am.Hop_Off);
        isDismountingGuard = true;
        isPossessing = false;
        PossessedGuard.currentstate = Guard.AlertState.isDizzy;
        PossessedGuard.rb.isKinematic = true;

        GuardBeingPossessed = null;

        StartCoroutine(GuardNotPossessedTimer());

        CanBeInGremlinMode = false;

        rb.useGravity = true;
        rb.isKinematic = false;

        PossessionTime = OGPossessionTime;
        PossessionDistance = OGPossessionDistance;
        PossessionMeter.fillAmount = 1;
    }

    IEnumerator GuardNotPossessedTimer()
    {
        LockedOutOfPossessingAGuard = true;

        yield return new WaitForSeconds(1f); // so player isnt insta killed isdismounting guard is basically invincibility frames 1s 
        isDismountingGuard = false;
        am.PlaySFX(am.dizzy_short);

        yield return new WaitForSeconds(TimeUntilGuardMovesAgain - 1f);

        if (PossessedGuard != null)
        {
            PossessedGuard.UpdateAlertStateVFX(Guard.AlertState.Idle);
            PossessedGuard.rb.isKinematic = false;
            PossessedGuard.agent.isStopped = false;

            Vector3 point;
            if (PossessedGuard.RandomPoint(PossessedGuard.CentrePoint.position, PossessedGuard.range, out point))
            {
                PossessedGuard.currentstate = Guard.AlertState.Roaming;
                Debug.DrawRay(point, Vector3.up, Color.red, 1.0f);
                PossessedGuard.agent.SetDestination(point);
            }

            PossessedGuard = null;
        }

        yield return new WaitForSeconds(TimeUntilCanPossessGuardAgain);
        LockedOutOfPossessingAGuard = false;
    }

    public void LockPlayerInput(bool state)
    {
        isInputLocked = state;
        if (state)
        {
            GuardManager.Instance.FreezeGuards("");
        }
        else
        {
            GuardManager.Instance.UnfreezeGuards("");
        }
    }

    public void ResetPlayerState()
    {
        if (PauseMenuParent.activeSelf)
        {
            PauseMenuParent.SetActive(false);
        }

        isSneaking = false;
        isWalking = true;
        isSprinting = false;
        isRamming = false;
        isPossessing = false;
        isFrozen = false;
        isPaused = false;
        isDismountingGuard = false;
        CanBeInGremlinMode = false;
        LockedOutOfPossessingAGuard = false;
        CanTriggerBeNormalMinigame = true;

        MinigameEnabledTimer = MinigameTimeMax;

        PossessionDistance = OGPossessionDistance;
        PossessionTime = OGPossessionTime;
        PossessionMeter.fillAmount = 1;
        SprintPoof.SendEvent("Walk");
    }
}

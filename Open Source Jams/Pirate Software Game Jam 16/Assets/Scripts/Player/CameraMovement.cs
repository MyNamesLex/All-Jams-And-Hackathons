using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Camera Correction")]
    public float RotationCorrectionSpeed = 2f;
    [Header("Camera Collision")]
    public float Margin = 0.1f;
    [Header("Player Movement")]
    public PlayerMovement PlayerMovement => PlayerMovement.Instance;

    [Header("Camera")]
    public bool LerpToForward;
    public float Height;
    public float LerpSpeed;
    public float MinY;
    public float MaxY;
    public float sensitivity;

    [Header("Target to Follow")]
    public Transform target;
    public Vector3 offset;

    [Header("Cam Collision Layer")]
    public LayerMask CamMask;
    public float SearchRadius;

    private float CamX;
    private float CamY;
    private float Distance = 3f;
    private Vector3 Pos;
    private Vector3 velocity;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (PlayerMovement.isPossessing)
        {
            Distance = 10;
            target = PlayerMovement.PossessedGuard.transform;
        }
        else
        {
            target = PlayerMovement.transform;
            Distance = 3;
        }

        CamX += Input.GetAxis("Mouse X") * sensitivity;
        CamY -= Input.GetAxis("Mouse Y") * sensitivity;
        CamY = Mathf.Clamp(CamY, MinY, MaxY);

        // Apply camera correction towards the target's facing direction when moving
        if (LerpToForward)
        {
            if (PlayerMovement.Instance.isWalking || PlayerMovement.Instance.isSprinting)
            {
                float targetYaw = target.eulerAngles.y;
                float angleDelta = Mathf.DeltaAngle(CamX, targetYaw);
                float corr = angleDelta * RotationCorrectionSpeed * Time.deltaTime;

                CamX = Mathf.Lerp(CamX, CamX + corr, 0.1f);
            }
        }


        Vector3 pivot = target.position + offset + Vector3.up * Height;
        Quaternion rotation = Quaternion.Euler(CamY, CamX, 0);
        Vector3 desiredPos = pivot - rotation * Vector3.forward * Distance;

        Vector3 direction = desiredPos - pivot;
        float distance = direction.magnitude;
        direction.Normalize();

        RaycastHit hit;
        if (Physics.SphereCast(pivot, SearchRadius, direction, out hit, distance, CamMask))
        {
            desiredPos = pivot + direction * (hit.distance - SearchRadius - Margin);
        }

        // Use SmoothDamp instead of Lerp
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref velocity,
            1f / LerpSpeed
        );

        transform.LookAt(target.position + offset);
    }
}
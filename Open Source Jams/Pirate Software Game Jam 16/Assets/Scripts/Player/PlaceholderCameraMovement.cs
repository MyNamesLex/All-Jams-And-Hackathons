using UnityEngine;

public class PlaceholderCameraMovement : MonoBehaviour
{
    [Header("Player Movement")]
    public PlayerMovement pm;

    [Header("Camera")]
    public float MinY;
    public float MaxY;
    public float Distance;
    public float Sensivity;

    [Header("Target to Follow")]
    public Transform target;

    private float CamX;
    private float CamY;

    void Update()
    {
        if (pm.isPossessing)
        {
            Distance = 10;
            target = pm.PossessedGuard.transform;
        }
        else
        {
            target = pm.transform;
            Distance = 7;
        }

        CamX += Input.GetAxis("Mouse X");
    }

    void LateUpdate()
    {
        // cam
        if (pm.isPossessing)
        {
            target = pm.PossessedGuard.transform;
        }
        else
        {
            target = pm.transform;
        }
        CamX += Input.GetAxis("Mouse X") * Sensivity * Time.deltaTime;
        CamY -= Input.GetAxis("Mouse Y") * Sensivity * Time.deltaTime;

        Vector3 Direction = new Vector3(0, 0, -Distance);

        Quaternion rotation = Quaternion.Euler(CamY, CamX, 0);

        transform.position = target.position + rotation * Direction;

        transform.LookAt(target.position);

    }
}

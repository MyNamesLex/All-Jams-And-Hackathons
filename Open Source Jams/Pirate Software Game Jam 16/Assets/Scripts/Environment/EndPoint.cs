using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndPoint : MonoBehaviour
{
    private GridManager GridManager => GridManager.Instance;
    public Transform ElevatorDoor1;
    public Transform ElevatorDoor2;
    public Vector3 door1EndPos;
    public Vector3 door2EndPos;

    private IEnumerator AnimateElevatorDoors()
    {
        ElevatorDoor1.DOLocalMove(door1EndPos, 1f).SetEase(Ease.InOutSine);
        ElevatorDoor2.DOLocalMove(door2EndPos, 1f).SetEase(Ease.InOutSine);
        yield return new WaitForSeconds(1f);
    }
    public int LevelIndex;
    private IEnumerator OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            yield return StartCoroutine(AnimateElevatorDoors());
            if (SceneManager.GetActiveScene().name == "Tutorial")
            {
                SceneManager.LoadScene("Play");
            }
            else
            {
                GridManager.SwitchToGrid(LevelIndex);
            }
        }
    }
}

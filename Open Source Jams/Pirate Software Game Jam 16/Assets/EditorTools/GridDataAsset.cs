using UnityEngine;

[CreateAssetMenu(fileName = "GridDataAsset", menuName = "Block/Grid Data Asset")]
public class GridDataAsset : ScriptableObject
{
    [Header("Splash")]
    public Sprite LevelSplash;
    public string LevelName;
    public AudioClip LevelMusic;

    [Header("Dimensions")]
    public int width;
    public int length;
    public int height;
    public float ThresholdAmount;
    public Vector3 CamStartPos;
    public Vector3 CamStartRot;
    public Vector3 SpawnLocation;
    public GameObject gridDecorationPrefab;

    [Header("Cells")]
    public int[] cells;
}

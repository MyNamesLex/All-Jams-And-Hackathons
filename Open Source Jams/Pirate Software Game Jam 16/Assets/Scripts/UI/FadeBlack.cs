using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class FadeEffect : MonoBehaviour
{
    public AudioManager AudioManager => AudioManager.Instance;
    [Tooltip("Main Menu Relevant Only")]
    public Camera MainCamera;
    public Transform cameraTransform;
    public RectTransform[] MainMenuUI;
    public RectTransform[] TutorialUI;
    public Image FadeBlackObj;
    public Vector3 CameraStartPos;
    public Vector3 CameraEndPos;
    public float CameraStartFocalLength;
    public float CameraEndFocalLength;

    public bool WantsTutorial;
    public Material SpecimenStartMaterial;
    public Material SpecimenEndMaterial;
    public GameObject SpecimenObj;
    private Coroutine CameraShakeCoroutine;

    [Header("Testing")]
    public bool TestMode;

    private void Start()
    {
        float shakeAmount = 0.01f;
        float duration = 0.5f;
        CameraShakeCoroutine = StartCoroutine(ShakeCamera(shakeAmount, duration));
    }

    private IEnumerator ShakeCamera(float shakeAmount, float duration)
    {
        while (true)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount),
                0
            );

            yield return cameraTransform.DOBlendableLocalMoveBy(randomOffset, duration)
                .SetEase(Ease.InOutSine)
                .WaitForCompletion();
        }
    }

    public void AskTutorial()
    {
        for (int i = 0; i < TutorialUI.Length; i++)
        {
            RectTransform ui = TutorialUI[i];
            ui.DOScale(1, 0.25f).SetEase(Ease.OutCubic).SetDelay(i * 0.2f);
        }
    }

    public void SendTutorialPref(bool pref)
    {
        WantsTutorial = pref;
    }

    public void TriggerStart()
    {
        AudioManager.PlaySFX(AudioManager.intro);
        for (int i = 0; i < MainMenuUI.Length; i++)
        {
            RectTransform ui = MainMenuUI[i];
            ui.DOScale(0, 0.25f).SetDelay(i * 0.1f).SetEase(Ease.OutCubic);
        }
        for (int i = 0; i < TutorialUI.Length; i++)
        {
            RectTransform ui = TutorialUI[i];
            ui.DOScale(0, 0.25f).SetDelay(i * 0.1f).SetEase(Ease.OutCubic);
        }

        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        yield return IntroAnimation();
        ControlFade(true);
        yield return new WaitForSeconds(1);
        ChangeScene("Tutorial");
    }

    private void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator IntroAnimation()
    {
        SpecimenObj.GetComponent<MeshRenderer>().material = SpecimenStartMaterial;
        cameraTransform.DOMove(CameraStartPos, 0.1f).SetEase(Ease.OutCubic);
        MainCamera.DOFieldOfView(CameraStartFocalLength, 0.1f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.1f);
        //CAMERA MOVE
        cameraTransform.DOMove(CameraEndPos, 1).SetEase(Ease.OutCubic);
        MainCamera.DOFieldOfView(CameraEndFocalLength, 1).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(1);
        //EYES OPEN
        SpecimenObj.GetComponent<MeshRenderer>().material = SpecimenEndMaterial;
    }

    private void ControlFade(bool isAnimatingIn)
    {
        FadeBlackObj.DOFade(isAnimatingIn ? 1 : 0, 1).SetEase(Ease.OutCubic);
    }
}


using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class FadeEffectTutorial : MonoBehaviour
{
    public Image FadeBlackObj;
    public float FadeSpeed;

    private void Start()
    {
        FadeBlackObj.DOFade(0, FadeSpeed).SetEase(Ease.Linear);
    }
}
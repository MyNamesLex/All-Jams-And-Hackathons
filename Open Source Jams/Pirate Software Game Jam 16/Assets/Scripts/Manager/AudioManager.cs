using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Grid Manager")]
    public GridManager gm => GridManager.Instance;
    [Header("AudioSources")]
    public AudioSource BGMAudio;
    public AudioSource SFXAudio;

    [Header("BGM Audio")]
    public AudioClip Game_Jam_Song_01_v1;
    public AudioClip Royal_Road_03_loopable_fast_v3;
    public AudioClip Royal_Road_03_loopable_normal;
    public AudioClip Technobabylon_2025_01_17;
    public AudioClip TripRiff_new;
    public AudioClip TripRiff_full;

    [Header("SFX Audio")]
    public AudioClip Alert;
    public AudioClip Chatter_Effect_v2;
    public AudioClip Chatter_effect;
    public AudioClip possess_v4;
    public AudioClip Cooldown;
    public AudioClip dizzy_short;
    public AudioClip Hop_Off;
    public AudioClip intro;
    public AudioClip Unlock;
    public AudioClip wall_crash;

    [Header("BGM Level Manager")]
    public AudioClip MainMenu;
    public AudioClip EndScreen;
    public AudioClip TutorialBGM;
    public AudioClip Lvl1BGM;
    public AudioClip Lvl2BGM;
    public AudioClip Lvl3BGM;
    public AudioClip Lvl4BGM;
    public AudioClip Lvl5BGM;
    public void Awake()
    {
        Instance = this;
    }
    public void Start()
    {
        if (gm == null)
        {
            Debug.Log(SceneManager.GetActiveScene().name);

            switch (SceneManager.GetActiveScene().name)
            {
                case "Tutorial":
                    PlayBGM(TutorialBGM);
                    break;
                case "MainMenu":
                    PlayBGM(MainMenu);
                    break;
                case "EndScreen":
                    PlayBGM(EndScreen);
                    break;
            }
        }
    }

    public void PlayBGM(AudioClip BGMAudioclip)
    {
        BGMAudio.clip = BGMAudioclip;
        BGMAudio.Play();
    }
    public void PlaySFX(AudioClip SFXAudioclip)
    {
        if (SFXAudio.isPlaying == false && SFXAudio.clip != SFXAudioclip)
        {
            SFXAudio.PlayOneShot(SFXAudioclip);
        }
    }
}

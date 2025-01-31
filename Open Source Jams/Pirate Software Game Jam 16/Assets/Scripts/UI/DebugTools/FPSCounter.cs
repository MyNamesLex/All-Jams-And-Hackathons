using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public GameObject DebugParent;
    public int FPS;
    public int AverageFPS;
    public int HighestFPS;
    public int LowestFPS;
    public int frameRange = 60;
    public int[] fpsBuffer;
    public int fpsBufferIndex;

    public TMP_Text highestFPSLabel, averageFPSLabel, lowestFPSLabel;

    private void Start()
    {
#if !UNITY_EDITOR || !DEVELOPMENT_BUILD
        DebugParent.SetActive(false);
#endif


    }
    void Update()
    {
        if (!DebugParent.activeSelf) return;

        if (fpsBuffer == null || fpsBuffer.Length != frameRange)
        {
            InitializeBuffer();
        }
        UpdateBuffer();
        CalculateFPS();

        highestFPSLabel.text = "Highest FPS: " + HighestFPS;
        averageFPSLabel.text = "Average FPS: " + AverageFPS;
        lowestFPSLabel.text = "Lowest FPS: " + LowestFPS;
    }

    void InitializeBuffer()
    {
        if (frameRange <= 0)
        {
            frameRange = 1;
        }
        fpsBuffer = new int[frameRange];
        fpsBufferIndex = 0;
    }

    void UpdateBuffer()
    {
        fpsBuffer[fpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);
        if (fpsBufferIndex >= frameRange)
        {
            fpsBufferIndex = 0;
        }
    }

    void CalculateFPS()
    {
        int sum = 0;
        int highest = 0;
        int lowest = int.MaxValue;
        for (int i = 0; i < frameRange; i++)
        {
            int fps = fpsBuffer[i];
            sum += fps;
            if (fps > highest)
            {
                highest = fps;
            }
            if (fps < lowest)
            {
                lowest = fps;
            }
        }
        FPS = (int)(sum / frameRange);
        AverageFPS = (int)(sum / frameRange);
        HighestFPS = highest;
        LowestFPS = lowest;
    }
}
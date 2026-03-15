using UnityEngine;

public class SetupFrameService : MonoBehaviour
{
    public int targetFPS = 60; // Set your desired frame rate here

    void Awake()
    {
        // Disable VSync to use Application.targetFrameRate
        QualitySettings.vSyncCount = 0; 

        // Set the target frame rate
        Application.targetFrameRate = targetFPS; 
    }
}

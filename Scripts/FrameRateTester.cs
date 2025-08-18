using UnityEngine;

public class FrameRateTester : MonoBehaviour
{
    //* Attach this script to the UserInterface game object.

    [SerializeField] int frameRate = -1;
    [SerializeField] bool vSync = true;

    void FixedUpdate()
    {
        Application.targetFrameRate = frameRate;
        // V-Sync'i açarsan targetFrameRate bir işe yaramaz haberin olsun.
        QualitySettings.vSyncCount = vSync ? 1 : 0;
    }
}

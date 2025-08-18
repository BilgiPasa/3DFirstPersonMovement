using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuManager : MonoBehaviour
{
    //* Attach this script to the UserInterface game object.

    public static bool gamePaused, dynamicFOV, settingsMenuOpened;
    const KeyCode escapeKey = KeyCode.Escape;
    [SerializeField] GameObject pauseMenuObject, settingsMenuObject, speedTextObject;
    [SerializeField] TextMeshProUGUI FOVText, mouseSensitivityText, speedText;
    [SerializeField] Toggle dynamicFOVToggle, speedTextToggle, increasedSensitivityToggle;
    [SerializeField] Slider FOVSlider, mouseSensitivitySlider;
    [SerializeField] PlayerSpawnAndSaveManager playerSpawnAndSaveManagerScript;

    void Start()
    {
        if (PlayerPrefs.GetInt("FOV") == 0)
        {
            PlayerPrefs.SetInt("FOV", 90);
        }

        if (PlayerPrefs.GetInt("mouseSensitivity") == 0)
        {
            PlayerPrefs.SetInt("mouseSensitivity", 100);
        }

        if (PlayerPrefs.GetInt("dynamicFOV") == 0)
        {
            PlayerPrefs.SetInt("dynamicFOV", 1);
        }

        if (PlayerPrefs.GetInt("speedTextObjectActive") == 0)
        {
            PlayerPrefs.SetInt("speedTextObjectActive", 1);
        }

        if (PlayerPrefs.GetInt("increasedSensitivity") == 0)
        {
            PlayerPrefs.SetInt("increasedSensitivity", -1);
        }

        PlayerCameraManager.normalFOV = PlayerPrefs.GetInt("FOV");
        PlayerCameraManager.sensitivity = PlayerPrefs.GetInt("mouseSensitivity");
    }

    void Update()
    {
        if (!(gamePaused || PlayerSpawnAndSaveManager.playerDied))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            speedText.text = $"Speed: {PlayerStatusManager.flatVelocityMagnitude}";
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (PlayerSpawnAndSaveManager.playerDied)
        {
            return;
        }

        if (Input.GetKeyDown(escapeKey))
        {
            if (!settingsMenuObject.activeSelf)
            {
                if (!gamePaused)
                {
                    Pause();
                }
                else
                {
                    Resume();
                }
            }
            else
            {
                GoBackToPauseMenu();
            }
        }

        PlayerCameraManager.normalFOV = PlayerPrefs.GetInt("FOV");
        PlayerCameraManager.sensitivity = PlayerPrefs.GetInt("mouseSensitivity");

        if (PlayerPrefs.GetInt("dynamicFOV") == 1)
        {
            dynamicFOV = true;
        }
        else if (PlayerPrefs.GetInt("dynamicFOV") == -1)
        {
            dynamicFOV = false;
        }

        if (PlayerPrefs.GetInt("speedTextObjectActive") == 1)
        {
            speedTextObject.SetActive(true);
        }
        else if (PlayerPrefs.GetInt("speedTextObjectActive") == -1)
        {
            speedTextObject.SetActive(false);
        }
    }

    void Pause()
    {
        playerSpawnAndSaveManagerScript.SavingTheGame();
        pauseMenuObject.SetActive(true);
        Time.timeScale = 0;
        gamePaused = true;
    }

    public void Resume()
    {
        pauseMenuObject.SetActive(false);
        Time.timeScale = 1;
        gamePaused = false;
    }

    public void Settings()
    {
        switch (PlayerCameraManager.normalFOV)
        {
            case 90:
                FOVText.text = "FOV: Normal";
                break;
            case 110:
                FOVText.text = "FOV: WIDE";
                break;
            case 30:
                FOVText.text = "FOV: Telescope";
                break;
            default:
                FOVText.text = $"FOV: {PlayerCameraManager.normalFOV}";
                break;
        }

        if (PlayerPrefs.GetInt("increasedSensitivity") == -1)
        {
            increasedSensitivityToggle.isOn = false;

            switch (PlayerCameraManager.sensitivity)
            {
                case 100:
                    mouseSensitivityText.text = "Mouse Sensitivity: Normal";
                    break;
                case 200:
                    mouseSensitivityText.text = "Mouse Sensitivity: FAST";
                    break;
                case 1:
                    mouseSensitivityText.text = "Mouse Sensitivity: Snail";
                    break;
                default:
                    mouseSensitivityText.text = $"Mouse Sensitivity: {PlayerCameraManager.sensitivity}";
                    break;
            }

            mouseSensitivitySlider.value = PlayerCameraManager.sensitivity;
        }
        else if (PlayerPrefs.GetInt("increasedSensitivity") == 1)
        {
            increasedSensitivityToggle.isOn = true;

            switch (PlayerCameraManager.sensitivity)
            {
                case 300:
                    mouseSensitivityText.text = "Mouse Sensitivity: VERY FAST";
                    break;
                case 400:
                    mouseSensitivityText.text = "Mouse Sensitivity: MAXIMUM";
                    break;
                default:
                    mouseSensitivityText.text = $"Mouse Sensitivity: {PlayerCameraManager.sensitivity}";
                    break;
            }

            mouseSensitivitySlider.value = PlayerCameraManager.sensitivity - 200;
        }

        if (PlayerPrefs.GetInt("dynamicFOV") == 1)
        {
            dynamicFOVToggle.isOn = true;
        }
        else if (PlayerPrefs.GetInt("dynamicFOV") == -1)
        {
            dynamicFOVToggle.isOn = false;
        }

        if (PlayerPrefs.GetInt("speedTextObjectActive") == 1)
        {
            speedTextToggle.isOn = true;
        }
        else if (PlayerPrefs.GetInt("speedTextObjectActive") == -1)
        {
            speedTextToggle.isOn = false;
        }

        FOVSlider.value = PlayerCameraManager.normalFOV;
        pauseMenuObject.SetActive(false);
        settingsMenuObject.SetActive(true);
        settingsMenuOpened = true;
    }

    public void FOV(float value)
    {
        switch (value)
        {
            case 90:
                FOVText.text = "FOV: Normal";
                break;
            case 110:
                FOVText.text = "FOV: WIDE";
                break;
            case 30:
                FOVText.text = "FOV: Telescope";
                break;
            default:
                FOVText.text = "FOV: " + Mathf.RoundToInt(value);
                break;
        }

        PlayerPrefs.SetInt("FOV", (int)FOVSlider.value);
    }

    public void Sensitivity(float value)
    {
        if (PlayerPrefs.GetInt("increasedSensitivity") == -1)
        {
            switch (value)
            {
                case 100:
                    mouseSensitivityText.text = "Mouse Sensitivity: Normal";
                    break;
                case 200:
                    mouseSensitivityText.text = "Mouse Sensitivity: FAST";
                    break;
                case 1:
                    mouseSensitivityText.text = "Mouse Sensitivity: Snail";
                    break;
                default:
                    mouseSensitivityText.text = "Mouse Sensitivity: " + Mathf.RoundToInt(value);
                    break;
            }

            PlayerPrefs.SetInt("mouseSensitivity", (int)mouseSensitivitySlider.value);
        }
        else if (PlayerPrefs.GetInt("increasedSensitivity") == 1)
        {
            switch (value)
            {
                case 100:
                    mouseSensitivityText.text = "Mouse Sensitivity: VERY FAST";
                    break;
                case 200:
                    mouseSensitivityText.text = "Mouse Sensitivity: MAXIMUM";
                    break;
                default:
                    mouseSensitivityText.text = "Mouse Sensitivity: " + Mathf.RoundToInt(value + 200);
                    break;
            }

            PlayerPrefs.SetInt("mouseSensitivity", (int)mouseSensitivitySlider.value + 200);
        }
    }

    public void DynamicFOVSwitch(bool active)
    {
        if (active)
        {
            PlayerPrefs.SetInt("dynamicFOV", 1);
        }
        else
        {
            PlayerPrefs.SetInt("dynamicFOV", -1);
        }
    }

    public void IncreasedSensitivitySwitch(bool active)
    {
        if (!active)
        {
            PlayerPrefs.SetInt("increasedSensitivity", -1);
        }
        else
        {
            PlayerPrefs.SetInt("increasedSensitivity", 1);
        }

        if (PlayerPrefs.GetInt("increasedSensitivity") == -1)
        {
            switch (mouseSensitivitySlider.value)
            {
                case 100:
                    mouseSensitivityText.text = "Mouse Sensitivity: Normal";
                    break;
                case 200:
                    mouseSensitivityText.text = "Mouse Sensitivity: FAST";
                    break;
                case 1:
                    mouseSensitivityText.text = "Mouse Sensitivity: Snail";
                    break;
                default:
                    mouseSensitivityText.text = "Mouse Sensitivity: " + Mathf.RoundToInt(mouseSensitivitySlider.value);
                    break;
            }

            PlayerPrefs.SetInt("mouseSensitivity", (int)mouseSensitivitySlider.value);
        }
        else if (PlayerPrefs.GetInt("increasedSensitivity") == 1)
        {
            switch (mouseSensitivitySlider.value)
            {
                case 100:
                    mouseSensitivityText.text = "Mouse Sensitivity: VERY FAST";
                    break;
                case 200:
                    mouseSensitivityText.text = "Mouse Sensitivity: MAXIMUM";
                    break;
                default:
                    mouseSensitivityText.text = "Mouse Sensitivity: " + Mathf.RoundToInt(mouseSensitivitySlider.value + 200);
                    break;
            }

            PlayerPrefs.SetInt("mouseSensitivity", (int)mouseSensitivitySlider.value + 200);
        }
    }

    public void SpeedTextSwitch(bool active)
    {
        if (active)
        {
            PlayerPrefs.SetInt("speedTextObjectActive", 1);
        }
        else
        {
            PlayerPrefs.SetInt("speedTextObjectActive", -1);
        }
    }

    public void GoBackToPauseMenu()
    {
        settingsMenuObject.SetActive(false);
        settingsMenuOpened = false;
        pauseMenuObject.SetActive(true);
    }

    public void QuittingGame()
    {
        Application.Quit();
    }
}

using System.Collections;
using UnityEngine;

public class PlayerSpawnAndSaveManager : MonoBehaviour
{
    //* Attach this script to the UserInterface game object.

    public static bool playerDied, spawnProtection;
    int normalSavingTheGameDelay = 20, pressingAltSavingTheGameDelay = 2, spawnProtectionSeconds = 3;
    float normalSavingTheGameTimer, pressingAltSavingTheGameTimer, playerWidthRadiusFromPlayerMovementManager;
    bool respawnButtonPressed;
    Transform playerTransform;
    [SerializeField] GameObject playerObject, deathMenuObject, pauseMenuObject, settingsMenuObject;
    [SerializeField] Transform playerModelTransform, cameraPositionTransform, cameraHolderTransform;
    [SerializeField] Rigidbody playerRigidbody;
    [SerializeField] Camera mainCamera;

    void Start()
    {
        playerTransform = playerObject.transform;
        playerWidthRadiusFromPlayerMovementManager = PlayerMovementManager.playerWidthRadius;
        StartCoroutine(LoadingTheSave());
    }

    void FixedUpdate()
    {
        // Autosave
        if (normalSavingTheGameTimer > 0)
        {
            normalSavingTheGameTimer -= Time.fixedDeltaTime;
        }
        else
        {
            SavingTheGame();
            normalSavingTheGameTimer = normalSavingTheGameDelay;
        }

        if (PlayerStatusManager.playerHealth <= 0)
        {
            if (!playerDied)
            {
                PlayerDeath();
            }

            if (respawnButtonPressed)
            {
                StartCoroutine(Respawning());
            }
        }

        if (playerRigidbody.position.y < -100 && !playerDied)
        {
            PlayerStatusManager.playerHealth = 0;
        }

        // Preventing not saving the game from Alt + F4
        if (pressingAltSavingTheGameTimer <= 0)
        {
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                SavingTheGame();
                pressingAltSavingTheGameTimer = pressingAltSavingTheGameDelay;
            }
            else
            {
                pressingAltSavingTheGameTimer = 0;
            }
        }
        else
        {
            pressingAltSavingTheGameTimer -= Time.fixedDeltaTime;
        }
    }

    public void SavingTheGame()
    {
        if (!playerDied)
        {
            PlayerPrefs.SetInt("playerDied", -1);
            PlayerPrefs.SetFloat("playerPositionX", playerRigidbody.position.x);
            PlayerPrefs.SetFloat("playerPositionY", playerRigidbody.position.y);
            PlayerPrefs.SetFloat("playerPositionZ", playerRigidbody.position.z);
        }
        else
        {
            PlayerPrefs.SetInt("playerDied", 1);
            PlayerPrefs.SetFloat("playerPositionX", playerTransform.position.x);
            PlayerPrefs.SetFloat("playerPositionY", playerTransform.position.y);
            PlayerPrefs.SetFloat("playerPositionZ", playerTransform.position.z);
        }

        PlayerPrefs.SetFloat("playerVelocityX", playerRigidbody.velocity.x);
        PlayerPrefs.SetFloat("playerVelocityY", playerRigidbody.velocity.y);
        PlayerPrefs.SetFloat("playerVelocityZ", playerRigidbody.velocity.z);
        PlayerPrefs.SetFloat("playerRotationX", PlayerCameraManager.xRotation);
        PlayerPrefs.SetFloat("playerRotationY", PlayerCameraManager.yRotation);
        PlayerPrefs.SetString("playerHealth", PlayerStatusManager.playerHealth.ToString());
    }

    void PlayerDeath()
    {
        PlayerDespawning();
        // Making death effects and writing "player died" to chat or something like that.
        SavingTheGame();
    }

    void PlayerDespawning()
    {
        playerDied = true;
        pauseMenuObject.SetActive(false);
        settingsMenuObject.SetActive(false);
        deathMenuObject.SetActive(true);
        PlayerStatusManager.playerHealth = 0;
        playerObject.SetActive(false);
        mainCamera.fieldOfView = PlayerPrefs.GetInt("FOV");
    }

    IEnumerator Respawning()
    {
        playerObject.SetActive(true);
        respawnButtonPressed = false;
        spawnProtection = true;
        PlayerMovementManager.startOfFall = 0;
        PlayerMovementManager.endOfFall = 0;
        PlayerMovementManager.fallDistance = 0;
        playerTransform.localScale = new Vector3(playerWidthRadiusFromPlayerMovementManager * 2, PlayerMovementManager.playerHeight / 2, playerWidthRadiusFromPlayerMovementManager * 2);
        PlayerMovementManager.crouching = false;
        playerRigidbody.position = Vector3.zero;
        playerRigidbody.velocity = Vector3.zero;
        PlayerCameraManager.xRotation = 0;
        PlayerCameraManager.yRotation = 0;
        PlayerStatusManager.playerHealth = 100;
        deathMenuObject.SetActive(false);
        playerDied = false;
        yield return new WaitForSeconds(spawnProtectionSeconds);
        spawnProtection = false;
        SavingTheGame();
    }

    IEnumerator LoadingTheSave()
    {
        if (PlayerPrefs.GetInt("playerDied") == 0)
        {
            PlayerPrefs.SetInt("playerDied", -1);
        }

        if (PlayerPrefs.GetInt("playerCrouching") == 0)
        {
            PlayerPrefs.SetInt("playerCrouching", -1);
        }

        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("playerHealth")))
        {
            PlayerStatusManager.playerHealth = int.Parse(PlayerPrefs.GetString("playerHealth"));
        }
        else
        {
            PlayerStatusManager.playerHealth = 100;
        }

        PlayerCameraManager.xRotation = PlayerPrefs.GetFloat("playerRotationX");
        PlayerCameraManager.yRotation = PlayerPrefs.GetFloat("playerRotationY");
        cameraHolderTransform.position = cameraPositionTransform.position;
        cameraHolderTransform.rotation = Quaternion.Euler(PlayerCameraManager.xRotation, PlayerCameraManager.yRotation, 0);
        playerModelTransform.rotation = Quaternion.Euler(0, PlayerCameraManager.yRotation, 0);

        if (PlayerPrefs.GetInt("playerDied") == -1)
        {
            spawnProtection = true;
            playerRigidbody.position = new Vector3(PlayerPrefs.GetFloat("playerPositionX"), PlayerPrefs.GetFloat("playerPositionY"), PlayerPrefs.GetFloat("playerPositionZ"));
            playerRigidbody.velocity = new Vector3(PlayerPrefs.GetFloat("playerVelocityX"), PlayerPrefs.GetFloat("playerVelocityY"), PlayerPrefs.GetFloat("playerVelocityZ"));

            if (PlayerPrefs.GetInt("playerCrouching") == -1)
            {
                playerTransform.localScale = new Vector3(playerWidthRadiusFromPlayerMovementManager * 2, PlayerMovementManager.playerHeight / 2, playerWidthRadiusFromPlayerMovementManager * 2);
                PlayerMovementManager.crouching = false;
            }
            else if (PlayerPrefs.GetInt("playerCrouching") == 1)
            {
                playerTransform.localScale = new Vector3(playerWidthRadiusFromPlayerMovementManager * 2, PlayerMovementManager.crouchHeight / 2, playerWidthRadiusFromPlayerMovementManager * 2);
                PlayerMovementManager.crouching = true;
            }

            yield return new WaitForSeconds(spawnProtectionSeconds);
            spawnProtection = false;
        }
        else if (PlayerPrefs.GetInt("playerDied") == 1)
        {
            PlayerDespawning();
            playerTransform.position = new Vector3(PlayerPrefs.GetFloat("playerPositionX"), PlayerPrefs.GetFloat("playerPositionY"), PlayerPrefs.GetFloat("playerPositionZ"));
        }
    }

    public void RespawnButtonPressed()
    {
        respawnButtonPressed = true;
    }
}

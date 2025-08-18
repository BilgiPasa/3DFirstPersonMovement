using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class PlayerMovementManager : MonoBehaviour
{
    //* Attach this script to the Player game object.
    //* In Unity Editor, make the gravity "-60".
    //* In Unity Editor, layer 3 should be "Static Normal Layer".
    //* In Unity Editor, layer 6 should be "Static Bouncy Layer".
    //* In the project settings, make the default Physics material a frictionless and not bouncy material.
    //* Don't change any constants' values if not necessary.

    [Header("Horizontal and Vertical")]
    public static int vertical, horizontal, normalMoveSpeed = 9, runSpeed = 12;
    public static bool onSlope;
    const int normalGroundDrag = 10;
    const float theMoveMultiplier = 625.005f, airMoveMultiplier = 0.08f, airDrag = 0.78739f, bouncyGroundDrag = 12.5f, minimum = 0.1f;
    int crouchSpeed = 6, theMoveSpeed;
    Vector3 normalizedMoveDirection, normalizedSlopeMoveDirection;
    Transform playerTransform;
    Rigidbody playerRigidbody;
    RaycastHit slopeHit;

    [Header("Crouch")]
    public static float playerHeight = 3, crouchHeight = 2, playerWidthRadius = 0.5f;
    public static bool crouching;
    bool dontUncrouch;

    [Header("Coyote Time")]
    const float coyoteTime = 0.15f;
    float coyoteTimeCounter;

    [Header("Jump And Fall")]
    public static float startOfFall, endOfFall, fallDistance;
    public static bool jumping, groundedForAll;
    const float groundedSphereRadius = 0.3f, jumpingCooldown = 0.1f, jumpAgainCooldown = 0.3f;
    int normalJumpForce = 21, bouncyJumpForce = 63, maxFallWithoutBouncyJumpCalculationByThisScript = 5, maxFallWithoutFallDamage = 10, maxFallWithoutParticles = 5;
    bool readyToJump = true, jumpingInput, groundedForBouncyEnvironment, playerTouchingToAnyGround, falling, wasFalling, wasGrounded, justBeforeGroundedForNormalEnvironment, justBeforeGroundedForBouncyEnvironment;

    [Header("Keybinds")]
    KeyCode forwardKey = KeyCode.W, leftKey = KeyCode.A, backwardKey = KeyCode.S, rightKey = KeyCode.D, jumpKey = KeyCode.Space, crouchKey = KeyCode.LeftShift;

    [Header("Inputs")]
    [SerializeField] Transform playerModelTransform;
    [SerializeField] CapsuleCollider playerCapsuleCollider;
    [SerializeField] ParticleSystem jumpingDownParticles;
    [SerializeField] LayerMask staticNormalLayer, staticBouncyLayer;

    void Awake()
    {
        playerTransform = transform;
        playerCapsuleCollider.height = 2;
        playerCapsuleCollider.radius = 0.5f;
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        playerRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        if (!PauseMenuManager.gamePaused)
        {// I didn't added the if not player died condition because if player dies, this script does not work because it is attached to the player.
            MovementInputs();
        }
    }

    void FixedUpdate()
    {// I didn't added the if not game paused condition because if game pauses, FixedUpdate pauses too.
        // And also I didn't added the if not player died condition because if player dies, this script does not work because it is attached to the player.
        // These functions' order are intentional, i wouldn't recommend you to change the order.
        GroundedCheckAndFallingCheckAndBouncyJumpAndFallDamageAndCoyoteTime();
        Jump();
        Crouch();
        Drag();
        Movement();
        GravityAndSpeedControl();
        WasFallingAndWasGroundedCheck();
    }

    void MovementInputs()
    {
        // You can use Input.GetAxis... for inputs. But, I wanted to build horizontal and vertical input myself.
        if (Input.GetKey(forwardKey) && !Input.GetKey(backwardKey))
        {
            vertical = 1;
        }
        else if (!Input.GetKey(forwardKey) && Input.GetKey(backwardKey))
        {
            vertical = -1;
        }
        else
        {
            vertical = 0;
        }

        if (Input.GetKey(rightKey) && !Input.GetKey(leftKey))
        {
            horizontal = 1;
        }
        else if (!Input.GetKey(rightKey) && Input.GetKey(leftKey))
        {
            horizontal = -1;
        }
        else
        {
            horizontal = 0;
        }

        jumpingInput = Input.GetKey(jumpKey);
    }

    void GroundedCheckAndFallingCheckAndBouncyJumpAndFallDamageAndCoyoteTime()
    {
        if (!crouching)
        {
            groundedForAll = Physics.CheckSphere(playerTransform.position - new Vector3(0, playerHeight / 2, 0), groundedSphereRadius, staticNormalLayer | staticBouncyLayer);
            groundedForBouncyEnvironment = Physics.CheckSphere(playerTransform.position - new Vector3(0, playerHeight / 2, 0), groundedSphereRadius, staticBouncyLayer);
        }
        else
        {
            groundedForAll = Physics.CheckSphere(playerTransform.position - new Vector3(0, crouchHeight / 2, 0), groundedSphereRadius, staticNormalLayer | staticBouncyLayer);
            groundedForBouncyEnvironment = Physics.CheckSphere(playerTransform.position - new Vector3(0, crouchHeight / 2, 0), groundedSphereRadius, staticBouncyLayer);
        }

        falling = !groundedForAll && playerRigidbody.velocity.y < -minimum;

        if (!wasFalling && falling)
        {
            if (!crouching)
            {
                startOfFall = playerTransform.position.y - playerHeight / 2;
            }
            else
            {
                startOfFall = playerTransform.position.y - crouchHeight / 2;
            }
        }

        if (!wasGrounded && groundedForAll)
        {
            if (!crouching)
            {
                endOfFall = playerTransform.position.y - playerHeight / 2;
            }
            else
            {
                endOfFall = playerTransform.position.y - crouchHeight / 2;
            }

            fallDistance = startOfFall - endOfFall;
            //print(fallDistance); // For testing
        }

        if (fallDistance > minimum)
        {
            PlayerStatusManager.fallDistanceIsBiggerThanMinimum = true;

            if (fallDistance > maxFallWithoutBouncyJumpCalculationByThisScript && groundedForBouncyEnvironment && !crouching && readyToJump && !jumping)
            {
                Jumping(bouncyJumpForce);
            }

            if (fallDistance > maxFallWithoutParticles && !jumpingDownParticles.isPlaying)
            {
                jumpingDownParticles.Play();
            }

            if (fallDistance > maxFallWithoutFallDamage && groundedForAll && !groundedForBouncyEnvironment && !PlayerSpawnAndSaveManager.spawnProtection)
            {
                PlayerStatusManager.playerHealth -= (int)fallDistance - maxFallWithoutFallDamage;
            }

            startOfFall = 0;
            endOfFall = 0;
            fallDistance = 0;
            wasFalling = true;
            wasGrounded = true;
        }

        if (groundedForAll)
        {
            coyoteTimeCounter = coyoteTime;
            justBeforeGroundedForNormalEnvironment = groundedForAll && !groundedForBouncyEnvironment;
            justBeforeGroundedForBouncyEnvironment = groundedForBouncyEnvironment;
        }
        else if (coyoteTimeCounter <= 0)
        {
            coyoteTimeCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }
    }

    void Jump()
    {
        if (jumpingInput && readyToJump && !jumping)
        {
            if (justBeforeGroundedForNormalEnvironment && ((!groundedForAll && coyoteTimeCounter > 0) || (groundedForAll && !groundedForBouncyEnvironment && playerTouchingToAnyGround)))
            {
                Jumping(normalJumpForce);
            }
            else if (justBeforeGroundedForBouncyEnvironment && ((!groundedForAll && coyoteTimeCounter > 0) || (groundedForAll && groundedForBouncyEnvironment && playerTouchingToAnyGround)))
            {
                Jumping(bouncyJumpForce);
            }
        }
    }

    void Jumping(int jumpForce)
    {
        readyToJump = false;
        jumping = true;
        playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
        playerRigidbody.AddForce(jumpForce * playerTransform.up, ForceMode.VelocityChange);
        Invoke(nameof(JumpAgainReset), jumpAgainCooldown);
        Invoke(nameof(JumpingReset), jumpingCooldown);
    }

    /* For a continuous jump, use JumpAgainReset. If you don't want to use JumpAgainReset, make a jump buffer
    function and use it but don't forget to add "coyoteTimeCounter = 0;" in your jumping function after the
    jumping force. But you don't need to do that in this script if you are using JumpAgainReset. */
    void JumpAgainReset()
    {
        readyToJump = true;
    }

    void JumpingReset() // For jump height consistency
    {
        jumping = false;
    }

    void Crouch()
    {
        if (jumping)
        {
            return;
        }

        if (!crouching && Input.GetKey(crouchKey))
        {
            playerTransform.localScale = new Vector3(playerWidthRadius * 2, crouchHeight / 2, playerWidthRadius * 2);

            if (groundedForAll)
            {
                playerRigidbody.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (playerHeight / 2 - crouchHeight / 2), playerTransform.position.z);
            }

            crouching = true;
            PlayerPrefs.SetInt("playerCrouching", 1);
        }
        else if (crouching)
        {// Bilgi için https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Physics.CheckCapsule.html sitesine bakabilirsin. -0.075f'i de girebildiği ama küçücük bir kısmı CapsuleCollider ile temas ettiği için uncrouch yapamama durumu olmasın diye koydum.
            dontUncrouch = Physics.CheckCapsule(playerTransform.position + new Vector3(0, playerHeight - crouchHeight / 2 - (playerWidthRadius - 0.01f) - 0.075f, 0), playerTransform.position + new Vector3(0, crouchHeight / 2 - (playerWidthRadius - 0.01f), 0), playerWidthRadius - 0.01f, staticNormalLayer | staticBouncyLayer);

            if (!Input.GetKey(crouchKey) && !dontUncrouch)
            {
                if (groundedForAll)
                {
                    playerRigidbody.position = new Vector3(playerTransform.position.x, playerTransform.position.y + (playerHeight / 2 - crouchHeight / 2), playerTransform.position.z);
                }

                playerTransform.localScale = new Vector3(playerWidthRadius * 2, playerHeight / 2, playerWidthRadius * 2);
                crouching = false;
                PlayerPrefs.SetInt("playerCrouching", -1);
            }
        }
    }

    void Drag()
    {
        if (groundedForAll && !jumping && !PlayerStatusManager.sliding)
        {
            playerRigidbody.drag = !groundedForBouncyEnvironment ? normalGroundDrag : bouncyGroundDrag;
        }
        else
        {
            playerRigidbody.drag = airDrag;
        }
    }

    void Movement()
    {
        normalizedMoveDirection = (playerModelTransform.forward * vertical + playerModelTransform.right * horizontal).normalized;
        onSlope = ((!crouching && Physics.Raycast(playerTransform.position, Vector3.down, out slopeHit, playerHeight / 2 + groundedSphereRadius * 2)) || (crouching && Physics.Raycast(playerTransform.position, Vector3.down, out slopeHit, crouchHeight / 2 + groundedSphereRadius * 2))) && slopeHit.normal != Vector3.up; // slopeHit.normal kısmını sona koyman lazım çünkü Raycast'i bilmeden hit olan şeyi hesaplamaya çalışırsan olmaz.

        if (playerRigidbody.drag != airDrag)
        {
            if (!onSlope)
            {
                playerRigidbody.AddForce(theMoveSpeed * theMoveMultiplier * Time.fixedDeltaTime * normalizedMoveDirection, ForceMode.Acceleration);
            }
            else
            {
                normalizedSlopeMoveDirection = Vector3.ProjectOnPlane(normalizedMoveDirection, slopeHit.normal);
                playerRigidbody.AddForce(theMoveSpeed * theMoveMultiplier * Time.fixedDeltaTime * normalizedSlopeMoveDirection, ForceMode.Acceleration);
            }
        }
        else
        {
            if (!onSlope)
            {
                playerRigidbody.AddForce(theMoveSpeed * theMoveMultiplier * airMoveMultiplier * Time.fixedDeltaTime * normalizedMoveDirection, ForceMode.Acceleration);
            }
            else
            {
                normalizedSlopeMoveDirection = Vector3.ProjectOnPlane(normalizedMoveDirection, slopeHit.normal);
                playerRigidbody.AddForce(theMoveSpeed * theMoveMultiplier * airMoveMultiplier * Time.fixedDeltaTime * normalizedSlopeMoveDirection, ForceMode.Acceleration);
            }
        }
    }

    void GravityAndSpeedControl()
    {
        if (groundedForAll && playerTouchingToAnyGround && onSlope)
        {
            playerRigidbody.useGravity = crouching || playerRigidbody.velocity.y > minimum;

            if (!crouching && playerRigidbody.velocity.y > minimum)
            {
                playerRigidbody.AddForce(new Vector3(0, 50, 0), ForceMode.Acceleration);
            }
        }
        else
        {
            playerRigidbody.useGravity = true;
        }

        if (crouching)
        {
            theMoveSpeed = crouchSpeed;
        }
        else if (PlayerStatusManager.running)
        {
            theMoveSpeed = runSpeed;
        }
        else
        {
            theMoveSpeed = normalMoveSpeed;
        }

        if (Mathf.Abs(playerRigidbody.velocity.z) <= minimum)
        {
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, playerRigidbody.velocity.y, 0);
        }

        if (Mathf.Abs(playerRigidbody.velocity.x) <= minimum)
        {
            playerRigidbody.velocity = new Vector3(0, playerRigidbody.velocity.y, playerRigidbody.velocity.z);
        }

        if (Mathf.Abs(playerRigidbody.velocity.y) <= minimum)
        {
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
        }
    }

    void WasFallingAndWasGroundedCheck()
    {
        wasFalling = falling;
        wasGrounded = groundedForAll;
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == 3 || collision.gameObject.layer == 6)
        {
            playerTouchingToAnyGround = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 3 || collision.gameObject.layer == 6)
        {
            playerTouchingToAnyGround = false;
        }
    }
}

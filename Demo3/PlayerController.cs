using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour, IMovableEntity
{
    public enum MovementAction
    {
        Stationary,
        Moving,
        AirMoving,
        AirStationary
    }

    public enum EventAction
    {
        OnPWC, // Primary Weapon Cooldown
        OnSWC, // Secondary Weapon Cooldown
        OnMovementPowerUp,
        OnWeaponPowerUp
    }

    public event Action<MovementAction> OnMovementStateChanged;
    public event Action<EventAction, bool> OnEventActionChanged;

    [SerializeField] private PlayerSettings settings;
    [SerializeField] private WeaponSettings primaryWeapon;
    [SerializeField] private WeaponSettings secondaryWeapon;

    public IPlayerMovementHandler movementHandler;
    public IPlayerShootingHandler shootingHandler;
    public IPlayerInputHandler inputHandler;

    private Rigidbody rb;
    private Transform playerCamera;
    private readonly float movementSpeedMod = 1;
    private readonly float jumpHeightMod = 1;
    private readonly float damageDealtMod = 1;
    private Vector3 spawnPoint;
    private Collider groundCollision;

    public Collider GroundCollision { get => groundCollision; set => groundCollision = value; }

    private void Awake()
    {
        // Initialize components and handlers
        rb = GetComponent<Rigidbody>();
        playerCamera = Camera.main.transform;
        movementHandler ??= new PlayerMovementHandler(rb, settings, playerCamera, movementSpeedMod, jumpHeightMod);
        shootingHandler ??= new PlayerShootingHandler(playerCamera, primaryWeapon, secondaryWeapon, damageDealtMod);
        inputHandler ??= new PlayerInputHandler();
        spawnPoint = transform.position;
    }

    private void Start()
    {
        SetStartingRotation();
    }

    private void Update()
    {
        // Handle player input, movement, shooting, and environment checks
        inputHandler.HandleInput();
        movementHandler.HandleRotation(inputHandler.MouseX, inputHandler.MouseY);
        movementHandler.HandleJump(inputHandler.IsJumping);
        shootingHandler.HandleShooting(inputHandler, Time.time, OnEventActionChanged);
        movementHandler.HandleMovement(inputHandler.MoveHorizontal, inputHandler.MoveVertical);
        HandleUpdatableActions();
        HandleAbyss();
    }

    private void SetStartingRotation()
    {
        // Set initial player and camera rotation
        rb.MoveRotation(Quaternion.Euler(Vector3.up * 180f));
        playerCamera.localRotation = Quaternion.identity;
    }

    private void HandleUpdatableActions()
    {
        // Check for state changes in movement and weapon cooldowns
        MovementAction newAction = movementHandler.DetermineMovementAction(inputHandler.MoveHorizontal, inputHandler.MoveVertical);
        if (newAction != movementHandler.CurrentMovementAction)
        {
            movementHandler.CurrentMovementAction = newAction;
            OnMovementStateChanged?.Invoke(newAction);
        }

        shootingHandler.CheckWeaponCooldowns(Time.time, OnEventActionChanged);
    }

    private void HandleAbyss()
    {
        // Respawn if the player falls below a certain height
        if (transform.position.y < -40f)
        {
            rb.velocity = Vector3.zero;
            transform.position = spawnPoint;
            SetStartingRotation();
        }
    }

    public void ApplySpeedModifier(float modifier, float duration)
    {
        // Temporarily modify speed with a coroutine
        StartCoroutine(ApplyModifierCoroutine(
            () => {
                movementHandler.MovementSpeedMod += modifier;
                OnEventActionChanged?.Invoke(EventAction.OnMovementPowerUp, true);
            },
            () => {
                movementHandler.MovementSpeedMod -= modifier;
                OnEventActionChanged?.Invoke(EventAction.OnMovementPowerUp, movementHandler.MovementSpeedMod != movementSpeedMod || movementHandler.JumpHeightMod != jumpHeightMod);
            },
            duration));
    }

    public void ApplyJumpHeightModifier(float modifier, float duration)
    {
        // Temporarily modify jump height with a coroutine
        StartCoroutine(ApplyModifierCoroutine(
            () => {
                movementHandler.JumpHeightMod += modifier;
                OnEventActionChanged?.Invoke(EventAction.OnMovementPowerUp, true);
            },
            () => {
                movementHandler.JumpHeightMod -= modifier;
                OnEventActionChanged?.Invoke(EventAction.OnMovementPowerUp, movementHandler.MovementSpeedMod != movementSpeedMod || movementHandler.JumpHeightMod != jumpHeightMod);
            },
            duration));
    }

    public void ApplyDamageDealtModifier(float modifier, float duration)
    {
        // Temporarily modify damage dealt with a coroutine
        StartCoroutine(ApplyModifierCoroutine(
            () => {
                shootingHandler.DamageDealtMod += modifier;
                OnEventActionChanged?.Invoke(EventAction.OnWeaponPowerUp, true);
            },
            () => {
                shootingHandler.DamageDealtMod -= modifier;
                OnEventActionChanged?.Invoke(EventAction.OnWeaponPowerUp, shootingHandler.DamageDealtMod != damageDealtMod);
            },
            duration));
    }

    private IEnumerator ApplyModifierCoroutine(Action applyModifier, Action revertModifier, float duration)
    {
        // Apply a modifier for a specified duration, then revert it
        applyModifier();
        if (duration > 0) yield return new WaitForSeconds(duration);
        revertModifier();
    }

    public void ModifyVelocity(Vector3 velocity)
    {
        movementHandler.VelocityMod = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Update ground collision status when colliding with ground layer
        if (collision.gameObject.layer != LayerMask.NameToLayer("Ground")) return;
        movementHandler.IsGrounded = true;
        groundCollision = collision.collider;
    }

    private void OnCollisionExit(Collision collision)
    {
        // Reset ground collision status when exiting ground collision
        if (collision.collider != groundCollision) return;
        movementHandler.IsGrounded = false;
        groundCollision = null;
    }
}

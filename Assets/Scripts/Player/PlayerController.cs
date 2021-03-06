using Input;
using Physics;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using World;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// Switches between force and velocity based movement.
        /// </summary>
        [Header("Movement")] [SerializeField] [Tooltip("Switches between force and velocity based movement")]
        private bool useForceBasedMovement = true;

        /// <summary>
        /// Magnitude of movement when using force
        /// </summary>
        [SerializeField] [Tooltip("Magnitude of movement when using force")]
        private float forceMagnitude = 50;

        /// <summary>
        /// Magnitude of movement when using velocity.
        /// </summary>
        [SerializeField] [Tooltip("Magnitude of movement when using velocity")]
        private float velocityMagnitude = 20;

        /// <summary>
        /// The force with which the player jumps.
        /// </summary>
        [Header("Jumping")] [SerializeField] [Tooltip("The force with which the player jumps")]
        private float jumpForce = 15.0f;

        [SerializeField] [Tooltip("Left stick Y-Value to jump")]
        private float controllerLeftStickJumpAxisValue = 0.8f;

        /// <summary>
        /// The angle which determines what counts as ground for jumping.
        /// </summary>
        [SerializeField] [Range(0, 90)] [Tooltip("The angle which determines what counts as ground for jumping")]
        private float groundAngle = 30.0f;

        /// <summary>
        /// Inner dead zone of the controller.
        /// </summary>
        [Header("Controller")] [SerializeField] [Tooltip("Inner dead zone of the controller")]
        private float innerDeadZone = 0.1f;

        /// <summary>
        /// Outer dead zone of the controller.
        /// </summary>
        [SerializeField] [Tooltip("Outer dead zone of the controller")]
        private float outerDeadZone = 0.9f;

        /// <summary>
        /// The ability to throw objects.
        /// </summary>
        [Header("Abilities")] [SerializeField] [Tooltip("The ability to throw objects")]
        private ThrowAbility throwAbility;

        /// <summary>
        /// How much the player gets slowed when holding something.
        /// </summary>
        [SerializeField] private float holdingSlownessFactor = 0.5f;

        /// <summary>
        /// The ability to change the gravity.
        /// </summary>
        [SerializeField] [Tooltip("The ability to change the gravity")]
        private ChangeGravityAbility changeGravityAbility;

        /// <summary>
        /// The PauseMenu component.
        /// </summary>
        [Header("Game Interaction")] [SerializeField] [Tooltip("The PauseMenu component")]
        private PauseMenu pauseMenu;

        /// <summary>
        /// The PlayerInput component.
        /// </summary>
        [SerializeField] [Tooltip("The PlayerInput component")]
        private PlayerInput playerInput;

        /// <summary>
        /// Whether the player is currently grounded.
        /// </summary>
        [Header("Debugging")] [SerializeField] [Tooltip("Whether the player is currently grounded")]
        private bool isGrounded;

        /// <summary>
        /// Prevents the player from picking up objects in certain situations (e.g. going to another puzzle).
        /// </summary>
        public bool CanPickUpObjects { get; set; } = true;

        /// <summary>
        /// Whether the player is currently using the mouse and keyboard or a controller.
        /// </summary>
        public bool IsUsingMouse { get; private set; }
        
        /// <summary>
        /// The input actions for the player (Unity's new input system).
        /// </summary>
        public PlayerInputAction InputAction { get; private set; }
        
        /// <summary>
        /// The <see cref="PhysicsBody2D"/> component of the player.
        /// </summary>
        private PhysicsBody2D _pb;

        /// <summary>
        /// The current move direction, set by the input system callback, used in FixedUpdate.
        /// </summary>
        private Vector2 _moveDirection;

        /// <summary>
        /// The main camera.
        /// </summary>
        private Camera _camera;

        /// <summary>
        /// The mouse and keyboard input scheme. Used for differentiation.
        /// </summary>
        private InputControlScheme _mouseControlScheme;

        private void Awake()
        {
            _camera = Camera.main;
            _pb = GetComponent<PhysicsBody2D>();
            // Subscribe to movement events
            InputAction = new PlayerInputAction();
            InputAction.Player.Move.performed += OnMove;
            // Required to stop player from moving, as performed will not always be called with Vector.zero
            InputAction.Player.Move.canceled += OnMoveCanceled;
            
            InputAction.Player.Jump.performed += _ => Jump();
            InputAction.Player.Throw.performed += OnThrow;
            InputAction.Player.PickUp.performed += OnPickUp;
            InputAction.Player.GravityUp.performed += _ => changeGravityAbility.SetGravity(GravityDirection.Up);
            InputAction.Player.GravityDown.performed += _ => changeGravityAbility.SetGravity(GravityDirection.Down);
            InputAction.Player.GravityLeft.performed += _ => changeGravityAbility.SetGravity(GravityDirection.Left);
            InputAction.Player.GravityRight.performed += _ => changeGravityAbility.SetGravity(GravityDirection.Right);
            
            // Separate input for MainMenu
            InputAction.PlayerMainMenu.GravityUp.performed += _ => changeGravityAbility.SetGravity(GravityDirection.Up);
            InputAction.PlayerMainMenu.GravityDown.performed += _ => changeGravityAbility.SetGravity(GravityDirection.Down);
            InputAction.PlayerMainMenu.GravityLeft.performed += _ => changeGravityAbility.SetGravity(GravityDirection.Left);
            InputAction.PlayerMainMenu.GravityRight.performed += _ => changeGravityAbility.SetGravity(GravityDirection.Right);

            // Get mouse/keyboard control scheme
            foreach (InputControlScheme controlScheme in InputAction.controlSchemes)
            {
                if (!controlScheme.SupportsDevice(Mouse.current))
                    continue;

                _mouseControlScheme = controlScheme;
                break;
            }

            // Pause menu
            if (pauseMenu != null)
                pauseMenu.OnContinue += OnPauseMenuContinue;
            InputAction.Player.Pause.performed += OnPause;
            InputAction.UI.UnPause.performed += OnUnPause;
        }

        private void OnEnable() => InputAction.Player.Enable();

        private void OnDisable() => InputAction.Disable();

        private void OnDestroy()
        {
            InputAction.Dispose();
            if (pauseMenu != null)
                pauseMenu.OnContinue -= OnPauseMenuContinue;
        }

        private void Start()
        {
            // Disable UI input
            InputAction.UI.Disable();
        }

        /// <summary>
        /// Read mouse and left stick input for aiming.
        /// </summary>
        private void Update()
        {
            IsUsingMouse = playerInput.currentControlScheme.Equals(_mouseControlScheme.name);
            
            // Skip if no camera for mouse input
            if (_camera == null)
                return;

            Vector2 lookDirection;
            // Get mouse for aiming
            if (IsUsingMouse)
            {
                Vector2 mousePosition = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                lookDirection = mousePosition - (Vector2) transform.position;
            }
            // Get controller for aiming
            else
            {
                lookDirection = Gamepad.current.rightStick.ReadValue();
            }

            // Turn player if aim is on other side.
            Vector3 localScale = transform.localScale;
            localScale.x = Vector2.SignedAngle(Vector2.up, lookDirection) > 0 ? -1 : 1;
            transform.localScale = localScale;

            // Set the direction for throwing
            throwAbility.SetThrowDirection(lookDirection.normalized);
        }

        /// <summary>
        /// Handle movement and jump.
        /// </summary>
        private void FixedUpdate()
        {
            // Apply dead zones to last received movement input
            Vector2 movement = ApplyDeadZones(_moveDirection, innerDeadZone, outerDeadZone);

            // Apply to rigidbody
            if (useForceBasedMovement)
            {
                float force = forceMagnitude * (throwAbility.IsHolding ? holdingSlownessFactor : 1.0f) * _pb.mass;
                _pb.ApplyForce(new Vector2(movement.x, 0.0f) * force);
            }
            else
            {
                _pb.SetVelocity(new Vector2(movement.x, 0.0f) * velocityMagnitude);
            }

            // If player moves up, jump
            if (isGrounded && movement.y > controllerLeftStickJumpAxisValue)
                Jump();
        }

        /// <summary>
        /// Checks if the player can jump again (isGrounded check).
        /// </summary>
        /// <param name="other"></param>
        private void OnCollisionEnter2D(Collision2D other)
        {
            // Collision Normals to determine if player is grounded
            if (Vector2.Angle(other.contacts[0].normal, Vector2.up) < groundAngle)
                isGrounded = true;
        }

        /// <summary>
        /// Drops the currently held object. Used by the level to prevent the player from taking objects to other puzzles.
        /// </summary>
        public void ForceDropPickedUpObject()
        {
            if (throwAbility.IsHolding)
            {
                throwAbility.Drop();
                // Remove mass depending on custom gravity
                _pb.RemoveMass(throwAbility.PickedUpBodyMass *
                               (PhysicsBody2D.GlobalGravityAcceleration / _pb.customGravity.y));
            }
        }

        /// <summary>
        /// Read movement from Input System.
        /// </summary>
        private void OnMove(InputAction.CallbackContext ctx)
        {
            _pb.collisionDragEnabled = false;
            _moveDirection = ctx.ReadValue<Vector2>();
        }

        /// <summary>
        /// Reset movement.
        /// </summary>
        private void OnMoveCanceled(InputAction.CallbackContext _)
        {
            _pb.collisionDragEnabled = true;
            _moveDirection = Vector2.zero;
        }

        /// <summary>
        /// Picks up a <see cref="Pickupable"/> object close-by.
        /// </summary>
        private void OnPickUp(InputAction.CallbackContext obj)
        {
            if (!CanPickUpObjects)
                return;

            if (throwAbility.IsHolding)
            {
                throwAbility.Drop();
                if (!throwAbility.IsHolding)
                {
                    // Remove mass depending on custom gravity
                    RemovePickedUpMass();
                }
            }
            else
            {
                throwAbility.PickUp();
                if (throwAbility.IsHolding)
                {
                    // Add mass from picked up body, depending on custom gravity
                    AddPickedUpMass();
                }
            }
        }

        /// <summary>
        /// Throw the currently hold object.
        /// </summary>
        private void OnThrow(InputAction.CallbackContext _)
        {
            if (!throwAbility.IsHolding)
                return;

            throwAbility.Throw();
            if (!throwAbility.IsHolding)
            {
                // Remove mass depending on custom gravity
                RemovePickedUpMass();
            }
        }

        /// <summary>
        /// If grounded, adds jump force to player.
        /// </summary>
        private void Jump()
        {
            if (!isGrounded)
                return;

            isGrounded = false;
            _pb.collisionDragEnabled = false;
            float force = jumpForce * (throwAbility.IsHolding ? holdingSlownessFactor : 1.0f) * _pb.mass;
            _pb.ApplyForce(new Vector2(0, force));
        }

        /// <summary>
        /// <see cref="OnPauseMenuContinue"/>.
        /// </summary>
        private void OnUnPause(InputAction.CallbackContext _)
        {
            OnPauseMenuContinue();
        }

        /// <summary>
        /// Pause game and show PauseMenu.
        /// </summary>
        private void OnPause(InputAction.CallbackContext _)
        {
            InputAction.UI.Enable();
            InputAction.Player.Disable();
            WorldManager.SetPaused(true);
            pauseMenu.SetVisibility(true);
        }

        /// <summary>
        /// Re-enables player input and continues the game
        /// </summary>
        private void OnPauseMenuContinue()
        {
            InputAction.UI.Disable();
            InputAction.Player.Enable();
            WorldManager.SetPaused(false);
            pauseMenu.SetVisibility(false);
        }

        /// <summary>
        /// Adds the mass of the picked up object to the player (respectively to custom gravity).
        /// </summary>
        private void AddPickedUpMass()
        {
            float gravityNormalized = PhysicsBody2D.GlobalGravityAcceleration / _pb.customGravity.y;
            _pb.AddMass(throwAbility.PickedUpBodyMass * gravityNormalized);
            _pb.dragAxisFactor.x += throwAbility.PickedUpBodyMass;
        }

        /// <summary>
        /// Removes the mass of the picked up object from the player (respectively to custom gravity).
        /// </summary>
        private void RemovePickedUpMass()
        {
            float gravityNormalized = PhysicsBody2D.GlobalGravityAcceleration / _pb.customGravity.y;
            _pb.RemoveMass(throwAbility.PickedUpBodyMass * gravityNormalized);
            _pb.dragAxisFactor.x -= throwAbility.PickedUpBodyMass;
        }

        /// <summary>
        /// Applies dead zones on an input-vector.
        /// Also normalizes the input, if necessary.
        /// </summary>
        /// <param name="input">Input as a vector</param>
        /// <param name="inner">Defines the inner dead zone</param>
        /// <param name="outer">Defines the outer dead zone</param>
        private static Vector2 ApplyDeadZones(Vector2 input, float inner, float outer)
        {
            // Apply dead zones per axis
            Vector2 result = new Vector2(ApplyDeadZones(input.x, inner, outer),
                ApplyDeadZones(input.y, inner, outer));
            // Normalize it
            return result.magnitude > 1f ? result.normalized : result;
        }

        /// <summary>
        /// Applies dead zones on one axis.
        /// </summary>
        /// <param name="value">The axis value</param>
        /// <param name="inner">Defines the inner dead zone</param>
        /// <param name="outer">Defines the outer dead zone</param>
        private static float ApplyDeadZones(float value, float inner, float outer)
        {
            // Use absolute for calculation, then map back to actual sign
            float abs = Mathf.Abs(value);
            // Returns t [0,1] which defines the interpolating value for c, between a and b
            return Mathf.InverseLerp(inner, outer, abs) * Mathf.Sign(value);
        }
    }
}
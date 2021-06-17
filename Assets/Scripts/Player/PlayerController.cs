using Physics;
using UnityEngine;
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
        /// Whether the player is currently grounded
        /// </summary>
        [Header("Debugging")] [SerializeField] [Tooltip("Whether the player is currently grounded")]
        private bool isGrounded;

        /// <summary>
        /// The current camera for the player. Used to determine the mouse position.
        /// </summary>
        public Camera CurrentCamera { get; set; }

        /// <summary>
        /// Prevents the player from picking up objects in certain situations (e.g. going to another puzzle).
        /// </summary>
        public bool CanPickUpObjects { get; set; } = true;

        /// <summary>
        /// The <see cref="PhysicsBody2D"/> component of the player.
        /// </summary>
        private PhysicsBody2D _pb;

        private void Awake()
        {
            _pb = GetComponent<PhysicsBody2D>();
        }

        /// <summary>
        /// Player input using the old input system.
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                changeGravityAbility.SetGravity(GravityDirection.Left);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                changeGravityAbility.SetGravity(GravityDirection.Right);
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                changeGravityAbility.SetGravity(GravityDirection.Up);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                changeGravityAbility.SetGravity(GravityDirection.Down);
            }

            if (Input.GetKeyDown(KeyCode.E) && CanPickUpObjects)
            {
                if (throwAbility.IsHolding)
                {
                    throwAbility.Drop();
                }
                else
                {
                    throwAbility.PickUp();
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (throwAbility.IsHolding)
                {
                    throwAbility.Throw();
                }
            }
        }

        /// <summary>
        /// Jumping and moving.
        /// </summary>
        private void FixedUpdate()
        {
            // Check if the player wants to jump and can jump
            if (isGrounded && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) ||
                               Input.GetKeyDown(KeyCode.Joystick1Button0)))
            {
                isGrounded = false;
                _pb.ApplyForce(transform.rotation *
                               new Vector2(0, jumpForce * (throwAbility.IsHolding ? holdingSlownessFactor : 1.0f)));
            }

            AddInput();
        }

        /// <summary>
        /// Checks if the player can jump again (isGrounded check).
        /// </summary>
        /// <param name="other"></param>
        private void OnCollisionEnter2D(Collision2D other)
        {
            // Collision Normals to determine if player is grounded
            if (Vector2.Angle(other.contacts[0].normal, transform.rotation * Vector2.up) < groundAngle)
                isGrounded = true;
        }

        /// <summary>
        /// Drops the currently held object.
        /// </summary>
        public void ForceDropPickedUpObject()
        {
            if (throwAbility.IsHolding)
                throwAbility.Drop();
        }

        /// <summary>
        /// Adds the continuous input using the old input system.
        /// </summary>
        private void AddInput()
        {
            // Get raw input for applying own dead zones
            float inputX = Input.GetAxisRaw("Horizontal");

            // Apply custom dead zones
            Vector2 input = ApplyDeadZones(new Vector2(inputX, 0), innerDeadZone, outerDeadZone);

            // Transform to correct player space
            input = transform.rotation * input;

            // Apply to rigidbody
            if (useForceBasedMovement)
                _pb.ApplyForce(input * (forceMagnitude * (throwAbility.IsHolding ? holdingSlownessFactor : 1.0f)));
            else
                _pb.SetVelocity(input * velocityMagnitude);

            // Get mouse for aiming
            Vector2 mousePosition = CurrentCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 lookDirection = (mousePosition - (Vector2) transform.position);

            // Turn player if aim is on other side.
            Vector3 localScale = transform.localScale;
            localScale.x = Vector2.SignedAngle(Vector2.up, lookDirection) > 0 ? -1 : 1;
            transform.localScale = localScale;

            // Set the direction for throwing
            throwAbility.SetThrowDirection(lookDirection.normalized);
        }

        /// <summary>
        /// Applies dead zones on an input-vector. Uses <see cref="Apply(float,float,float)"/> on each axis.
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
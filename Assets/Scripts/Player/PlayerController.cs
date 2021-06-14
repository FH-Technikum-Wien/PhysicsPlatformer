using Physics;
using UnityEngine;
using World;

namespace Player
{
    /// <summary>
    /// Linear drag of "20" is pretty good
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")] [SerializeField] [Tooltip("Switches between force and velocity based movement")]
        private bool useForceBasedMovement = true;

        [SerializeField] [Tooltip("Magnitude of movement when using force")]
        private float forceMagnitude = 50;

        [SerializeField] [Tooltip("Magnitude of movement when using velocity")]
        private float velocityMagnitude = 20;

        [Header("Jumping")] [SerializeField] [Tooltip("The force with which the player jumps")]
        private float jumpForce = 15.0f;

        [SerializeField] [Range(0, 90)] [Tooltip("The angle which determines what counts as ground for jumping")]
        private float groundAngle = 30.0f;

        [Header("Controller")] [SerializeField] [Tooltip("Inner dead zone of the controller")]
        private float innerDeadZone = 0.1f;

        [SerializeField] [Tooltip("Outer dead zone of the controller")]
        private float outerDeadZone = 0.9f;

        [Header("Abilities")] [SerializeField] [Tooltip("The ability to throw objects")]
        private ThrowAbility throwAbility;

        [SerializeField] private float holdingSlownessFactor = 0.5f;

        [SerializeField] [Tooltip("The ability to turn the world around!")]
        private ChangeGravityAbility changeGravityAbility;


        [Header("Debugging")] [SerializeField] [Tooltip("Whether the player is currently grounded")]
        private bool isGrounded;

        private PhysicsBody2D _pb;
        private Vector2 _velocityChange;

        private Vector2 _levelStartPosition;
        private CheckPoint _lastCheckPoint;

        private void Awake()
        {
            _pb = GetComponent<PhysicsBody2D>();
            // Save start position for respawning with no active checkpoint
            _levelStartPosition = transform.position;
        }

        private void Update()
        {
            if (isGrounded && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) ||
                               Input.GetKeyDown(KeyCode.Joystick1Button0)))
            {
                isGrounded = false;
                _pb.ApplyForce(transform.rotation *
                               new Vector2(0, jumpForce * (throwAbility.IsHolding ? holdingSlownessFactor : 1.0f)));
            }

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

            if (Input.GetKeyDown(KeyCode.E))
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

        private void FixedUpdate()
        {
            AddInput();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            // Collision Normals to determine if player is grounded
            if (Vector2.Angle(other.contacts[0].normal, transform.rotation * Vector2.up) < groundAngle)
                isGrounded = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out CheckPoint checkPoint))
                return;

            if (_lastCheckPoint == null || _lastCheckPoint.Index < checkPoint.Index)
                _lastCheckPoint = checkPoint;
        }

        public void Kill()
        {
            changeGravityAbility.SetGravity(GravityDirection.Down);
            transform.position = _lastCheckPoint != null
                ? _lastCheckPoint.transform.position
                : (Vector3) _levelStartPosition;
        }

        public void AddMass(float massToAdd)
        {
            _pb.AddMass(massToAdd);
        }

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

            // Get mouse for looking around
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 lookDirection = (mousePosition - (Vector2) transform.position);

            if (Vector2.SignedAngle(Vector2.up, lookDirection) > 0)
                transform.localScale = new Vector3(-1, 1.5f, 1);
            else
                transform.localScale = new Vector3(1, 1.5f, 1);

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
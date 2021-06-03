using UnityEngine;

namespace Physics
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PhysicsBody2D : MonoBehaviour
    {
        [SerializeField] [Tooltip("The mass of this physics body")]
        private float mass = 1.0f;

        [SerializeField] [Tooltip("Factor applied to the global gravity")]
        private float gravityScale = 1.0f;


        [Header("Drag")]
        [SerializeField]
        [Tooltip("A factor applied to the drag on each axis. " +
                 "Can be used to customise the drag for each axis (not realistic)")]
        private Vector2 dragAxisFactor = new Vector2(1.0f, 1.0f);

        [SerializeField] [Tooltip("The drag coefficient of the object/shape")]
        private float dragCoefficient = 1f;

        [SerializeField] [Tooltip("The density of the material the physics body is currently in")]
        private float materialDensity = 1.225f;

        [SerializeField] [Tooltip("The area affected by the material density creating the drag")]
        private float frontalArea = 1;

        [SerializeField] [Tooltip("Use quickDrag instead of correct drag [0-1]")]
        private bool useQuickDrag;

        [SerializeField] [Tooltip("QuickDrag amount. Higher than 1 reverts, lower than 0 accelerates")] [Range(-1f, 2f)]
        private float quickDrag = 0.05f;

        [Header("Bounciness")]
        [SerializeField]
        [Tooltip("The bounciness of the physics body. < 0 -> 'Phasing' | 0-1 -> Bounce | > 1 -> Explosion")]
        private float bounciness = 1.0f;

        [Header("Debugging")] [SerializeField] public bool showDebugValues = false;

        [HideInInspector] [Tooltip("The current velocity")]
        public Vector2 currentVelocity;

        [HideInInspector] [Tooltip("Whether we reached terminal velocity on Y")]
        public bool terminalVelocityXReached;

        [HideInInspector] [Tooltip("Whether we reached terminal velocity on Y")]
        public bool terminalVelocityYReached;

        [HideInInspector] [Tooltip("")] public bool addVelocityToBody;

        [HideInInspector] [Tooltip("")] public Vector2 velocityToAdd;

        /// <summary>
        /// The gravity applied to all <see cref="PhysicsBody2D"/>s.
        /// </summary>
        public static Vector2 GlobalGravity = new Vector2(0.0f, 9.81f);

        public float Mass => mass;
        public Vector2 Velocity => _previousVelocity;

        private Rigidbody2D _rb;
        private Vector2 _previousVelocity;
        private float _oldQuickDrag;

        private Vector2 _baseVelocity = Vector2.zero;

        private readonly float _defaultMaterialDensity =
            MaterialDensity.GetMaterialDensity(MaterialDensity.MaterialType.Air);

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            // Disable all functionality by resetting values
            _rb.mass = mass;
            _rb.drag = 0;
            _rb.angularDrag = 0;
            _rb.gravityScale = 0;
        }

        private void FixedUpdate()
        {
            // Debug values
            if (addVelocityToBody)
            {
                addVelocityToBody = false;
                _rb.velocity = velocityToAdd;
            }

            currentVelocity = _rb.velocity;
            terminalVelocityXReached = Mathf.Approximately(Mathf.Abs(_previousVelocity.x), Mathf.Abs(_rb.velocity.x));
            terminalVelocityYReached = Mathf.Approximately(Mathf.Abs(_previousVelocity.y), Mathf.Abs(_rb.velocity.y));

            if (useQuickDrag)
            {
                ApplyGravity();
                ApplyQuickDrag();
            }
            else
            {
                ApplyDrag();
                ApplyGravity();
            }

            _previousVelocity = _rb.velocity;
        }

        /// <summary>
        /// Coefficient of restitution (bounciness)
        /// </summary>
        /// <param name="other"></param>
        private void OnCollisionEnter2D(Collision2D other)
        {
            Vector2 velocity = _previousVelocity;
            if (other.collider.TryGetComponent(out PhysicsBody2D body))
            {
                velocity = (mass * velocity + body.Mass * body.Velocity +
                            body.Mass * bounciness * (body.Velocity - velocity)) / (mass + body.Mass);
            }
            else
            {
                // Simplified version for collisions with non-rigidbodies
                velocity = bounciness * -velocity / mass;
            }

            _rb.velocity = velocity;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Change drag
            if (!other.TryGetComponent(out DragChanger dragChanger))
                return;
            _oldQuickDrag = quickDrag;
            quickDrag = dragChanger.quickDrag;
            materialDensity = dragChanger.materialDensity;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Revert drag
            if (!other.TryGetComponent(out DragChanger dragChanger))
                return;
            // Revert drag
            materialDensity = _defaultMaterialDensity;
            quickDrag = _oldQuickDrag;
        }

        public void ApplyForce(Vector2 force)
        {
            _rb.AddForce(force);
        }

        /// <summary>
        /// Sets the velocity of the physics body directly, ignoring it's mass.
        /// </summary>
        /// <param name="newVelocity">The new velocity of the physics body.</param>
        public void SetVelocity(Vector2 newVelocity)
        {
            _rb.velocity = newVelocity;
        }

        /// <summary>
        /// Applies drag to the current velocity of the physics body.
        /// The drag factor is in-between 0 and 1.
        /// <para>
        /// This function also includes moving with other objects, by lerping to <see cref="_baseVelocity"/>.
        /// </para>
        /// For checking: https://www.calctool.org/CALC/eng/aerospace/terminal
        /// </summary>
        private void ApplyDrag()
        {
            Vector2 velocity = _rb.velocity;

            // Treat x and y separately
            float xDrag = dragAxisFactor.x * dragCoefficient * materialDensity * frontalArea
                * velocity.x * velocity.x / 2 * mass;
            float yDrag = dragAxisFactor.y * dragCoefficient * materialDensity * frontalArea
                * velocity.y * velocity.y / 2 * mass;

            float xAbs = Mathf.Abs(velocity.x);
            float yAbs = Mathf.Abs(velocity.y);
            // Drag factor goes from 0 to 1
            xDrag = xAbs > 0.001f ? xDrag / xAbs : 1.0f;
            yDrag = yAbs > 0.001f ? yDrag / yAbs : 1.0f;

            // Lerp between current velocity and the base velocity depending on the drag factor
            float horizontalVelocity = Mathf.Lerp(velocity.x, _baseVelocity.x, xDrag * Time.fixedDeltaTime);
            float verticalVelocity = Mathf.Lerp(velocity.y, _baseVelocity.y, yDrag * Time.fixedDeltaTime);

            velocity = new Vector2(horizontalVelocity, verticalVelocity);
            _rb.velocity = velocity;
        }

        private void ApplyQuickDrag()
        {
            _rb.velocity *= (1 - quickDrag);
        }

        private void ApplyGravity()
        {
            _rb.velocity -= GlobalGravity * (gravityScale * Time.fixedDeltaTime);
        }
    }
}
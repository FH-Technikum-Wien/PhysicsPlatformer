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

        [Header("Drag")] [SerializeField] [Tooltip("Use quickDrag instead of correct drag [0-1]")]
        private bool useQuickDrag;

        [SerializeField] [Tooltip("QuickDrag amount. Higher than 1 reverts, lower than 0 accelerates")] [Range(-1f, 2f)]
        private float quickDrag = 0.05f;

        [SerializeField] private float dragCoefficient = 1f;
        [SerializeField] private float materialDensity = 1.225f;
        [SerializeField] private float frontalArea = 1;

        [Header("Debugging")] [SerializeField] public bool showDebugValues = false;

        [HideInInspector] [Tooltip("The current velocity")]
        public Vector2 currentVelocity;

        [HideInInspector] [Tooltip("Whether we reached terminal velocity on Y")]
        public bool terminalVelocityXReached;

        [HideInInspector] [Tooltip("Whether we reached terminal velocity on Y")]
        public bool terminalVelocityYReached;

        /// <summary>
        /// The gravity applied to all <see cref="PhysicsBody2D"/>s.
        /// </summary>
        public static Vector2 GlobalGravity = new Vector2(0.0f, 9.81f);

        private Rigidbody2D _rb;
        private Vector2 _previousVelocity;
        private float _oldQuickDrag;

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

            // Debug values
            currentVelocity = _rb.velocity;
            terminalVelocityXReached = Mathf.Approximately(Mathf.Abs(_previousVelocity.x), Mathf.Abs(_rb.velocity.x));
            terminalVelocityYReached = Mathf.Approximately(Mathf.Abs(_previousVelocity.y), Mathf.Abs(_rb.velocity.y));
            _previousVelocity = _rb.velocity;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out DragChanger dragChanger))
                return;
            _oldQuickDrag = quickDrag;
            quickDrag = dragChanger.quickDrag;
            materialDensity = dragChanger.materialDensity;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.TryGetComponent(out DragChanger dragChanger))
                return;
            // Revert drag
            materialDensity = _defaultMaterialDensity;
            quickDrag = _oldQuickDrag;
        }

        private void ApplyDrag()
        {
            Vector2 velocity = _rb.velocity;

            // Treat x and y separately
            float verticalDrag = 0.5f * dragCoefficient * materialDensity * frontalArea
                                 * velocity.y * velocity.y * -Mathf.Sign(velocity.y);
            float horizontalDrag = 0.5f * dragCoefficient * materialDensity * frontalArea
                                   * velocity.x * velocity.x * -Mathf.Sign(velocity.x);

            velocity -= new Vector2(-horizontalDrag, -verticalDrag) * Time.fixedDeltaTime;
            _rb.velocity = velocity;
        }

        private void ApplyQuickDrag()
        {
            _rb.velocity *= (1 - quickDrag);
        }

        private void ApplyGravity()
        {
            _rb.velocity -= GlobalGravity * (gravityScale * mass * Time.fixedDeltaTime);
        }
    }
}
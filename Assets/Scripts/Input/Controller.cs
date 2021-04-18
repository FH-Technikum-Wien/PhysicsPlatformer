using Physics;
using UnityEngine;

namespace Input
{
    /// <summary>
    /// Linear drag of "20" is pretty good
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Controller : MonoBehaviour
    {
        [Header("Movement")] [SerializeField] [Tooltip("Switches between force and velocity based movement")]
        private bool useForceBasedMovement = true;

        [SerializeField] [Tooltip("Magnitude of movement when using force")]
        private float forceMagnitude = 200;

        [SerializeField] [Tooltip("Magnitude of movement when using velocity")]
        private float velocityMagnitude = 20;

        [Header("Jumping")] [SerializeField] [Tooltip("The force with which the player jumps")]
        private float jumpForce = 10.0f;

        [Header("Gravity")] [SerializeField] [Tooltip("The amount of gravity applied to the player")]
        private Vector2 gravity = new Vector2(0.0f, -9.81f);

        [Header(("Drag"))] [SerializeField] [Tooltip("Use quickDrag instead of correct drag [0-1]")]
        private bool useQuickDrag;

        [SerializeField] [Tooltip("QuickDrag amount. Higher than 1 reverts, lower than 0 accelerates")] [Range(-1f, 2f)]
        private float quickDrag = 0.05f;

        [SerializeField] [Tooltip("The amount of drag that the player has")]
        private Vector2 drag = new Vector2(20, 0);

        [Header("Terminal Velocity")] [SerializeField] [Tooltip("Whether we reached terminal velocity on Y")]
        private bool terminalVelocityX;

        [SerializeField] [Tooltip("Whether we reached terminal velocity on Y")]
        private bool terminalVelocityY;

        [Header("Controller")] [SerializeField] [Tooltip("Inner dead zone of the controller")]
        private float innerDeadZone = 0.1f;

        [SerializeField] [Tooltip("Outer dead zone of the controller")]
        private float outerDeadZone = 0.9f;

        private Rigidbody2D _rb;
        private Vector2 _velocityChange;
        private Vector2 _oldDrag;
        private float _oldQuickDrag;

        private Vector2 _prevVelocity;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _oldDrag = drag;
            _oldQuickDrag = quickDrag;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space) && Mathf.Abs(_rb.velocity.y) < 0.001f)
            {
                _rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            }
        }

        private void FixedUpdate()
        {
            AddInput();
            AddGravity();

            ApplyVelocityChange();
        }

        private void AddInput()
        {
            // Get raw input for applying own dead zones
            float inputX = UnityEngine.Input.GetAxisRaw("Horizontal");
            //float inputY = UnityEngine.Input.GetAxisRaw("Vertical");
            float inputY = 0.0f;
            // Apply custom dead zones
            Vector2 input = DeadZones.Apply(new Vector2(inputX, inputY), innerDeadZone, outerDeadZone);
            // Apply to rigidbody
            if (useForceBasedMovement)
                _rb.AddForce(input * forceMagnitude);
            else
                _velocityChange = input * velocityMagnitude;
        }

        private void AddGravity()
        {
            _velocityChange += gravity;
        }

        private void ApplyVelocityChange()
        {
            Vector2 velocity = _rb.velocity;

            if (useQuickDrag)
            {
                velocity += _velocityChange * Time.fixedDeltaTime;
                velocity *= (1 - quickDrag);
            }
            else
            {
                velocity += new Vector2(-drag.x * velocity.x + _velocityChange.x,
                    -drag.y * velocity.y + _velocityChange.y) * Time.fixedDeltaTime;
            }

            _rb.velocity = velocity;
            _velocityChange = Vector2.zero;

            terminalVelocityX = HasReachedTerminalVelocityX;
            terminalVelocityY = HasReachedTerminalVelocityY;

            _prevVelocity = velocity;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out DragChanger dragChanger))
                return;
            _oldDrag = drag;
            _oldQuickDrag = quickDrag;
            // Get drag from current material/liquid/air/...
            drag = dragChanger.drag;
            quickDrag = dragChanger.quickDrag;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.TryGetComponent(out DragChanger dragChanger))
                return;
            // Revert drag
            drag = _oldDrag;
            quickDrag = _oldQuickDrag;
        }

        private bool HasReachedTerminalVelocityX => Mathf.Approximately(_prevVelocity.x, _rb.velocity.x);
        private bool HasReachedTerminalVelocityY => Mathf.Approximately(_prevVelocity.y, _rb.velocity.y);
    }
}
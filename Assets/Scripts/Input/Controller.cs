using System;
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
        private float forceMagnitude = 50;

        [SerializeField] [Tooltip("Magnitude of movement when using velocity")]
        private float velocityMagnitude = 20;

        [Header("Jumping")] [SerializeField] [Tooltip("The force with which the player jumps")]
        private float jumpForce = 15.0f;

        [SerializeField] [Range(0, 90)] [Tooltip("The angle which determines what counts as ground for jumping")]
        private float groundAngle = 30.0f;

        [SerializeField] [Tooltip("Whether the player is currently grounded")]
        private bool isGrounded;

        [SerializeField] [Tooltip("QuickDrag amount. Higher than 1 reverts, lower than 0 accelerates")] [Range(-1f, 2f)]
        private float quickDrag = 0.05f;

        [SerializeField] [Tooltip("The amount of drag that the player has")]
        private Vector2 drag = new Vector2(5, 1);

        [Header("Controller")] [SerializeField] [Tooltip("Inner dead zone of the controller")]
        private float innerDeadZone = 0.1f;

        [SerializeField] [Tooltip("Outer dead zone of the controller")]
        private float outerDeadZone = 0.9f;

        private Rigidbody2D _rb;
        private Vector2 _velocityChange;
        private Vector2 _oldDrag;
        private float _oldQuickDrag;

        private bool _isGrounded;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _oldDrag = drag;
            _oldQuickDrag = quickDrag;
        }

        private void Update()
        {
            if (_isGrounded && (UnityEngine.Input.GetKeyDown(KeyCode.Space) || UnityEngine.Input.GetKeyDown(KeyCode.W)))
            {
                _isGrounded = false;
                _rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            }
        }

        private void FixedUpdate()
        {
            AddInput();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            // Collision Normals to determine if player is grounded
            if (Math.Acos(other.contacts[0].normal.y) < Mathf.Deg2Rad * groundAngle)
                _isGrounded = true;
        }

        private void AddInput()
        {
            // Get raw input for applying own dead zones
            float inputX = UnityEngine.Input.GetAxisRaw("Horizontal");

            // Apply custom dead zones
            Vector2 input = DeadZones.Apply(new Vector2(inputX, 0), innerDeadZone, outerDeadZone);

            // Apply to rigidbody
            if (useForceBasedMovement)
                _rb.AddForce(input * forceMagnitude);
            else
                _rb.velocity = input * velocityMagnitude;
        }
    }
}
using System;
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

        [Header("Debugging")] [SerializeField] [Tooltip("Whether the player is currently grounded")]
        private bool isGrounded;

        private PhysicsBody2D _pb;
        private Vector2 _velocityChange;

        private void Awake()
        {
            _pb = GetComponent<PhysicsBody2D>();
        }

        private void Update()
        {
            if (isGrounded && (UnityEngine.Input.GetKeyDown(KeyCode.Space) || UnityEngine.Input.GetKeyDown(KeyCode.W)))
            {
                isGrounded = false;
                _pb.ApplyForce(new Vector2(0, jumpForce));
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
                isGrounded = true;
        }

        private void AddInput()
        {
            // Get raw input for applying own dead zones
            float inputX = UnityEngine.Input.GetAxisRaw("Horizontal");

            // Apply custom dead zones
            Vector2 input = DeadZones.Apply(new Vector2(inputX, 0), innerDeadZone, outerDeadZone);

            // Apply to rigidbody
            if (useForceBasedMovement)
                _pb.ApplyForce(input * forceMagnitude);
            else
                _pb.SetVelocity(input * velocityMagnitude);
        }
    }
}
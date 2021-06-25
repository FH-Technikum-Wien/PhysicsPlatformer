using System.Collections.Generic;
using Physics;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(PhysicsBody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class MovingPlatform : MonoBehaviour
    {
        /// <summary>
        /// The frequency of th oscillation.
        /// </summary>
        [SerializeField] [Tooltip("The frequency of th oscillation.")]
        private float frequency = 1.0f;

        /// <summary>
        /// The amplitude of the sin/cos oscillation.
        /// </summary>
        [SerializeField] [Tooltip("The amplitude of the sin/cos oscillation.")]
        private float amplitude = 1.0f;

        /// <summary>
        /// Which axis the platform should move in.
        /// </summary>
        [SerializeField] [Tooltip("Which axis the platform should move in.")]
        private Vector2 moveDirection = Vector2.right;

        /// <summary>
        /// Use cos instead of sin. Simply applies pi/2 offset.
        /// </summary>
        [SerializeField] [Tooltip("Use cos instead of sin. Simply applies pi/2 offset.")]
        private bool useCos;

        /// <summary>
        /// Objects at this angle are considered to be standing on it and are moved with it.
        /// </summary>
        [SerializeField] [Tooltip("Objects at this angle are considered to be standing on it and are moved with it.")]
        private float movingWithAngle = 10.0f;

        /// <summary>
        /// The underlying <see cref="PhysicsBody2D"/>.
        /// </summary>
        private PhysicsBody2D _pb;

        /// <summary>
        /// Current progress-time used for the sin-function
        /// </summary>
        private float _time;

        /// <summary>
        /// Start position of the platform
        /// </summary>
        private Vector2 _startPosition;

        /// <summary>
        /// List of all bodies currently standing on the platform.
        /// </summary>
        private readonly List<PhysicsBody2D> _bodies = new List<PhysicsBody2D>();

        /// <summary>
        /// Internal velocity, used for Euler-Cromer.
        /// </summary>
        private Vector2 _velocity;

        private void Awake()
        {
            _pb = GetComponent<PhysicsBody2D>();
            _startPosition = transform.position;
        }

        private void FixedUpdate()
        {
            _time += Time.fixedDeltaTime;

            float progress = Mathf.Sin(_time * 2.0f * Mathf.PI * frequency + (useCos ? Mathf.PI / 2.0f : 0.0f));
            progress *= amplitude;

            // Convert progress to acceleration
            Vector2 distance = (_startPosition + moveDirection.normalized * progress) - (Vector2) transform.position;
            Vector2 newVelocity = distance / Time.fixedDeltaTime;
            Vector2 acceleration = (newVelocity - _velocity) / Time.fixedDeltaTime;

            // Euler-Cromer Method
            _velocity += acceleration * Time.fixedDeltaTime;
            _pb.SetPosition((Vector2) _pb.transform.position + _velocity * Time.fixedDeltaTime);

            // Apply current velocity as base velocity and acceleration change to all bodies
            foreach (PhysicsBody2D body in _bodies)
            {
                body.SetBaseVelocity(_velocity);
                body.AddVelocity(acceleration * Time.fixedDeltaTime);
            }
        }

        /// <summary>
        /// Add colliding bodies to list of objects to move with the platform
        /// </summary>
        private void OnCollisionEnter2D(Collision2D other)
        {
            // Only add if its on top
            if (other.collider.TryGetComponent(out PhysicsBody2D body) &&
                Vector2.Angle(-other.contacts[0].normal, Vector2.up) < movingWithAngle)
            {
                _bodies.Add(body);
                body.SetBaseVelocity(_pb.Velocity);
            }
        }

        /// <summary>
        /// Remove body from list of objects to move with the platform
        /// </summary>
        /// <param name="other"></param>
        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.collider.TryGetComponent(out PhysicsBody2D body))
            {
                _bodies.Remove(body);
                body.ResetBaseVelocity();
            }
        }
    }
}
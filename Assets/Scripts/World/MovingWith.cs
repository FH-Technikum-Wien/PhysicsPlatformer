using System.Collections.Generic;
using Physics;
using UnityEngine;

namespace World
{
    public class MovingWith : MonoBehaviour
    {
        /// <summary>
        /// Objects at this angle are considered to be standing on it and are moved with it.
        /// </summary>
        [SerializeField] [Tooltip("Objects at this angle are considered to be standing on it and are moved with it.")]
        private float movingWithAngle = 10.0f;

        /// <summary>
        /// List of all bodies currently standing on the platform.
        /// </summary>
        private readonly List<PhysicsBody2D> _bodies = new List<PhysicsBody2D>();

        /// <summary>
        /// The last set velocity. Used for applying to objects that move with this object.
        /// </summary>
        private Vector2 _lastVelocity;

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
                body.SetBaseVelocity(_lastVelocity);
            }
        }

        /// <summary>
        /// Remove body from list of objects to move with the platform
        /// </summary>
        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.collider.TryGetComponent(out PhysicsBody2D body))
            {
                _bodies.Remove(body);
                body.ResetBaseVelocity();
            }
        }
        
        /// <summary>
        /// Applies the given velocity and acceleration change to all objects currently moving with it.
        /// </summary>
        /// <param name="velocity">Current velocity of this object</param>
        /// <param name="acceleration">Current acceleration of this object</param>
        public void ApplyAccelerationAndVelocity(Vector2 velocity, Vector2 acceleration)
        {
            _lastVelocity = velocity;
            // Apply current velocity as base velocity and acceleration change to all bodies
            foreach (PhysicsBody2D body in _bodies)
            {
                body.SetBaseVelocity(velocity);
                body.AddVelocity(acceleration * Time.fixedDeltaTime);
            }
        }
    }
}

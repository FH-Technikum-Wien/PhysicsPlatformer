using System;
using UnityEngine;
using World;

namespace Physics
{
    [RequireComponent(typeof(PhysicsBody2D))]
    public class Pendulum : MonoBehaviour
    {
        /// <summary>
        /// The pivot point is where the pendulum is attached.
        /// </summary>
        [SerializeField] [Tooltip("The pivot point is where the pendulum is attached")]
        private Transform pivot;

        /// <summary>
        /// The length of the pendulum.
        /// </summary>
        [SerializeField] [Tooltip("The length of the pendulum")]
        private float length;
        
        /// <summary>
        /// The drag of the pendulum that slows it down.
        /// </summary>
        [SerializeField] [Tooltip("The drag of the pendulum that slows it down")]
        private float drag = 0.2f;

        /// <summary>
        /// The line renderer component for drawing the connection to the pivot.
        /// </summary>
        [SerializeField] [Tooltip("The line renderer component for drawing the connection to the pivot")]
        private LineRenderer lineRenderer;
        
        /// <summary>
        /// Component for moving objects with the pendulum.
        /// </summary>
        [SerializeField] [Tooltip("Component for moving objects with the pendulum")]
        private MovingWith movingWith;

        /// <summary>
        /// The underlying PhysicsBody2D.
        /// </summary>
        private PhysicsBody2D _pb;

        /// <summary>
        /// The current difference to the origin/pivot.
        /// </summary>
        private Vector2 _displacementVector;
        
        /// <summary>
        /// The length of the pendulum.
        /// </summary>
        private float _length;

        /// <summary>
        /// The current velocity of the pendulum. Used for the Euler-Cromer method.
        /// </summary>
        private Vector2 _velocity;

        private void Awake()
        {
            _pb = GetComponent<PhysicsBody2D>();
            _displacementVector = pivot.position - transform.position;
            // Either use given length or magnitude of the displacement to the pivot
            _length = length != 0 ? length : _displacementVector.magnitude;

            lineRenderer.widthMultiplier = 0.1f;
        }

        private void FixedUpdate()
        {
            // Render a line from the platform to the pivot to imitate a connection with it.
            VisualizeConnectionToPivot();

            // Get displacement to pivot
            _displacementVector = pivot.position - transform.position;
            Vector2 tensionDirection = _displacementVector.normalized;

            // Gravity in the direction of the string (opposing tension)
            float gravityMagnitudeOpposingTension = Vector2.Dot(-tensionDirection,
                WorldManager.GravityDirectionToVector[WorldManager.GravityDirection]);

            float dotV = Vector2.Dot(_velocity.normalized, Vector3.Cross(tensionDirection, Vector3.back));

            float perpendicularVelocity = _velocity.magnitude * dotV;

            Vector2 tensionFromWeight = tensionDirection * (PhysicsBody2D.GlobalGravityAcceleration * gravityMagnitudeOpposingTension);
            Vector2 tensionFromCentrifuge = perpendicularVelocity * perpendicularVelocity / _length * tensionDirection;
            
            // Apply force from centrifuge, weight and gravity to pendulum
            // Euler-Cromer Method
            Vector2 acceleration = tensionFromCentrifuge + tensionFromWeight - PhysicsBody2D.GlobalGravity;
            _velocity += acceleration * Time.fixedDeltaTime;
            
            // Apply drag
            _velocity = Vector2.Lerp(_velocity, Vector2.zero, drag * Time.fixedDeltaTime);
            
            // Move pendulum
            _pb.SetPosition((Vector2) _pb.transform.position + _velocity * Time.fixedDeltaTime);
            
            // Apply movement to all objects that should move with the pendulum
            movingWith.ApplyAccelerationAndVelocity(_velocity, acceleration);
        }

        /// <summary>
        /// Resets the pendulum by resetting its velocity
        /// </summary>
        public void ResetPendulum()
        {
            _velocity = Vector2.zero;
        }

        /// <summary>
        /// Renders a line between platform and pivot.
        /// </summary>
        private void VisualizeConnectionToPivot()
        {
            lineRenderer.positionCount = 0;
            lineRenderer.SetPosition(lineRenderer.positionCount++, transform.position);
            lineRenderer.SetPosition(lineRenderer.positionCount++, pivot.position);
        }
    }
}
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
        /// The line renderer component for drawing the connection to the pivot.
        /// </summary>
        [SerializeField] [Tooltip("The line renderer component for drawing the connection to the pivot")]
        private LineRenderer lineRenderer;
        
        /// <summary>
        /// Component for moving objects with the pendulum.
        /// </summary>
        [SerializeField] [Tooltip("Component for moving objects with the pendulum")]
        private MovingWith movingWith;

        private PhysicsBody2D _pb;

        private Vector2 _displacementVector;
        private float _length;

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
            VisualizeConnectionToPivot();

            // Apply gravity
            _velocity -= PhysicsBody2D.GlobalGravity * Time.fixedDeltaTime;

            _displacementVector = pivot.position - transform.position;
            Vector2 tensionDirection = _displacementVector.normalized;

            // Gravity in the direction of the string (opposing tension)
            float gravityMagnitudeOpposingTension = Vector2.Dot(-tensionDirection,
                WorldManager.GravityDirectionToVector[WorldManager.GravityDirection]);

            float dotV = Vector2.Dot(_velocity.normalized, Vector3.Cross(tensionDirection, Vector3.back));

            float perpendicularVelocity = _velocity.magnitude * dotV;

            Vector2 tensionFromWeight = tensionDirection * (PhysicsBody2D.GlobalGravityAcceleration * gravityMagnitudeOpposingTension);
            Vector2 tensionFromCentrifuge = perpendicularVelocity * perpendicularVelocity / _length * tensionDirection;

            Vector2 acceleration = tensionFromCentrifuge + tensionFromWeight;
            _velocity += acceleration * Time.fixedDeltaTime;
            
            // Apply drag
            _velocity = Vector2.Lerp(_velocity, Vector2.zero, 0.2f * Time.fixedDeltaTime);
            
            // Move pendulum
            _pb.SetPosition((Vector2) _pb.transform.position + _velocity * Time.fixedDeltaTime);
            
            
            
            movingWith.ApplyAccelerationAndVelocity(_velocity, acceleration);
        }

        private void VisualizeConnectionToPivot()
        {
            lineRenderer.positionCount = 0;
            lineRenderer.SetPosition(lineRenderer.positionCount++, transform.position);
            lineRenderer.SetPosition(lineRenderer.positionCount++, pivot.position);
        }
    }
}
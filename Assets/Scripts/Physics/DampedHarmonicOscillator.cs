using System;
using UnityEngine;
using Util;

namespace Physics
{
    [RequireComponent(typeof(PhysicsBody2D))]
    public class DampedHarmonicOscillator : MonoBehaviour
    {
        /// <summary>
        /// Aka. drag for the spring.
        /// </summary>
        [SerializeField] [Tooltip("Aka. drag for the spring")]
        private float gamma;

        /// <summary>
        /// Spring constant.
        /// </summary>
        [SerializeField] [Tooltip("Spring constant")]
        private float omega;

        /// <summary>
        /// Sqrt(4 * omega * omega) -> Defines whether its over-, under- or critical damping.
        /// </summary>
        [SerializeField]
        [ReadOnly]
        [Tooltip("Sqrt(4 * omega * omega) -> Defines whether its over-, under- or critical damping")]
        private float sqrt4TimesOmegaSquared;

        /// <summary>
        /// Alternative origin the oscillator returns to.
        /// </summary>
        [SerializeField] [Tooltip("Alternative origin the oscillator returns to")]
        private Vector3 alternativeOrigin;

        /// <summary>
        /// The underlying PhysicsBody2D component.
        /// </summary>
        private PhysicsBody2D _pb;

        /// <summary>
        /// The origin the oscillator returns to.
        /// </summary>
        private Vector3 _origin;

        /// <summary>
        /// Update Sqrt(4 * omega * omega) in Inspector
        /// </summary>
        private void OnValidate()
        {
            sqrt4TimesOmegaSquared = Mathf.Sqrt(4 * omega * omega);
        }


        private void Awake()
        {
            _pb = GetComponent<PhysicsBody2D>();
            // If alternativeOrigin is defined, use it, otherwise use current
            _origin = alternativeOrigin == Vector3.zero ? transform.position : alternativeOrigin;
        }

        // Move oscillator back to origin
        private void FixedUpdate()
        {
            sqrt4TimesOmegaSquared = Mathf.Sqrt(4 * omega * omega);
            float b = gamma * _pb.mass;
            float k = omega * omega * _pb.mass;
            Vector2 difference = _origin - transform.position;

            _pb.ApplyForce(difference * k - b * _pb.CachedVelocity);
        }
    }
}
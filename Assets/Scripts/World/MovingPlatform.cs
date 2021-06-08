using System.Collections.Generic;
using Physics;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(PhysicsBody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] [Tooltip("")] private float frequency = 1.0f;
        [SerializeField] [Tooltip("")] private float amplitude = 1.0f;
        [SerializeField] [Tooltip("")] private Vector2 moveDirection = Vector2.right;
        [SerializeField] [Tooltip("")] private bool useCos;

        private PhysicsBody2D _pb;
        private float _time;
        private Vector2 _startPosition;

        private readonly List<PhysicsBody2D> _bodies = new List<PhysicsBody2D>();


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

            // Convert progress to velocity
            Vector2 distance = (_startPosition + moveDirection.normalized * progress) - (Vector2) transform.position;
            Vector2 newVelocity = distance / Time.fixedDeltaTime;
            Vector2 acceleration = (newVelocity - _pb.Velocity) / Time.fixedDeltaTime;
            _pb.AddVelocity(acceleration * Time.fixedDeltaTime);

            foreach (PhysicsBody2D body in _bodies)
            {
                body.SetBaseVelocity(_pb.Velocity);
                body.AddVelocity(acceleration * Time.fixedDeltaTime);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.TryGetComponent(out PhysicsBody2D body))
            {
                _bodies.Add(body);
                body.SetBaseVelocity(_pb.Velocity);
            }
        }

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
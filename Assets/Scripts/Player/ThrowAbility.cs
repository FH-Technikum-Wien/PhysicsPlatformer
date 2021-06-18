using Physics;
using UnityEngine;
using World;

namespace Player
{
    [RequireComponent(typeof(Collider2D))]
    public class ThrowAbility : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float predictionLength;

        [SerializeField] private Transform throwOrigin;
        [SerializeField] private Vector2 throwDirection;
        [SerializeField] private float throwStrength = 5.0f;
        [SerializeField] private LayerMask mask;

        public bool IsHolding => _pickedUpBody != null;

        public float PickedUpBodyMass => _pickedUpBody.mass;

        private PhysicsBody2D _pickedUpBody;

        private float _throwFactor = 1.0f;

        private void Awake()
        {
            throwDirection = throwDirection.normalized;
            lineRenderer.widthMultiplier = 0.1f;
        }

        private void FixedUpdate()
        {
            lineRenderer.positionCount = 0;
            if (_pickedUpBody == null)
                return;

            VisualizeThrow();
            _pickedUpBody.transform.position = throwOrigin.position;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PhysicsBody2D body))
                return;

            // Enable collision
            body.Collider2D.isTrigger = false;
        }

        public void SetThrowFactor(float throwFactor)
        {
            _throwFactor = throwFactor;
        }

        public void SetThrowDirection(Vector2 directionNormalized)
        {
            throwDirection = directionNormalized;
        }

        public void PickUp()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 2.0f, mask);

            foreach (Collider2D currentCollider in colliders)
            {
                if (!currentCollider.TryGetComponent(out Pickupable pickupable))
                    continue;

                _pickedUpBody = pickupable.PhysicsBody2D;
                _pickedUpBody.gameObject.layer = gameObject.layer;
                _pickedUpBody.SetEnabled(false);
                return;
            }
        }

        public void Throw()
        {
            if (!IsHolding)
                return;

            // Disable collision
            _pickedUpBody.Collider2D.isTrigger = true;

            _pickedUpBody.SetEnabled(true);
            _pickedUpBody.SetVelocity(throwDirection * (throwStrength * _throwFactor));
            _pickedUpBody.gameObject.layer = 0;
            _pickedUpBody = null;
        }

        public void Drop()
        {
            // Disable collision
            _pickedUpBody.Collider2D.isTrigger = false;

            _pickedUpBody.SetEnabled(true);
            _pickedUpBody.gameObject.layer = 0;
            _pickedUpBody = null;
        }

        private void VisualizeThrow()
        {
            for (float i = 0; i < Time.fixedDeltaTime * predictionLength; i += Time.fixedDeltaTime)
            {
                // SUVAT
                Vector2 u = throwDirection * (throwStrength * _throwFactor);
                Vector2 a = -PhysicsBody2D.GlobalGravity * _pickedUpBody.gravityScale;
                Vector2 s = u * i + a * (0.5f * i * i);

                Vector2 position = (Vector2) (throwOrigin.position) + s;

                // Stop prediction if colliding with something
                Collider2D collider2D = Physics2D.OverlapCircle(position, 0.01f, mask);
                if (collider2D != null)
                    return;

                lineRenderer.SetPosition(lineRenderer.positionCount++, position);
            }
        }
    }
}
using Physics;
using UnityEngine;
using World;

namespace Player
{
    [RequireComponent(typeof(Collider2D))]
    public class ThrowAbility : MonoBehaviour
    {
        /// <summary>
        /// Used for rendering the prediction.
        /// </summary>
        [SerializeField] [Tooltip("Used for rendering the prediction")]
        private LineRenderer lineRenderer;

        /// <summary>
        /// The number of steps for the prediction. Defines the length (one step == Time.fixedDeltaTime).
        /// </summary>
        [SerializeField] private float predictionLength;

        /// <summary>
        /// Position that the object is thrown from.
        /// </summary>
        [SerializeField] [Tooltip("Position that the object is thrown from")]
        private Transform throwOrigin;

        /// <summary>
        /// The direction of the throw.
        /// </summary>
        [SerializeField] private Vector2 throwDirection;

        /// <summary>
        /// The strength of the throw. Defines the distance the object can be thrown.
        /// </summary>
        [SerializeField] [Tooltip("The strength of the throw. Defines the distance the object can be thrown")]
        private float throwStrength = 5.0f;

        /// <summary>
        /// Used for not-stopping the prediction on collision with specified layers. 
        /// </summary>
        [SerializeField] [Tooltip("Used for not-stopping the prediction on collision with specified layers")]
        private LayerMask mask;
        
        /// <summary>
        /// The radius for searching for pickupables and picking them up.
        /// </summary>
        [SerializeField] [Tooltip("The radius for searching for pickupables and picking them up.")]
        private float pickUpRadius = 2.0f;


        /// <summary>
        /// True if an object is held.
        /// </summary>
        public bool IsHolding => _pickedUpBody != null;

        /// <summary>
        /// The mass of the body currently held.
        /// </summary>
        public float PickedUpBodyMass { get; private set; }

        /// <summary>
        /// The PhysicsBody component of the picked up body.
        /// </summary>
        private PhysicsBody2D _pickedUpBody;

        /// <summary>
        /// Additional factor that decreases or increases the throw distance.
        /// </summary>
        private float _throwFactor = 1.0f;

        private void Awake()
        {
            throwDirection = throwDirection.normalized;
            lineRenderer.widthMultiplier = 0.1f;
        }

        private void FixedUpdate()
        {
            // Reset line renderer
            lineRenderer.positionCount = 0;
            
            // Only show prediction if something is held.
            if (_pickedUpBody == null)
                return;

            VisualizeThrow();
            // Keep picked up body at the throw position.
            _pickedUpBody.transform.position = throwOrigin.position;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PhysicsBody2D body))
                return;

            // Enable collision after its outside the players collider.
            body.Collider2D.isTrigger = false;
        }

        /// <summary>
        /// Sets the throw direction for the prediction and the actual throw.
        /// </summary>
        /// <param name="directionNormalized">Normalized direction vector.</param>
        public void SetThrowDirection(Vector2 directionNormalized)
        {
            throwDirection = directionNormalized;
        }

        /// <summary>
        /// Looks for a pickupable around the player and picks the first one up.
        /// </summary>
        public void PickUp()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickUpRadius, mask);

            foreach (Collider2D currentCollider in colliders)
            {
                if (!currentCollider.TryGetComponent(out Pickupable pickupable))
                    continue;

                _pickedUpBody = pickupable.PhysicsBody2D;
                _pickedUpBody.ResetCollisionFlags();
                PickedUpBodyMass = _pickedUpBody.mass;
                _pickedUpBody.gameObject.layer = gameObject.layer;
                _pickedUpBody.SetEnabled(false);
                return;
            }
        }

        /// <summary>
        /// Throws the currently held body.
        /// </summary>
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

        /// <summary>
        /// Drops the currently held body.
        /// </summary>
        public void Drop()
        {
            // Disable collision
            _pickedUpBody.Collider2D.isTrigger = false;

            _pickedUpBody.SetEnabled(true);
            _pickedUpBody.gameObject.layer = 0;
            _pickedUpBody = null;
        }

        /// <summary>
        /// Renders a prediction using SUVAT.
        /// </summary>
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
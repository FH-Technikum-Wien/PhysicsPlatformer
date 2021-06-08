using UnityEngine;
using Util;

namespace Physics
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PhysicsBody2D : MonoBehaviour
    {
        public enum Type { Dynamic, Kinematic }


        [SerializeField] [Tooltip("The type of the physics body. Kinematic will be treated with INF mass.")]
        private Type bodyType = Type.Dynamic;

        [HideInInspector] [Tooltip("The mass of this physics body")]
        public float mass = 1.0f;

        [HideInInspector] [Tooltip("Factor applied to the global gravity")]
        public float gravityScale = 1.0f;

        [HideInInspector]
        [Tooltip("A factor applied to the drag on each axis. " +
                 "Can be used to customise the drag for each axis (not realistic)")]
        public Vector2 dragAxisFactor = new Vector2(1.0f, 1.0f);

        [HideInInspector] [Tooltip("The drag coefficient of the object/shape")]
        public float dragCoefficient = 1f;

        [HideInInspector] [Tooltip("The density of the material the physics body is currently in")]
        public float materialDensity = 1.225f;

        [HideInInspector] [Tooltip("The area affected by the material density creating the drag")]
        public float frontalArea = 1;

        [HideInInspector] [Tooltip("Use quickDrag instead of correct drag [0-1]")]
        public bool useQuickDrag;

        [HideInInspector]
        [Tooltip("QuickDrag amount. Higher than 1 reverts, lower than 0 accelerates")]
        [Range(-1f, 2f)]
        public float quickDrag = 0.05f;

        [HideInInspector]
        [Tooltip("The bounciness of the physics body. < 0 -> 'Phasing' | 0-1 -> Bounce | > 1 -> Explosion")]
        public float bounciness = 1.0f;

        [HideInInspector] public bool showDebugValues = false;

        [HideInInspector] [Tooltip("The current velocity")]
        public Vector2 currentVelocity;

        [HideInInspector] [Tooltip("Whether we reached terminal velocity on Y")]
        public bool terminalVelocityXReached;

        [HideInInspector] [Tooltip("Whether we reached terminal velocity on Y")]
        public bool terminalVelocityYReached;

        [HideInInspector] [Tooltip("")] public bool addVelocityToBody;

        [HideInInspector] [Tooltip("")] public Vector2 velocityToAdd;

        /// <summary>
        /// The gravity applied to all <see cref="PhysicsBody2D"/>s.
        /// </summary>
        public static Vector2 GlobalGravity = new Vector2(0.0f, 9.81f);

        public Type BodyType => bodyType;
        public float Mass => mass;
        public Vector2 Velocity => _rb.velocity;
        public Vector2 CachedVelocity => _cachedVelocity;
        public Vector2 BaseVelocity => _baseVelocity;

        private Rigidbody2D _rb;
        private Vector2 _cachedVelocity;
        private float _oldQuickDrag;

        private Vector2 _baseVelocity = Vector2.zero;
        private bool _resetBaseVelocity;
        private float _resetBaseVelocityDelay = 0.5f;
        private float _resetBaseVelocityTime = 0.0f;

        private readonly float _defaultMaterialDensity =
            MaterialDensity.GetMaterialDensity(MaterialDensity.MaterialType.Air);

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();

            // Disable/Set rigidbody behaviour 
            _rb.sharedMaterial = new PhysicsMaterial2D("NoBounce") {bounciness = 0.0f, friction = 1.0f};

            if (bodyType == Type.Kinematic)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.useFullKinematicContacts = true;
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
            else
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
                _rb.mass = mass;
                _rb.drag = 0;
                _rb.angularDrag = 0;
                _rb.gravityScale = 0;
            }
        }

        private void FixedUpdate()
        {
            // Debug values
            if (addVelocityToBody)
            {
                addVelocityToBody = false;
                _rb.velocity = velocityToAdd;
            }

            currentVelocity = _rb.velocity;
            terminalVelocityXReached = Mathf.Approximately(Mathf.Abs(_cachedVelocity.x), Mathf.Abs(_rb.velocity.x));
            terminalVelocityYReached = Mathf.Approximately(Mathf.Abs(_cachedVelocity.y), Mathf.Abs(_rb.velocity.y));

            // Base velocity reset over time
            if (_resetBaseVelocity && _resetBaseVelocityTime + _resetBaseVelocityDelay <= Time.time)
            {
                _baseVelocity = Vector2.Lerp(_baseVelocity, Vector2.zero, 2.0f * Time.fixedDeltaTime);

                if (_baseVelocity.sqrMagnitude < 0.1f)
                {
                    _baseVelocity = Vector2.zero;
                    _resetBaseVelocity = false;
                }
            }

            // Only apply gravity and drag if dynamic
            if (bodyType == Type.Dynamic)
            {
                if (useQuickDrag)
                {
                    ApplyGravity();
                    ApplyQuickDrag();
                }
                else
                {
                    ApplyDrag();
                    ApplyGravity();
                }
            }

            _cachedVelocity = _rb.velocity;
        }

        /// <summary>
        /// Use Coefficient of restitution (bounciness) and reflection to determine new velocity
        /// </summary>
        private void OnCollisionEnter2D(Collision2D other)
        {
            // Skip if kinematic
            if (bodyType == Type.Kinematic)
                return;

            Vector2 collisionNormal = other.contacts[0].normal;

            // Simplified collision with non physics bodies or kinematic physic bodies
            if (!other.collider.TryGetComponent(out PhysicsBody2D body) || body.bodyType == Type.Kinematic)
            {
                CollisionWithStatic(collisionNormal);
            }
            else if (body.bodyType == Type.Dynamic)
            {
                CollisionWithDynamicPhysicsBody(body, collisionNormal);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Change drag
            if (!other.TryGetComponent(out DragChanger dragChanger))
                return;
            _oldQuickDrag = quickDrag;
            quickDrag = dragChanger.quickDrag;
            materialDensity = dragChanger.materialDensity;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Revert drag
            if (!other.TryGetComponent(out DragChanger dragChanger))
                return;
            // Revert drag
            materialDensity = _defaultMaterialDensity;
            quickDrag = _oldQuickDrag;
        }

        public void ApplyForce(Vector2 force)
        {
            _rb.AddForce(force);
        }

        public void AddVelocity(Vector2 velocity)
        {
            _rb.velocity += velocity;
        }

        /// <summary>
        /// Sets the velocity of the physics body directly, ignoring it's mass.
        /// </summary>
        /// <param name="newVelocity">The new velocity of the physics body.</param>
        public void SetVelocity(Vector2 newVelocity)
        {
            _rb.velocity = newVelocity;
        }

        /// <summary>
        /// Sets the base velocity of this physics body. Needed for moving platforms.
        /// </summary>
        /// <param name="newBaseVelocity">The new base velocity.</param>
        public void SetBaseVelocity(Vector2 newBaseVelocity)
        {
            _resetBaseVelocity = false;
            _baseVelocity = newBaseVelocity;
        }

        /// <summary>
        /// Resets the base velocity by lerping it back to zero
        /// </summary>
        public void ResetBaseVelocity()
        {
            if (_resetBaseVelocity)
                return;

            _resetBaseVelocityTime = Time.time;
            _resetBaseVelocity = true;
        }

        /// <summary>
        /// Applies drag to the current velocity of the physics body.
        /// The drag factor is in-between 0 and 1.
        /// <para>
        /// This function also includes moving with other objects, by lerping to <see cref="_baseVelocity"/>.
        /// </para>
        /// For checking: https://www.calctool.org/CALC/eng/aerospace/terminal
        /// </summary>
        private void ApplyDrag()
        {
            Vector2 velocity = _rb.velocity - _baseVelocity;

            // Treat x and y separately
            float xDrag = dragAxisFactor.x * dragCoefficient * materialDensity * frontalArea
                * velocity.x * velocity.x / (2 * mass);
            float yDrag = dragAxisFactor.y * dragCoefficient * materialDensity * frontalArea
                * velocity.y * velocity.y / (2 * mass);

            _rb.velocity += new Vector2(
                xDrag * -Mathf.Sign(velocity.x),
                yDrag * -Mathf.Sign(velocity.y)
            ) * Time.fixedDeltaTime;
        }

        private void ApplyQuickDrag()
        {
            _rb.velocity *= (1 - quickDrag);
        }

        private void ApplyGravity()
        {
            _rb.velocity -= GlobalGravity * (gravityScale * Time.fixedDeltaTime);
        }

        private void CollisionWithStatic(Vector2 collisionNormal)
        {
            Vector2 collisionNormalAbs = collisionNormal.Abs();
            _rb.velocity = bounciness * Vector2.Reflect(_cachedVelocity * collisionNormalAbs, collisionNormal) +
                           _cachedVelocity * (Vector2.one - collisionNormalAbs);
        }

        private void CollisionWithDynamicPhysicsBody(PhysicsBody2D body, Vector2 collisionNormal)
        {
            float mixedBounciness = (bounciness + body.bounciness) / 2.0f;

            // Calculate velocity after collision with another physicsBody2D
            Vector2 newVelocity =
                (mass * _cachedVelocity + body.Mass * body.CachedVelocity +
                 body.Mass * mixedBounciness * (body.CachedVelocity - _cachedVelocity)) / (mass + body.Mass);

            float magnitude =
                (mass * _cachedVelocity.magnitude + body.Mass * body.CachedVelocity.magnitude + body.Mass *
                    bounciness * (body.CachedVelocity.magnitude - _cachedVelocity.magnitude)) / (mass + body.Mass);

            // If the collision causes a reflection (negative mag), reflect the new velocity off the collision normal
            newVelocity = magnitude < 0 ? Vector2.Reflect(-newVelocity, collisionNormal) : newVelocity;

            _rb.velocity = newVelocity;
        }
    }
}
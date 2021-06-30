using System;
using UnityEngine;
using Util;

namespace Physics
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PhysicsBody2D : MonoBehaviour
    {
        public static float GlobalGravityAcceleration = 9.81f;

        /// <summary>
        /// The gravity applied to all <see cref="PhysicsBody2D"/>s.
        /// </summary>
        public static Vector2 GlobalGravity = new Vector2(0.0f, GlobalGravityAcceleration);

        public enum Type
        {
            Dynamic,
            Kinematic
        }

        /// <summary>
        /// The type of the physics body. Kinematic will be treated with INF mass.
        /// </summary>
        [SerializeField] [Tooltip("The type of the physics body. Kinematic will be treated with INF mass.")]
        private Type bodyType = Type.Dynamic;

        /// <summary>
        /// Whether the rotation of the physics body should be locked.
        /// </summary>
        [SerializeField] [Tooltip("Whether the rotation of the physics body should be locked.")]
        private bool freezeRotation = true;

        /// <summary>
        /// Whether to use the global gravity or a custom one.
        /// </summary>
        [SerializeField] [Tooltip("Whether to use the global gravity or a custom one.")] [HideInInspector]
        public bool useCustomGravity;

        /// <summary>
        /// Custom gravity applied to the body
        /// </summary>
        [HideInInspector] public Vector2 customGravity = new Vector2(0.0f, 9.81f);

        /// <summary>
        /// Factor applied to the global gravity
        /// </summary>
        [HideInInspector] public float gravityScale = 1.0f;

        /// <summary>
        /// The mass of this physics body
        /// </summary>
        [HideInInspector] public float mass = 1.0f;

        /// <summary>
        /// A factor applied to the drag on each axis. Can be used to customise the drag for each axis (not realistic)
        /// </summary>
        [HideInInspector] public Vector2 dragAxisFactor = new Vector2(1.0f, 1.0f);

        /// <summary>
        /// The drag coefficient of the object/shape
        /// </summary>
        [HideInInspector] public float dragCoefficient = 1f;

        /// <summary>
        /// The density of the element the physics body is currently in
        /// </summary>
        [HideInInspector] public float elementDensity = 1.225f;

        /// <summary>
        /// The area affected by the material density creating the drag
        /// </summary>
        [HideInInspector] public float frontalArea = 1;

        /// <summary>
        /// Use quickDrag instead of correct drag [0-1]
        /// </summary>
        [HideInInspector] public bool useQuickDrag;

        /// <summary>
        /// QuickDrag amount. Higher than 1 reverts, lower than 0 accelerates
        /// </summary>
        [HideInInspector] public float quickDrag = 0.05f;

        /// <summary>
        /// If enabled, will add a drag on collision with static objects
        /// </summary>
        [HideInInspector] public bool collisionDragEnabled = false;

        /// <summary>
        /// The amount of collision drag applied to the object when colliding.
        /// </summary>
        [HideInInspector] public float collisionDrag = 0.25f;
        
        /// <summary>
        /// If enabled, will add a drag on collision with dynamic objects
        /// </summary>
        [HideInInspector] public bool collisionDragWithDynamicsEnabled = false;

        /// <summary>
        /// The bounciness of the physics body. < 0 -> 'Phasing' | 0-1 -> Bounce | > 1 -> Explosion
        /// </summary>
        [HideInInspector] public float bounciness = 1.0f;

        /// <summary>
        /// Display debug values and buttons
        /// </summary>
        [HideInInspector] public bool showDebugValues;

        /// <summary>
        /// The current velocity
        /// </summary>
        [HideInInspector] public Vector2 currentVelocity;

        /// <summary>
        /// Whether we reached terminal velocity on Y
        /// </summary>
        [HideInInspector] public bool terminalVelocityXReached;

        /// <summary>
        /// Whether we reached terminal velocity on Y
        /// </summary>
        [HideInInspector] public bool terminalVelocityYReached;

        /// <summary>
        /// Adds the specified velocity to the body
        /// </summary>
        [HideInInspector] public bool addVelocityToBody;

        /// <summary>
        /// The velocity to add with the 'AddVelocityToBody' button
        /// </summary>
        [HideInInspector] public Vector2 velocityToAdd;


        /// <summary>
        /// The type of this body
        /// </summary>
        public Type BodyType => bodyType;

        /// <summary>
        /// The current velocity of the body (-> rigidbody.velocity)
        /// </summary>
        public Vector2 Velocity => _rb.velocity;

        /// <summary>
        /// The last velocity (before collisions)
        /// </summary>
        public Vector2 CachedVelocity { get; private set; }

        /// <summary>
        /// The base velocity is the body inherits from e.g. a moving platform
        /// </summary>
        public Vector2 BaseVelocity { get; private set; }

        public Collider2D Collider2D => _collider2D;

        /// <summary>
        /// The internal rigidbody component
        /// </summary>
        private Rigidbody2D _rb;

        /// <summary>
        /// The internal collider2D component
        /// </summary>
        private Collider2D _collider2D;

        /// <summary>
        /// The old drag for quick drag
        /// </summary>
        private float _oldQuickDrag;

        /// <summary>
        /// Whether the base velocity should be reset (over time)
        /// </summary>
        private bool _resetBaseVelocity;

        /// <summary>
        /// The delay for the base velocity reset
        /// </summary>
        private float _resetBaseVelocityDelay = 0.5f;

        /// <summary>
        /// The current delay timer for the reset of the base velocity
        /// </summary>
        private float _resetBaseVelocityTime;

        /// <summary>
        /// Whether this body is currently colliding with a static object (or kinematic PhysicsBody2D)
        /// </summary>
        private bool _isCollidingWithStatic;
        
        /// <summary>
        /// Whether this body is currently colliding with a dynamic object
        /// </summary>
        private bool _isCollidingWithDynamic;

        /// <summary>
        /// The default element density the body is moving through. Should probably always be air
        /// </summary>
        private readonly float _defaultElementDensity =
            ElementDensity.GetMaterialDensity(ElementDensity.ElementType.Air);

        private const float COLLISION_MAGNITUDE_TOLERANCE = -0.1f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider2D = GetComponent<Collider2D>();

            // Disable/Set rigidbody behaviour 
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (freezeRotation)
                _rb.freezeRotation = true;

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
            terminalVelocityXReached = Mathf.Approximately(Mathf.Abs(CachedVelocity.x), Mathf.Abs(_rb.velocity.x));
            terminalVelocityYReached = Mathf.Approximately(Mathf.Abs(CachedVelocity.y), Mathf.Abs(_rb.velocity.y));

            // Base velocity reset over time
            if (_resetBaseVelocity && _resetBaseVelocityTime + _resetBaseVelocityDelay <= Time.time)
            {
                BaseVelocity = Vector2.Lerp(BaseVelocity, Vector2.zero, 2.0f * Time.fixedDeltaTime);

                if (BaseVelocity.sqrMagnitude < 0.1f)
                {
                    BaseVelocity = Vector2.zero;
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

                // Only apply collision drag if enabled
                if (collisionDragEnabled)
                {
                    // Only apply if collision was with static or dynamic is enabled
                    if (_isCollidingWithStatic || collisionDragWithDynamicsEnabled && _isCollidingWithDynamic)
                    {
                        ApplyCollisionDrag();
                        _isCollidingWithStatic = false;
                        _isCollidingWithDynamic = false;
                    }
                }
            }

            // After everything is applied, cache last velocity (At collision rb.velocity is zero)
            CachedVelocity = _rb.velocity;
        }

        /// <summary>
        /// Use Coefficient of restitution (bounciness) and reflection to determine new velocity.
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

        /// <summary>
        /// Checks if the body is currently colliding with any static or dynamic objects.
        /// </summary>
        /// <param name="other"></param>
        private void OnCollisionStay2D(Collision2D other)
        {
            if (!other.collider.TryGetComponent(out PhysicsBody2D body) || body.bodyType == Type.Kinematic)
            {
                _isCollidingWithStatic = true;
            }
            else if (body.bodyType == Type.Dynamic)
            {
                _isCollidingWithDynamic = true;
            }
        }

        /// <summary>
        /// Moving into another element with a different density.
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Change drag
            if (!other.TryGetComponent(out Element dragChanger))
                return;
            _oldQuickDrag = quickDrag;
            quickDrag = dragChanger.quickDrag;
            elementDensity = dragChanger.ElementDensity;
        }

        /// <summary>
        /// Moving out of another element, back to the previous element.
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerExit2D(Collider2D other)
        {
            // Revert drag
            if (!other.TryGetComponent(out Element _))
                return;
            // Revert drag
            elementDensity = _defaultElementDensity;
            quickDrag = _oldQuickDrag;
        }

        /// <summary>
        /// Sets the global gravity direction by applying the <see cref="GlobalGravityAcceleration"/> to the given direction.
        /// </summary>
        /// <param name="normalizedDirection">The new direction of the gravity.</param>
        public static void SetGlobalGravityDirection(Vector2 normalizedDirection)
        {
            GlobalGravity = normalizedDirection * GlobalGravityAcceleration;
        }

        /// <summary>
        /// Applies a simple force, using <see cref="Rigidbody2D.AddForce(UnityEngine.Vector2)"/>.
        /// </summary>
        /// <param name="force"></param>
        public void ApplyForce(Vector2 force)
        {
            _rb.AddForce(force);
        }

        /// <summary>
        /// Adds the given velocity to the internal rigidbody velocity.
        /// </summary>
        public void AddVelocity(Vector2 velocity)
        {
            _rb.velocity += velocity;
        }

        /// <summary>
        /// Sets the position of the physics body.
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            _rb.MovePosition(position);
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
            BaseVelocity = newBaseVelocity;
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
        /// Enables/Disables the PhysicsBody behaviour and the activates/deactivates the underlying rigidbody simulation.
        /// </summary>
        /// <param name="setEnabled"></param>
        public void SetEnabled(bool setEnabled)
        {
            enabled = setEnabled;
            _rb.simulated = setEnabled;
        }

        /// <summary>
        /// Adds a mass to the body (also adds it to the internal rigidbody)
        /// </summary>
        public void AddMass(float massToAdd)
        {
            mass += massToAdd;
            _rb.mass += massToAdd;
        }

        /// <summary>
        /// Removes a mass to the body (also removes it from the internal rigidbody)
        /// </summary>
        public void RemoveMass(float massToRemove)
        {
            mass -= massToRemove;
            _rb.mass -= massToRemove;
        }

        /// <summary>
        /// Resets all collision flags
        /// </summary>
        public void ResetCollisionFlags()
        {
            _isCollidingWithStatic = false;
            _isCollidingWithDynamic = false;
        }

        public void SetConstraints(RigidbodyConstraints2D constraints)
        {
            _rb.constraints = constraints;
        }

        public void AddConstraint(RigidbodyConstraints2D constraint)
        {
            _rb.constraints |= constraint;
        }

        /// <summary>
        /// Applies drag to the current velocity of the physics body.
        /// <para>
        /// This function also includes moving with other objects, by lerping to <see cref="BaseVelocity"/>.
        /// </para>
        /// For checking: https://www.calctool.org/CALC/eng/aerospace/terminal
        /// </summary>
        private void ApplyDrag()
        {
            Vector2 velocity = _rb.velocity - BaseVelocity;

            // Treat x and y separately
            float xDrag = dragAxisFactor.x * dragCoefficient * elementDensity * frontalArea
                * velocity.x * velocity.x / (2 * mass);
            float yDrag = dragAxisFactor.y * dragCoefficient * elementDensity * frontalArea
                * velocity.y * velocity.y / (2 * mass);

            _rb.velocity += new Vector2(
                xDrag * -Mathf.Sign(velocity.x),
                yDrag * -Mathf.Sign(velocity.y)
            ) * Time.fixedDeltaTime;
        }

        /// <summary>
        /// Applies quick drag to the velocity.
        /// </summary>
        private void ApplyQuickDrag()
        {
            _rb.velocity = Vector2.Lerp(_rb.velocity, BaseVelocity, quickDrag);
        }

        /// <summary>
        /// Applies the collision drag as quick drag to the velocity.
        /// </summary>
        private void ApplyCollisionDrag()
        {
            Vector2 velocity = _rb.velocity;
            // Only apply on non-gravity axis (no drag when falling)
            if (GlobalGravity.x == 0.0 || useCustomGravity && customGravity.x == 0.0)
            {
                velocity.x = Mathf.Lerp(velocity.x, BaseVelocity.x, collisionDrag);
            }
            else
            {
                velocity.y = Mathf.Lerp(velocity.y, BaseVelocity.y, collisionDrag);
            }
            _rb.velocity = velocity;
        }

        /// <summary>
        /// Adds the gravity to the body
        /// </summary>
        private void ApplyGravity()
        {
            // Either apply global gravity or custom gravity
            _rb.velocity -= (useCustomGravity ? customGravity : GlobalGravity * gravityScale) * Time.fixedDeltaTime;
        }

        /// <summary>
        /// Handle a collision with a static object or a kinematic body
        /// </summary>
        /// <param name="collisionNormal">The collision normal.</param>
        private void CollisionWithStatic(Vector2 collisionNormal)
        {
            Vector2 collisionNormalAbs = collisionNormal.Abs();
            _rb.velocity = bounciness * Vector2.Reflect(CachedVelocity * collisionNormalAbs, collisionNormal) +
                           CachedVelocity * (Vector2.one - collisionNormalAbs);
        }

        /// <summary>
        /// Handle collision with another physics body
        /// </summary>
        /// <param name="body">The collided body</param>
        /// <param name="collisionNormal">The collision normal</param>
        private void CollisionWithDynamicPhysicsBody(PhysicsBody2D body, Vector2 collisionNormal)
        {
            float mixedBounciness = (bounciness + body.bounciness) / 2.0f;

            // Calculate velocity after collision with another physicsBody2D
            Vector2 newVelocity =
                (mass * CachedVelocity + body.mass * body.CachedVelocity +
                 body.mass * mixedBounciness * (body.CachedVelocity - CachedVelocity)) / (mass + body.mass);

            float magnitude =
                (mass * CachedVelocity.magnitude + body.mass * body.CachedVelocity.magnitude + body.mass *
                    bounciness * (body.CachedVelocity.magnitude - CachedVelocity.magnitude)) / (mass + body.mass);

            // If the collision causes a reflection (negative mag), reflect the new velocity off the collision normal
            newVelocity = magnitude < COLLISION_MAGNITUDE_TOLERANCE
                ? Vector2.Reflect(-newVelocity, collisionNormal)
                : newVelocity;

            _rb.velocity = newVelocity;
        }
    }
}
using UnityEngine;
using Util;

namespace Physics
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PhysicsBody2D : MonoBehaviour
    {
        /// <summary>
        /// The gravity applied to all <see cref="PhysicsBody2D"/>s.
        /// </summary>
        public static Vector2 GlobalGravity = new Vector2(0.0f, 9.81f);

        public enum Type { Dynamic, Kinematic }

        /// <summary>
        /// The type of the physics body. Kinematic will be treated with INF mass.
        /// </summary>
        [SerializeField] [Tooltip("The type of the physics body. Kinematic will be treated with INF mass.")]
        private Type bodyType = Type.Dynamic;

        /// <summary>
        /// The mass of this physics body
        /// </summary>
        public float Mass { get; set; } = 1.0f;

        /// <summary>
        /// Factor applied to the global gravity
        /// </summary>
        public float GravityScale { get; set; } = 1.0f;

        /// <summary>
        /// A factor applied to the drag on each axis. Can be used to customise the drag for each axis (not realistic)
        /// </summary>
        public Vector2 DragAxisFactor { get; set; } = new Vector2(1.0f, 1.0f);

        /// <summary>
        /// The drag coefficient of the object/shape
        /// </summary>
        public float DragCoefficient { get; set; } = 1f;

        /// <summary>
        /// The density of the element the physics body is currently in
        /// </summary>
        public float ElementDensity { get; set; } = 1.225f;

        /// <summary>
        /// The area affected by the material density creating the drag
        /// </summary>
        public float FrontalArea { get; set; } = 1;

        /// <summary>
        /// Use quickDrag instead of correct drag [0-1]
        /// </summary>
        public bool UseQuickDrag { get; set; }

        /// <summary>
        /// QuickDrag amount. Higher than 1 reverts, lower than 0 accelerates
        /// </summary>
        public float QuickDrag { get; set; } = 0.05f;

        /// <summary>
        /// The bounciness of the physics body. < 0 -> 'Phasing' | 0-1 -> Bounce | > 1 -> Explosion
        /// </summary>
        public float Bounciness { get; set; } = 1.0f;

        /// <summary>
        /// Display debug values and buttons
        /// </summary>
        public bool ShowDebugValues { get; set; }

        /// <summary>
        /// The current velocity
        /// </summary>
        public Vector2 CurrentVelocity { get; set; }

        /// <summary>
        /// Whether we reached terminal velocity on Y
        /// </summary>
        public bool TerminalVelocityXReached { get; set; }

        /// <summary>
        /// Whether we reached terminal velocity on Y
        /// </summary>
        public bool TerminalVelocityYReached { get; set; }

        /// <summary>
        /// Adds the specified velocity to the body
        /// </summary>
        public bool AddVelocityToBody { get; set; }

        /// <summary>
        /// The velocity to add with the 'AddVelocityToBody' button
        /// </summary>
        public Vector2 VelocityToAdd { get; set; }


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

        /// <summary>
        /// The internal rigidbody component
        /// </summary>
        private Rigidbody2D _rb;

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
        /// The default element density the body is moving through. Should probably always be air
        /// </summary>
        private readonly float _defaultElementDensity =
            Physics.ElementDensity.GetMaterialDensity(Physics.ElementDensity.ElementType.Air);

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
                _rb.mass = Mass;
                _rb.drag = 0;
                _rb.angularDrag = 0;
                _rb.gravityScale = 0;
            }
        }

        private void FixedUpdate()
        {
            // Debug values
            if (AddVelocityToBody)
            {
                AddVelocityToBody = false;
                _rb.velocity = VelocityToAdd;
            }

            CurrentVelocity = _rb.velocity;
            TerminalVelocityXReached = Mathf.Approximately(Mathf.Abs(CachedVelocity.x), Mathf.Abs(_rb.velocity.x));
            TerminalVelocityYReached = Mathf.Approximately(Mathf.Abs(CachedVelocity.y), Mathf.Abs(_rb.velocity.y));

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
                if (UseQuickDrag)
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
        /// Moving into another element with a different density.
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Change drag
            if (!other.TryGetComponent(out Element dragChanger))
                return;
            _oldQuickDrag = QuickDrag;
            QuickDrag = dragChanger.quickDrag;
            ElementDensity = dragChanger.ElementDensity;
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
            ElementDensity = _defaultElementDensity;
            QuickDrag = _oldQuickDrag;
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
        /// <param name="velocity"></param>
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
        /// Applies drag to the current velocity of the physics body.
        /// The drag factor is in-between 0 and 1.
        /// <para>
        /// This function also includes moving with other objects, by lerping to <see cref="BaseVelocity"/>.
        /// </para>
        /// For checking: https://www.calctool.org/CALC/eng/aerospace/terminal
        /// </summary>
        private void ApplyDrag()
        {
            Vector2 velocity = _rb.velocity - BaseVelocity;

            // Treat x and y separately
            float xDrag = DragAxisFactor.x * DragCoefficient * ElementDensity * FrontalArea
                * velocity.x * velocity.x / (2 * Mass);
            float yDrag = DragAxisFactor.y * DragCoefficient * ElementDensity * FrontalArea
                * velocity.y * velocity.y / (2 * Mass);

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
            _rb.velocity *= (1 - QuickDrag);
        }

        /// <summary>
        /// Adds the gravity to the body
        /// </summary>
        private void ApplyGravity()
        {
            _rb.velocity -= GlobalGravity * (GravityScale * Time.fixedDeltaTime);
        }

        /// <summary>
        /// Handle a collision with a static object or a kinematic body
        /// </summary>
        /// <param name="collisionNormal">The collision normal.</param>
        private void CollisionWithStatic(Vector2 collisionNormal)
        {
            Vector2 collisionNormalAbs = collisionNormal.Abs();
            _rb.velocity = Bounciness * Vector2.Reflect(CachedVelocity * collisionNormalAbs, collisionNormal) +
                           CachedVelocity * (Vector2.one - collisionNormalAbs);
        }

        /// <summary>
        /// Handle collision with another physics body
        /// </summary>
        /// <param name="body">The collided body</param>
        /// <param name="collisionNormal">The collision normal</param>
        private void CollisionWithDynamicPhysicsBody(PhysicsBody2D body, Vector2 collisionNormal)
        {
            float mixedBounciness = (Bounciness + body.Bounciness) / 2.0f;

            // Calculate velocity after collision with another physicsBody2D
            Vector2 newVelocity =
                (Mass * CachedVelocity + body.Mass * body.CachedVelocity +
                 body.Mass * mixedBounciness * (body.CachedVelocity - CachedVelocity)) / (Mass + body.Mass);

            float magnitude =
                (Mass * CachedVelocity.magnitude + body.Mass * body.CachedVelocity.magnitude + body.Mass *
                    Bounciness * (body.CachedVelocity.magnitude - CachedVelocity.magnitude)) / (Mass + body.Mass);

            // If the collision causes a reflection (negative mag), reflect the new velocity off the collision normal
            newVelocity = magnitude < 0 ? Vector2.Reflect(-newVelocity, collisionNormal) : newVelocity;

            _rb.velocity = newVelocity;
        }
    }
}
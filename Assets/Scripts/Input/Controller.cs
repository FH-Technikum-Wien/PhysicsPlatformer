using UnityEngine;

namespace Input
{
    /// <summary>
    /// Linear drag of "20" is pretty good
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Controller : MonoBehaviour
    {
        [SerializeField] [Tooltip("Switches between force and velocity based movement")]
        private bool useForceBasedMovement = true;

        [SerializeField] [Tooltip("Magnitude of movement when using force")]
        private float forceMagnitude = 300;

        [SerializeField] [Tooltip("Magnitude of movement when using velocity")]
        private float velocityMagnitude = 20;

        [SerializeField] [Tooltip("Inner dead zone of the controller")]
        private float innerDeadZone = 0.1f;

        [SerializeField] [Tooltip("Outer dead zone of the controller")]
        private float outerDeadZone = 0.9f;

        private Rigidbody2D _rigid;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            // Get raw input for applying own dead zones
            float inputX = UnityEngine.Input.GetAxisRaw("Horizontal");
            float inputY = UnityEngine.Input.GetAxisRaw("Vertical");
            // Apply custom dead zones
            Vector2 input = DeadZones.Apply(new Vector2(inputX, inputY), innerDeadZone, outerDeadZone);
            // Apply to rigidbody
            if (useForceBasedMovement)
                _rigid.AddForce(input * forceMagnitude);
            else
                _rigid.velocity = input * velocityMagnitude;
        }
    }
}
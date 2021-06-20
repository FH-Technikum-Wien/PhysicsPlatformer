using UnityEngine;

namespace World
{
    /// <summary>
    /// A gravity gate requires a defined gravity direction to let things through.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GravityGate : MonoBehaviour
    {
        [SerializeField] private GravityDirection requiredGravity = GravityDirection.Down;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private Collider2D _collider;

        private void OnValidate()
        {
            spriteRenderer.color = WorldManager.GravityDirectionToColor[requiredGravity];
        }

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            spriteRenderer.color = WorldManager.GravityDirectionToColor[requiredGravity];
        }

        private void Update()
        {
            Color gateColor = spriteRenderer.color;
            if (WorldManager.GravityDirection == requiredGravity)
            {
                _collider.enabled = false;
                gateColor.a = 0.25f;
            }
            else
            {
                _collider.enabled = true;
                gateColor.a = 0.75f;
            }

            spriteRenderer.color = gateColor;
        }
    }
}
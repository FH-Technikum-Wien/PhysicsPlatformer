using System.Collections.Generic;
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

        private readonly Dictionary<GravityDirection, Color> _requiredGravityToColor =
            new Dictionary<GravityDirection, Color>
            {
                {GravityDirection.Down, Color.blue},
                {GravityDirection.Left, new Color(0.62f, 0f, 1f)},
                {GravityDirection.Right, Color.green},
                {GravityDirection.Up, Color.yellow},
            };

        private void OnValidate()
        {
            spriteRenderer.color = _requiredGravityToColor[requiredGravity];
        }

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            spriteRenderer.color = _requiredGravityToColor[requiredGravity];
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
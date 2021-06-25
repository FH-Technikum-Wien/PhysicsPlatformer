using Physics;
using UnityEngine;

namespace World
{
    /// <summary>
    /// Small script to tag objects as pickupables.
    /// </summary>
    [RequireComponent(typeof(PhysicsBody2D))]
    public class Pickupable : MonoBehaviour
    {
        public PhysicsBody2D PhysicsBody2D { get; private set; }

        private void Awake()
        {
            PhysicsBody2D = GetComponent<PhysicsBody2D>();
        }
    }
}
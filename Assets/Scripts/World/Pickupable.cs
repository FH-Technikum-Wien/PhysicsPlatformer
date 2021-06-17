using Physics;
using UnityEngine;

namespace World
{
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
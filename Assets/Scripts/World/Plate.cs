using System.Collections.Generic;
using Physics;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(Collider2D))]
    public class Plate : MonoBehaviour
    {
        public readonly List<PhysicsBody2D> bodies = new List<PhysicsBody2D>();

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.TryGetComponent(out PhysicsBody2D body))
                bodies.Add(body);
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.collider.TryGetComponent(out PhysicsBody2D body) && bodies.Contains(body))
                bodies.Remove(body);
        }
    }
}
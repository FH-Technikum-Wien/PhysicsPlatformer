using System.Collections.Generic;
using Physics;
using UnityEngine;
using UnityEngine.Events;
using World;

namespace Player
{
    public class ChangeGravityAbility : MonoBehaviour
    {
        public static event UnityAction<GravityDirection> OnGravityChange;

        private readonly Dictionary<GravityDirection, Vector2> _gravityDirectionToVectorMap =
            new Dictionary<GravityDirection, Vector2>
            {
                {GravityDirection.Down, new Vector2(0, 1)},
                {GravityDirection.Left, new Vector2(1, 0)},
                {GravityDirection.Right, new Vector2(-1, 0)},
                {GravityDirection.Up, new Vector2(0, -1)},
                {GravityDirection.None, new Vector2(0, 0)},
            };

        /// <summary>
        /// Sets the global gravity for all <see cref="PhysicsBody2D"/> bodies to the given direction.
        /// </summary>
        /// <param name="gravityDirection">The new direction of the global gravity.</param>
        public void SetGravity(GravityDirection gravityDirection)
        {
            PhysicsBody2D.SetGlobalGravityDirection(_gravityDirectionToVectorMap[gravityDirection]);
            OnGravityChange?.Invoke(gravityDirection);
        }
    }
}
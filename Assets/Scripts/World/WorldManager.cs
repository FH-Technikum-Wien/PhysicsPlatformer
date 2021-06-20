using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public enum GravityDirection
    {
        Left,
        Down,
        Right,
        Up,
        None
    }

    public class WorldManager : MonoBehaviour
    {
        public static GravityDirection GravityDirection = GravityDirection.Down;

        public static Dictionary<GravityDirection, Color> GravityDirectionToColor =
            new Dictionary<GravityDirection, Color>
            {
                {GravityDirection.Down, Color.blue},
                {GravityDirection.Left, new Color(0.62f, 0f, 1f)},
                {GravityDirection.Right, Color.green},
                {GravityDirection.Up, Color.yellow},
            };

        public static void ChangeGravityDirection(int gravityChange)
        {
            GravityDirection = (GravityDirection) (((int) GravityDirection + gravityChange + 4) % 4);
        }
    }
}
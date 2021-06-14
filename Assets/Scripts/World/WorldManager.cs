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

        public static void ChangeGravityDirection(int gravityChange)
        {
            GravityDirection = (GravityDirection) (((int) GravityDirection + gravityChange + 4) % 4);
        }
    }
}
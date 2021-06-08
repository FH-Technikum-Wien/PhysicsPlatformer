using UnityEngine;

namespace Util
{
    public static class Vector2Extension
    {
        public static Vector2 Abs(this Vector2 vector2)
        {
            return new Vector2(Mathf.Abs(vector2.x), Mathf.Abs(vector2.y));
        }
    }
}
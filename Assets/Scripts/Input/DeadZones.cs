using UnityEngine;

namespace Input
{
    public static class DeadZones
    {
        /// <summary>
        /// Applies dead zones on one axis.
        /// </summary>
        /// <param name="value">The axis value</param>
        /// <param name="innerDeadZone">Defines the inner dead zone</param>
        /// <param name="outerDeadZone">Defines the outer dead zone</param>
        /// <returns></returns>
        public static float Apply(float value, float innerDeadZone, float outerDeadZone)
        {
            // Use absolute for calculation, then map back to actual sign
            float abs = Mathf.Abs(value);
            // Returns t [0,1] which defines the interpolating value for c, between a and b
            return Mathf.InverseLerp(innerDeadZone, outerDeadZone, abs) * Mathf.Sign(value);
        }

        /// <summary>
        /// Applies dead zones on an input-vector. Uses <see cref="Apply(float,float,float)"/> on each axis
        /// </summary>
        /// <param name="input">Input as a vector</param>
        /// <param name="innerDeadZone">Defines the inner dead zone</param>
        /// <param name="outerDeadZone">Defines the outer dead zone</param>
        /// <returns></returns>
        public static Vector2 Apply(Vector2 input, float innerDeadZone, float outerDeadZone)
        {
            // Apply dead zones per axis
            Vector2 result = new Vector2(Apply(input.x, innerDeadZone, outerDeadZone),
                Apply(input.y, innerDeadZone, outerDeadZone));
            // Clamp it
            return result.magnitude > 1f ? result.normalized : result;
        }

        public static Vector2 OldApply(Vector2 input, float innerDeadZone, float outerDeadZone)
        {
            float magnitude = input.magnitude;
            float t = Mathf.InverseLerp(innerDeadZone, outerDeadZone, magnitude);
            return input.normalized * t;
        }
    }
}
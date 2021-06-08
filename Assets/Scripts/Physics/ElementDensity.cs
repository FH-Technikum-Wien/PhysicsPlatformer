using System;
using UnityEngine;

namespace Physics
{
    public class ElementDensity : MonoBehaviour
    {
        public enum ElementType { Vacuum, Air, Water }

        public static float GetMaterialDensity(ElementType elementType)
        {
            return elementType switch
            {
                ElementType.Vacuum => 0.0f,
                ElementType.Air => 1.225f,
                ElementType.Water => 15.0f,
                _ => throw new ArgumentOutOfRangeException(nameof(elementType), elementType, null)
            };
        }
    }
}
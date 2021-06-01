using System;
using UnityEngine;

namespace Physics
{
    public class MaterialDensity : MonoBehaviour
    {
        public enum MaterialType { Vacuum, Air, Water }

        public static float GetMaterialDensity(MaterialType materialType)
        {
            return materialType switch
            {
                MaterialType.Vacuum => 0.0f,
                MaterialType.Air => 1.225f,
                MaterialType.Water => 15.0f,
                _ => throw new ArgumentOutOfRangeException(nameof(materialType), materialType, null)
            };
        }
    }
}
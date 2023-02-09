using System;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Represent minimal and maximal 2D point.
    /// </summary>
    [Serializable]
    public struct MinMaxRange
    {
        [SerializeField]
        public int minX;
        [SerializeField]
        public int minY;
        [SerializeField]
        public int maxX;
        [SerializeField]
        public int maxY;

        public MinMaxRange(int minX, int minY, int maxX, int maxY)
        {
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
        }
    
        public void Set(int minX, int minY, int maxX, int maxY)
        {
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
        }

        public bool IsDefault => minX == 0 && minY == 0 && maxX == 0 && maxY == 0;
    
        public bool IsPointInRange(Vector2Int hunkPoint) =>
            minX <= hunkPoint.x
            && minY <= hunkPoint.y
            && maxX >= hunkPoint.x
            && maxY >= hunkPoint.y;
    }
}
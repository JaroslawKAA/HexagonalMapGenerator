using System;
using UnityEngine.Serialization;

[Serializable]
public struct MinMaxRange
{
    public int minX;
    public int minY;
    public int maxX;
    public int maxY;

    public MinMaxRange(int minX, int minY, int maxX, int maxY)
    {
        this.minX = minX;
        this.minY = minY;
        this.maxX = maxX;
        this.maxY = maxY;
    }

    public bool IsDefault => minX == 0 && minY == 0 && maxX == 0 && maxY == 0;
}
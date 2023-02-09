using System;
using HexagonTilemapEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace PathFinding
{
    [Serializable]
    public class PathNode
    {
        // SERIALIZED
        [FormerlySerializedAs("_realtimeMapGenerator")] [SerializeField]
        private RuntimeMapLoader runtimeMapLoader;

        [SerializeField]
        private HexTile _hexTile;

        public int gCost;
        public int hCost;
        public int fCost;

        public PathNode cameFromNode;

        // PROPERTIES
        public HexTile HexTile => _hexTile;

        // PUBLIC
        public PathNode(RuntimeMapLoader runtimeMapLoader, HexTile hexTile)
        {
            this.runtimeMapLoader = runtimeMapLoader;
            _hexTile = hexTile;
        }

        public void CalculateFCost() => fCost = gCost + hCost;
        public override string ToString() => $"{_hexTile.Coordinates.x},{_hexTile.Coordinates.y}";
    }
}

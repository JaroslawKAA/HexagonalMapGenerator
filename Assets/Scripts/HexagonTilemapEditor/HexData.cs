using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexagonTilemapEditor
{
    [Serializable]
    [HideReferenceObjectPicker]
    public class HexData
    {
        [ReadOnly]
        public int prefabId;
        
        [ReadOnly]
        public Vector2Int coordinates;

        public HexData(int prefabId, Vector2Int coordinates)
        {
            this.prefabId = prefabId;
            this.coordinates = coordinates;
        }
    }
}

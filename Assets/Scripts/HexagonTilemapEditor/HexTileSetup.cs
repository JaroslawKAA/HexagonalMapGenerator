using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexagonTilemapEditor
{
    [Serializable]
    public class HexTileSetup
    {
        public int id;

        [Required]
        public Sprite sprite;

        [Range(0, 100)]
        public int frequency = 100;

        public bool interactable = false;
        public bool walkable = false;
    }
}
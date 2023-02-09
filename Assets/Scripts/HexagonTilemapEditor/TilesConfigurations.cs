using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexagonTilemapEditor
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Tiles Configuration", fileName = "TilesConfigurations", order = 0)]
    public class TilesConfigurations : SerializedScriptableObject
    {
        [SerializeField]
        [TableList(AlwaysExpanded = true)]
        [ValidateInput("HasAnyElements", defaultMessage: "Configuration should contain one element at last.")]
        [ValidateInput("ValidateFrequency")]
        [InfoBox("Sum of frequency should be equal 100%. " +
                 "Last member will be calculated automatically.")]
        private List<HexTileSetup> configurations = new() { new HexTileSetup() };

        public List<HexTileSetup> Configurations => configurations;

        #region Odin inspector validation

        // Odin validation method
        // ReSharper disable once UnusedMember.Local
        private bool ValidateFrequency()
        {
            if (!configurations.Any())
                return false;

            configurations.Last().frequency = 100 - configurations
                .Take(configurations.Count - 1)
                .Sum(t => t.frequency);
            return true;
        }

        // Odin validation method
        // ReSharper disable once UnusedMember.Local
        private bool HasAnyElements() => configurations.Any();

        #endregion
    }
}

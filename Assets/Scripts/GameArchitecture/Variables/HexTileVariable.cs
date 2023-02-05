using HexagonTilemapEditor;
using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	public class HexTileEvent : UnityEvent<HexTile> { }

	[CreateAssetMenu(
	    fileName = "HexTileVariable.asset",
	    menuName = SOArchitecture_Utility.VARIABLE_SUBMENU + "Hex Tile Game Event",
	    order = 120)]
	public class HexTileVariable : BaseVariable<HexTile, HexTileEvent>
	{
	}
}
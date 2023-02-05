using HexagonTilemapEditor;
using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	[CreateAssetMenu(
	    fileName = "HexTileGameEvent.asset",
	    menuName = SOArchitecture_Utility.GAME_EVENT + "Hex Tile Game Event",
	    order = 120)]
	public sealed class HexTileGameEvent : GameEventBase<HexTile>
	{
	}
}
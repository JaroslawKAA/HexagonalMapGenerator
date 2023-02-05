using HexagonTilemapEditor;
using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[AddComponentMenu(SOArchitecture_Utility.EVENT_LISTENER_SUBMENU + "HexTile")]
	public sealed class HexTileGameEventListener 
		: BaseGameEventListener<HexTile, HexTileGameEvent, HexTileUnityEvent>
	{
	}
}
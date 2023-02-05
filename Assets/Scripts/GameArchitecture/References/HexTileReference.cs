using HexagonTilemapEditor;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	public sealed class HexTileReference : BaseReference<HexTile, HexTileVariable>
	{
	    public HexTileReference() : base() { }
	    public HexTileReference(HexTile value) : base(value) { }
	}
}
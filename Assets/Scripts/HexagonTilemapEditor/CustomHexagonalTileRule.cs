using UnityEngine;

public class CustomHexagonalTileRule : HexagonalRuleTile
{
    [SerializeField]
    private HexTile _hexTile;

    public HexTile HexTile
    {
        get => _hexTile;
        set => _hexTile = value;
    }
}

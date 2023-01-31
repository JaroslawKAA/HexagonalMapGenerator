using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Hunk : SerializedMonoBehaviour
{
   [ShowInInspector]
   [ReadOnly]
   [DictionaryDrawerSettings]
   private Dictionary<Vector2Int, HexTile> hexagonalTiles = new Dictionary<Vector2Int, HexTile>();

   public void RegisterTile(Vector2Int key, HexTile hexTile)
   {
      hexagonalTiles[key] = hexTile;
   }

   public HexTile GetTile(Vector2Int coords)
   {
      return hexagonalTiles[coords];
   }
}

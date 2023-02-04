using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class HexesPool : MonoBehaviour
{
    [SerializeField] [Required]
    private HexTile objectToPull;

    [SerializeField] [Required]
    private HexagonTilemapEditorSO _config;

    [SerializeField] [ReadOnly]
    private int _amount;

    private List<HexTile> _pooledHexes;

    private void Awake()
    {
        _amount = 15 * _config.HunkSize.x * _config.HunkSize.y;

        _pooledHexes = new List<HexTile>();
        HexTile tmp;
        for (int i = 0; i < _amount; i++)
        {
            tmp = Instantiate(objectToPull);
            tmp.gameObject.SetActive(false);
            tmp.transform.parent = transform;
            _pooledHexes.Add(tmp);
        }
    }

    private HexTile _tmpLastHex;

    public HexTile GetPulledObject()
    {
        for (int i = 0; i < _amount; i++)
            if (!_pooledHexes[i].isActiveAndEnabled)
            {
                _pooledHexes[i].gameObject.SetActive(true);

                // Swap first and last elements in collection to speed up access
                _tmpLastHex = _pooledHexes[_amount - 1];
                _pooledHexes[_amount - 1] = _pooledHexes[i];
                _pooledHexes[i] = _tmpLastHex;

                return _pooledHexes[i];
            }

        return null;
    }
}
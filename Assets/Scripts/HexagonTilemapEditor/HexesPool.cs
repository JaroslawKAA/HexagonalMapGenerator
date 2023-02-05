using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class HexesPool : MonoBehaviour
{
    [SerializeField] [Required]
    private GameObject objectToPull;

    [SerializeField] [Required]
    private HexagonTilemapEditorSO _config;

    [SerializeField] [ReadOnly]
    private int _amount;

    private Dictionary<int, GameObject> _pooledHexes;

    private GameObject _tempObj;
    
    private void Awake()
    {
        Debug.Log("Pooling hexes...");
        // Keep reserve
        _amount = 15 * _config.HunkSize.x * _config.HunkSize.y;

        _pooledHexes = new Dictionary<int, GameObject>();
        for (int i = 0; i < _amount; i++)
        {
            _tempObj = Instantiate(objectToPull);
            _tempObj.gameObject.SetActive(false);
            _tempObj.transform.parent = transform;
            _pooledHexes.Add(i, _tempObj);
        }
        Debug.Log("Pooling hexes finished.");
    }

    private GameObject _swapTemp;
    public GameObject GetPulledObject()
    {

        foreach (int key in _pooledHexes.Keys)
        {
            if (!_pooledHexes[key].activeSelf)
            {
                _tempObj = _pooledHexes[key];
                // Move object to end
                _swapTemp = _pooledHexes[_amount - 1];
                _pooledHexes[_amount - 1] = _tempObj;
                _pooledHexes[key] = _swapTemp;
                break;
            }
        }
        
        _tempObj.gameObject.SetActive(true);
        return _tempObj;
    }
}
using HexagonTilemapEditor;
using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace UI
{
    public class StatsPanel : MonoBehaviour
    {
        // SERIALIZED
        [Title("Depend")]
        [SerializeField] [Required]
        private TMP_Text _statPrefab;

        [SerializeField] [Required]
        private HexTileGameEvent onHexSelected;

        // EVENT
        private void Awake()
        {
            onHexSelected.AddListener(OnHexSelected);
            Hide();
        }

        // PUBLIC
        public void DisplayStats(string[] stats)
        {
            CleanStats();
        
            foreach (string stat in stats)
            {
                TMP_Text record = Instantiate(_statPrefab, transform);
                record.text = stat;
            }
        
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
        
        // PRIVATE
        private void OnHexSelected(HexTile tile)
        {
            if (tile)
                DisplayStats(tile.Stats);
            else
                Hide();
        }

        private void CleanStats()
        {
            for (int i = transform.childCount - 1; i >= 0; i--) 
                Destroy(transform.GetChild(i).gameObject);
        }
    }
}

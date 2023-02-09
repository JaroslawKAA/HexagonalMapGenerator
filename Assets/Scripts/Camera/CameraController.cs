using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Camera
{
    public class CameraController : MonoBehaviour
    {
        // SERIALIZED
        [FormerlySerializedAs("moveSpeed")]
        [Title("Config")]
        [SerializeField]
        private float _moveSpeed = 1f;

        [Title("Dependencies")]
        [SerializeField] [Required]
        private UnityEngine.Camera _camera;

        // PRIVATE
        private Vector3 _startClickPos;
        private Vector3 _endClickPos;

        private Vector3 _moveVector;

        // EVENT
        private void Update()
        {
            // Start drag camera
            if (Input.GetMouseButtonDown(1)) 
                _startClickPos = _camera.ScreenToWorldPoint(Input.mousePosition);

            // Update camera pos
            if (Input.GetMouseButton(1))
            {
                _endClickPos = _camera.ScreenToWorldPoint(Input.mousePosition);
                _moveVector = (_startClickPos - _endClickPos) * (_moveSpeed * Time.deltaTime);
                transform.position += _moveVector;
            }
        }
    }
}
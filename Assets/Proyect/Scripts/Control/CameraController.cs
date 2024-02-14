using UnityEngine;
using UnityEngine.InputSystem;

namespace Burmuruk.Tesis.Control
{
    class CameraController : MonoBehaviour
    {
        [Header("Referenes")]
        [SerializeField] new GameObject camera;
        [SerializeField] GameObject body;
        [SerializeField] GameObject pivot;
        [Header("Settings")]
        [SerializeField] float maxHeight = 45;
        [SerializeField] float minHeight = 45;
        [SerializeField] float verticalSensitivity = 1f;
        [SerializeField] float horizontalSensitivity = 1f;

        float m_rotationX = 0;
        private Vector2 m_direction = default;

        private void Update()
        {
            MoveCamera();
        }

        public void LookAt(InputAction.CallbackContext context)
        {
            Vector2 direction = context.ReadValue<Vector2>();

        }

        private void MoveCamera()
        {
            
        }
    }
}

using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Burmuruk.Tesis.Control
{
    class CameraController : MonoBehaviour
    {
        //[Header("Referenes")]
        PlayerManager playerManager;
        new CinemachineVirtualCamera camera;

        private void Awake()
        {
            playerManager = FindObjectOfType<PlayerManager>();
            playerManager.OnPlayerChanged += SetTarget;
            camera = GetComponent<CinemachineVirtualCamera>();
        }

        void SetTarget()
        {
            camera.Follow = playerManager.CurPlayer;
        }
    }
}

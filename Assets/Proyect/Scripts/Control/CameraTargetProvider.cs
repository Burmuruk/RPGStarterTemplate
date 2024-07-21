using Cinemachine;
using UnityEngine;

namespace Burmuruk.Tesis.Control
{
    class CameraTargetProvider : MonoBehaviour
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

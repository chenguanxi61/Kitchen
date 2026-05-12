using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollowLocalPlayer : MonoBehaviour
{
    private static CameraFollowLocalPlayer instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject gameObject = new GameObject(nameof(CameraFollowLocalPlayer));
        instance = gameObject.AddComponent<CameraFollowLocalPlayer>();
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        TryFollowLocalPlayer();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        Player.OnAnyPlayerSpawned -= Player_OnAnyPlayerSpawned;
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryFollowLocalPlayer();
    }

    private void Player_OnAnyPlayerSpawned()
    {
        TryFollowLocalPlayer();
    }

    private void TryFollowLocalPlayer()
    {
        if (Player.LocalInstance == null)
        {
            return;
        }

        CinemachineVirtualCamera virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (virtualCamera == null)
        {
            return;
        }

        Transform playerTransform = Player.LocalInstance.transform;
        virtualCamera.Follow = playerTransform;
        virtualCamera.LookAt = null;

        CinemachineFramingTransposer framingTransposer =
            virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (framingTransposer == null)
        {
            framingTransposer = virtualCamera.AddCinemachineComponent<CinemachineFramingTransposer>();
        }

        framingTransposer.m_CameraDistance = 10f;
        framingTransposer.m_TrackedObjectOffset = Vector3.zero;
    }
}

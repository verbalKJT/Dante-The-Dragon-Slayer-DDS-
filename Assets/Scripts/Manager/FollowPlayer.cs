
using Unity.Cinemachine;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private CinemachineCamera _camera;
    
    private Transform _player;

    void Awake()
    {
        _camera = GetComponent<CinemachineCamera>();
        _player = GameObject.FindWithTag("Player").transform;
        _camera.Follow = _player;
        _camera.LookAt = _player;
    }
}

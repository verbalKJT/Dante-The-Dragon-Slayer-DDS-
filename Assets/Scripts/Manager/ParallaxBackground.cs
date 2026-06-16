using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    
    private Camera _cam;
    [SerializeField] private Vector2 multiplier;
    private Vector3 _previousTransform;
    void Start()
    {
        if (_cam == null)
        {
            _cam = Camera.main;
            _previousTransform = _cam.transform.position;
        }
    }

    
    void LateUpdate()
    {
        if (_cam == null) return;
        
        // 현 프레임 카메라 위치에서 이전 프레임 카메라 위치 빼기
        Vector3 deltaMovement = _cam.transform.position - _previousTransform;
        
        transform.position += new Vector3(deltaMovement.x * multiplier.x, deltaMovement.y * multiplier.y, 0f);
        
        // 갱신
        _previousTransform = _cam.transform.position;
    }
}

using UnityEngine;

namespace TarodevController
{
    [CreateAssetMenu]
    public class ScriptableStats : ScriptableObject
    {
        [Header("LAYERS")] 
        [Tooltip("플레이어가 속한 레이어로 설정하세요.")]
        public LayerMask PlayerLayer;

        [Header("INPUT")] 
        [Tooltip("모든 입력값을 정수(0 또는 1)로 딱 떨어지게 만듭니다. 게임패드 사용 시 캐릭터가 천천히 걷는 것을 방지합니다. 게임패드와 키보드의 조작감을 동일하게 맞추기 위해 true(참)로 설정하는 것을 권장합니다.")]
        public bool SnapInput = true;

        [Tooltip("사다리를 타거나 절벽을 오르기 위해 필요한 최소 수직 입력값입니다. 컨트롤러(조이패드) 사용 시 원치 않게 매달리는 현상을 방지합니다."), Range(0.01f, 0.99f)]
        public float VerticalDeadZoneThreshold = 0.3f;

        [Tooltip("좌우 이동이 인식되기 위해 필요한 최소 수평 입력값입니다. 컨트롤러 쏠림 현상(드리프트)으로 인한 원치 않는 자동 이동을 방지합니다."), Range(0.01f, 0.99f)]
        public float HorizontalDeadZoneThreshold = 0.1f;

        [Header("MOVEMENT")] 
        [Tooltip("최대 수평 이동 속도입니다.")]
        public float MaxSpeed = 14;

        [Tooltip("플레이어가 수평 속도를 얻는 능력치(가속도)입니다.")]
        public float Acceleration = 120;

        [Tooltip("플레이어가 땅에서 멈출 때까지의 감속도입니다.")]
        public float GroundDeceleration = 60;

        [Tooltip("공중에서 이동 입력을 멈췄을 때만 적용되는 공중 감속도입니다.")]
        public float AirDeceleration = 30;

        [Tooltip("땅에 닿아 있을 때 지속적으로 가해지는 하향 힘입니다. 경사면에서 미끄러지지 않도록 돕습니다."), Range(0f, -10f)]
        public float GroundingForce = -1f;

        [Tooltip("바닥 및 천장 충돌을 감지하는 여유 거리입니다."), Range(0f, 0.5f)]
        public float GrounderDistance = 0.05f;

        [Header("JUMP")] 
        [Tooltip("점프 시 즉각적으로 가해지는 수직 속도(점프력)입니다.")]
        public float JumpPower = 36;

        [Tooltip("최대 수직 이동(낙하) 속도입니다.")]
        public float MaxFallSpeed = 40;

        [Tooltip("플레이어의 낙하 속도가 증가하는 가속도입니다. (일명 '공중 중력')")]
        public float FallAcceleration = 110;
        

        [Tooltip("코요테 점프(Coyote Jump)가 유지되는 시간입니다. 코요테 점프는 절벽에서 발이 허공으로 떨어진 직후에도 잠시 동안 점프할 수 있게 해주는 기능입니다.")]
        public float CoyoteTime = .15f;

        [Tooltip("점프 입력을 미리 저장(버퍼링)하는 시간입니다. 땅에 닿기 직전에 점프 키를 미리 눌러도, 땅에 닿자마자 즉시 점프가 실행되도록 해줍니다.")]
        public float JumpBuffer = .2f;
    }
}
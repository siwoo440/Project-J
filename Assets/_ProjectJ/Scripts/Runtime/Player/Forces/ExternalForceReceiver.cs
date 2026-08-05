using UnityEngine; // Unity 벡터와 컴포넌트 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스
{ // 네임스페이스 범위 시작
    public abstract class ExternalForceReceiver : MonoBehaviour // 외부 힘 수신 공통 컴포넌트
    { // 클래스 범위 시작
        public Transform ForceReceiverTransform => transform; // 힘을 받을 대상 위치

        public abstract bool TryReceiveExternalForce(Vector3 direction, float force); // 외부 힘 적용 시도 규칙
    } // 클래스 범위 종료
} // 네임스페이스 범위 종료

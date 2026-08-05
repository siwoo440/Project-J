using UnityEngine; // Unity 벡터와 컴포넌트 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 외부 힘 수신 범위
    public abstract class ExternalForceReceiver : MonoBehaviour // 외부 힘 수신 공통 컴포넌트 선언
    { // 외부 힘 수신 기능 범위
        public Transform ForceReceiverTransform => transform; // 힘을 받을 대상 위치 반환

        public abstract bool TryReceiveExternalForce(Vector3 direction, float force); // 기존 방향과 세기 기반 외부 힘 적용 규칙

        public virtual bool TryReceiveExternalForce(ExternalForceRequest request) // 원인과 결합 방식을 포함한 외부 힘 적용 시도
        { // 통합 외부 힘 적용 범위
            float force = request.Velocity.magnitude; // 기존 수신기에 전달할 힘 크기 계산
            Vector3 direction = force <= 0.0001f ? Vector3.zero : request.Velocity / force; // 기존 수신기에 전달할 힘 방향 계산
            return TryReceiveExternalForce(direction, force); // 기존 외부 힘 규칙으로 요청 전달
        } // 통합 외부 힘 적용 범위 종료
    } // 외부 힘 수신 기능 범위 종료
} // 외부 힘 수신 범위 종료

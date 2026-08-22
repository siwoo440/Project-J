using ProjectJ.Obstacles; // 기존 AirBag 설정 사용
using UnityEngine; // Unity 기본 타입 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent] // 동일 Bridge 중복 방지
    [RequireComponent(typeof(AirBagObstacle))] // 기존 AirBag 값 재사용
    [RequireComponent(typeof(BoxCollider))] // Trigger 영역 보장
    [RequireComponent(typeof(Rigidbody))] // Trigger 이벤트 보장
    public sealed class ProjectJNetworkAirBagBridge :
        MonoBehaviour
    {
        private AirBagObstacle airBag; // 기존 AirBag 설정 참조
        private BoxCollider trigger; // 네트워크 Player 감지 Trigger
        private Rigidbody body; // Trigger용 Rigidbody

        private void Awake()
        {
            ResolveReferences(); // 구성 요소 준비

            if (airBag != null)
            {
                airBag.enabled = false; // 기존 Rigidbody Player 전용 Collision 로직 차단
            }

            if (trigger != null)
            {
                trigger.isTrigger = true; // Network Player 접촉 Trigger 사용
            }

            if (body != null)
            {
                body.isKinematic = true; // AirBag 위치 고정
                body.useGravity = false; // 중력 비활성화
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (
                other == null ||
                airBag == null
            )
            {
                return; // 잘못된 접촉 처리
            }

            ProjectJNetworkExternalGameplay target =
                other.GetComponentInParent<ProjectJNetworkExternalGameplay>(); // Network Player 탐색

            if (target == null)
            {
                return; // Network Player가 아니면 무시
            }

            Vector3 pushDirection =
                AirBagObstacle.CalculatePushDirection(
                    transform,
                    other.bounds.center,
                    airBag.LocalPushDirection,
                    airBag.ContactSpread
                ); // 기존 AirBag 방향 계산 재사용

            Vector3 velocityChange =
                pushDirection *
                airBag.HorizontalVelocityChange; // 기존 AirBag 힘 사용

            target.TryApplyExternalVelocityChange(
                ProjectJExternalForceSource.AirBag,
                velocityChange
            ); // State Authority에서만 실제 적용
        }

        private void ResolveReferences()
        {
            if (airBag == null)
            {
                airBag = GetComponent<AirBagObstacle>(); // 기존 AirBag 탐색
            }

            if (trigger == null)
            {
                trigger = GetComponent<BoxCollider>(); // Trigger 탐색
            }

            if (body == null)
            {
                body = GetComponent<Rigidbody>(); // Rigidbody 탐색
            }
        }
    }
}

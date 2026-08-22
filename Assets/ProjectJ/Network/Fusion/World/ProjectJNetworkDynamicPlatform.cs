using Fusion; // NetworkBehaviour와 NetworkTransform 사용
using ProjectJ.Platforms; // 기존 동적 플랫폼 사용
using UnityEngine; // Unity 기본 타입 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent] // 동일 동기화 컴포넌트 중복 방지
    [RequireComponent(typeof(NetworkObject))] // Fusion Scene Object 보장
    [RequireComponent(typeof(NetworkTransform))] // 이동·회전 결과 동기화
    public sealed class ProjectJNetworkDynamicPlatform :
        NetworkBehaviour
    {
        private MovingPlatform movingPlatform; // 기존 이동 플랫폼
        private RotatingPlatform rotatingPlatform; // 기존 회전 플랫폼
        private GhostPlatform ghostPlatform; // 기존 유령 플랫폼
        private BoxCollider ghostCollider; // 유령 플랫폼 충돌 상태
        private Renderer ghostRenderer; // 유령 플랫폼 외형 상태

        [Networked] // Ghost Platform 상태 동기화
        private int NetworkGhostStateValue
        {
            get;
            set;
        }

        [Networked] // Ghost Platform 투명도 동기화
        private float NetworkGhostAlpha
        {
            get;
            set;
        }

        public GhostPlatformState GhostState =>
            (GhostPlatformState)NetworkGhostStateValue; // 현재 Ghost 상태 조회

        public override void Spawned()
        {
            ResolveReferences(); // 기존 플랫폼 참조 준비
            ConfigureAuthorityMode(); // Host만 기존 이동 로직 실행

            if (
                Object.HasStateAuthority &&
                ghostPlatform != null
            )
            {
                CaptureGhostAuthorityState(); // 최초 Ghost 상태 저장
            }
        }

        public override void FixedUpdateNetwork()
        {
            ResolveReferences(); // 런타임 참조 보정
            ConfigureAuthorityMode(); // 권한별 기존 스크립트 상태 유지

            if (
                Object.HasStateAuthority &&
                ghostPlatform != null
            )
            {
                CaptureGhostAuthorityState(); // Host 상태를 Networked 값으로 저장
            }
        }

        private void Update()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                Object.HasStateAuthority ||
                ghostPlatform == null
            )
            {
                return; // Proxy Ghost만 표현 보정
            }

            ApplyGhostProxyState(); // Host에서 받은 상태 적용
        }

        private void ConfigureAuthorityMode()
        {
            if (Object == null || !Object.IsValid)
            {
                return; // NetworkObject 준비 전 처리
            }

            bool runAuthorityLogic = Object.HasStateAuthority; // Host 실행 여부

            if (movingPlatform != null)
            {
                movingPlatform.enabled = runAuthorityLogic; // Moving은 Host만 계산
            }

            if (rotatingPlatform != null)
            {
                rotatingPlatform.enabled = runAuthorityLogic; // Rotating은 Host만 계산
            }

            if (ghostPlatform != null)
            {
                ghostPlatform.enabled = runAuthorityLogic; // Ghost 주기는 Host만 계산
            }
        }

        private void CaptureGhostAuthorityState()
        {
            NetworkGhostStateValue =
                (int)ghostPlatform.CurrentState; // 현재 Ghost 상태 저장

            NetworkGhostAlpha =
                ReadRendererAlpha(ghostRenderer); // Host 투명도 저장
        }

        private void ApplyGhostProxyState()
        {
            GhostPlatformState state =
                (GhostPlatformState)NetworkGhostStateValue; // 동기화 상태 조회

            bool hidden =
                state == GhostPlatformState.Hidden; // 숨김 여부 계산

            if (ghostCollider != null)
            {
                ghostCollider.enabled = !hidden; // Collider 상태 동기화
            }

            if (ghostRenderer == null)
            {
                return; // Renderer 없음 처리
            }

            ghostRenderer.enabled = !hidden; // Renderer 표시 상태 동기화

            if (!hidden)
            {
                WriteRendererAlpha(
                    ghostRenderer,
                    Mathf.Clamp01(NetworkGhostAlpha)
                ); // Warning Fade를 Host 값으로 동기화
            }
        }

        private void ResolveReferences()
        {
            if (movingPlatform == null)
            {
                movingPlatform = GetComponent<MovingPlatform>(); // Moving 탐색
            }

            if (rotatingPlatform == null)
            {
                rotatingPlatform = GetComponent<RotatingPlatform>(); // Rotating 탐색
            }

            if (ghostPlatform == null)
            {
                ghostPlatform = GetComponent<GhostPlatform>(); // Ghost 탐색
            }

            if (
                ghostPlatform != null &&
                ghostCollider == null
            )
            {
                ghostCollider = GetComponent<BoxCollider>(); // Ghost Collider 탐색
            }

            if (
                ghostPlatform != null &&
                ghostRenderer == null
            )
            {
                ghostRenderer = GetComponent<Renderer>(); // Root Renderer 우선 탐색

                if (ghostRenderer == null)
                {
                    ghostRenderer = GetComponentInChildren<Renderer>(true); // 자식 Renderer 보정
                }
            }
        }

        private static float ReadRendererAlpha(Renderer renderer)
        {
            if (
                renderer == null ||
                renderer.sharedMaterial == null
            )
            {
                return 1f; // Material 없음 기본값
            }

            Material material = renderer.sharedMaterial; // Runtime Material 조회

            if (material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor").a; // URP BaseColor Alpha 반환
            }

            if (material.HasProperty("_Color"))
            {
                return material.GetColor("_Color").a; // 일반 Color Alpha 반환
            }

            return 1f; // Alpha 속성 없음 처리
        }

        private static void WriteRendererAlpha(
            Renderer renderer,
            float alpha
        )
        {
            if (
                renderer == null ||
                renderer.sharedMaterial == null
            )
            {
                return; // Material 없음 처리
            }

            Material material = renderer.sharedMaterial; // Proxy Runtime Material 조회

            if (material.HasProperty("_BaseColor"))
            {
                Color color = material.GetColor("_BaseColor"); // 기존 색상 조회
                color.a = alpha; // 동기화 Alpha 적용
                material.SetColor("_BaseColor", color); // URP 색상 갱신
                return;
            }

            if (material.HasProperty("_Color"))
            {
                Color color = material.GetColor("_Color"); // 기존 색상 조회
                color.a = alpha; // 동기화 Alpha 적용
                material.SetColor("_Color", color); // 일반 색상 갱신
            }
        }
    }
}

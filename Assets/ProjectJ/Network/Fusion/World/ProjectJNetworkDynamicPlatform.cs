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
        private const int MaxNetworkPassengerColliders = 32; // 플랫폼 위 Network Player 탐색 한도
        private const float NetworkPassengerProbeHeight = 0.45f; // 플랫폼 상단 승객 탐색 높이
        private const float NetworkPassengerHorizontalScale = 0.94f; // 가장자리 오탐 방지 범위
        private MovingPlatform movingPlatform; // 기존 이동 플랫폼
        private RotatingPlatform rotatingPlatform; // 기존 회전 플랫폼
        private GhostPlatform ghostPlatform; // 기존 유령 플랫폼
        private BoxCollider platformCollider; // Network Player 승객 탐색 Collider
        private BoxCollider ghostCollider; // 유령 플랫폼 충돌 상태
        private Renderer ghostRenderer; // 유령 플랫폼 외형 상태

        private readonly Collider[] networkPassengerOverlaps =
            new Collider[MaxNetworkPassengerColliders]; // 승객 탐색 결과 재사용

        private readonly ProjectJNetworkPlayer[] movedNetworkPassengers =
            new ProjectJNetworkPlayer[MaxNetworkPassengerColliders]; // 동일 Player 중복 이동 방지

        private bool hasAuthorityPose; // 이전 Host 플랫폼 Pose 존재 여부
        private Vector3 previousAuthorityPosition; // 이전 Host 플랫폼 위치
        private Quaternion previousAuthorityRotation; // 이전 Host 플랫폼 회전

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

        public int PassengerCarryCount
        {
            get;
            private set;
        } // 누적 Network Player 운반 횟수

        public int LastPassengerCount
        {
            get;
            private set;
        } // 마지막 Tick 운반 인원

        public float LastPlatformDeltaDistance
        {
            get;
            private set;
        } // 마지막 플랫폼 이동 거리

        public override void Spawned()
        {
            ResolveReferences(); // 기존 플랫폼 참조 준비
            ConfigureAuthorityMode(); // Host만 기존 이동 로직 실행

            if (Object.HasStateAuthority)
            {
                CaptureAuthorityPose(); // 플랫폼 승객 이동 기준 Pose 저장

                if (ghostPlatform != null)
                {
                    CaptureGhostAuthorityState(); // 최초 Ghost 상태 저장
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            ResolveReferences(); // 런타임 참조 보정
            ConfigureAuthorityMode(); // 권한별 기존 스크립트 상태 유지

            if (!Object.HasStateAuthority)
            {
                return; // 플랫폼 이동·승객 판정은 Host만 실행
            }

            CarryNetworkPassengersAuthority(); // Rigidbody 없는 Fusion Player도 플랫폼 이동 상속

            if (ghostPlatform != null)
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

        private void CaptureAuthorityPose()
        {
            previousAuthorityPosition = transform.position; // 현재 위치 저장
            previousAuthorityRotation = transform.rotation; // 현재 회전 저장
            hasAuthorityPose = true; // 다음 Tick부터 Delta 계산 허용
        }

        private void CarryNetworkPassengersAuthority()
        {
            Vector3 currentPosition = transform.position; // 현재 Host 플랫폼 위치
            Quaternion currentRotation = transform.rotation; // 현재 Host 플랫폼 회전

            if (!hasAuthorityPose)
            {
                previousAuthorityPosition = currentPosition; // 첫 Tick 위치 저장
                previousAuthorityRotation = currentRotation; // 첫 Tick 회전 저장
                hasAuthorityPose = true;
                LastPassengerCount = 0;
                LastPlatformDeltaDistance = 0f;
                return;
            }

            Vector3 positionDelta =
                currentPosition - previousAuthorityPosition; // 위치 변화량

            float rotationDelta =
                Quaternion.Angle(
                    previousAuthorityRotation,
                    currentRotation
                ); // 회전 변화량

            LastPlatformDeltaDistance =
                positionDelta.magnitude; // 디버그용 이동 거리

            if (
                positionDelta.sqrMagnitude <= 0.0000001f &&
                rotationDelta <= 0.001f
            )
            {
                LastPassengerCount = 0;
                previousAuthorityPosition = currentPosition;
                previousAuthorityRotation = currentRotation;
                return; // 플랫폼 Pose 변화가 없으면 승객 이동 불필요
            }

            LastPassengerCount =
                MoveNetworkPassengersAuthority(
                    previousAuthorityPosition,
                    previousAuthorityRotation,
                    currentPosition,
                    currentRotation
                ); // 플랫폼 Pose Delta를 승객에게 전달

            PassengerCarryCount +=
                LastPassengerCount; // 누적 운반 횟수 기록

            previousAuthorityPosition = currentPosition; // 다음 Tick 기준 갱신
            previousAuthorityRotation = currentRotation;
        }

        private int MoveNetworkPassengersAuthority(
            Vector3 oldPlatformPosition,
            Quaternion oldPlatformRotation,
            Vector3 newPlatformPosition,
            Quaternion newPlatformRotation
        )
        {
            if (
                platformCollider == null ||
                !platformCollider.enabled
            )
            {
                return 0; // Collider가 없거나 Ghost Hidden이면 승객 없음
            }

            Vector3 worldCenter =
                transform.TransformPoint(
                    platformCollider.center
                ); // Collider 중심을 월드 좌표로 변환

            Vector3 lossyScale = transform.lossyScale;

            Vector3 scaledSize =
                new Vector3(
                    Mathf.Abs(
                        platformCollider.size.x *
                        lossyScale.x
                    ),
                    Mathf.Abs(
                        platformCollider.size.y *
                        lossyScale.y
                    ),
                    Mathf.Abs(
                        platformCollider.size.z *
                        lossyScale.z
                    )
                ); // 실제 월드 크기 계산

            Vector3 halfExtents =
                scaledSize * 0.5f;

            Vector3 probeCenter =
                worldCenter +
                transform.up *
                (
                    halfExtents.y +
                    NetworkPassengerProbeHeight * 0.5f
                ); // 플랫폼 윗면 바로 위를 탐색

            Vector3 probeHalfExtents =
                new Vector3(
                    halfExtents.x *
                        NetworkPassengerHorizontalScale,
                    NetworkPassengerProbeHeight * 0.5f,
                    halfExtents.z *
                        NetworkPassengerHorizontalScale
                );

            int overlapCount =
                Physics.OverlapBoxNonAlloc(
                    probeCenter,
                    probeHalfExtents,
                    networkPassengerOverlaps,
                    transform.rotation,
                    Physics.AllLayers,
                    QueryTriggerInteraction.Ignore
                ); // Network Player Layer와 무관하게 탐색

            int movedCount = 0;

            for (
                int index = 0;
                index < overlapCount;
                index++
            )
            {
                Collider overlap =
                    networkPassengerOverlaps[index];

                networkPassengerOverlaps[index] = null;

                if (overlap == null)
                {
                    continue;
                }

                ProjectJNetworkPlayer player =
                    overlap.GetComponentInParent<
                        ProjectJNetworkPlayer
                    >();

                if (
                    player == null ||
                    ContainsMovedPlayer(
                        movedCount,
                        player
                    )
                )
                {
                    continue; // Player가 아니거나 동일 Player 중복 Collider
                }

                movedNetworkPassengers[movedCount] =
                    player;

                Vector3 targetPosition =
                    PlatformPassengerCarrier
                        .CalculatePassengerPosition(
                            player.transform.position,
                            oldPlatformPosition,
                            oldPlatformRotation,
                            newPlatformPosition,
                            newPlatformRotation
                        ); // 이동·회전 플랫폼의 월드 Delta 적용

                if (
                    player
                        .ApplyPlatformPassengerPositionAuthority(
                            targetPosition
                        )
                )
                {
                    movedCount++;
                }
                else
                {
                    movedNetworkPassengers[movedCount] = null;
                }

                if (
                    movedCount >=
                    movedNetworkPassengers.Length
                )
                {
                    break;
                }
            }

            ClearMovedPassengerCache(
                movedCount
            );

            return movedCount;
        }

        private bool ContainsMovedPlayer(
            int movedCount,
            ProjectJNetworkPlayer player
        )
        {
            for (
                int index = 0;
                index < movedCount;
                index++
            )
            {
                if (
                    movedNetworkPassengers[index] ==
                    player
                )
                {
                    return true;
                }
            }

            return false;
        }

        private void ClearMovedPassengerCache(
            int movedCount
        )
        {
            for (
                int index = 0;
                index < movedCount;
                index++
            )
            {
                movedNetworkPassengers[index] =
                    null;
            }
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

            if (platformCollider == null)
            {
                platformCollider = GetComponent<BoxCollider>(); // 승객 탐색 Collider 조회
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

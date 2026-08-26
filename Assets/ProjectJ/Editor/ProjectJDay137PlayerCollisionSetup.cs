using System.IO; // Player Source 읽기와 저장 사용
using System.Text; // UTF8 저장 사용
using UnityEditor; // Editor 메뉴와 Asset 갱신 사용
using UnityEngine; // Debug 출력 사용

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay137PlayerCollisionSetup
    {
        private const string NetworkPlayerSourcePath =
            "Assets/ProjectJ/Network/Fusion/Player/ProjectJNetworkPlayer.cs"; // 수정 대상 Player Source 경로

        private static readonly UTF8Encoding Utf8WithoutBom =
            new UTF8Encoding(
                false
            ); // BOM 없는 UTF8 저장

        [MenuItem(
            "Project J/Day137/Apply Player Collision Fix"
        )]
        private static void ApplyPlayerCollisionFix()
        {
            if (!File.Exists(NetworkPlayerSourcePath))
            {
                Debug.LogError(
                    "[Project J/Day137] ProjectJNetworkPlayer.cs를 찾지 못했습니다. / " +
                    NetworkPlayerSourcePath
                ); // Player Source 누락 오류 출력

                return;
            }

            string source =
                File.ReadAllText(
                    NetworkPlayerSourcePath
                ); // 최신 Player Source 읽기

            string newline =
                source.Contains(
                    "\r\n"
                )
                    ? "\r\n"
                    : "\n"; // 기존 줄바꿈 형식 확인

            if (!PatchUsing(ref source, newline))
            {
                LogPatternMismatch();
                return;
            }

            if (!PatchConstants(ref source, newline))
            {
                LogPatternMismatch();
                return;
            }

            if (!PatchBuffers(ref source, newline))
            {
                LogPatternMismatch();
                return;
            }

            if (!PatchHorizontalMovement(ref source, newline))
            {
                LogPatternMismatch();
                return;
            }

            if (!PatchLandingCondition(ref source, newline))
            {
                LogPatternMismatch();
                return;
            }

            if (!PatchCollisionHelpers(ref source, newline))
            {
                LogPatternMismatch();
                return;
            }

            File.WriteAllText(
                NetworkPlayerSourcePath,
                source,
                Utf8WithoutBom
            ); // 충돌 수정 Player Source 저장

            AssetDatabase.Refresh(); // Unity Source 재컴파일 요청

            Debug.Log(
                "[Project J/Day137] Player 수평 충돌·Wall Slide·계단 Step Up 패치 적용 완료."
            ); // 적용 결과 출력
        }

        private static bool PatchUsing(
            ref string source,
            string newline
        )
        {
            string movementUsing =
                "using ProjectJ.Movement; // Player 충돌 이동 정책 사용"; // 신규 Movement Using 내용

            if (source.Contains(movementUsing))
            {
                return true; // 이미 Using 적용됨
            }

            string itemsUsing =
                "using ProjectJ.Items; // 깃털 신발 수치 정책 사용"; // 기존 Items Using 기준

            if (!source.Contains(itemsUsing))
            {
                return false; // 최신 Using 패턴 불일치
            }

            source =
                source.Replace(
                    itemsUsing,
                    itemsUsing +
                    newline +
                    movementUsing
                ); // Movement 정책 Using 추가

            return true; // Using 패치 성공
        }

        private static bool PatchConstants(
            ref string source,
            string newline
        )
        {
            if (
                source.Contains(
                    "private const float HorizontalCollisionSkin ="
                )
            )
            {
                return true; // 이미 충돌 상수 적용됨
            }

            string anchor =
                "        private const float StandClearanceRadiusScale = 0.95f;"; // 기존 자세 여유 상수 기준

            if (!source.Contains(anchor))
            {
                return false; // 최신 상수 패턴 불일치
            }

            string addition =
                anchor +
                newline +
                "        private const float HorizontalCollisionSkin =" +
                newline +
                "            0.03f; // 수평 충돌 Query 여유 거리" +
                newline +
                "        private const float MaximumStepHeight =" +
                newline +
                "            0.35f; // 자동 계단 오르기 최대 높이" +
                newline +
                "        private const float StepForwardProbeDistance =" +
                newline +
                "            0.05f; // 계단 상단 확인 전방 여유 거리" +
                newline +
                "        private const float GroundProbeRadiusScale =" +
                newline +
                "            0.9f; // Capsule 발바닥 Ground 검사 반경 배율" +
                newline +
                "        private const float MinimumGroundNormalY =" +
                newline +
                "            0.5f; // Ground로 허용할 최소 위쪽 법선" +
                newline +
                "        private const int HorizontalSlideIterationCount =" +
                newline +
                "            2; // 벽과 모서리 Slide 반복 횟수"; // 신규 충돌 상수 묶음

            source =
                source.Replace(
                    anchor,
                    addition
                ); // Player 충돌 상수 추가

            return true; // 상수 패치 성공
        }

        private static bool PatchBuffers(
            ref string source,
            string newline
        )
        {
            if (
                source.Contains(
                    "private readonly RaycastHit[] movementHitBuffer"
                )
            )
            {
                return true; // 이미 충돌 버퍼 적용됨
            }

            string anchor =
                "        private readonly Collider[] standOverlapBuffer = new Collider[16];"; // 기존 Overlap Buffer 기준

            if (!source.Contains(anchor))
            {
                return false; // 최신 Buffer 패턴 불일치
            }

            string addition =
                anchor +
                newline +
                "        private readonly RaycastHit[] movementHitBuffer = new RaycastHit[16]; // 수평 CapsuleCast와 계단 Raycast 버퍼" +
                newline +
                "        private readonly Collider[] movementOverlapBuffer = new Collider[16]; // 이동 위치 Overlap 검사 버퍼"; // 신규 충돌 버퍼 묶음

            source =
                source.Replace(
                    anchor,
                    addition
                ); // 충돌 Query 버퍼 추가

            return true; // Buffer 패치 성공
        }

        private static bool PatchHorizontalMovement(
            ref string source,
            string newline
        )
        {
            if (
                source.Contains(
                    "ResolveHorizontalMovement(" + newline +
                    "                    currentPosition,"
                )
            )
            {
                return true; // 이미 수평 충돌 이동 적용됨
            }

            string startMarker =
                "            Vector3 currentPosition = transform.position;" +
                newline +
                "            Vector3 nextPosition = currentPosition;" +
                newline +
                newline +
                "            nextPosition +="; // 기존 수평 이동 시작 패턴

            int startIndex =
                source.IndexOf(
                    startMarker,
                    System.StringComparison.Ordinal
                ); // 기존 수평 이동 시작 위치 검색

            if (startIndex < 0)
            {
                return false; // 최신 수평 이동 시작 패턴 불일치
            }

            string endMarker =
                newline +
                newline +
                "            if (moveDirection.sqrMagnitude > 0.0001f)"; // 회전 처리 시작 패턴

            int endIndex =
                source.IndexOf(
                    endMarker,
                    startIndex,
                    System.StringComparison.Ordinal
                ); // 기존 수평 이동 종료 위치 검색

            if (endIndex < 0)
            {
                return false; // 최신 수평 이동 종료 패턴 불일치
            }

            string replacement =
                "            Vector3 currentPosition = transform.position;" +
                newline +
                "            Vector3 horizontalDisplacement =" +
                newline +
                "                moveDirection *" +
                newline +
                "                horizontalMoveSpeed *" +
                newline +
                "                deltaTime; // 이번 Tick 수평 이동량 계산" +
                newline +
                newline +
                "            Vector3 nextPosition =" +
                newline +
                "                ResolveHorizontalMovement(" +
                newline +
                "                    currentPosition," +
                newline +
                "                    horizontalDisplacement," +
                newline +
                "                    NetworkGrounded," +
                newline +
                "                    out bool steppedUp" +
                newline +
                "                ); // CapsuleCast 기반 벽 충돌·Slide·계단 이동 계산" +
                newline +
                newline +
                "            if (steppedUp)" +
                newline +
                "            {" +
                newline +
                "                NetworkVerticalVelocity = 0f; // 계단 상승 중 낙하 속도 제거" +
                newline +
                "                NetworkGrounded = true; // 계단 상승 직후 Ground 유지" +
                newline +
                "            }" +
                newline +
                newline +
                "            nextPosition.y +=" +
                newline +
                "                NetworkVerticalVelocity *" +
                newline +
                "                deltaTime; // 수직 이동 적용"; // 신규 충돌 이동 블록

            source =
                source.Substring(
                    0,
                    startIndex
                ) +
                replacement +
                source.Substring(
                    endIndex
                ); // 기존 Transform 직접 수평 이동 교체

            return true; // 수평 이동 패치 성공
        }

        private static bool PatchLandingCondition(
            ref string source,
            string newline
        )
        {
            string patchedMarker =
                "            if (" +
                newline +
                "                !steppedUp &&" +
                newline +
                "                NetworkVerticalVelocity <= 0f &&"; // 신규 착지 조건 패턴

            if (source.Contains(patchedMarker))
            {
                return true; // 이미 Step Up 착지 보호 적용됨
            }

            string oldMarker =
                "            if (" +
                newline +
                "                NetworkVerticalVelocity <= 0f &&" +
                newline +
                "                TryGetLandingGroundHeight(currentPosition, nextPosition, out float landingHeight)"; // 기존 착지 조건 패턴

            if (!source.Contains(oldMarker))
            {
                return false; // 최신 착지 조건 패턴 불일치
            }

            string replacement =
                "            if (" +
                newline +
                "                !steppedUp &&" +
                newline +
                "                NetworkVerticalVelocity <= 0f &&" +
                newline +
                "                TryGetLandingGroundHeight(currentPosition, nextPosition, out float landingHeight)"; // Step Up 직후 하단 바닥 재스냅 차단

            source =
                source.Replace(
                    oldMarker,
                    replacement
                ); // 착지 조건에 Step Up 보호 추가

            return true; // 착지 조건 패치 성공
        }

        private static bool PatchCollisionHelpers(
            ref string source,
            string newline
        )
        {
            if (
                source.Contains(
                    "        private Vector3 ResolveHorizontalMovement("
                ) &&
                source.Contains(
                    "        private bool TryFindClosestBodyHit("
                )
            )
            {
                return true; // 이미 충돌 Helper 적용됨
            }

            string startMarker =
                "        private bool TryGetGroundHeight("; // 기존 Ground Helper 시작 패턴

            int startIndex =
                source.IndexOf(
                    startMarker,
                    System.StringComparison.Ordinal
                ); // Ground Helper 시작 위치 검색

            if (startIndex < 0)
            {
                return false; // 최신 Ground Helper 시작 패턴 불일치
            }

            string endMarker =
                "        private bool IsJetpackCeilingBlocked("; // 기존 천장 Helper 시작 패턴

            int endIndex =
                source.IndexOf(
                    endMarker,
                    startIndex,
                    System.StringComparison.Ordinal
                ); // Ground Helper 종료 위치 검색

            if (endIndex < 0)
            {
                return false; // 최신 Ground Helper 종료 패턴 불일치
            }

            string replacement =
                BuildCollisionHelperBlock(
                    newline
                ); // 신규 충돌 Helper 묶음 생성

            source =
                source.Substring(
                    0,
                    startIndex
                ) +
                replacement +
                source.Substring(
                    endIndex
                ); // Ground Helper를 Capsule 기반 충돌 구조로 교체

            return true; // 충돌 Helper 패치 성공
        }

        private static string BuildCollisionHelperBlock(
            string newline
        )
        {
            string block = @"        private Vector3 ResolveHorizontalMovement(
            Vector3 startPosition,
            Vector3 horizontalDisplacement,
            bool canStep,
            out bool steppedUp
        )
        {
            steppedUp =
                false; // Step Up 결과 초기화

            if (
                horizontalDisplacement.sqrMagnitude <=
                0.0001f
            )
            {
                return startPosition; // 수평 이동 없음 처리
            }

            Vector3 position =
                startPosition; // 충돌 계산 시작 위치

            Vector3 remainingDisplacement =
                horizontalDisplacement; // 남은 수평 이동량 초기화

            bool stepAttemptAvailable =
                canStep &&
                NetworkVerticalVelocity <= 0f; // Ground 상태에서만 Step Up 허용

            for (
                int iteration = 0;
                iteration < HorizontalSlideIterationCount;
                iteration++
            )
            {
                if (
                    remainingDisplacement.sqrMagnitude <=
                    0.0001f
                )
                {
                    break; // 남은 이동 없음 처리
                }

                if (
                    !TryFindClosestBodyHit(
                        position,
                        remainingDisplacement,
                        out RaycastHit blockingHit
                    )
                )
                {
                    position +=
                        remainingDisplacement; // 충돌 없음 전체 이동 적용

                    break; // 수평 이동 종료
                }

                float requestedDistance =
                    remainingDisplacement.magnitude; // 현재 이동 요청 거리

                if (
                    blockingHit.distance >
                    requestedDistance
                )
                {
                    position +=
                        remainingDisplacement; // 실제 이동 범위 밖 충돌 무시

                    break; // 수평 이동 종료
                }

                if (
                    stepAttemptAvailable &&
                    TryResolveStepUp(
                        position,
                        remainingDisplacement,
                        blockingHit,
                        out Vector3 stepPosition
                    )
                )
                {
                    position =
                        stepPosition; // 계단 위 위치 적용

                    steppedUp =
                        true; // Step Up 성공 표시

                    break; // 계단 상승 후 이동 종료
                }

                Vector3 moveDirection =
                    remainingDisplacement.normalized; // 현재 이동 방향 계산

                float travelDistance =
                    ProjectJCharacterCollisionPolicy.ResolveTravelDistance(
                        requestedDistance,
                        blockingHit.distance
                    ); // 벽 앞 허용 이동 거리 계산

                position +=
                    moveDirection *
                    travelDistance; // 벽 앞까지 이동

                Vector3 consumedDisplacement =
                    moveDirection *
                    travelDistance; // 소비된 이동량 계산

                Vector3 leftoverDisplacement =
                    remainingDisplacement -
                    consumedDisplacement; // 충돌 후 남은 이동량 계산

                remainingDisplacement =
                    ProjectJCharacterCollisionPolicy.ResolveSlideDisplacement(
                        leftoverDisplacement,
                        blockingHit.normal
                    ); // 벽 접선 방향 Slide 계산

                stepAttemptAvailable =
                    false; // Slide 중 추가 Step Up 차단
            }

            return position; // 충돌 해결 수평 위치 반환
        }

        private bool TryResolveStepUp(
            Vector3 startPosition,
            Vector3 horizontalDisplacement,
            RaycastHit blockingHit,
            out Vector3 stepPosition
        )
        {
            stepPosition =
                startPosition; // 실패 기본 위치 설정

            if (
                bodyCollider == null ||
                horizontalDisplacement.sqrMagnitude <=
                0.0001f
            )
            {
                return false; // Collider 또는 이동 없음 처리
            }

            Vector3 moveDirection =
                horizontalDisplacement.normalized; // 계단 접근 방향 계산

            Vector3 stepProbePosition =
                blockingHit.point +
                moveDirection *
                StepForwardProbeDistance; // 충돌 면 바로 뒤 계단 상단 검사 위치

            Vector3 stepProbeOrigin =
                new Vector3(
                    stepProbePosition.x,
                    startPosition.y +
                    MaximumStepHeight +
                    GroundProbeStartHeight,
                    stepProbePosition.z
                ); // 최대 Step 높이 위 Raycast 시작점

            float stepProbeDistance =
                MaximumStepHeight +
                GroundProbeStartHeight +
                GroundProbeDistance; // 계단 상단 하향 검사 거리

            if (
                !TryFindRayGroundHit(
                    stepProbeOrigin,
                    stepProbeDistance,
                    out float stepGroundHeight
                )
            )
            {
                return false; // 계단 상단 없음 처리
            }

            if (
                !ProjectJCharacterCollisionPolicy.IsStepHeightAllowed(
                    startPosition.y,
                    stepGroundHeight,
                    MaximumStepHeight
                )
            )
            {
                return false; // 너무 높은 발판 자동 오르기 차단
            }

            float stepHeight =
                stepGroundHeight -
                startPosition.y; // 실제 Step 상승 높이 계산

            Vector3 raisedStartPosition =
                startPosition +
                Vector3.up *
                stepHeight; // 계단 상단 높이로 Player 상승

            if (
                !IsBodyPositionClear(
                    raisedStartPosition
                )
            )
            {
                return false; // 상승 위치 몸통 충돌 차단
            }

            if (
                TryFindClosestBodyHit(
                    raisedStartPosition,
                    horizontalDisplacement,
                    out RaycastHit raisedHit
                ) &&
                raisedHit.distance <=
                horizontalDisplacement.magnitude
            )
            {
                return false; // 계단 위 이동 경로 추가 장애물 차단
            }

            Vector3 candidatePosition =
                raisedStartPosition +
                horizontalDisplacement; // 계단 상승 후 수평 후보 위치

            if (
                !IsBodyPositionClear(
                    candidatePosition
                )
            )
            {
                return false; // 계단 위 최종 위치 Overlap 차단
            }

            stepPosition =
                candidatePosition; // 안전한 Step Up 위치 반환

            return true; // 계단 상승 허용
        }

        private bool TryFindClosestBodyHit(
            Vector3 footPosition,
            Vector3 displacement,
            out RaycastHit closestHit
        )
        {
            closestHit =
                default; // 충돌 결과 초기화

            float distance =
                displacement.magnitude; // 이동 거리 계산

            if (distance <= 0.0001f)
            {
                return false; // 이동 없음 처리
            }

            GetBodyCastCapsule(
                footPosition,
                out Vector3 bottomPoint,
                out Vector3 topPoint,
                out float queryRadius
            ); // 현재 몸통 Capsule Query 계산

            Vector3 direction =
                displacement /
                distance; // 수평 이동 방향 정규화

            int hitCount =
                Physics.CapsuleCastNonAlloc(
                    bottomPoint,
                    topPoint,
                    queryRadius,
                    direction,
                    movementHitBuffer,
                    distance +
                    HorizontalCollisionSkin,
                    Physics.AllLayers,
                    QueryTriggerInteraction.Ignore
                ); // 수평 CapsuleCast 실행

            float closestDistance =
                float.PositiveInfinity; // 최근접 충돌 거리 초기화

            bool foundHit =
                false; // 외부 충돌 발견 여부 초기화

            for (
                int index = 0;
                index < hitCount;
                index++
            )
            {
                RaycastHit hit =
                    movementHitBuffer[index]; // 현재 CapsuleCast 결과 조회

                Collider hitCollider =
                    hit.collider; // 현재 충돌 Collider 조회

                if (
                    hitCollider == null ||
                    IsOwnCollider(
                        hitCollider
                    ) ||
                    hit.distance >=
                    closestDistance
                )
                {
                    continue; // 자기 Collider와 더 먼 충돌 제외
                }

                closestDistance =
                    hit.distance; // 최근접 거리 갱신

                closestHit =
                    hit; // 최근접 충돌 결과 갱신

                foundHit =
                    true; // 외부 충돌 발견 표시
            }

            return foundHit; // 최근접 외부 Collider 충돌 여부 반환
        }

        private bool IsBodyPositionClear(
            Vector3 footPosition
        )
        {
            GetBodyCastCapsule(
                footPosition,
                out Vector3 bottomPoint,
                out Vector3 topPoint,
                out float queryRadius
            ); // 후보 위치 Capsule Query 계산

            int overlapCount =
                Physics.OverlapCapsuleNonAlloc(
                    bottomPoint,
                    topPoint,
                    queryRadius,
                    movementOverlapBuffer,
                    Physics.AllLayers,
                    QueryTriggerInteraction.Ignore
                ); // 후보 위치 몸통 Overlap 검사

            for (
                int index = 0;
                index < overlapCount;
                index++
            )
            {
                Collider candidate =
                    movementOverlapBuffer[index]; // 현재 Overlap Collider 조회

                if (
                    candidate == null ||
                    IsOwnCollider(
                        candidate
                    )
                )
                {
                    continue; // 자기 Collider 제외
                }

                return false; // 외부 Collider Overlap 발견
            }

            return true; // 후보 위치 몸통 공간 확보
        }

        private void GetBodyCastCapsule(
            Vector3 footPosition,
            out Vector3 bottomPoint,
            out Vector3 topPoint,
            out float queryRadius
        )
        {
            float colliderRadius =
                bodyCollider != null
                    ? bodyCollider.radius
                    : BodyColliderRadius; // 현재 몸통 반경 조회

            float colliderHeight =
                bodyCollider != null
                    ? bodyCollider.height
                    : (
                        NetworkIsCrouching
                            ? CrouchColliderHeight
                            : StandingColliderHeight
                    ); // 현재 몸통 높이 조회

            colliderRadius =
                Mathf.Max(
                    0.05f,
                    colliderRadius
                ); // 최소 몸통 반경 보정

            colliderHeight =
                Mathf.Max(
                    colliderRadius *
                    2f,
                    colliderHeight
                ); // Capsule 최소 높이 보정

            queryRadius =
                Mathf.Max(
                    0.02f,
                    colliderRadius -
                    HorizontalCollisionSkin
                ); // Ground 접촉 오검출 방지 Query 반경 축소

            bottomPoint =
                footPosition +
                Vector3.up *
                colliderRadius; // Capsule 하단 구 중심 계산

            topPoint =
                footPosition +
                Vector3.up *
                (
                    colliderHeight -
                    colliderRadius
                ); // Capsule 상단 구 중심 계산
        }

        private bool TryGetGroundHeight(
            Vector3 position,
            float probeDistance,
            out float groundHeight
        )
        {
            float probeRadius =
                GetGroundProbeRadius(); // 현재 Ground Sphere 반경 계산

            Vector3 origin =
                position +
                Vector3.up *
                (
                    probeRadius +
                    GroundProbeStartHeight
                ); // Player 발 위 Ground Sphere 시작점 계산

            float castDistance =
                GroundProbeStartHeight +
                Mathf.Max(
                    0f,
                    probeDistance
                ); // Ground 하향 검사 거리 계산

            return TryFindGroundHit(
                origin,
                probeRadius,
                castDistance,
                out groundHeight
            ); // Capsule 발바닥 범위 Ground 검사
        }

        private bool TryGetLandingGroundHeight(
            Vector3 currentPosition,
            Vector3 nextPosition,
            out float groundHeight
        )
        {
            float downwardTravel =
                Mathf.Max(
                    0f,
                    currentPosition.y -
                    nextPosition.y
                ); // 이번 Tick 하향 이동 거리 계산

            float probeRadius =
                GetGroundProbeRadius(); // 착지 Sphere 반경 계산

            Vector3 origin =
                new Vector3(
                    nextPosition.x,
                    currentPosition.y +
                    probeRadius +
                    GroundProbeStartHeight,
                    nextPosition.z
                ); // 현재 발 높이 기준 착지 Sphere 시작점

            float castDistance =
                GroundProbeStartHeight +
                downwardTravel +
                GroundProbeDistance; // 낙하 거리 포함 착지 검사 거리

            return TryFindGroundHit(
                origin,
                probeRadius,
                castDistance,
                out groundHeight
            ); // 이동 후 발바닥 범위 착지 검사
        }

        private bool TryFindGroundHit(
            Vector3 origin,
            float probeRadius,
            float castDistance,
            out float groundHeight
        )
        {
            int hitCount =
                Physics.SphereCastNonAlloc(
                    origin,
                    probeRadius,
                    Vector3.down,
                    groundHitBuffer,
                    castDistance,
                    Physics.AllLayers,
                    QueryTriggerInteraction.Ignore
                ); // 발바닥 SphereCast 실행

            float closestDistance =
                float.PositiveInfinity; // 최근접 Ground 거리 초기화

            bool foundGround =
                false; // Ground 발견 여부 초기화

            groundHeight =
                0f; // Ground 높이 초기화

            for (
                int index = 0;
                index < hitCount;
                index++
            )
            {
                RaycastHit hit =
                    groundHitBuffer[index]; // 현재 Ground 충돌 결과 조회

                Collider hitCollider =
                    hit.collider; // 현재 Ground Collider 조회

                if (
                    hitCollider == null ||
                    IsOwnCollider(
                        hitCollider
                    ) ||
                    !ProjectJCharacterCollisionPolicy.IsWalkableGroundNormal(
                        hit.normal,
                        MinimumGroundNormalY
                    ) ||
                    hit.distance >=
                    closestDistance
                )
                {
                    continue; // 자기 Collider·수직 벽·더 먼 Ground 제외
                }

                closestDistance =
                    hit.distance; // 최근접 Ground 거리 갱신

                groundHeight =
                    hit.point.y; // Ground 표면 높이 저장

                foundGround =
                    true; // Ground 발견 표시
            }

            return foundGround; // Ground 존재 여부 반환
        }

        private bool TryFindRayGroundHit(
            Vector3 origin,
            float castDistance,
            out float groundHeight
        )
        {
            int hitCount =
                Physics.RaycastNonAlloc(
                    origin,
                    Vector3.down,
                    movementHitBuffer,
                    castDistance,
                    Physics.AllLayers,
                    QueryTriggerInteraction.Ignore
                ); // 계단 상단 하향 Raycast 실행

            float closestDistance =
                float.PositiveInfinity; // 최근접 계단 상단 거리 초기화

            bool foundGround =
                false; // 계단 상단 발견 여부 초기화

            groundHeight =
                0f; // 계단 상단 높이 초기화

            for (
                int index = 0;
                index < hitCount;
                index++
            )
            {
                RaycastHit hit =
                    movementHitBuffer[index]; // 현재 계단 Raycast 결과 조회

                Collider hitCollider =
                    hit.collider; // 현재 계단 Collider 조회

                if (
                    hitCollider == null ||
                    IsOwnCollider(
                        hitCollider
                    ) ||
                    !ProjectJCharacterCollisionPolicy.IsWalkableGroundNormal(
                        hit.normal,
                        MinimumGroundNormalY
                    ) ||
                    hit.distance >=
                    closestDistance
                )
                {
                    continue; // 자기 Collider·수직 면·더 먼 상단 제외
                }

                closestDistance =
                    hit.distance; // 최근접 계단 거리 갱신

                groundHeight =
                    hit.point.y; // 계단 상단 높이 저장

                foundGround =
                    true; // 계단 상단 발견 표시
            }

            return foundGround; // 계단 상단 존재 여부 반환
        }

        private float GetGroundProbeRadius()
        {
            float colliderRadius =
                bodyCollider != null
                    ? bodyCollider.radius
                    : BodyColliderRadius; // 현재 발바닥 반경 기준 조회

            return Mathf.Max(
                0.05f,
                colliderRadius *
                GroundProbeRadiusScale
            ); // Capsule 발바닥 Ground 검사 반경 반환
        }

";
            return block.Replace(
                "\n",
                newline
            ); // 기존 Source 줄바꿈 형식 적용
        }

        private static void LogPatternMismatch()
        {
            Debug.LogError(
                "[Project J/Day137] 최신 main의 ProjectJNetworkPlayer.cs 패턴과 일치하지 않아 자동 수정을 중단했습니다."
            ); // Source 패턴 불일치 오류 출력
        }
    }
}

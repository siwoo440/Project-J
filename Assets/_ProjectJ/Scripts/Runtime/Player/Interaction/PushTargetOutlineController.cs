using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using UnityEngine; // Unity 렌더러와 경계 기능 참조
using UnityEngine.Rendering; // 그림자 표시 설정 참조

namespace ProjectJ.Player // 플레이어 상호작용 기능 네임스페이스 선언
{ // 밀치기 대상 외곽선 범위
    [DisallowMultipleComponent] // 대상 외곽선 컴포넌트 중복 방지
    [RequireComponent(typeof(PlayerPushController))] // 밀치기 대상 제공 컴포넌트 보장
    public sealed class PushTargetOutlineController : MonoBehaviour // 현재 밀치기 대상의 경계 외곽선 표시 컴포넌트 선언
    { // 대상 외곽선 표시 범위
        private const int OutlineLineCount = 6; // 위아래 사각형과 네 수직선 개수
        private const int BoxCornerCount = 8; // 경계 상자 꼭짓점 개수

        [SerializeField] private Material outlineMaterial; // 선택 외곽선 전용 재질
        [SerializeField] private Color readyColor = Color.cyan; // 밀치기 가능 상태 외곽선 색상
        [SerializeField] private Color cooldownColor = Color.gray; // 재사용 대기 상태 외곽선 색상
        [SerializeField, Min(0.001f)] private float lineWidth = 0.035f; // 외곽선 굵기
        [SerializeField, Min(0f)] private float boundsPadding = 0.08f; // 대상 경계 바깥쪽 표시 여유

        private readonly LineRenderer[] outlineLines = new LineRenderer[OutlineLineCount]; // 경계 상자 외곽선 렌더러 배열
        private readonly Vector3[] boxCorners = new Vector3[BoxCornerCount]; // 현재 대상 경계 꼭짓점 배열
        private PlayerPushController pushController; // 현재 밀치기 대상 제공자
        private Material runtimeMaterial; // 기본 재질이 없을 때 생성한 런타임 재질
        private bool ownsRuntimeMaterial; // 런타임 재질 소유 여부

        private void Awake() // 외곽선 참조와 선 렌더러 준비
        { // 외곽선 준비 범위
            pushController = GetComponent<PlayerPushController>(); // 같은 오브젝트의 밀치기 컴포넌트 조회

            if (!TryPrepareMaterial()) // 외곽선 재질 준비 결과 확인
            { // 재질 준비 실패 범위
                enabled = false; // 외곽선 기능 비활성화
                return; // 외곽선 준비 중단
            } // 재질 준비 실패 범위 종료

            CreateOutlineLines(); // 런타임 경계 외곽선 렌더러 생성
            SetOutlineVisible(false); // 시작 시 외곽선 숨김
        } // 외곽선 준비 범위 종료

        private void LateUpdate() // 밀치기 대상 선정 뒤 외곽선 위치 갱신
        { // 외곽선 프레임 갱신 범위
            ExternalForceReceiver currentTarget = pushController.CurrentTarget; // 현재 밀치기 가능 대상 조회

            if (currentTarget == null || !TryGetTargetBounds(currentTarget, out Bounds targetBounds)) // 대상과 표시 가능한 경계 확인
            { // 표시 대상 없음 범위
                SetOutlineVisible(false); // 모든 외곽선 숨김
                return; // 위치 갱신 생략
            } // 표시 대상 없음 범위 종료

            targetBounds.Expand(boundsPadding * 2f); // 모든 방향의 외곽선 여유 적용
            FillBoxCorners(targetBounds); // 현재 경계 기반 여덟 꼭짓점 계산
            UpdateOutlineColor(pushController.IsReady ? readyColor : cooldownColor); // 밀치기 가능 상태 기반 색상 적용
            UpdateOutlinePositions(); // 여섯 외곽선 위치 적용
            SetOutlineVisible(true); // 현재 대상 외곽선 표시
        } // 외곽선 프레임 갱신 범위 종료

        private void OnDestroy() // 런타임 외곽선 재질 정리
        { // 외곽선 정리 범위
            if (!ownsRuntimeMaterial || runtimeMaterial == null) // 직접 생성한 재질 존재 여부 확인
            { // 재질 정리 생략 범위
                return; // 재질 제거 생략
            } // 재질 정리 생략 범위 종료

            Destroy(runtimeMaterial); // 런타임 생성 재질 제거
            runtimeMaterial = null; // 제거된 재질 참조 초기화
        } // 외곽선 정리 범위 종료

        private bool TryPrepareMaterial() // Inspector 재질 또는 기본 스프라이트 재질 준비
        { // 외곽선 재질 준비 범위
            if (outlineMaterial != null) // Inspector 외곽선 재질 연결 확인
            { // Inspector 재질 사용 범위
                runtimeMaterial = outlineMaterial; // 연결된 외곽선 재질 사용
                ownsRuntimeMaterial = false; // 외부 재질 비소유 상태 저장
                return true; // 외곽선 재질 준비 성공 반환
            } // Inspector 재질 사용 범위 종료

            Shader fallbackShader = Shader.Find("Sprites/Default"); // 런타임 기본 선 표시 셰이더 검색

            if (fallbackShader == null) // 기본 셰이더 검색 실패 확인
            { // 기본 셰이더 없음 범위
                ProjectLog.Error(ProjectLogCategory.Gameplay, "외곽선 Material이 없고 Sprites/Default 셰이더도 찾지 못했습니다.", "PUSH_OUTLINE_MATERIAL_MISSING", this); // 외곽선 재질 누락 오류 출력
                return false; // 외곽선 재질 준비 실패 반환
            } // 기본 셰이더 없음 범위 종료

            runtimeMaterial = new Material(fallbackShader); // 기본 셰이더 기반 런타임 재질 생성
            runtimeMaterial.name = "Runtime Push Target Outline"; // 런타임 재질 식별 이름 지정
            ownsRuntimeMaterial = true; // 런타임 재질 소유 상태 저장
            return true; // 외곽선 재질 준비 성공 반환
        } // 외곽선 재질 준비 범위 종료

        private void CreateOutlineLines() // 경계 상자용 여섯 선 렌더러 생성
        { // 외곽선 렌더러 생성 범위
            for (int index = 0; index < OutlineLineCount; index++) // 필요한 선 렌더러 개수 순회
            { // 선 렌더러 생성 순회 범위
                GameObject lineObject = new GameObject($"PushTargetOutlineLine_{index}"); // 현재 외곽선 전용 자식 오브젝트 생성
                lineObject.transform.SetParent(transform, false); // 플레이어 아래 런타임 외곽선 오브젝트 배치
                LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>(); // 선 표시 컴포넌트 추가
                lineRenderer.useWorldSpace = true; // 월드 좌표 기반 경계 표시 설정
                lineRenderer.sharedMaterial = runtimeMaterial; // 준비된 외곽선 재질 연결
                lineRenderer.startWidth = lineWidth; // 선 시작 굵기 적용
                lineRenderer.endWidth = lineWidth; // 선 끝 굵기 적용
                lineRenderer.numCapVertices = 2; // 선 끝부분 둥근 처리 적용
                lineRenderer.numCornerVertices = 2; // 선 모서리 둥근 처리 적용
                lineRenderer.shadowCastingMode = ShadowCastingMode.Off; // 외곽선 그림자 생성 비활성화
                lineRenderer.receiveShadows = false; // 외곽선 그림자 수신 비활성화
                lineRenderer.lightProbeUsage = LightProbeUsage.Off; // 외곽선 라이트 프로브 사용 비활성화
                lineRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off; // 외곽선 반사 프로브 사용 비활성화
                lineRenderer.loop = index < 2; // 위아래 두 사각형만 순환 선으로 설정
                lineRenderer.positionCount = index < 2 ? 4 : 2; // 사각형과 수직선별 꼭짓점 개수 설정
                outlineLines[index] = lineRenderer; // 생성한 선 렌더러 배열 저장
            } // 선 렌더러 생성 순회 범위 종료
        } // 외곽선 렌더러 생성 범위 종료

        private bool TryGetTargetBounds(ExternalForceReceiver targetReceiver, out Bounds targetBounds) // 대상 렌더러와 충돌체 전체 경계 계산
        { // 대상 경계 계산 범위
            targetBounds = new Bounds(targetReceiver.ForceReceiverTransform.position, Vector3.zero); // 대상 위치 기반 빈 경계 초기화
            bool hasBounds = false; // 유효 경계 발견 여부 초기화
            Renderer[] targetRenderers = targetReceiver.GetComponentsInChildren<Renderer>(true); // 대상과 자식의 모든 렌더러 조회

            for (int index = 0; index < targetRenderers.Length; index++) // 대상 렌더러 순회
            { // 렌더러 경계 순회 범위
                Renderer targetRenderer = targetRenderers[index]; // 현재 대상 렌더러 조회

                if (targetRenderer == null || !targetRenderer.enabled || targetRenderer is LineRenderer) // 빈 렌더러와 비활성 렌더러와 선 렌더러 확인
                { // 렌더러 제외 범위
                    continue; // 현재 렌더러 경계 생략
                } // 렌더러 제외 범위 종료

                EncapsulateBounds(targetRenderer.bounds, ref targetBounds, ref hasBounds); // 현재 렌더러 경계를 전체 경계에 결합
            } // 렌더러 경계 순회 범위 종료

            if (hasBounds) // 렌더러 기반 경계 확보 여부 확인
            { // 렌더러 경계 사용 범위
                return true; // 대상 경계 계산 성공 반환
            } // 렌더러 경계 사용 범위 종료

            Collider[] targetColliders = targetReceiver.GetComponentsInChildren<Collider>(true); // 렌더러 없는 대상의 모든 충돌체 조회

            for (int index = 0; index < targetColliders.Length; index++) // 대상 충돌체 순회
            { // 충돌체 경계 순회 범위
                Collider targetCollider = targetColliders[index]; // 현재 대상 충돌체 조회

                if (targetCollider == null || !targetCollider.enabled) // 빈 충돌체와 비활성 충돌체 확인
                { // 충돌체 제외 범위
                    continue; // 현재 충돌체 경계 생략
                } // 충돌체 제외 범위 종료

                EncapsulateBounds(targetCollider.bounds, ref targetBounds, ref hasBounds); // 현재 충돌체 경계를 전체 경계에 결합
            } // 충돌체 경계 순회 범위 종료

            return hasBounds; // 렌더러 또는 충돌체 기반 경계 확보 여부 반환
        } // 대상 경계 계산 범위 종료

        private static void EncapsulateBounds(Bounds sourceBounds, ref Bounds combinedBounds, ref bool hasBounds) // 하나의 경계를 전체 경계에 결합
        { // 경계 결합 범위
            if (!hasBounds) // 첫 유효 경계 여부 확인
            { // 첫 경계 적용 범위
                combinedBounds = sourceBounds; // 첫 경계를 전체 경계로 적용
                hasBounds = true; // 유효 경계 존재 상태 저장
                return; // 첫 경계 결합 완료
            } // 첫 경계 적용 범위 종료

            combinedBounds.Encapsulate(sourceBounds); // 기존 전체 경계에 현재 경계 포함
        } // 경계 결합 범위 종료

        private void FillBoxCorners(Bounds bounds) // 축 정렬 경계 상자의 여덟 꼭짓점 계산
        { // 경계 꼭짓점 계산 범위
            Vector3 minimum = bounds.min; // 경계 최소 좌표 조회
            Vector3 maximum = bounds.max; // 경계 최대 좌표 조회
            boxCorners[0] = new Vector3(minimum.x, minimum.y, minimum.z); // 아래쪽 왼쪽 뒤 꼭짓점 저장
            boxCorners[1] = new Vector3(maximum.x, minimum.y, minimum.z); // 아래쪽 오른쪽 뒤 꼭짓점 저장
            boxCorners[2] = new Vector3(maximum.x, minimum.y, maximum.z); // 아래쪽 오른쪽 앞 꼭짓점 저장
            boxCorners[3] = new Vector3(minimum.x, minimum.y, maximum.z); // 아래쪽 왼쪽 앞 꼭짓점 저장
            boxCorners[4] = new Vector3(minimum.x, maximum.y, minimum.z); // 위쪽 왼쪽 뒤 꼭짓점 저장
            boxCorners[5] = new Vector3(maximum.x, maximum.y, minimum.z); // 위쪽 오른쪽 뒤 꼭짓점 저장
            boxCorners[6] = new Vector3(maximum.x, maximum.y, maximum.z); // 위쪽 오른쪽 앞 꼭짓점 저장
            boxCorners[7] = new Vector3(minimum.x, maximum.y, maximum.z); // 위쪽 왼쪽 앞 꼭짓점 저장
        } // 경계 꼭짓점 계산 범위 종료

        private void UpdateOutlinePositions() // 여섯 선 렌더러에 현재 경계 꼭짓점 적용
        { // 외곽선 위치 적용 범위
            SetLoopPositions(outlineLines[0], 0, 1, 2, 3); // 아래쪽 사각형 위치 적용
            SetLoopPositions(outlineLines[1], 4, 5, 6, 7); // 위쪽 사각형 위치 적용
            SetEdgePositions(outlineLines[2], 0, 4); // 왼쪽 뒤 수직선 위치 적용
            SetEdgePositions(outlineLines[3], 1, 5); // 오른쪽 뒤 수직선 위치 적용
            SetEdgePositions(outlineLines[4], 2, 6); // 오른쪽 앞 수직선 위치 적용
            SetEdgePositions(outlineLines[5], 3, 7); // 왼쪽 앞 수직선 위치 적용
        } // 외곽선 위치 적용 범위 종료

        private void SetLoopPositions(LineRenderer lineRenderer, int first, int second, int third, int fourth) // 사각형 선의 네 꼭짓점 적용
        { // 사각형 꼭짓점 적용 범위
            lineRenderer.SetPosition(0, boxCorners[first]); // 첫 번째 사각형 꼭짓점 적용
            lineRenderer.SetPosition(1, boxCorners[second]); // 두 번째 사각형 꼭짓점 적용
            lineRenderer.SetPosition(2, boxCorners[third]); // 세 번째 사각형 꼭짓점 적용
            lineRenderer.SetPosition(3, boxCorners[fourth]); // 네 번째 사각형 꼭짓점 적용
        } // 사각형 꼭짓점 적용 범위 종료

        private void SetEdgePositions(LineRenderer lineRenderer, int startCorner, int endCorner) // 수직선의 시작과 끝 꼭짓점 적용
        { // 수직선 꼭짓점 적용 범위
            lineRenderer.SetPosition(0, boxCorners[startCorner]); // 수직선 시작 꼭짓점 적용
            lineRenderer.SetPosition(1, boxCorners[endCorner]); // 수직선 끝 꼭짓점 적용
        } // 수직선 꼭짓점 적용 범위 종료

        private void UpdateOutlineColor(Color color) // 전체 외곽선 색상 적용
        { // 외곽선 색상 적용 범위
            for (int index = 0; index < outlineLines.Length; index++) // 모든 외곽선 렌더러 순회
            { // 외곽선 색상 순회 범위
                outlineLines[index].startColor = color; // 선 시작 색상 적용
                outlineLines[index].endColor = color; // 선 끝 색상 적용
            } // 외곽선 색상 순회 범위 종료
        } // 외곽선 색상 적용 범위 종료

        private void SetOutlineVisible(bool isVisible) // 전체 외곽선 표시 상태 적용
        { // 외곽선 표시 적용 범위
            for (int index = 0; index < outlineLines.Length; index++) // 모든 외곽선 렌더러 순회
            { // 외곽선 표시 순회 범위
                if (outlineLines[index] != null) // 생성된 선 렌더러 존재 확인
                { // 선 렌더러 존재 범위
                    outlineLines[index].enabled = isVisible; // 요청된 표시 상태 적용
                } // 선 렌더러 존재 범위 종료
            } // 외곽선 표시 순회 범위 종료
        } // 외곽선 표시 적용 범위 종료
    } // 대상 외곽선 표시 범위 종료
} // 밀치기 대상 외곽선 범위 종료

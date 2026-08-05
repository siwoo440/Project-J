using System.Collections.Generic; // 레이어 원본 목록 기능 참조
using UnityEngine; // Unity 충돌과 시간 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 보호 기능 묶음
    [DisallowMultipleComponent] // 보호 컴포넌트 중복 방지
    [RequireComponent(typeof(CharacterController))] // 캐릭터 충돌 컴포넌트 보장
    [RequireComponent(typeof(PlayerExternalForceController))] // 외부 힘 컴포넌트 보장
    public sealed class PlayerRespawnProtectionController : MonoBehaviour // 부활 직후 보호 관리 컴포넌트 선언
    { // 부활 보호 기능 묶음
        [SerializeField, Min(0f)] private float protectionDuration = 3f; // 부활 후 전체 보호 시간
        [SerializeField] private string playerLayerName = "Player"; // 평상시 플레이어 레이어 이름
        [SerializeField] private string protectionLayerName = "RespawnProtection"; // 보호 중 플레이어 레이어 이름

        private readonly Dictionary<GameObject, int> originalLayers = new Dictionary<GameObject, int>(); // 보호 전 오브젝트 레이어 저장소
        private float protectionRemaining; // 남은 부활 보호 시간
        private int playerLayer = -1; // 평상시 플레이어 레이어 번호
        private int protectionLayer = -1; // 보호 중 플레이어 레이어 번호

        public bool IsProtected => RespawnProtectionRules.IsProtected(protectionRemaining); // 현재 보호 활성 상태 반환
        public float ProtectionRemaining => protectionRemaining; // 남은 보호 시간 반환
        public float ProtectionDuration => protectionDuration; // 전체 보호 시간 반환

        private void Awake() // 보호 레이어 번호 준비
        { // 보호 기능 준비 처리
            ResolveLayers(); // Inspector 레이어 이름을 번호로 변환
        } // 보호 기능 준비 종료

        private void OnValidate() // Inspector 보호 설정값 보정
        { // Inspector 설정 보정 처리
            protectionDuration = RespawnProtectionRules.ClampDuration(protectionDuration); // 음수가 없는 보호 시간 보장
        } // Inspector 설정 보정 종료

        private void Update() // 보호 시간 갱신
        { // 보호 시간 프레임 처리
            if (!IsProtected) // 보호 비활성 상태 확인
            { // 보호 미진행 처리
                return; // 보호 시간 갱신 생략
            } // 보호 미진행 처리 종료

            protectionRemaining = RespawnProtectionRules.CalculateRemaining(protectionRemaining, Time.deltaTime); // 현재 프레임만큼 보호 시간 감소

            if (!IsProtected) // 보호 시간 만료 확인
            { // 보호 만료 처리
                RestoreOriginalLayers(); // 평상시 충돌 레이어 복구
            } // 보호 만료 처리 종료
        } // 보호 시간 프레임 처리 종료

        private void OnDisable() // 컴포넌트 비활성화 시 보호 정리
        { // 비활성화 정리 처리
            StopProtection(); // 보호 시간과 레이어 즉시 복구
        } // 비활성화 정리 종료

        public void BeginProtection() // 부활 직후 보호 시작
        { // 보호 시작 처리
            protectionRemaining = RespawnProtectionRules.ClampDuration(protectionDuration); // 설정된 전체 보호 시간 적용

            if (!IsProtected) // 보호 시간이 없는 설정 확인
            { // 보호 생략 처리
                RestoreOriginalLayers(); // 남아 있는 보호 레이어 복구
                return; // 보호 시작 생략
            } // 보호 생략 처리 종료

            if (playerLayer < 0 || protectionLayer < 0) // 보호 레이어 번호 준비 여부 확인
            { // 레이어 재확인 처리
                ResolveLayers(); // 현재 프로젝트 레이어 번호 다시 조회
            } // 레이어 재확인 처리 종료

            ApplyProtectionLayers(); // 플레이어 몸체에 보호 레이어 적용
        } // 보호 시작 처리 종료

        public void StopProtection() // 부활 보호 즉시 종료
        { // 보호 종료 처리
            protectionRemaining = 0f; // 남은 보호 시간 제거
            RestoreOriginalLayers(); // 평상시 충돌 레이어 복구
        } // 보호 종료 처리 종료

        private void ResolveLayers() // 플레이어와 보호 레이어 번호 조회
        { // 레이어 번호 조회 처리
            playerLayer = LayerMask.NameToLayer(playerLayerName); // 평상시 플레이어 레이어 번호 저장
            protectionLayer = LayerMask.NameToLayer(protectionLayerName); // 보호 중 플레이어 레이어 번호 저장

            if (playerLayer < 0 || protectionLayer < 0) // 필수 레이어 누락 확인
            { // 레이어 누락 안내 처리
                Debug.LogError($"부활 보호 레이어를 찾을 수 없습니다. Player={playerLayerName}, Protection={protectionLayerName}", this); // 누락된 레이어 설정 오류 출력
            } // 레이어 누락 안내 처리 종료
        } // 레이어 번호 조회 처리 종료

        private void ApplyProtectionLayers() // 플레이어 몸체 충돌체에 보호 레이어 적용
        { // 보호 레이어 적용 처리
            if (playerLayer < 0 || protectionLayer < 0) // 유효한 레이어 번호 확인
            { // 레이어 적용 차단 처리
                return; // 잘못된 레이어 적용 생략
            } // 레이어 적용 차단 처리 종료

            RestoreOriginalLayers(); // 기존 보호 기록을 먼저 안전 복구
            Collider[] playerColliders = GetComponentsInChildren<Collider>(true); // 플레이어와 자식 충돌체 전체 조회

            for (int index = 0; index < playerColliders.Length; index++) // 플레이어 충돌체 순회
            { // 충돌체 레이어 변경 처리
                Collider playerCollider = playerColliders[index]; // 현재 플레이어 충돌체 조회

                if (playerCollider == null) // 빈 충돌체 확인
                { // 빈 충돌체 제외 처리
                    continue; // 현재 충돌체 처리 생략
                } // 빈 충돌체 제외 처리 종료

                GameObject colliderObject = playerCollider.gameObject; // 현재 충돌체 소유 오브젝트 조회

                if (colliderObject.layer != playerLayer) // 평상시 플레이어 레이어 여부 확인
                { // 별도 기능 레이어 보존 처리
                    continue; // 트리거와 기능 전용 레이어 변경 생략
                } // 별도 기능 레이어 보존 처리 종료

                if (!originalLayers.ContainsKey(colliderObject)) // 현재 오브젝트 원본 레이어 저장 여부 확인
                { // 원본 레이어 저장 처리
                    originalLayers.Add(colliderObject, colliderObject.layer); // 보호 전 레이어 번호 저장
                } // 원본 레이어 저장 처리 종료

                colliderObject.layer = protectionLayer; // 부활 보호 전용 레이어 적용
            } // 충돌체 레이어 변경 처리 종료
        } // 보호 레이어 적용 처리 종료

        private void RestoreOriginalLayers() // 보호 전 충돌 레이어 복구
        { // 원본 레이어 복구 처리
            foreach (KeyValuePair<GameObject, int> layerEntry in originalLayers) // 저장된 레이어 항목 순회
            { // 레이어 항목 복구 처리
                if (layerEntry.Key != null) // 복구 대상 오브젝트 존재 확인
                { // 유효 오브젝트 복구 처리
                    layerEntry.Key.layer = layerEntry.Value; // 보호 전 레이어 번호 복구
                } // 유효 오브젝트 복구 처리 종료
            } // 레이어 항목 복구 처리 종료

            originalLayers.Clear(); // 복구 완료 레이어 기록 제거
        } // 원본 레이어 복구 처리 종료
    } // 부활 보호 기능 묶음 종료
} // 플레이어 보호 기능 묶음 종료

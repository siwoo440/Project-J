using ProjectJ.Items; // 아이템 사용 관리자 기능 참조
using TMPro; // TextMeshPro 텍스트 기능 참조
using UnityEngine; // Unity 컴포넌트 기능 참조

namespace ProjectJ.UI // 프로젝트 Canvas UI 네임스페이스 선언
{ // 프로젝트 Canvas UI 기능 묶음
    [DisallowMultipleComponent] // 사용 안내 표시 중복 방지
    public sealed class ItemUseStatusView : MonoBehaviour // 아이템 사용 성공과 실패 HUD 표시 선언
    { // 아이템 사용 안내 표시 묶음
        [SerializeField] private PlayerItemUseController itemUseController; // 사용 결과 문구 제공자 저장
        [SerializeField] private TMP_Text messageText; // 사용 결과 표시 TextMeshPro 저장

        private void Awake() // 실행 시작 시 필수 참조 준비
        { // 필수 참조 준비 처리
            itemUseController = itemUseController == null ? FindFirstObjectByType<PlayerItemUseController>() : itemUseController; // 현재 Scene 아이템 사용 관리자 자동 연결
            messageText = messageText == null ? GetComponent<TMP_Text>() : messageText; // 같은 오브젝트 TextMeshPro 자동 연결

            if (messageText != null) // 문구 표시 컴포넌트 존재 여부 확인
            { // 최초 문구 숨김 처리
                messageText.text = string.Empty; // 시작 시 빈 사용 결과 표시
            } // 최초 문구 숨김 처리 종료
        } // 필수 참조 준비 처리 종료

        private void OnEnable() // HUD 활성화 시 사용 결과 이벤트 연결
        { // 사용 결과 이벤트 연결 처리
            if (itemUseController != null) // 아이템 사용 관리자 존재 여부 확인
            { // 사용 결과 이벤트 연결 처리
                itemUseController.UseMessageChanged += HandleUseMessageChanged; // 사용 결과 문구 변경 이벤트 연결
                HandleUseMessageChanged(itemUseController.CurrentMessage); // 현재 저장 문구 즉시 표시
            } // 사용 결과 이벤트 연결 처리 종료
        } // 사용 결과 이벤트 연결 처리 종료

        private void OnDisable() // HUD 비활성화 시 사용 결과 이벤트 해제
        { // 사용 결과 이벤트 해제 처리
            if (itemUseController != null) // 아이템 사용 관리자 존재 여부 확인
            { // 사용 결과 이벤트 해제 처리
                itemUseController.UseMessageChanged -= HandleUseMessageChanged; // 사용 결과 문구 변경 이벤트 해제
            } // 사용 결과 이벤트 해제 처리 종료
        } // 사용 결과 이벤트 해제 처리 종료

        private void HandleUseMessageChanged(string message) // 새 사용 결과 문구 HUD 적용
        { // 사용 결과 문구 표시 처리
            if (messageText != null) // TextMeshPro 참조 존재 여부 확인
            { // 사용 결과 문구 적용 처리
                messageText.text = message ?? string.Empty; // 빈 값을 허용한 새 문구 표시
                messageText.enabled = !string.IsNullOrWhiteSpace(message); // 표시할 문구가 있을 때만 TextMeshPro 렌더링 활성화
            } // 사용 결과 문구 적용 처리 종료
        } // 사용 결과 문구 표시 처리 종료

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureForEditor(PlayerItemUseController newItemUseController, TMP_Text newMessageText) // 자동 설정 도구용 사용 안내 참조 연결
        { // 자동 설정 도구용 사용 안내 참조 연결 처리
            itemUseController = newItemUseController; // 아이템 사용 관리자 저장
            messageText = newMessageText; // 사용 결과 TextMeshPro 저장
        } // 자동 설정 도구용 사용 안내 참조 연결 처리 종료
#endif // Editor 전용 설정 종료
    } // 아이템 사용 안내 표시 묶음 종료
} // 프로젝트 Canvas UI 기능 묶음 종료

using UnityEngine; // Unity 기본 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스
{ // 네임스페이스 범위 시작
    public enum PlayerControlState // 플레이어 제어 상태 종류
    { // 열거형 범위 시작
        Gameplay, // 정상 플레이 상태
        Respawning, // 부활 처리 상태
        MatchFinished // 경기 종료 상태
    } // 열거형 범위 종료

    [DisallowMultipleComponent] // 상태 컴포넌트 중복 방지
    public sealed class PlayerStateController : MonoBehaviour // 플레이어 제어 상태 관리 컴포넌트
    { // 클래스 범위 시작
        public PlayerControlState CurrentState { get; private set; } = PlayerControlState.Gameplay; // 현재 제어 상태
        public bool CanAcceptGameplayInput => CurrentState == PlayerControlState.Gameplay; // 게임 입력 허용 여부
        public bool CanMove => CurrentState == PlayerControlState.Gameplay; // 이동 허용 여부
        public bool CanUseAction => CurrentState == PlayerControlState.Gameplay; // 상호작용 허용 여부
        public bool IsRespawning => CurrentState == PlayerControlState.Respawning; // 부활 상태 여부
        public bool IsMatchFinished => CurrentState == PlayerControlState.MatchFinished; // 경기 종료 여부

        public bool TryBeginRespawn() // 부활 상태 전환 시도
        { // 메서드 범위 시작
            if (CurrentState != PlayerControlState.Gameplay) // 정상 플레이 상태 확인
            { // 조건 범위 시작
                return false; // 상태 전환 실패 반환
            } // 조건 범위 종료

            CurrentState = PlayerControlState.Respawning; // 부활 상태 적용
            return true; // 상태 전환 성공 반환
        } // 메서드 범위 종료

        public void CompleteRespawn() // 부활 완료 상태 적용
        { // 메서드 범위 시작
            if (CurrentState != PlayerControlState.Respawning) // 현재 부활 상태 확인
            { // 조건 범위 시작
                return; // 잘못된 완료 요청 생략
            } // 조건 범위 종료

            CurrentState = PlayerControlState.Gameplay; // 정상 플레이 상태 복구
        } // 메서드 범위 종료

        public void FinishMatch() // 경기 종료 상태 적용
        { // 메서드 범위 시작
            CurrentState = PlayerControlState.MatchFinished; // 경기 종료 상태 저장
        } // 메서드 범위 종료

        public void ResetForNewMatch() // 새 경기 상태 초기화
        { // 메서드 범위 시작
            CurrentState = PlayerControlState.Gameplay; // 정상 플레이 상태 적용
        } // 메서드 범위 종료
    } // 클래스 범위 종료
} // 네임스페이스 범위 종료

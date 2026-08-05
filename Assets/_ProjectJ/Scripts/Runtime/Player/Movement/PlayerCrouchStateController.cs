using ProjectJ.Data; // 플레이어 앉기 데이터 참조
using UnityEngine; // Unity 수학 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 네임스페이스 범위 시작
    public enum PlayerPostureState // 플레이어 자세 상태 종류 선언
    { // 열거형 범위 시작
        Standing, // 완전한 서기 상태
        EnteringCrouch, // 앉기 전환 상태
        Crouching, // 완전한 앉기 상태
        StandingBlocked, // 머리 위 장애물로 서기 차단 상태
        ExitingCrouch // 서기 전환 상태
    } // 열거형 범위 종료

    public sealed class PlayerCrouchStateController // 앉기 높이와 자세 상태 제어기 선언
    { // 클래스 범위 시작
        private const float HeightTolerance = 0.001f; // 높이 비교 허용 오차

        private readonly PlayerCrouchSettings settings; // 앉기 설정 저장

        public float CurrentHeight { get; private set; } // 현재 충돌체 높이
        public float TargetHeight { get; private set; } // 현재 목표 충돌체 높이
        public PlayerPostureState CurrentState { get; private set; } // 현재 자세 상태
        public bool IsCrouching => CurrentState != PlayerPostureState.Standing; // 앉기 계열 상태 반환
        public bool IsStandingBlocked => CurrentState == PlayerPostureState.StandingBlocked; // 서기 차단 상태 반환
        public bool IsReceivingExternalForce { get; private set; } // 밀치기와 외부 힘 적용 상태
        public bool CanJump => CurrentState == PlayerPostureState.Standing; // 완전한 서기 상태의 점프 허용 여부

        public PlayerCrouchStateController(PlayerCrouchSettings settings) // 앉기 설정 기반 자세 제어기 생성
        { // 메서드 범위 시작
            this.settings = settings; // 전달된 앉기 설정 저장
            Reset(); // 초기 서기 상태 적용
        } // 메서드 범위 종료

        public void Tick(float deltaTime, bool crouchRequested, bool canStandUp, bool isReceivingExternalForce) // 한 프레임 자세 상태 갱신
        { // 메서드 범위 시작
            float safeDeltaTime = Mathf.Max(0f, deltaTime); // 음수가 아닌 프레임 시간 보정
            bool mustRemainCrouched = crouchRequested || !canStandUp; // 입력 또는 공간 부족에 따른 앉기 유지 여부
            TargetHeight = mustRemainCrouched ? settings.CrouchingHeight : settings.StandingHeight; // 현재 조건의 목표 높이 선택
            CurrentHeight = Mathf.MoveTowards(CurrentHeight, TargetHeight, settings.HeightTransitionSpeed * safeDeltaTime); // 목표 높이를 향한 부드러운 전환
            IsReceivingExternalForce = isReceivingExternalForce; // 외부 힘 적용 상태 저장
            CurrentState = ResolveState(crouchRequested, canStandUp); // 높이와 조건 기반 자세 상태 결정
        } // 메서드 범위 종료

        public void Reset() // 자세와 높이 상태 초기화
        { // 메서드 범위 시작
            CurrentHeight = settings.StandingHeight; // 충돌체 서기 높이 복원
            TargetHeight = settings.StandingHeight; // 목표 서기 높이 복원
            CurrentState = PlayerPostureState.Standing; // 완전한 서기 상태 적용
            IsReceivingExternalForce = false; // 외부 힘 상태 제거
        } // 메서드 범위 종료

        private PlayerPostureState ResolveState(bool crouchRequested, bool canStandUp) // 현재 조건에 맞는 자세 상태 반환
        { // 메서드 범위 시작
            if (crouchRequested) // 앉기 입력 유지 확인
            { // 조건 범위 시작
                if (CurrentHeight <= settings.CrouchingHeight + HeightTolerance) // 앉기 목표 높이 도달 확인
                { // 조건 범위 시작
                    return PlayerPostureState.Crouching; // 완전한 앉기 상태 반환
                } // 조건 범위 종료

                return PlayerPostureState.EnteringCrouch; // 앉기 전환 상태 반환
            } // 조건 범위 종료

            if (!canStandUp) // 머리 위 공간 부족 확인
            { // 조건 범위 시작
                return PlayerPostureState.StandingBlocked; // 서기 차단 상태 반환
            } // 조건 범위 종료

            if (CurrentHeight >= settings.StandingHeight - HeightTolerance) // 서기 목표 높이 도달 확인
            { // 조건 범위 시작
                return PlayerPostureState.Standing; // 완전한 서기 상태 반환
            } // 조건 범위 종료

            return PlayerPostureState.ExitingCrouch; // 서기 전환 상태 반환
        } // 메서드 범위 종료
    } // 클래스 범위 종료
} // 네임스페이스 범위 종료

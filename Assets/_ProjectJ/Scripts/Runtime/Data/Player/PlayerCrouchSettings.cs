using System; // Unity 직렬화 가능한 값 형식 기능 참조
using UnityEngine; // Unity Inspector 속성과 직렬화 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    [Serializable] // Unity Inspector 직렬화 지정
    public struct PlayerCrouchSettings // 앉기와 CharacterController 크기 설정 값 형식 선언
    {
        [SerializeField, Min(0.01f)] private float crouchMoveSpeed; // 앉은 상태의 이동 속도 저장
        [SerializeField, Min(0.01f)] private float standingHeight; // 서 있는 상태의 CharacterController 높이 저장
        [SerializeField, Min(0.01f)] private float crouchingHeight; // 앉은 상태의 CharacterController 높이 저장
        [SerializeField, Min(0.01f)] private float controllerRadius; // 공통 CharacterController 반지름 저장
        [SerializeField, Min(0.01f)] private float heightTransitionSpeed; // 서기와 앉기 높이 전환 속도 저장
        [SerializeField, Min(0f)] private float standClearancePadding; // 일어서기 공간 검사 여유값 저장

        public float CrouchMoveSpeed => crouchMoveSpeed; // 앉은 상태 이동 속도 반환
        public float StandingHeight => standingHeight; // 서 있는 상태 높이 반환
        public float CrouchingHeight => crouchingHeight; // 앉은 상태 높이 반환
        public float ControllerRadius => controllerRadius; // CharacterController 반지름 반환
        public float HeightTransitionSpeed => heightTransitionSpeed; // 높이 전환 속도 반환
        public float StandClearancePadding => standClearancePadding; // 일어서기 공간 검사 여유값 반환

        public PlayerCrouchSettings( // 앉기 설정 값 생성
            float crouchMoveSpeed, // 앉은 상태 이동 속도 입력
            float standingHeight, // 서 있는 상태 높이 입력
            float crouchingHeight, // 앉은 상태 높이 입력
            float controllerRadius, // CharacterController 반지름 입력
            float heightTransitionSpeed, // 높이 전환 속도 입력
            float standClearancePadding) // 일어서기 공간 검사 여유값 입력
        {
            this.crouchMoveSpeed = crouchMoveSpeed; // 전달된 앉기 이동 속도 저장
            this.standingHeight = standingHeight; // 전달된 서기 높이 저장
            this.crouchingHeight = crouchingHeight; // 전달된 앉기 높이 저장
            this.controllerRadius = controllerRadius; // 전달된 CharacterController 반지름 저장
            this.heightTransitionSpeed = heightTransitionSpeed; // 전달된 높이 전환 속도 저장
            this.standClearancePadding = standClearancePadding; // 전달된 공간 검사 여유값 저장
        }

        public static PlayerCrouchSettings CreateDefault() // 7일차 기본 앉기 설정 생성
        {
            return new PlayerCrouchSettings(3.5f, 2f, 1.2f, 0.45f, 8f, 0.05f); // 프로토타입용 앉기와 충돌체 초기값 반환
        }

        public bool IsValid(float moveSpeed, out string reason) // 기본 이동 속도와 비교한 앉기 설정 유효 여부 검사
        {
            if (crouchMoveSpeed <= 0f || crouchMoveSpeed > moveSpeed) // 앉기 이동 속도의 양수와 상한 여부 확인
            {
                reason = "앉기 이동 속도는 0보다 크고 기본 이동 속도 이하여야 합니다."; // 앉기 이동 속도 오류 사유 저장
                return false; // 앉기 설정 검사 실패 반환
            }

            if (controllerRadius <= 0f) // CharacterController 반지름이 양수인지 확인
            {
                reason = "CharacterController 반지름은 0보다 커야 합니다."; // 반지름 오류 사유 저장
                return false; // 앉기 설정 검사 실패 반환
            }

            float minimumCapsuleHeight = controllerRadius * 2f; // 캡슐 형태 유지에 필요한 최소 높이 계산

            if (standingHeight < minimumCapsuleHeight) // 서기 높이가 캡슐 최소 높이 이상인지 확인
            {
                reason = "서기 높이는 CharacterController 지름 이상이어야 합니다."; // 서기 높이 오류 사유 저장
                return false; // 앉기 설정 검사 실패 반환
            }

            if (crouchingHeight < minimumCapsuleHeight || crouchingHeight >= standingHeight) // 앉기 높이의 최소값과 서기 높이 미만 여부 확인
            {
                reason = "앉기 높이는 CharacterController 지름 이상이고 서기 높이보다 작아야 합니다."; // 앉기 높이 오류 사유 저장
                return false; // 앉기 설정 검사 실패 반환
            }

            if (heightTransitionSpeed <= 0f) // 높이 전환 속도가 양수인지 확인
            {
                reason = "앉기 높이 전환 속도는 0보다 커야 합니다."; // 높이 전환 속도 오류 사유 저장
                return false; // 앉기 설정 검사 실패 반환
            }

            if (standClearancePadding < 0f) // 일어서기 공간 검사 여유값이 음수인지 확인
            {
                reason = "일어서기 공간 검사 여유값은 0 이상이어야 합니다."; // 공간 검사 여유값 오류 사유 저장
                return false; // 앉기 설정 검사 실패 반환
            }

            reason = string.Empty; // 검사 성공 결과로 오류 사유 초기화
            return true; // 앉기 설정 검사 성공 반환
        }
    }
}

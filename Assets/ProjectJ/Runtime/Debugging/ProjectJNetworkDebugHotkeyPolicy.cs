using System; // 디버그 기능 열거형 조회
using System.Collections.Generic; // 단축키 중복 검사 집합
using UnityEngine.InputSystem; // 입력 키 열거형 사용

namespace ProjectJ.Debugging // Runtime 진단 정책 네임스페이스
{
    public enum ProjectJNetworkDebugAction // 네트워크 디버그 기능 종류
    {
        SoloStart = 0, // 단독 경기 시작
        MovementDiagnostics = 1, // 이동 품질 진단
        MeasurementReset = 2, // 측정 구간 초기화
        ForceMatchEnd = 3 // 강제 경기 종료
    }

    public static class ProjectJNetworkDebugHotkeyPolicy // 네트워크 디버그 단축키 공통 정책
    {
        public static Key GetKey( // 디버그 기능별 전용 키 조회
            ProjectJNetworkDebugAction action // 조회할 디버그 기능
        )
        {
            switch (action) // 디버그 기능별 키 분기
            {
                case ProjectJNetworkDebugAction.SoloStart: // 단독 시작 기능 확인
                    return Key.F5; // F5 키 반환

                case ProjectJNetworkDebugAction.MovementDiagnostics: // 이동 진단 기능 확인
                    return Key.F6; // F6 키 반환

                case ProjectJNetworkDebugAction.MeasurementReset: // 측정 초기화 기능 확인
                    return Key.F10; // F10 키 반환

                case ProjectJNetworkDebugAction.ForceMatchEnd: // 강제 종료 기능 확인
                    return Key.F11; // F11 키 반환

                default: // 미등록 디버그 기능 처리
                    return Key.None; // 미지정 키 반환
            }
        }

        public static bool HasUniqueBindings() // 전체 단축키 중복 여부 확인
        {
            HashSet<Key> usedKeys = // 이미 사용한 키 저장소
                new HashSet<Key>(); // 빈 키 집합 생성

            Array actions = // 등록된 디버그 기능 목록
                Enum.GetValues( // 열거형 값 전체 조회
                    typeof(ProjectJNetworkDebugAction) // 네트워크 디버그 기능 타입
                );

            foreach (ProjectJNetworkDebugAction action in actions) // 디버그 기능별 반복
            {
                Key key = // 현재 기능 전용 키
                    GetKey( // 단축키 정책 조회
                        action // 현재 기능 전달
                    );

                if ( // 미지정 또는 중복 키 확인
                    key == Key.None || // 미지정 키 여부
                    !usedKeys.Add(key) // 기존 키 중복 여부
                )
                {
                    return false; // 잘못된 단축키 구성 반환
                }
            }

            return true; // 전체 단축키 고유 구성 반환
        }

        public static bool CanForceMatchEnd( // 강제 경기 종료 허용 여부 계산
            bool isGameSceneActive, // Game Scene 활성 여부
            bool hasStateAuthority, // State Authority 보유 여부
            bool isMatchCoordinator, // Match Coordinator 일치 여부
            bool gameplayInputAllowed // 경기 입력 허용 여부
        )
        {
            return // 전체 강제 종료 조건 반환
                isGameSceneActive && // Game Scene 실행 확인
                hasStateAuthority && // State Authority 보유 확인
                isMatchCoordinator && // Match Coordinator 일치 확인
                gameplayInputAllowed; // 실제 경기 진행 확인
        }
    }
}

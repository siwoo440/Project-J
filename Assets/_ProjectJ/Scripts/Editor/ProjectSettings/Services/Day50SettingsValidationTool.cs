using ProjectJ.Core.Services; // 사용자 설정 데이터와 관리자 기능 참조
using UnityEditor; // Unity Editor 메뉴 기능 참조
using UnityEngine; // Unity Console과 Play Mode 상태 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스 선언
{ // 50일차 설정 기반 검증 도구 정의
    internal static class Day50SettingsValidationTool // 설정 데이터와 SettingsManager 연결 상태 검증 도구 선언
    { // 저장값을 변경하지 않는 안전 진단 기능 정의
        private const string MenuPath = ProjectJEditorMenuPaths.ProjectSettingsServices + "/50일차 설정 기반 검증 (Day 50일차)"; // Unity 상단 메뉴 경로 선언

        [MenuItem(MenuPath)] // Unity 상단 메뉴에 50일차 설정 검증 항목 등록
        private static void ValidateSettingsFoundation() // 설정 기본값·복사·JSON·런타임 관리자 상태 검증
        { // 51일차 UI 구현 전 설정 기반 상태 확인
            ProjectUserSettings defaults = ProjectUserSettings.CreateDefault(); // 현재 환경 기반 기본 설정 생성
            ProjectUserSettings workingCopy = defaults.Clone(); // 독립 작업 복사본 생성

            if (!defaults.ContentEquals(workingCopy)) // 기본 설정과 복사본 동일 여부 확인
            { // 복사 기능 이상 처리
                Debug.LogError("[ProjectJ][Day50] 설정 작업 복사본 생성 검증에 실패했습니다."); // 복사 기능 실패 오류 출력
                return; // 추가 검증 중단
            } // 복사 기능 이상 처리 완료

            string json = SettingsJsonSerializer.Serialize(defaults, false); // 기본 설정 메모리 JSON 직렬화

            if (!SettingsJsonSerializer.TryDeserialize(json, out ProjectUserSettings loaded, out string failureReason)) // 메모리 JSON 역직렬화 성공 여부 확인
            { // JSON 변환 이상 처리
                Debug.LogError($"[ProjectJ][Day50] 설정 JSON 왕복 검증에 실패했습니다. {failureReason}"); // JSON 검증 실패 오류 출력
                return; // 추가 검증 중단
            } // JSON 변환 이상 처리 완료

            if (!defaults.ContentEquals(loaded)) // JSON 왕복 결과 동일 여부 확인
            { // 설정 내용 불일치 처리
                Debug.LogError("[ProjectJ][Day50] JSON 저장 전후 설정 값이 일치하지 않습니다."); // JSON 내용 불일치 오류 출력
                return; // 추가 검증 중단
            } // 설정 내용 불일치 처리 완료

            if (!EditorApplication.isPlaying) // 현재 Play Mode 실행 여부 확인
            { // Edit Mode 검증 완료 처리
                Debug.Log("[ProjectJ][Day50] 기본값·작업 복사본·JSON 구조 검증 완료 | Play Mode에서 다시 실행하면 SettingsManager 연결도 확인할 수 있습니다."); // Edit Mode 검증 완료 로그 출력
                return; // 런타임 관리자 검증 없이 종료
            } // Edit Mode 검증 완료 처리 완료

            if (!SettingsManager.IsReady) // Play Mode 설정 관리자 준비 여부 확인
            { // Bootstrap 서비스 초기화 이상 처리
                Debug.LogError("[ProjectJ][Day50] Play Mode에서 SettingsManager가 준비되지 않았습니다. Bootstrap 시작 흐름을 확인합니다."); // 관리자 미준비 오류 출력
                return; // 런타임 검증 중단
            } // Bootstrap 서비스 초기화 이상 처리 완료

            ProjectUserSettings runtimeSnapshot = SettingsManager.CreateWorkingCopy(); // 현재 런타임 설정 독립 복사본 생성

            if (runtimeSnapshot == null) // 런타임 설정 복사본 누락 여부 확인
            { // 런타임 설정 누락 처리
                Debug.LogError("[ProjectJ][Day50] SettingsManager 작업 복사본을 생성하지 못했습니다."); // 런타임 복사 실패 오류 출력
                return; // 검증 중단
            } // 런타임 설정 누락 처리 완료

            Debug.Log("[ProjectJ][Day50] 설정 기반 전체 검증 완료 | SettingsManager 준비 | 기본값·복사·JSON 구조 정상"); // 전체 설정 기반 검증 성공 로그 출력
        } // 설정 기본값·복사·JSON·런타임 관리자 상태 검증 완료
    } // 저장값을 변경하지 않는 안전 진단 기능 정의 완료
} // 프로젝트 Editor 기능 네임스페이스 정의 완료

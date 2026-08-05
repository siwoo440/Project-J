using System; // 설정 중 발생한 예외 처리 기능 참조
using System.Collections.Generic; // 검증 오류 목록 기능 참조
using UnityEditor; // Unity 에디터 메뉴와 대화상자 기능 참조
using UnityEngine; // Unity Console 로그 기능 참조

namespace ProjectJ.Editor // 프로젝트 에디터 전용 네임스페이스 선언
{
    internal static class Day08PhysicsLayerSetupTool // 8일차 물리 레이어와 충돌 행렬 자동 구성 메뉴 선언
    {
        private const string ConfigureMenuPath = "Project J/Day 08/Configure Physics Layers"; // 물리 레이어 자동 구성 메뉴 경로 선언
        private const string ValidateMenuPath = "Project J/Day 08/Validate Physics Layers"; // 물리 레이어 검증 메뉴 경로 선언

        [MenuItem(ConfigureMenuPath)] // Unity 상단 메뉴에 물리 레이어 구성 항목 등록
        private static void ConfigurePhysicsLayers() // Project J 전용 물리 레이어와 충돌 행렬 자동 구성
        {
            try // 프로젝트 설정 변경 예외 처리 시작
            {
                ProjectPhysicsLayerEditorUtility.Configure(); // 레이어 이름과 3D 물리 충돌 행렬 적용
                List<string> errors = ProjectPhysicsLayerEditorUtility.CollectValidationErrors(); // 적용 직후 전체 물리 레이어 설정 검증

                if (errors.Count > 0) // 설정 검증 오류 존재 여부 확인
                {
                    LogValidationErrors(errors); // 발견된 모든 설정 오류 Console 출력
                    EditorUtility.DisplayDialog("Project J Day 08", $"물리 레이어 구성 후 오류 {errors.Count}개를 발견했습니다. Console을 확인합니다.", "확인"); // 구성 실패 대화상자 표시
                    return; // 성공 로그와 대화상자 표시 생략
                }

                Debug.Log("[Day08] Project J 물리 레이어 8개와 3D 충돌 행렬 구성을 완료했습니다."); // 물리 레이어 구성 완료 로그 출력
                EditorUtility.DisplayDialog("Project J Day 08", "물리 레이어 8개와 충돌 행렬 구성을 완료했습니다.", "확인"); // 구성 성공 대화상자 표시
            }
            catch (Exception exception) // 프로젝트 설정 변경 중 발생한 모든 예외 처리
            {
                Debug.LogException(exception); // 전체 예외 정보 Console 출력
                EditorUtility.DisplayDialog("Project J Day 08", "물리 레이어 구성에 실패했습니다. Console의 오류 내용을 확인합니다.", "확인"); // 구성 실패 대화상자 표시
            }
        }

        [MenuItem(ValidateMenuPath)] // Unity 상단 메뉴에 물리 레이어 검증 항목 등록
        private static void ValidatePhysicsLayers() // 현재 Project J 물리 레이어 이름과 충돌 행렬 검증
        {
            List<string> errors = ProjectPhysicsLayerEditorUtility.CollectValidationErrors(); // 현재 프로젝트 물리 레이어 설정 오류 수집

            if (errors.Count == 0) // 물리 레이어 설정 오류가 없는지 확인
            {
                Debug.Log("[Day08] Project J 물리 레이어 이름과 충돌 행렬 검증을 통과했습니다."); // 물리 레이어 검증 성공 로그 출력
                EditorUtility.DisplayDialog("Project J Day 08", "물리 레이어 이름과 충돌 행렬이 모두 정상입니다.", "확인"); // 검증 성공 대화상자 표시
                return; // 오류 로그 처리 생략
            }

            LogValidationErrors(errors); // 발견된 모든 설정 오류 Console 출력
            EditorUtility.DisplayDialog("Project J Day 08", $"물리 레이어 설정 오류 {errors.Count}개를 발견했습니다. Console을 확인합니다.", "확인"); // 검증 실패 대화상자 표시
        }

        [MenuItem(ConfigureMenuPath, true)] // 물리 레이어 구성 메뉴 활성 조건 등록
        [MenuItem(ValidateMenuPath, true)] // 물리 레이어 검증 메뉴 활성 조건 등록
        private static bool ValidateEditorMenu() // Play Mode가 아닐 때만 8일차 메뉴 실행 허용
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Play Mode 진입 또는 실행 중이 아닌 경우 활성화
        }

        private static void LogValidationErrors(IReadOnlyList<string> errors) // 물리 레이어 검증 오류 전체 Console 출력
        {
            for (int index = 0; index < errors.Count; index++) // 모든 검증 오류 순회
            {
                Debug.LogError($"[Day08] {errors[index]}"); // 현재 물리 레이어 설정 오류 출력
            }
        }
    }
}

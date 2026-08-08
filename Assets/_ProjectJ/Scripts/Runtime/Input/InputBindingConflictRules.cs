using System; // 문자열 비교 기능 참조
using UnityEngine.InputSystem; // InputAction과 InputBinding 기능 참조

namespace ProjectJ.Input // 프로젝트 입력 네임스페이스 선언
{ // 사용자 키 재지정 대상 검색과 중복 검사 기능 구성
    public static class InputBindingConflictRules // Keyboard&Mouse 바인딩 검색과 충돌 규칙 선언
    { // 설정 메뉴와 테스트가 공유할 순수 입력 규칙 구성
        public static int FindKeyboardMouseBindingIndex(InputAction action, string compositePartName = null) // 액션의 키보드·마우스 재지정 대상 인덱스 검색
        { // 일반 버튼과 2DVector 복합 바인딩 파트 공통 검색
            if (action == null) // 입력 액션 누락 여부 확인
            { // 잘못된 검색 요청 방어
                return -1; // 검색 실패 인덱스 반환
            } // 잘못된 검색 요청 방어 마무리

            for (int index = 0; index < action.bindings.Count; index++) // 액션 전체 바인딩 순회
            { // 현재 바인딩 재지정 대상 여부 검사
                InputBinding binding = action.bindings[index]; // 현재 바인딩 데이터 조회

                if (!BelongsToKeyboardMouse(binding)) // Keyboard&Mouse Control Scheme 여부 확인
                { // 게임패드와 기타 장치 바인딩 제외
                    continue; // 다음 바인딩 검사
                } // 게임패드와 기타 장치 바인딩 제외 마무리

                if (!string.IsNullOrWhiteSpace(compositePartName)) // 이동 방향 같은 복합 바인딩 파트 검색 여부 확인
                { // 지정된 복합 파트 이름 검색
                    if (binding.isPartOfComposite && string.Equals(binding.name, compositePartName, StringComparison.OrdinalIgnoreCase)) // 복합 파트 이름과 장치 그룹 일치 여부 확인
                    { // 요청한 이동 방향 바인딩 발견
                        return index; // 복합 파트 바인딩 인덱스 반환
                    } // 요청한 이동 방향 바인딩 발견 마무리

                    continue; // 다른 복합 파트 또는 일반 바인딩 제외
                } // 지정된 복합 파트 이름 검색 마무리

                if (!binding.isComposite && !binding.isPartOfComposite) // 일반 단일 키 바인딩 여부 확인
                { // 일반 액션 재지정 대상 발견
                    return index; // 일반 키보드·마우스 바인딩 인덱스 반환
                } // 일반 액션 재지정 대상 발견 마무리
            } // 현재 바인딩 재지정 대상 여부 검사 마무리

            return -1; // 재지정 가능한 Keyboard&Mouse 바인딩 없음 반환
        } // 액션의 키보드·마우스 재지정 대상 인덱스 검색 마무리

        public static bool HasDuplicateEffectivePath(InputActionMap actionMap, InputAction targetAction, int targetBindingIndex) // 새 키가 다른 Gameplay 조작과 중복되는지 검사
        { // 같은 Keyboard&Mouse 유효 경로를 사용하는 다른 바인딩 검색
            if (actionMap == null || targetAction == null || targetBindingIndex < 0 || targetBindingIndex >= targetAction.bindings.Count) // 중복 검사 대상 유효성 확인
            { // 잘못된 중복 검사 요청 방어
                return false; // 검사할 대상 없음 반환
            } // 잘못된 중복 검사 요청 방어 마무리

            string targetPath = targetAction.bindings[targetBindingIndex].effectivePath; // 새로 적용된 대상 유효 제어 경로 조회

            if (string.IsNullOrWhiteSpace(targetPath)) // 대상 제어 경로 누락 여부 확인
            { // 빈 바인딩 중복 검사 제외
                return false; // 중복 없음 반환
            } // 빈 바인딩 중복 검사 제외 마무리

            foreach (InputAction action in actionMap.actions) // Gameplay 액션 전체 순회
            { // 현재 액션의 Keyboard&Mouse 바인딩 검사
                for (int index = 0; index < action.bindings.Count; index++) // 현재 액션 바인딩 전체 순회
                { // 현재 바인딩과 대상 제어 경로 비교
                    if (object.ReferenceEquals(action, targetAction) && index == targetBindingIndex) // 자기 자신 바인딩 여부 확인
                    { // 자기 자신 중복 검사 제외
                        continue; // 다음 바인딩 검사
                    } // 자기 자신 중복 검사 제외 마무리

                    InputBinding binding = action.bindings[index]; // 비교할 현재 바인딩 조회

                    if (binding.isComposite || !BelongsToKeyboardMouse(binding)) // 복합 루트 또는 다른 장치 바인딩 여부 확인
                    { // 사용자 키 충돌 검사 대상 제외
                        continue; // 다음 바인딩 검사
                    } // 사용자 키 충돌 검사 대상 제외 마무리

                    if (string.Equals(binding.effectivePath, targetPath, StringComparison.OrdinalIgnoreCase)) // 다른 조작과 같은 유효 제어 경로 여부 확인
                    { // 중복 키 발견
                        return true; // 키 충돌 존재 반환
                    } // 중복 키 발견 마무리
                } // 현재 바인딩과 대상 제어 경로 비교 마무리
            } // 현재 액션의 Keyboard&Mouse 바인딩 검사 마무리

            return false; // 다른 조작과 중복되지 않은 키 반환
        } // 새 키가 다른 Gameplay 조작과 중복되는지 검사 마무리

        private static bool BelongsToKeyboardMouse(InputBinding binding) // 바인딩 Control Scheme에 Keyboard&Mouse가 포함되는지 확인
        { // 세미콜론으로 연결될 수 있는 Input System 그룹 문자열 검사
            if (string.IsNullOrWhiteSpace(binding.groups)) // Control Scheme 그룹 누락 여부 확인
            { // 그룹 없는 복합 루트와 일반 바인딩 제외
                return false; // Keyboard&Mouse 대상 아님 반환
            } // 그룹 없는 복합 루트와 일반 바인딩 제외 마무리

            string[] groups = binding.groups.Split(';'); // Input System Control Scheme 그룹 목록 분리

            for (int index = 0; index < groups.Length; index++) // 전체 바인딩 그룹 순회
            { // 현재 그룹 이름 비교
                if (string.Equals(groups[index], ProjectInputNames.KeyboardMouseScheme, StringComparison.Ordinal)) // 프로젝트 Keyboard&Mouse 그룹과 일치 여부 확인
                { // Keyboard&Mouse 바인딩 확인
                    return true; // Keyboard&Mouse 대상 반환
                } // Keyboard&Mouse 바인딩 확인 마무리
            } // 현재 그룹 이름 비교 마무리

            return false; // Keyboard&Mouse 그룹 없음 반환
        } // 바인딩 Control Scheme에 Keyboard&Mouse가 포함되는지 확인 마무리
    } // Keyboard&Mouse 바인딩 검색과 충돌 규칙 마무리
} // 프로젝트 입력 네임스페이스 마무리

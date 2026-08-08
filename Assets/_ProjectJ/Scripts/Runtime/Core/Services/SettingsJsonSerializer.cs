using System; // JSON 변환 예외와 버전 헤더 직렬화 기능 참조
using UnityEngine; // Unity JsonUtility 기능 참조

namespace ProjectJ.Core.Services // 프로젝트 공통 서비스 네임스페이스 선언
{ // 사용자 설정 JSON 변환 기능 구성
    public static class SettingsJsonSerializer // 설정 데이터 직렬화와 역직렬화 담당 형식 선언
    { // 파일 입출력과 분리된 순수 JSON 변환 기능 구성
        [Serializable] // JSON 버전 헤더 직렬화 대상 지정
        private sealed class SettingsVersionHeader // 설정 JSON 버전만 먼저 읽기 위한 최소 형식 선언
        { // 버전 헤더 데이터 구성
            public int Version; // 설정 JSON 버전 값
        } // 설정 JSON 버전 최소 형식 마무리

        public static string Serialize(ProjectUserSettings settings, bool prettyPrint = true) // 사용자 설정의 안전한 JSON 문자열 생성
        { // 원본 설정을 변경하지 않는 직렬화 처리
            if (settings == null) // 직렬화 대상 누락 여부 확인
            { // 잘못된 직렬화 요청 방어
                throw new ArgumentNullException(nameof(settings)); // null 설정 직렬화 예외 발생
            } // 잘못된 직렬화 요청 방어 마무리

            ProjectUserSettings snapshot = settings.Clone(); // 원본과 분리된 설정 복사본 생성
            snapshot.Validate(); // 저장 전 설정값 안전 범위 보정
            return JsonUtility.ToJson(snapshot, prettyPrint); // 보정된 설정 JSON 문자열 반환
        } // 사용자 설정의 안전한 JSON 문자열 생성 마무리

        public static bool TryDeserialize(string json, out ProjectUserSettings settings, out string failureReason) // JSON 사용자 설정 변환 시도
        { // 손상 데이터와 버전 마이그레이션 안전 처리
            settings = null; // 변환 실패 기본 설정 결과 준비
            failureReason = string.Empty; // 변환 실패 원인 기본값 준비

            if (string.IsNullOrWhiteSpace(json)) // JSON 문자열 누락 여부 확인
            { // 빈 설정 데이터 처리
                failureReason = "설정 JSON이 비어 있습니다."; // 빈 JSON 실패 원인 저장
                return false; // 설정 변환 실패 반환
            } // 빈 설정 데이터 처리 마무리

            try // Unity JSON 변환 예외 감시
            { // 설정 JSON 버전과 전체 데이터 변환 처리
                SettingsVersionHeader versionHeader = JsonUtility.FromJson<SettingsVersionHeader>(json); // 설정 JSON 버전 헤더 우선 변환

                if (versionHeader == null || versionHeader.Version <= 0) // 버전 헤더 누락 또는 잘못된 값 확인
                { // 버전 정보 누락 처리
                    failureReason = "설정 파일 버전 정보가 없습니다."; // 버전 누락 실패 원인 저장
                    return false; // 설정 변환 실패 반환
                } // 버전 정보 누락 처리 마무리

                if (versionHeader.Version > ProjectUserSettings.CurrentVersion) // 현재 코드보다 새로운 설정 버전 확인
                { // 미래 버전 설정 거부 처리
                    failureReason = $"설정 파일 버전 {versionHeader.Version}을 지원하지 않습니다."; // 미래 버전 실패 원인 저장
                    return false; // 설정 변환 실패 반환
                } // 미래 버전 설정 거부 처리 마무리

                ProjectUserSettings loadedSettings = JsonUtility.FromJson<ProjectUserSettings>(json); // JSON 사용자 설정 객체 변환

                if (loadedSettings == null) // 변환 결과 누락 여부 확인
                { // 비어 있는 변환 결과 처리
                    failureReason = "설정 JSON 결과가 비어 있습니다."; // 변환 결과 누락 원인 저장
                    return false; // 설정 변환 실패 반환
                } // 비어 있는 변환 결과 처리 마무리

                if (versionHeader.Version == 1) // 50일차 이전 설정 버전 확인
                { // 버전 1에서 버전 2 기본값 보강
                    loadedSettings.Brightness = 1f; // 기존 사용자에게 기본 밝기 100퍼센트 적용
                    loadedSettings.UiVolume = 1f; // 기존 사용자에게 기본 UI 음량 100퍼센트 적용
                    loadedSettings.Version = ProjectUserSettings.CurrentVersion; // 마이그레이션 완료 버전 적용
                } // 버전 1에서 버전 2 기본값 보강 마무리
                else if (versionHeader.Version != ProjectUserSettings.CurrentVersion) // 지원 목록에 없는 과거 버전 확인
                { // 알 수 없는 과거 버전 거부 처리
                    failureReason = $"설정 파일 버전 {versionHeader.Version}을 지원하지 않습니다."; // 과거 버전 실패 원인 저장
                    return false; // 설정 변환 실패 반환
                } // 알 수 없는 과거 버전 거부 처리 마무리

                loadedSettings.Validate(); // 불러온 설정값 안전 범위 보정
                settings = loadedSettings; // 정상 변환된 설정 결과 저장
                return true; // 설정 변환 성공 반환
            } // 설정 JSON 버전과 전체 데이터 변환 처리 마무리
            catch (Exception exception) // 손상된 JSON 변환 실패 처리
            { // JSON 예외 복구 정보 생성
                failureReason = exception.Message; // 실제 JSON 변환 오류 내용 저장
                return false; // 설정 변환 실패 반환
            } // 손상된 JSON 변환 실패 처리 마무리
        } // JSON 사용자 설정 변환 시도 마무리
    } // 설정 데이터 직렬화와 역직렬화 담당 형식 마무리
} // 프로젝트 공통 서비스 네임스페이스 마무리

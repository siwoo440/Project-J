using System; // JSON 변환 예외 기능 참조
using UnityEngine; // Unity JsonUtility 기능 참조

namespace ProjectJ.Core.Services // 프로젝트 공통 서비스 네임스페이스 선언
{ // 사용자 설정 JSON 변환 기능 정의
    public static class SettingsJsonSerializer // 설정 데이터 직렬화와 역직렬화 담당 형식 선언
    { // 파일 입출력과 분리된 순수 JSON 변환 기능 정의
        public static string Serialize(ProjectUserSettings settings, bool prettyPrint = true) // 사용자 설정의 안전한 JSON 문자열 생성
        { // 원본 설정을 변경하지 않는 직렬화 처리
            if (settings == null) // 직렬화 대상 누락 여부 확인
            { // 잘못된 직렬화 요청 처리
                throw new ArgumentNullException(nameof(settings)); // null 설정 직렬화 예외 발생
            } // 잘못된 직렬화 요청 처리 완료

            ProjectUserSettings snapshot = settings.Clone(); // 원본과 분리된 설정 복사본 생성
            snapshot.Validate(); // 저장 전 설정값 안전 범위 보정
            return JsonUtility.ToJson(snapshot, prettyPrint); // 보정된 설정 JSON 문자열 반환
        } // 사용자 설정의 안전한 JSON 문자열 생성 완료

        public static bool TryDeserialize(string json, out ProjectUserSettings settings, out string failureReason) // JSON 사용자 설정 변환 시도
        { // 손상 데이터와 지원하지 않는 버전의 안전한 거부 처리
            settings = null; // 변환 실패 기본 설정 결과 준비
            failureReason = string.Empty; // 변환 실패 원인 기본값 준비

            if (string.IsNullOrWhiteSpace(json)) // JSON 문자열 누락 여부 확인
            { // 빈 설정 데이터 처리
                failureReason = "설정 JSON이 비어 있습니다."; // 빈 JSON 실패 원인 저장
                return false; // 설정 변환 실패 반환
            } // 빈 설정 데이터 처리 완료

            try // Unity JSON 변환 예외 감시
            { // 설정 JSON 변환 처리
                ProjectUserSettings loadedSettings = JsonUtility.FromJson<ProjectUserSettings>(json); // JSON 사용자 설정 객체 변환

                if (loadedSettings == null) // 변환 결과 누락 여부 확인
                { // 비어 있는 변환 결과 처리
                    failureReason = "설정 JSON 결과가 비어 있습니다."; // 변환 결과 누락 원인 저장
                    return false; // 설정 변환 실패 반환
                } // 비어 있는 변환 결과 처리 완료

                if (loadedSettings.Version != ProjectUserSettings.CurrentVersion) // 지원하지 않는 설정 버전 여부 확인
                { // 버전 불일치 처리
                    failureReason = $"설정 파일 버전 {loadedSettings.Version}을 지원하지 않습니다."; // 버전 불일치 원인 저장
                    return false; // 설정 변환 실패 반환
                } // 버전 불일치 처리 완료

                loadedSettings.Validate(); // 불러온 설정값 안전 범위 보정
                settings = loadedSettings; // 정상 변환된 설정 결과 저장
                return true; // 설정 변환 성공 반환
            } // 설정 JSON 변환 처리 완료
            catch (Exception exception) // 손상된 JSON 변환 실패 처리
            { // JSON 예외 복구 정보 생성
                failureReason = exception.Message; // 실제 JSON 변환 오류 내용 저장
                return false; // 설정 변환 실패 반환
            } // 손상된 JSON 변환 실패 처리 완료
        } // JSON 사용자 설정 변환 시도 완료
    } // 파일 입출력과 분리된 순수 JSON 변환 기능 정의 완료
} // 프로젝트 공통 서비스 네임스페이스 정의 완료

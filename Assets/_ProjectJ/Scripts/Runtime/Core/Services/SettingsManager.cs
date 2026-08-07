using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조

namespace ProjectJ.Core.Services // 프로젝트 공통 서비스 네임스페이스 선언
{ // 설정 UI와 런타임 코드가 사용할 단일 진입점 정의
    public static class SettingsManager // SettingsService 접근을 통합하는 설정 관리자 선언
    { // 작업 복사본과 적용·저장·불러오기·초기화 공개 기능 정의
        public static bool IsReady // 설정 서비스 사용 가능 여부 반환
        { // 공통 서비스 초기화 상태 기반 준비 여부 계산
            get // 설정 관리자 준비 상태 조회
            { // 등록 서비스와 초기화 상태 확인
                return GameServiceRegistry.TryGet(out SettingsService service) && service.State == GameServiceState.Initialized; // 초기화 완료 설정 서비스 존재 여부 반환
            } // 설정 관리자 준비 상태 조회 완료
        } // 설정 서비스 사용 가능 여부 반환 완료

        public static ProjectUserSettings Current // 현재 설정의 안전한 읽기 복사본 반환
        { // 외부 코드의 직접 원본 수정 방지
            get // 현재 설정 스냅샷 조회
            { // 등록된 설정 서비스에서 복사본 생성
                return GetService().CreateSnapshot(); // 현재 설정 독립 복사본 반환
            } // 현재 설정 스냅샷 조회 완료
        } // 현재 설정의 안전한 읽기 복사본 반환 완료

        public static ProjectUserSettings CreateWorkingCopy() // 설정 화면에서 수정할 작업 복사본 생성
        { // 현재 저장 설정을 변경하지 않는 편집 데이터 준비
            return GetService().CreateSnapshot(); // 현재 설정 독립 복사본 반환
        } // 설정 화면에서 수정할 작업 복사본 생성 완료

        public static ProjectUserSettings CreateDefaultWorkingCopy() // 기본값 버튼 미리보기용 작업 복사본 생성
        { // 현재 저장 설정을 건드리지 않는 기본값 준비
            return ProjectUserSettings.CreateDefault(); // 현재 실행 환경 기반 기본 설정 반환
        } // 기본값 버튼 미리보기용 작업 복사본 생성 완료

        public static bool TryCreateWorkingCopy(out ProjectUserSettings settings) // 설정 서비스 준비 여부를 포함한 작업 복사본 생성 시도
        { // 초기 Scene이나 테스트 환경의 안전한 접근 지원
            if (!GameServiceRegistry.TryGet(out SettingsService service) || service.State != GameServiceState.Initialized) // 설정 서비스 준비 실패 여부 확인
            { // 설정 서비스 미준비 처리
                settings = null; // 작업 복사본 없음 결과 저장
                return false; // 작업 복사본 생성 실패 반환
            } // 설정 서비스 미준비 처리 완료

            settings = service.CreateSnapshot(); // 현재 설정 독립 복사본 생성
            return true; // 작업 복사본 생성 성공 반환
        } // 설정 서비스 준비 여부를 포함한 작업 복사본 생성 시도 완료

        public static bool Apply(ProjectUserSettings workingCopy) // 작업 복사본을 실제 설정으로 적용하고 저장
        { // 51일차 적용 버튼이 사용할 통합 기능
            return GetService().ApplySettings(workingCopy); // 설정 서비스 전체 적용 결과 반환
        } // 작업 복사본을 실제 설정으로 적용하고 저장 완료

        public static bool Save() // 현재 설정을 설정 파일에 다시 저장
        { // 명시적 저장 요청 통합
            return GetService().SaveCurrent(); // 현재 설정 저장 결과 반환
        } // 현재 설정을 설정 파일에 다시 저장 완료

        public static bool Reload() // 디스크 설정 파일을 다시 불러와 런타임에 적용
        { // 설정 취소와 외부 변경 복구에 사용할 기능
            return GetService().ReloadFromDisk(); // 설정 다시 불러오기 결과 반환
        } // 디스크 설정 파일을 다시 불러와 런타임에 적용 완료

        public static bool ResetToDefaults() // 현재 설정을 기본값으로 즉시 초기화하고 저장
        { // 전체 설정 초기화 기능 통합
            return GetService().ResetToDefaults(); // 기본값 초기화와 저장 결과 반환
        } // 현재 설정을 기본값으로 즉시 초기화하고 저장 완료

        private static SettingsService GetService() // 초기화 완료 설정 서비스 조회
        { // 설정 관리자 공개 기능의 공통 서비스 확인
            if (!GameServiceRegistry.TryGet(out SettingsService service)) // 설정 서비스 등록 여부 확인
            { // 설정 서비스 누락 처리
                throw new System.InvalidOperationException("SettingsService가 등록되지 않았습니다."); // 미등록 설정 서비스 접근 예외 발생
            } // 설정 서비스 누락 처리 완료

            if (service.State != GameServiceState.Initialized) // 설정 서비스 초기화 완료 여부 확인
            { // 초기화 전 접근 처리
                ProjectLog.Warning(ProjectLogCategory.Core, "설정 서비스 초기화 완료 전에 SettingsManager가 호출되었습니다.", "SETTINGS_MANAGER_NOT_READY"); // 초기화 전 접근 경고 출력
                throw new System.InvalidOperationException("SettingsService 초기화가 완료되지 않았습니다."); // 초기화 전 접근 예외 발생
            } // 초기화 전 접근 처리 완료

            return service; // 준비 완료 설정 서비스 반환
        } // 초기화 완료 설정 서비스 조회 완료
    } // 작업 복사본과 적용·저장·불러오기·초기화 공개 기능 정의 완료
} // 프로젝트 공통 서비스 네임스페이스 정의 완료

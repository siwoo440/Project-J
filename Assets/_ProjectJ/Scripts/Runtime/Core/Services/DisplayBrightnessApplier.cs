using UnityEngine; // Unity GameObject와 Camera 기능 참조
using UnityEngine.Rendering; // URP 공통 Volume 기능 참조
using UnityEngine.Rendering.Universal; // URP Color Adjustments와 Camera 확장 기능 참조
using UnityEngine.SceneManagement; // Scene 변경 뒤 카메라 재적용 기능 참조

namespace ProjectJ.Core.Services // 프로젝트 공통 서비스 네임스페이스 선언
{ // 사용자 화면 밝기 런타임 적용 기능 구성
    public static class DisplayBrightnessApplier // 전역 URP Post Exposure 기반 밝기 적용기 선언
    { // Scene 전환에도 유지되는 전역 Volume 관리 기능 구성
        private const string RuntimeObjectName = "ProjectJ_DisplayBrightness"; // 런타임 전역 밝기 오브젝트 이름
        private const int DefaultLayerIndex = 0; // 전역 Volume이 사용할 Default Layer 번호
        private static GameObject runtimeObject; // 전역 밝기 Volume 소유 오브젝트
        private static Volume globalVolume; // 전역 URP Volume 컴포넌트
        private static VolumeProfile runtimeProfile; // 런타임 전용 Volume Profile
        private static ColorAdjustments colorAdjustments; // Post Exposure 조정 컴포넌트
        private static bool sceneHookRegistered; // Scene 로드 이벤트 연결 여부

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // Domain Reload 설정과 무관한 정적 상태 초기화
        private static void ResetRuntimeState() // Play Mode 시작 시 정적 참조 초기화
        { // 이전 Play Mode 이벤트와 참조 정리
            SceneManager.sceneLoaded -= HandleSceneLoaded; // 이전 Scene 로드 이벤트 중복 연결 제거
            runtimeObject = null; // 런타임 오브젝트 정적 참조 초기화
            globalVolume = null; // Volume 정적 참조 초기화
            runtimeProfile = null; // Volume Profile 정적 참조 초기화
            colorAdjustments = null; // Color Adjustments 정적 참조 초기화
            sceneHookRegistered = false; // Scene 이벤트 등록 상태 초기화
        } // Play Mode 시작 시 정적 참조 초기화 마무리

        public static void Apply(float brightness) // 저장된 밝기 값을 현재 URP 화면에 적용
        { // 전역 Volume 생성과 Post Exposure 갱신
            EnsureRuntimeVolume(); // 전역 밝기 Volume 준비
            float safeBrightness = Mathf.Clamp(brightness, 0.5f, 1.5f); // 저장 밝기 안전 범위 보정
            colorAdjustments.postExposure.overrideState = true; // Post Exposure 재정의 활성화
            colorAdjustments.postExposure.value = CalculatePostExposure(safeBrightness); // 밝기 배율을 Exposure 값으로 변환 적용
            EnablePostProcessingForAllCameras(); // 현재 Scene 모든 카메라의 후처리 활성화
        } // 저장된 밝기 값을 현재 URP 화면에 적용 마무리

        public static float CalculatePostExposure(float brightness) // 설정 밝기 배율을 URP Post Exposure 값으로 변환
        { // 50퍼센트부터 150퍼센트를 -1부터 +1 스톱으로 변환
            float safeBrightness = Mathf.Clamp(brightness, 0.5f, 1.5f); // 밝기 입력 안전 범위 보정
            return (safeBrightness - 1f) * 2f; // 100퍼센트를 0 스톱으로 하는 Exposure 반환
        } // 설정 밝기 배율을 URP Post Exposure 값으로 변환 마무리

        private static void EnsureRuntimeVolume() // 전역 밝기 Volume과 Profile 준비
        { // 최초 적용 시 런타임 후처리 인프라 생성
            if (runtimeObject != null && globalVolume != null && runtimeProfile != null && colorAdjustments != null) // 기존 전역 밝기 인프라 유효성 확인
            { // 이미 준비된 인프라 재사용
                RegisterSceneHook(); // Scene 로드 이벤트 연결 상태 보장
                return; // 중복 생성 방지
            } // 이미 준비된 인프라 재사용 마무리

            runtimeObject = new GameObject(RuntimeObjectName); // 전역 밝기 관리 오브젝트 생성
            runtimeObject.layer = DefaultLayerIndex; // Default Layer 적용
            Object.DontDestroyOnLoad(runtimeObject); // Scene 전환 뒤 전역 밝기 오브젝트 유지
            globalVolume = runtimeObject.AddComponent<Volume>(); // 전역 Volume 컴포넌트 추가
            globalVolume.isGlobal = true; // 위치와 무관한 전역 Volume 설정
            globalVolume.priority = 1000f; // 사용자 밝기 설정 우선순위 적용
            globalVolume.weight = 1f; // 사용자 밝기 효과 전체 가중치 적용
            runtimeProfile = ScriptableObject.CreateInstance<VolumeProfile>(); // 런타임 전용 Volume Profile 생성
            runtimeProfile.hideFlags = HideFlags.DontSave; // Scene과 Asset에 임시 Profile 저장 방지
            colorAdjustments = runtimeProfile.Add<ColorAdjustments>(true); // Color Adjustments Override 생성
            globalVolume.profile = runtimeProfile; // 전역 Volume에 런타임 Profile 연결
            RegisterSceneHook(); // Scene 전환 뒤 카메라 후처리 재활성화 이벤트 연결
        } // 전역 밝기 Volume과 Profile 준비 마무리

        private static void RegisterSceneHook() // Scene 로드 이벤트 한 번만 연결
        { // 새 Scene 카메라 후처리 자동 활성화 준비
            if (sceneHookRegistered) // 기존 Scene 이벤트 연결 여부 확인
            { // 중복 이벤트 연결 방지
                return; // 기존 연결 유지
            } // 중복 이벤트 연결 방지 마무리

            SceneManager.sceneLoaded += HandleSceneLoaded; // Scene 로드 완료 이벤트 연결
            sceneHookRegistered = true; // Scene 이벤트 연결 상태 저장
        } // Scene 로드 이벤트 한 번만 연결 마무리

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode loadMode) // 새 Scene 로드 뒤 카메라 후처리 활성화
        { // Scene 종류와 무관한 모든 카메라 재검사
            EnablePostProcessingForAllCameras(); // 새 Scene 카메라에 URP 후처리 활성화
        } // 새 Scene 로드 뒤 카메라 후처리 활성화 마무리

        private static void EnablePostProcessingForAllCameras() // 현재 로드된 모든 Camera에 URP 후처리 활성화
        { // 사용자 밝기 Volume이 실제 화면에 반영되도록 Camera 설정 보정
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None); // 활성과 비활성 Camera 전체 조회

            for (int index = 0; index < cameras.Length; index++) // 현재 로드된 모든 Camera 순회
            { // 현재 Camera의 URP 추가 데이터 준비
                Camera camera = cameras[index]; // 현재 Camera 참조 조회

                if (camera == null) // 파괴된 Camera 참조 여부 확인
                { // 잘못된 Camera 항목 건너뛰기
                    continue; // 다음 Camera 검사
                } // 잘못된 Camera 항목 건너뛰기 마무리

                UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData(); // URP 추가 Camera 데이터 조회 또는 자동 생성
                cameraData.renderPostProcessing = true; // 현재 Camera 후처리 활성화
                int currentMask = cameraData.volumeLayerMask.value; // 현재 Camera Volume Layer Mask 조회
                cameraData.volumeLayerMask = currentMask | (1 << DefaultLayerIndex); // Default Layer 전역 Volume 포함 보장
            } // 현재 Camera의 URP 추가 데이터 준비 마무리
        } // 현재 로드된 모든 Camera에 URP 후처리 활성화 마무리
    } // 전역 URP Post Exposure 기반 밝기 적용기 마무리
} // 프로젝트 공통 서비스 네임스페이스 마무리

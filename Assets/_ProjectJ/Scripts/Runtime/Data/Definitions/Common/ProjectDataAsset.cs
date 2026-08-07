using UnityEngine; // Unity ScriptableObject와 직렬화 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    public abstract class ProjectDataAsset : ScriptableObject // 모든 프로젝트 데이터 에셋의 공통 기반 선언
    {
        [SerializeField] private string dataId; // 데이터의 영구 고유 ID 저장
        [SerializeField] private string displayName; // 개발과 UI에서 확인할 데이터 표시 이름 저장
        [SerializeField] private ProjectDataVersion version = new ProjectDataVersion(1, 0, 0); // 데이터 구조와 내용 버전 저장

        public abstract ProjectDataCategory Category { get; } // 파생 데이터 에셋의 분류 반환 규칙 선언
        public string DataId => dataId; // 데이터 고유 ID 반환
        public string DisplayName => displayName; // 데이터 표시 이름 반환
        public ProjectDataVersion Version => version; // 데이터 버전 반환

#if UNITY_EDITOR
        public void SetEditorIdentity(string newDataId, string newDisplayName, ProjectDataVersion newVersion) // Editor 도구와 EditMode 테스트용 식별 정보 설정
        {
            dataId = newDataId; // 전달된 데이터 ID 저장
            displayName = newDisplayName; // 전달된 표시 이름 저장
            version = newVersion; // 전달된 데이터 버전 저장
        }
#endif
    }
}

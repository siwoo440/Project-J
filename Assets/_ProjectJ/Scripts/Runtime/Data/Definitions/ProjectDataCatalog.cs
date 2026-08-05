using System.Collections.Generic; // 데이터 에셋 목록 기능 참조
using UnityEngine; // Unity ScriptableObject와 직렬화 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    [CreateAssetMenu(fileName = "ProjectDataCatalog", menuName = "Project J/Data/Runtime Catalog")] // 런타임 데이터 카탈로그 생성 메뉴 등록
    public sealed class ProjectDataCatalog : ScriptableObject // 런타임 포함 데이터 에셋 목록 선언
    {
        [SerializeField] private List<ProjectDataAsset> assets = new List<ProjectDataAsset>(); // 빌드에 포함할 데이터 에셋 목록 저장

        public IReadOnlyList<ProjectDataAsset> Assets => assets; // 읽기 전용 데이터 에셋 목록 반환
        public int Count => assets.Count; // 등록된 데이터 에셋 수 반환

#if UNITY_EDITOR
        public void SetEditorAssets(IReadOnlyList<ProjectDataAsset> newAssets) // Editor 카탈로그 데이터 에셋 목록 교체
        {
            assets.Clear(); // 기존 카탈로그 목록 제거

            if (newAssets == null) // 새 데이터 목록 누락 여부 확인
            {
                return; // 빈 카탈로그 상태 유지
            }

            for (int index = 0; index < newAssets.Count; index++) // 새 데이터 목록 전체 순회
            {
                assets.Add(newAssets[index]); // 현재 데이터 에셋 카탈로그 등록
            }
        }
#endif
    }
}

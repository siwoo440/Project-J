using UnityEngine; // Unity ScriptableObject 생성 메뉴 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    [CreateAssetMenu(fileName = "AudioData", menuName = "Project J/Data/Audio")] // Project 창 데이터 에셋 생성 메뉴 등록
    public sealed class AudioDataDefinition : ProjectDataAsset // Audio 데이터 정의 에셋 선언
    {
        public override ProjectDataCategory Category => ProjectDataCategory.Audio; // Audio 데이터 분류 반환
    }
}

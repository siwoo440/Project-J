using ProjectJ.Core; // 런타임 어셈블리 형식 참조

namespace ProjectJ.Editor // 프로젝트 에디터 전용 네임스페이스 선언
{
    internal static class EditorAssemblyMarker // 에디터 어셈블리 연결 확인용 형식 선언
    {
        internal static string GetRuntimeAssemblyName() // 런타임 어셈블리 참조 상태 반환
        {
            return RuntimeAssemblyMarker.AssemblyName; // 런타임 어셈블리 이름 반환
        }
    }
}

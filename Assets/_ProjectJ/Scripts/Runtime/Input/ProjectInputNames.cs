namespace ProjectJ.Input // 프로젝트 입력 네임스페이스 선언
{
    public static class ProjectInputNames // 입력 에셋의 경로와 이름 상수 관리 형식 선언
    {
        public const string AssetPath = "Assets/_ProjectJ/Settings/Input/InputSystem_Actions.inputactions"; // 프로젝트 입력 액션 에셋 경로 선언
        public const string KeyboardMouseScheme = "Keyboard&Mouse"; // 키보드와 마우스 Control Scheme 이름 선언
        public const string GamepadScheme = "Gamepad"; // 게임패드 Control Scheme 이름 선언

        public static class Gameplay // Gameplay 액션 맵 이름 상수 모음 선언
        {
            public const string Map = "Gameplay"; // Gameplay 액션 맵 이름 선언
            public const string Move = "Move"; // 이동 액션 이름 선언
            public const string Look = "Look"; // 시점 액션 이름 선언
            public const string Jump = "Jump"; // 점프 액션 이름 선언
            public const string Sprint = "Sprint"; // 달리기 액션 이름 선언
            public const string Crouch = "Crouch"; // 앉기 액션 이름 선언
            public const string Push = "Push"; // 밀치기 액션 이름 선언
            public const string UseItem = "UseItem"; // 아이템 사용 액션 이름 선언
            public const string SelectPreviousItem = "SelectPreviousItem"; // 이전 아이템 슬롯 선택 액션 이름 선언
            public const string SelectNextItem = "SelectNextItem"; // 다음 아이템 슬롯 선택 액션 이름 선언
            public const string ShowItem = "ShowItem"; // 아이템 보여주기 액션 이름 선언
            public const string DropItem = "DropItem"; // 아이템 버리기 액션 이름 선언
            public const string Interact = "Interact"; // 상호작용 액션 이름 선언
            public const string Scoreboard = "Scoreboard"; // 순위표 액션 이름 선언
            public const string Pause = "Pause"; // 일시정지 액션 이름 선언
        }

        public static class UI // UI 액션 맵 이름 상수 모음 선언
        {
            public const string Map = "UI"; // UI 액션 맵 이름 선언
            public const string Navigate = "Navigate"; // UI 이동 액션 이름 선언
            public const string Submit = "Submit"; // UI 확인 액션 이름 선언
            public const string Cancel = "Cancel"; // UI 취소 액션 이름 선언
            public const string Point = "Point"; // UI 포인터 위치 액션 이름 선언
            public const string Click = "Click"; // UI 클릭 액션 이름 선언
            public const string ScrollWheel = "ScrollWheel"; // UI 스크롤 액션 이름 선언
        }
    }
}

using System; // 공통 C# 기능 사용
using System.Collections.Generic; // Dictionary와 IReadOnlyList 기능 사용
using System.IO; // 폴더와 Meta 파일 기능 사용
using ProjectJ.Map; // 기존 정육면체 맵 모듈 기능 사용
using UnityEditor; // Unity Editor 자산 생성 기능 사용
using UnityEngine; // Unity 오브젝트와 수학 기능 사용

namespace ProjectJ.Editor // Project J Editor 기능 네임스페이스
{
    public static class Day144CubeModuleSetup // 144일차 10m 정육면체 모듈 생성 도구
    {
        private const float ModuleSize = 10f; // 정육면체 모듈 한 변 크기
        private const float WallThickness = 0.4f; // Greybox 벽과 바닥 두께
        private const float FloorCenterY = -4.8f; // 기본 바닥 중심 높이
        private const float FloorTopY = -4.6f; // 기본 바닥 윗면 높이
        private const int ExpectedDay144PrefabCount = 40; // 144일차 바리에이션 예상 개수
        private const int ExpectedDay25PrefabCount = 7; // 기존 Day25 기본 모듈 예상 개수
        private const string ModuleRootPath = "Assets/ProjectJ/Prefabs/Map/Modules"; // 공통 모듈 루트 경로
        private const string Day25RootPath = ModuleRootPath + "/Day25"; // 기존 기본 모듈 경로
        private const string Day144RootPath = ModuleRootPath + "/Day144"; // 신규 바리에이션 경로

        private static readonly MapModuleFaceDirection[] AllDirections = // 6방향 Socket 순서
        {
            MapModuleFaceDirection.North, // 북쪽 방향
            MapModuleFaceDirection.South, // 남쪽 방향
            MapModuleFaceDirection.East, // 동쪽 방향
            MapModuleFaceDirection.West, // 서쪽 방향
            MapModuleFaceDirection.Up, // 위쪽 방향
            MapModuleFaceDirection.Down // 아래쪽 방향
        }; // 6방향 Socket 순서 종료

        private static readonly LegacyPrefabSpec[] LegacyPrefabSpecs = // Day25 기존 Prefab ID와 GUID 목록
        {
            new LegacyPrefabSpec("PJ_Module_Branch_SouthNorthEast", "cbd94446076b526489369d6502c3d324", LegacyModuleKind.Branch), // Branch 정보
            new LegacyPrefabSpec("PJ_Module_Corner_SouthEast", "9a81b0754e0c3f34f853f708d8817f88", LegacyModuleKind.Corner), // Corner 정보
            new LegacyPrefabSpec("PJ_Module_Drop_SouthNorth_EastDrop", "509fa0d334d61c043b1b227c27aa3341", LegacyModuleKind.Drop), // Drop 정보
            new LegacyPrefabSpec("PJ_Module_Merge_SouthWestNorth", "b7e6f67971d95ca4f8048aeff8cce916", LegacyModuleKind.Merge), // Merge 정보
            new LegacyPrefabSpec("PJ_Module_Start_SouthUp", "477f30a163813d64eb22869f95a2e470", LegacyModuleKind.Start), // Start 정보
            new LegacyPrefabSpec("PJ_Module_Straight_SouthNorth", "b8638a5119fdef649986356225db4ded", LegacyModuleKind.Straight), // Straight 정보
            new LegacyPrefabSpec("PJ_Module_Vertical_DownUp", "1691b72085d119146ad1985017ca29b2", LegacyModuleKind.Vertical) // Vertical 정보
        }; // Day25 기존 Prefab 목록 종료

        [MenuItem("ProjectJ/Day144/1. Rebuild All Modules To 10m And Add Variations")] // 전체 재구축 메뉴 등록
        public static void RebuildAllModulesToTenMeters() // Day25와 Day144를 10m 규격으로 일괄 생성
        {
            EnsureFolderWithGuid(ModuleRootPath, "ca95e270e21e5664b8b60f89d82b2923"); // Modules 폴더 원래 GUID 보장
            EnsureFolderWithGuid(Day25RootPath, "76b5edeeceb1f5348ac52cb0fe403928"); // Day25 폴더 원래 GUID 보장
            DeleteDay144ModulesInternal(); // 기존 Day144 생성물 정리
            EnsureFolder(Day144RootPath); // Day144 폴더 생성
            RebuildLegacyDay25Modules(); // 기존 7종을 10m 규격으로 재생성
            CreateDay144Modules(); // 신규 40종 바리에이션 생성
            AssetDatabase.SaveAssets(); // 생성 자산 저장
            AssetDatabase.Refresh(); // Project 창 갱신
            ValidateTenMeterModules(); // 전체 모듈 검증
            Debug.Log("[ProjectJ][Day144] Day25 7종 + Day144 40종을 10x10x10 정육면체 규격으로 생성 완료."); // 완료 로그 출력
        }

        [MenuItem("ProjectJ/Day144/2. Validate 10m Cube Modules")] // 전체 검증 메뉴 등록
        public static void ValidateTenMeterModules() // 10m 모듈 규격 검증
        {
            int errorCount = 0; // 전체 오류 수 초기화
            errorCount += ValidateFolder(Day25RootPath, ExpectedDay25PrefabCount); // Day25 모듈 검증
            errorCount += ValidateFolder(Day144RootPath, ExpectedDay144PrefabCount); // Day144 모듈 검증

            if (!Mathf.Approximately(MapModule.DefaultModuleSize, ModuleSize)) // Runtime 기본 크기 검사
            {
                Debug.LogError("[ProjectJ][Day144] MapModule.DefaultModuleSize가 10이 아닙니다."); // Runtime 규격 오류 출력
                errorCount++; // 오류 수 증가
            }

            if (errorCount == 0) // 전체 검증 성공 검사
            {
                Debug.Log("[ProjectJ][Day144] 10m Cube Module Validation PASS - Day25 7종 + Day144 40종 / 10x10x10 / 6 Socket."); // 검증 성공 로그 출력
            }
            else // 전체 검증 실패 처리
            {
                Debug.LogError("[ProjectJ][Day144] 10m Cube Module Validation FAIL - 오류 " + errorCount + "개."); // 검증 실패 로그 출력
            }
        }

        [MenuItem("ProjectJ/Day144/3. Delete Day144 Variations Only")] // Day144만 삭제 메뉴 등록
        public static void DeleteDay144VariationsOnly() // Day144 신규 모듈만 삭제
        {
            DeleteDay144ModulesInternal(); // Day144 폴더 삭제
            AssetDatabase.SaveAssets(); // 삭제 내용 저장
            AssetDatabase.Refresh(); // Project 창 갱신
            Debug.Log("[ProjectJ][Day144] Day144 바리에이션만 삭제 완료. Day25 10m 기본 모듈은 유지됩니다."); // 완료 로그 출력
        }

        [MenuItem("ProjectJ/Day144/4. Rebuild Day25 Base Modules To 10m")] // Day25만 재생성 메뉴 등록
        public static void RebuildDay25BaseModulesOnly() // Day25 7종만 10m로 재생성
        {
            EnsureFolderWithGuid(ModuleRootPath, "ca95e270e21e5664b8b60f89d82b2923"); // Modules 폴더 GUID 보장
            EnsureFolderWithGuid(Day25RootPath, "76b5edeeceb1f5348ac52cb0fe403928"); // Day25 폴더 GUID 보장
            RebuildLegacyDay25Modules(); // Day25 모듈 재생성
            AssetDatabase.SaveAssets(); // 생성 내용 저장
            AssetDatabase.Refresh(); // Project 창 갱신
            Debug.Log("[ProjectJ][Day144] 기존 Day25 7종을 10x10x10 규격으로 재생성 완료."); // 완료 로그 출력
        }

        private static int ValidateFolder(string folderPath, int expectedCount) // 폴더 단위 Prefab 규격 검증
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath }); // Prefab GUID 검색
            int errorCount = 0; // 현재 폴더 오류 수 초기화

            if (prefabGuids.Length != expectedCount) // 예상 Prefab 개수 검사
            {
                Debug.LogError("[ProjectJ][Day144] " + folderPath + " 예상 Prefab " + expectedCount + "종 / 현재 " + prefabGuids.Length + "종."); // 개수 오류 출력
                errorCount++; // 오류 수 증가
            }

            for (int index = 0; index < prefabGuids.Length; index++) // Prefab 전체 순회
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[index]); // Prefab 경로 조회
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath); // Prefab 로드

                if (prefab == null) // Prefab 로드 실패 검사
                {
                    Debug.LogError("[ProjectJ][Day144] Prefab 로드 실패: " + prefabPath); // 로드 오류 출력
                    errorCount++; // 오류 수 증가
                    continue; // 다음 Prefab 검사
                }

                MapModule module = prefab.GetComponent<MapModule>(); // MapModule 조회

                if (module == null) // MapModule 누락 검사
                {
                    Debug.LogError("[ProjectJ][Day144] MapModule 누락: " + prefab.name, prefab); // 컴포넌트 오류 출력
                    errorCount++; // 오류 수 증가
                    continue; // 다음 Prefab 검사
                }

                if (!module.IsDefinitionValid()) // 6 Socket과 Entrance Exit 규칙 검사
                {
                    Debug.LogError("[ProjectJ][Day144] Module 정의 오류: " + prefab.name, prefab); // 정의 오류 출력
                    errorCount++; // 오류 수 증가
                }

                if (!Mathf.Approximately(module.ModuleSize, ModuleSize)) // 10m 크기 검사
                {
                    Debug.LogError("[ProjectJ][Day144] Module Size가 10이 아님: " + prefab.name, prefab); // 크기 오류 출력
                    errorCount++; // 오류 수 증가
                }

                if (module.ExitCount < 1 || module.ExitCount > 4) // Exit 1~4 규칙 검사
                {
                    Debug.LogError("[ProjectJ][Day144] Exit 개수 1~4 규칙 위반: " + prefab.name, prefab); // Exit 오류 출력
                    errorCount++; // 오류 수 증가
                }

                IReadOnlyList<MapModuleSocket> sockets = module.Sockets; // Socket 목록 조회

                if (sockets == null || sockets.Count != 6) // Socket 6개 규칙 검사
                {
                    Debug.LogError("[ProjectJ][Day144] Socket 6개 규칙 위반: " + prefab.name, prefab); // Socket 개수 오류 출력
                    errorCount++; // 오류 수 증가
                    continue; // 세부 Socket 검사 생략
                }

                for (int socketIndex = 0; socketIndex < sockets.Count; socketIndex++) // Socket 전체 순회
                {
                    MapModuleSocket socket = sockets[socketIndex]; // 현재 Socket 조회
                    Vector3 expectedPosition = MapModule.GetDirectionVector(socket.Direction) * (ModuleSize * 0.5f); // ±5m Socket 위치 계산
                    float socketError = Vector3.Distance(socket.transform.localPosition, expectedPosition); // 실제 위치 오차 계산

                    if (socketError > 0.001f) // Socket Cell 경계 정렬 검사
                    {
                        Debug.LogError("[ProjectJ][Day144] Socket이 ±5m Cell 경계와 맞지 않음: " + prefab.name + " / " + socket.name, prefab); // Socket 위치 오류 출력
                        errorCount++; // 오류 수 증가
                    }

                    if (socket.State == MapModuleFaceState.Exit && socket.Direction == MapModuleFaceDirection.Down) // 정상 진행 하강 Exit 검사
                    {
                        Debug.LogError("[ProjectJ][Day144] 정상 경로 Down Exit 금지 규칙 위반: " + prefab.name, prefab); // 하강 경로 오류 출력
                        errorCount++; // 오류 수 증가
                    }
                }
            }

            return errorCount; // 현재 폴더 오류 수 반환
        }

        private static void RebuildLegacyDay25Modules() // 기존 Day25 7종을 10m로 재생성
        {
            for (int index = 0; index < LegacyPrefabSpecs.Length; index++) // 기존 규격 전체 순회
            {
                LegacyPrefabSpec spec = LegacyPrefabSpecs[index]; // 현재 기존 모듈 규격 조회
                string prefabPath = Day25RootPath + "/" + spec.ModuleId + ".prefab"; // 기존 Prefab 경로 계산
                EnsurePrefabMetaGuid(prefabPath, spec.Guid); // 기존 GUID 보장
                Dictionary<MapModuleFaceDirection, MapModuleFaceState> states = CreateLegacyStates(spec.Kind); // 기존 Face 상태 구성
                TraversalStyle style = GetLegacyTraversalStyle(spec.Kind); // 기존 이동 구조 선택
                CreateModulePrefab(Day25RootPath, spec.ModuleId, states, style); // 10m Prefab 저장 또는 덮어쓰기
                EnsurePrefabMetaGuid(prefabPath, spec.Guid); // 저장 후 기존 GUID 재확인
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate); // GUID 즉시 반영
            }

            Debug.Log("[ProjectJ][Day144] Day25 기본 모듈 7종을 기존 GUID 유지 상태로 10m 규격 재생성 완료."); // 완료 로그 출력
        }

        private static void CreateDay144Modules() // 144일차 바리에이션 40종 생성
        {
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Straight_SouthNorth", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.None); // 기본 직선 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Straight_Narrow", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.NarrowLane); // 좁은 직선 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Straight_Slalom", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.Slalom); // 슬라럼 직선 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Straight_Pillars", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.Pillars); // 기둥 직선 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Corner_SouthEast", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.East, MapModuleFaceState.Exit), TraversalStyle.None); // 우회전 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Corner_SouthWest", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.West, MapModuleFaceState.Exit), TraversalStyle.None); // 좌회전 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Corner_SouthEast_Narrow", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.East, MapModuleFaceState.Exit), TraversalStyle.CornerNarrowEast); // 좁은 우회전 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Corner_SouthWest_Narrow", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.West, MapModuleFaceState.Exit), TraversalStyle.CornerNarrowWest); // 좁은 좌회전 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Jump_Single", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.JumpSingle); // 단일 점프 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Jump_Double", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.JumpDouble); // 2연속 점프 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Jump_SteppingStones", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.JumpStones); // 징검다리 점프 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Bridge_Narrow", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.NarrowBridge); // 좁은 다리 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_LowPassage_Center", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.LowCenter); // 중앙 앉기 통로 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_LowPassage_Left", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.LowLeft); // 좌측 앉기 통로 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_LowPassage_Right", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.LowRight); // 우측 앉기 통로 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_RaisedCenter_SouthNorth", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.RaisedCenter); // 중앙 상승 지형 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Branch2_SouthNorthEast", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit, MapModuleFaceDirection.East, MapModuleFaceState.Exit), TraversalStyle.None); // 우측 2분기 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Branch2_SouthNorthWest", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit, MapModuleFaceDirection.West, MapModuleFaceState.Exit), TraversalStyle.None); // 좌측 2분기 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Branch3_SouthNorthEastWest", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit, MapModuleFaceDirection.East, MapModuleFaceState.Exit, MapModuleFaceDirection.West, MapModuleFaceState.Exit), TraversalStyle.None); // 3분기 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_BranchVertical_SouthNorthUp", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit, MapModuleFaceDirection.Up, MapModuleFaceState.Exit), TraversalStyle.SideRampEast); // 직진과 상승 분기 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_BranchVertical_SouthEastUp", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.East, MapModuleFaceState.Exit, MapModuleFaceDirection.Up, MapModuleFaceState.Exit), TraversalStyle.SideRampWest); // 우회전과 상승 분기 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Branch4_SouthNorthEastWestUp", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit, MapModuleFaceDirection.East, MapModuleFaceState.Exit, MapModuleFaceDirection.West, MapModuleFaceState.Exit, MapModuleFaceDirection.Up, MapModuleFaceState.Exit), TraversalStyle.SideRampEast); // 4방향 분기 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Merge2_SouthWestNorth", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.West, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.None); // 서쪽 2합류 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Merge2_SouthEastNorth", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.East, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.None); // 동쪽 2합류 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Merge3_SouthEastWestNorth", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.East, MapModuleFaceState.Entrance, MapModuleFaceDirection.West, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.None); // 3합류 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_MergeVertical_DownSouthNorth", CreateStates(MapModuleFaceDirection.Down, MapModuleFaceState.Entrance, MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.DownLanding); // 아래와 남쪽 합류 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_MergeVertical_DownEastNorth", CreateStates(MapModuleFaceDirection.Down, MapModuleFaceState.Entrance, MapModuleFaceDirection.East, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.DownLanding); // 아래와 동쪽 합류 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Merge4_SouthEastWestDownNorth", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.East, MapModuleFaceState.Entrance, MapModuleFaceDirection.West, MapModuleFaceState.Entrance, MapModuleFaceDirection.Down, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.DownLanding); // 4경로 합류 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_VerticalRamp_DownUp", CreateStates(MapModuleFaceDirection.Down, MapModuleFaceState.Entrance, MapModuleFaceDirection.Up, MapModuleFaceState.Exit), TraversalStyle.VerticalRamp); // 수직 Ramp 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_VerticalStairs_DownUp", CreateStates(MapModuleFaceDirection.Down, MapModuleFaceState.Entrance, MapModuleFaceDirection.Up, MapModuleFaceState.Exit), TraversalStyle.VerticalStairs); // 수직 계단 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_VerticalPlatforms_DownUp", CreateStates(MapModuleFaceDirection.Down, MapModuleFaceState.Entrance, MapModuleFaceDirection.Up, MapModuleFaceState.Exit), TraversalStyle.VerticalPlatforms); // 수직 점프 발판 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_ClimbTurn_SouthUp", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.Up, MapModuleFaceState.Exit), TraversalStyle.SouthToUpRamp); // 남쪽에서 위쪽 전환 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_ClimbTurn_EastUp", CreateStates(MapModuleFaceDirection.East, MapModuleFaceState.Entrance, MapModuleFaceDirection.Up, MapModuleFaceState.Exit), TraversalStyle.EastToUpRamp); // 동쪽에서 위쪽 전환 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_LandingTurn_DownNorth", CreateStates(MapModuleFaceDirection.Down, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit), TraversalStyle.DownLanding); // 아래에서 북쪽 전환 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_LandingTurn_DownEast", CreateStates(MapModuleFaceDirection.Down, MapModuleFaceState.Entrance, MapModuleFaceDirection.East, MapModuleFaceState.Exit), TraversalStyle.DownLanding); // 아래에서 동쪽 전환 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_LandingTurn_DownWest", CreateStates(MapModuleFaceDirection.Down, MapModuleFaceState.Entrance, MapModuleFaceDirection.West, MapModuleFaceState.Exit), TraversalStyle.DownLanding); // 아래에서 서쪽 전환 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Drop_SouthNorth_EastDrop", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit, MapModuleFaceDirection.East, MapModuleFaceState.Drop), TraversalStyle.None); // 동쪽 낙하 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Drop_SouthNorth_WestDrop", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit, MapModuleFaceDirection.West, MapModuleFaceState.Drop), TraversalStyle.None); // 서쪽 낙하 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_Drop_SouthNorth_DoubleSide", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit, MapModuleFaceDirection.East, MapModuleFaceState.Drop, MapModuleFaceDirection.West, MapModuleFaceState.Drop), TraversalStyle.NarrowBridge); // 양측 낙하 좁은 길 생성
            CreateModulePrefab(Day144RootPath, "PJ144_Module_DropCorner_SouthEast_WestDrop", CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.East, MapModuleFaceState.Exit, MapModuleFaceDirection.West, MapModuleFaceState.Drop), TraversalStyle.CornerNarrowEast); // 코너 낙하 생성
        }

        private static GameObject CreateModulePrefab(string rootPath, string moduleId, Dictionary<MapModuleFaceDirection, MapModuleFaceState> states, TraversalStyle traversalStyle) // 공통 Prefab 생성
        {
            string prefabPath = rootPath + "/" + moduleId + ".prefab"; // 저장 경로 계산
            GameObject root = new GameObject(moduleId); // 임시 모듈 루트 생성
            MapModule module = root.AddComponent<MapModule>(); // 기존 MapModule 추가
            Transform geometryRoot = CreateEmptyChild(root.transform, "Geometry"); // Geometry 부모 생성
            Transform socketsRoot = CreateEmptyChild(root.transform, "Sockets"); // Sockets 부모 생성
            Transform gameplayRoot = CreateEmptyChild(root.transform, "Gameplay"); // Gameplay 부모 생성
            CreateEmptyChild(gameplayRoot, "ObstacleSpawnAreas"); // 장애물 영역 부모 생성
            CreateEmptyChild(gameplayRoot, "ItemSpawnAreas"); // 아이템 영역 부모 생성
            CreateEmptyChild(gameplayRoot, "NoSpawnAreas"); // 배치 금지 영역 부모 생성
            MapModuleSocket[] sockets = new MapModuleSocket[AllDirections.Length]; // 6방향 Socket 배열 생성

            for (int index = 0; index < AllDirections.Length; index++) // 6방향 순회
            {
                MapModuleFaceDirection direction = AllDirections[index]; // 현재 방향 조회
                MapModuleFaceState state = states[direction]; // 현재 Face 상태 조회

                if (state == MapModuleFaceState.Closed && ShouldCreateDefaultClosedFace(direction, traversalStyle)) // 기본 막힌 Face 생성 여부 검사
                {
                    CreateFaceGeometry(geometryRoot, direction); // 벽 또는 바닥 또는 천장 생성
                }

                sockets[index] = CreateSocket(socketsRoot, direction, state); // Socket 생성
            }

            CreateTraversalGeometry(geometryRoot, gameplayRoot, traversalStyle); // 내부 이동 형태 생성
            module.Configure(moduleId, ModuleSize, sockets); // 10m 모듈 데이터 적용

            if (!module.IsDefinitionValid()) // 생성된 정의 검증
            {
                UnityEngine.Object.DestroyImmediate(root); // 임시 오브젝트 정리
                throw new InvalidOperationException(moduleId + " Module 정의가 유효하지 않습니다."); // 생성 오류 출력
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath); // 기존 자산 덮어쓰기 또는 신규 저장
            UnityEngine.Object.DestroyImmediate(root); // 임시 오브젝트 정리
            return prefab; // 저장 Prefab 반환
        }

        private static bool ShouldCreateDefaultClosedFace(MapModuleFaceDirection direction, TraversalStyle style) // 기본 Face 생략 여부 판정
        {
            bool customBottom = style == TraversalStyle.JumpSingle || style == TraversalStyle.JumpDouble || style == TraversalStyle.JumpStones || style == TraversalStyle.NarrowBridge || style == TraversalStyle.DownLanding; // 커스텀 바닥 사용 여부 계산

            if (customBottom && direction == MapModuleFaceDirection.Down) // 커스텀 바닥과 Down Face 검사
            {
                return false; // 전체 바닥 생성 생략
            }

            return true; // 기본 Face 생성 허용
        }

        private static void CreateTraversalGeometry(Transform geometryRoot, Transform gameplayRoot, TraversalStyle style) // 내부 형태 생성 분기
        {
            switch (style) // 형태 종류 선택
            {
                case TraversalStyle.None: CreateStandardSafeZones(gameplayRoot); break; // 기본 평면 처리
                case TraversalStyle.NarrowLane: CreateNarrowLane(geometryRoot); CreateStandardSafeZones(gameplayRoot); break; // 좁은 길 처리
                case TraversalStyle.Slalom: CreateSlalom(geometryRoot); CreateStandardSafeZones(gameplayRoot); break; // 슬라럼 처리
                case TraversalStyle.Pillars: CreatePillars(geometryRoot); CreateStandardSafeZones(gameplayRoot); break; // 기둥 처리
                case TraversalStyle.CornerNarrowEast: CreateCornerNarrow(geometryRoot, true); CreateStandardSafeZones(gameplayRoot); break; // 우측 좁은 코너 처리
                case TraversalStyle.CornerNarrowWest: CreateCornerNarrow(geometryRoot, false); CreateStandardSafeZones(gameplayRoot); break; // 좌측 좁은 코너 처리
                case TraversalStyle.JumpSingle: CreateJumpSingle(geometryRoot); CreateStandardSafeZones(gameplayRoot); break; // 단일 점프 처리
                case TraversalStyle.JumpDouble: CreateJumpDouble(geometryRoot); CreateStandardSafeZones(gameplayRoot); break; // 2연속 점프 처리
                case TraversalStyle.JumpStones: CreateJumpStones(geometryRoot); CreateStandardSafeZones(gameplayRoot); break; // 징검다리 처리
                case TraversalStyle.NarrowBridge: CreateNarrowBridge(geometryRoot); CreateStandardSafeZones(gameplayRoot); break; // 좁은 다리 처리
                case TraversalStyle.LowCenter: CreateLowPassage(geometryRoot, 0f); CreateStandardSafeZones(gameplayRoot); break; // 중앙 낮은 통로 처리
                case TraversalStyle.LowLeft: CreateLowPassage(geometryRoot, -2.5f); CreateStandardSafeZones(gameplayRoot); break; // 좌측 낮은 통로 처리
                case TraversalStyle.LowRight: CreateLowPassage(geometryRoot, 2.5f); CreateStandardSafeZones(gameplayRoot); break; // 우측 낮은 통로 처리
                case TraversalStyle.RaisedCenter: CreateRaisedCenter(geometryRoot); CreateStandardSafeZones(gameplayRoot); break; // 중앙 상승 지형 처리
                case TraversalStyle.VerticalRamp: CreateSwitchbackRamp(geometryRoot, false); CreateVerticalNoSpawnZone(gameplayRoot); break; // 수직 Ramp 처리
                case TraversalStyle.VerticalStairs: CreateSwitchbackStairs(geometryRoot); CreateVerticalNoSpawnZone(gameplayRoot); break; // 수직 계단 처리
                case TraversalStyle.VerticalPlatforms: CreateVerticalPlatforms(geometryRoot); CreateVerticalNoSpawnZone(gameplayRoot); break; // 수직 발판 처리
                case TraversalStyle.SouthToUpRamp: CreateSwitchbackRamp(geometryRoot, true); CreateVerticalNoSpawnZone(gameplayRoot); break; // 남쪽에서 상승 처리
                case TraversalStyle.EastToUpRamp: CreateRotatedRamp(geometryRoot); CreateVerticalNoSpawnZone(gameplayRoot); break; // 동쪽에서 상승 처리
                case TraversalStyle.DownLanding: CreateBottomRingLanding(geometryRoot); CreateStandardSafeZones(gameplayRoot); break; // 아래쪽 진입 처리
                case TraversalStyle.SideRampEast: CreateSideRampToUp(geometryRoot, 2.8f); CreateVerticalNoSpawnZone(gameplayRoot); break; // 우측 상승 분기 처리
                case TraversalStyle.SideRampWest: CreateSideRampToUp(geometryRoot, -2.8f); CreateVerticalNoSpawnZone(gameplayRoot); break; // 좌측 상승 분기 처리
                default: throw new ArgumentOutOfRangeException(nameof(style), style, null); // 잘못된 형태 예외 출력
            }
        }

        private static void CreateStandardSafeZones(Transform gameplayRoot) // 기본 Spawn 영역 Marker 생성
        {
            Transform obstacleRoot = gameplayRoot.Find("ObstacleSpawnAreas"); // 장애물 영역 부모 조회
            Transform itemRoot = gameplayRoot.Find("ItemSpawnAreas"); // 아이템 영역 부모 조회
            Transform noSpawnRoot = gameplayRoot.Find("NoSpawnAreas"); // 금지 영역 부모 조회
            CreateMarker(obstacleRoot, "ObstacleArea_Center", new Vector3(0f, FloorTopY + 0.05f, 0f), new Vector3(4f, 1f, 4f)); // 중앙 장애물 후보 영역 생성
            CreateMarker(itemRoot, "ItemArea_Left", new Vector3(-2.6f, FloorTopY + 0.05f, 0f), new Vector3(1.8f, 1f, 1.8f)); // 좌측 아이템 후보 영역 생성
            CreateMarker(itemRoot, "ItemArea_Right", new Vector3(2.6f, FloorTopY + 0.05f, 0f), new Vector3(1.8f, 1f, 1.8f)); // 우측 아이템 후보 영역 생성
            CreateMarker(noSpawnRoot, "NoSpawn_MainPath", new Vector3(0f, FloorTopY + 0.05f, 0f), new Vector3(2.2f, 2f, 8f)); // 필수 진행선 보호 영역 생성
        }

        private static void CreateVerticalNoSpawnZone(Transform gameplayRoot) // 수직 이동 전체 보호 Marker 생성
        {
            Transform noSpawnRoot = gameplayRoot.Find("NoSpawnAreas"); // 금지 영역 부모 조회
            CreateMarker(noSpawnRoot, "NoSpawn_VerticalTraversal", Vector3.zero, new Vector3(9f, 10f, 9f)); // 수직 구조 전체 보호 영역 생성
        }

        private static void CreateNarrowLane(Transform parent) // 3m 폭 좁은 직선 길 생성
        {
            CreateBox(parent, "LaneBlock_Left", new Vector3(-3.25f, FloorTopY + 1.25f, 0f), new Vector3(3.5f, 2.5f, 7f), Quaternion.identity); // 왼쪽 공간 제한 블록 생성
            CreateBox(parent, "LaneBlock_Right", new Vector3(3.25f, FloorTopY + 1.25f, 0f), new Vector3(3.5f, 2.5f, 7f), Quaternion.identity); // 오른쪽 공간 제한 블록 생성
        }

        private static void CreateSlalom(Transform parent) // 좌우 회피형 지형 생성
        {
            CreateBox(parent, "Slalom_A", new Vector3(-1.8f, FloorTopY + 1.25f, -2.5f), new Vector3(1.5f, 2.5f, 1.5f), Quaternion.identity); // 첫 장애 기둥 생성
            CreateBox(parent, "Slalom_B", new Vector3(1.8f, FloorTopY + 1.25f, 0f), new Vector3(1.5f, 2.5f, 1.5f), Quaternion.identity); // 두 번째 장애 기둥 생성
            CreateBox(parent, "Slalom_C", new Vector3(-1.8f, FloorTopY + 1.25f, 2.5f), new Vector3(1.5f, 2.5f, 1.5f), Quaternion.identity); // 세 번째 장애 기둥 생성
        }

        private static void CreatePillars(Transform parent) // 기둥 통과형 지형 생성
        {
            CreateBox(parent, "Pillar_NW", new Vector3(-2f, FloorTopY + 1.5f, 2f), new Vector3(1f, 3f, 1f), Quaternion.identity); // 북서 기둥 생성
            CreateBox(parent, "Pillar_NE", new Vector3(2f, FloorTopY + 1.5f, 2f), new Vector3(1f, 3f, 1f), Quaternion.identity); // 북동 기둥 생성
            CreateBox(parent, "Pillar_SW", new Vector3(-2f, FloorTopY + 1.5f, -2f), new Vector3(1f, 3f, 1f), Quaternion.identity); // 남서 기둥 생성
            CreateBox(parent, "Pillar_SE", new Vector3(2f, FloorTopY + 1.5f, -2f), new Vector3(1f, 3f, 1f), Quaternion.identity); // 남동 기둥 생성
        }

        private static void CreateCornerNarrow(Transform parent, bool eastExit) // 좁은 L자 코너 지형 생성
        {
            float side = eastExit ? -1f : 1f; // 막을 반대쪽 방향 계산
            CreateBox(parent, "CornerBlock_Long", new Vector3(side * 3.1f, FloorTopY + 1.25f, 0.8f), new Vector3(3.2f, 2.5f, 6f), Quaternion.identity); // 코너 외측 공간 제한 생성
            CreateBox(parent, "CornerBlock_Top", new Vector3(side * 0.8f, FloorTopY + 1.25f, 3.2f), new Vector3(4.6f, 2.5f, 2.8f), Quaternion.identity); // 북쪽 공간 제한 생성
        }

        private static void CreateJumpSingle(Transform parent) // 2m 단일 점프 구간 생성
        {
            const float gap = 2f; // 점프 간격 설정
            float length = (ModuleSize - gap) * 0.5f; // 앞뒤 플랫폼 길이 계산
            float offset = gap * 0.5f + length * 0.5f; // 플랫폼 중심 거리 계산
            CreateBox(parent, "Floor_South", new Vector3(0f, FloorCenterY, -offset), new Vector3(ModuleSize, WallThickness, length), Quaternion.identity); // 남쪽 플랫폼 생성
            CreateBox(parent, "Floor_North", new Vector3(0f, FloorCenterY, offset), new Vector3(ModuleSize, WallThickness, length), Quaternion.identity); // 북쪽 플랫폼 생성
        }

        private static void CreateJumpDouble(Transform parent) // 1.4m 간격 2연속 점프 생성
        {
            CreateBox(parent, "Jump_Start", new Vector3(0f, FloorCenterY, -3.9f), new Vector3(ModuleSize, WallThickness, 2.2f), Quaternion.identity); // 시작 플랫폼 생성
            CreateBox(parent, "Jump_Middle", new Vector3(0f, FloorCenterY, 0f), new Vector3(ModuleSize, WallThickness, 2.8f), Quaternion.identity); // 중앙 플랫폼 생성
            CreateBox(parent, "Jump_End", new Vector3(0f, FloorCenterY, 3.9f), new Vector3(ModuleSize, WallThickness, 2.2f), Quaternion.identity); // 도착 플랫폼 생성
        }

        private static void CreateJumpStones(Transform parent) // 징검다리 점프 지형 생성
        {
            CreateBox(parent, "Stone_Start", new Vector3(0f, FloorCenterY, -4f), new Vector3(ModuleSize, WallThickness, 2f), Quaternion.identity); // 시작 바닥 생성
            CreateBox(parent, "Stone_01", new Vector3(-1.8f, FloorCenterY, -2f), new Vector3(2.2f, WallThickness, 1.8f), Quaternion.identity); // 첫 징검다리 생성
            CreateBox(parent, "Stone_02", new Vector3(1.8f, FloorCenterY, 0f), new Vector3(2.2f, WallThickness, 1.8f), Quaternion.identity); // 두 번째 징검다리 생성
            CreateBox(parent, "Stone_03", new Vector3(-1.8f, FloorCenterY, 2f), new Vector3(2.2f, WallThickness, 1.8f), Quaternion.identity); // 세 번째 징검다리 생성
            CreateBox(parent, "Stone_End", new Vector3(0f, FloorCenterY, 4f), new Vector3(ModuleSize, WallThickness, 2f), Quaternion.identity); // 도착 바닥 생성
        }

        private static void CreateNarrowBridge(Transform parent) // 낙하 위험이 있는 좁은 다리 생성
        {
            CreateBox(parent, "Bridge", new Vector3(0f, FloorCenterY, 0f), new Vector3(2.6f, WallThickness, ModuleSize), Quaternion.identity); // 중앙 다리 생성
        }

        private static void CreateLowPassage(Transform parent, float corridorCenterX) // 앉기 전용 낮은 통로 생성
        {
            const float corridorWidth = 3.2f; // 통로 폭 설정
            const float crouchClearance = 1.5f; // 앉기 통과 높이 설정
            float ceilingCenterY = FloorTopY + crouchClearance + WallThickness * 0.5f; // 천장 중심 높이 계산
            CreateBox(parent, "LowPassage_Ceiling", new Vector3(corridorCenterX, ceilingCenterY, 0f), new Vector3(corridorWidth, WallThickness, 5f), Quaternion.identity); // 낮은 천장 생성
            float leftEdge = corridorCenterX - corridorWidth * 0.5f; // 통로 왼쪽 경계 계산
            float rightEdge = corridorCenterX + corridorWidth * 0.5f; // 통로 오른쪽 경계 계산

            if (leftEdge > -5f) // 왼쪽 막힘 영역 존재 검사
            {
                float width = leftEdge + 5f; // 왼쪽 블록 폭 계산
                CreateBox(parent, "LowBlock_Left", new Vector3(-5f + width * 0.5f, FloorTopY + 1.25f, 0f), new Vector3(width, 2.5f, 5f), Quaternion.identity); // 왼쪽 우회 차단 생성
            }

            if (rightEdge < 5f) // 오른쪽 막힘 영역 존재 검사
            {
                float width = 5f - rightEdge; // 오른쪽 블록 폭 계산
                CreateBox(parent, "LowBlock_Right", new Vector3(rightEdge + width * 0.5f, FloorTopY + 1.25f, 0f), new Vector3(width, 2.5f, 5f), Quaternion.identity); // 오른쪽 우회 차단 생성
            }
        }

        private static void CreateRaisedCenter(Transform parent) // 중앙 단차와 양쪽 Ramp 생성
        {
            const float raisedHeight = 0.8f; // 중앙 단차 높이 설정
            CreateBox(parent, "RaisedPlatform", new Vector3(0f, FloorTopY + raisedHeight - 0.2f, 0f), new Vector3(6f, 0.4f, 3f), Quaternion.identity); // 중앙 높은 플랫폼 생성
            CreateRampBetween(parent, "RaisedRamp_South", new Vector3(0f, FloorTopY + 0.1f, -3.4f), new Vector3(0f, FloorTopY + raisedHeight, -1.5f), 4f); // 남쪽 진입 Ramp 생성
            CreateRampBetween(parent, "RaisedRamp_North", new Vector3(0f, FloorTopY + raisedHeight, 1.5f), new Vector3(0f, FloorTopY + 0.1f, 3.4f), 4f); // 북쪽 이탈 Ramp 생성
        }

        private static void CreateSwitchbackRamp(Transform parent, bool hasClosedBottomFloor) // 10m Cell 지그재그 Ramp 생성
        {
            float bottomY = hasClosedBottomFloor ? FloorTopY + 0.15f : -4.35f; // 하단 시작 높이 계산
            CreateBox(parent, "Ramp_BottomLanding", new Vector3(-2f, bottomY - 0.2f, -3.5f), new Vector3(4.2f, 0.4f, 2.2f), Quaternion.identity); // 하단 Landing 생성
            CreateRampBetween(parent, "Ramp_A", new Vector3(-2f, bottomY, -3.3f), new Vector3(-2f, -0.1f, 3.3f), 3.2f); // 첫 Ramp 생성
            CreateBox(parent, "Ramp_MiddleLanding", new Vector3(0f, 0.1f, 3.3f), new Vector3(7f, 0.4f, 2.2f), Quaternion.identity); // 중앙 Landing 생성
            CreateRampBetween(parent, "Ramp_B", new Vector3(2f, 0.3f, 3.3f), new Vector3(2f, 4.25f, -3.3f), 3.2f); // 두 번째 Ramp 생성
            CreateBox(parent, "Ramp_TopLanding", new Vector3(0f, 4.45f, -3.3f), new Vector3(7f, 0.4f, 2.2f), Quaternion.identity); // 상단 Landing 생성
        }

        private static void CreateRotatedRamp(Transform parent) // 동쪽 진입용 90도 회전 Ramp 생성
        {
            Transform rotatedRoot = CreateEmptyChild(parent, "RotatedRampRoot"); // 회전용 부모 생성
            rotatedRoot.localRotation = Quaternion.Euler(0f, 90f, 0f); // Y축 90도 회전 적용
            CreateSwitchbackRamp(rotatedRoot, true); // 회전된 Ramp 생성
        }

        private static void CreateSideRampToUp(Transform parent, float xPosition) // 수평 경로를 남기는 측면 상승 Ramp 생성
        {
            CreateBox(parent, "SideRamp_BottomLanding", new Vector3(xPosition, FloorCenterY + 0.2f, -3.5f), new Vector3(2.6f, 0.4f, 2f), Quaternion.identity); // 측면 하단 Landing 생성
            CreateRampBetween(parent, "SideRamp_A", new Vector3(xPosition, FloorTopY + 0.1f, -3.2f), new Vector3(xPosition, -0.1f, 3.2f), 2.4f); // 측면 첫 Ramp 생성
            CreateBox(parent, "SideRamp_MiddleLanding", new Vector3(xPosition, 0.1f, 3.2f), new Vector3(2.8f, 0.4f, 2f), Quaternion.identity); // 측면 중앙 Landing 생성
            CreateRampBetween(parent, "SideRamp_B", new Vector3(xPosition, 0.3f, 3.2f), new Vector3(xPosition, 4.25f, -3.2f), 2.4f); // 측면 두 번째 Ramp 생성
            CreateBox(parent, "SideRamp_TopLanding", new Vector3(xPosition, 4.45f, -3.2f), new Vector3(2.8f, 0.4f, 2f), Quaternion.identity); // 측면 상단 Landing 생성
        }

        private static void CreateSwitchbackStairs(Transform parent) // 0.35m 단차 수직 계단 생성
        {
            const int stepsPerFlight = 13; // 한 Flight 계단 수 설정
            const float stepHeight = 0.35f; // Player 최대 0.4m 이하 단차 설정
            const float stepDepth = 0.55f; // 계단 깊이 설정
            const float stepWidth = 3f; // 계단 폭 설정
            float firstStartY = -4.55f; // 첫 Flight 시작 높이 설정
            float firstStartZ = -3.4f; // 첫 Flight 시작 Z 설정

            for (int index = 0; index < stepsPerFlight; index++) // 첫 Flight 생성
            {
                float y = firstStartY + stepHeight * (index + 1) - stepHeight * 0.5f; // 현재 단 Y 계산
                float z = firstStartZ + stepDepth * index; // 현재 단 Z 계산
                CreateBox(parent, "Stair_A_" + index.ToString("00"), new Vector3(-2f, y, z), new Vector3(stepWidth, stepHeight, stepDepth + 0.04f), Quaternion.identity); // 첫 Flight 단 생성
            }

            float middleY = firstStartY + stepHeight * stepsPerFlight; // 중앙 높이 계산
            float middleZ = firstStartZ + stepDepth * (stepsPerFlight - 1); // 중앙 Z 계산
            CreateBox(parent, "Stair_MiddleLanding", new Vector3(0f, middleY - 0.2f, middleZ), new Vector3(7f, 0.4f, 2f), Quaternion.identity); // 중앙 Landing 생성

            for (int index = 0; index < stepsPerFlight; index++) // 두 번째 Flight 생성
            {
                float y = middleY + stepHeight * (index + 1) - stepHeight * 0.5f; // 현재 단 Y 계산
                float z = middleZ - stepDepth * index; // 현재 단 Z 계산
                CreateBox(parent, "Stair_B_" + index.ToString("00"), new Vector3(2f, y, z), new Vector3(stepWidth, stepHeight, stepDepth + 0.04f), Quaternion.identity); // 두 번째 Flight 단 생성
            }

            float topY = middleY + stepHeight * stepsPerFlight; // 최종 높이 계산
            CreateBox(parent, "Stair_TopLanding", new Vector3(0f, topY - 0.2f, -2.8f), new Vector3(7f, 0.4f, 3f), Quaternion.identity); // 상단 Landing 생성
        }

        private static void CreateVerticalPlatforms(Transform parent) // 1.2m 높이차 플랫폼 등반 생성
        {
            float[] heights = { -3.4f, -2.2f, -1f, 0.2f, 1.4f, 2.6f, 3.8f }; // 플랫폼 높이 목록 생성
            float[] xPositions = { -2f, 2f, -2f, 2f, -2f, 2f, 0f }; // 좌우 배치 목록 생성
            float[] zPositions = { -2.8f, -1.9f, -1f, 0f, 1f, 1.9f, 2.8f }; // 전후 배치 목록 생성

            for (int index = 0; index < heights.Length; index++) // 플랫폼 전체 순회
            {
                CreateBox(parent, "VerticalPlatform_" + index.ToString("00"), new Vector3(xPositions[index], heights[index], zPositions[index]), new Vector3(3f, 0.4f, 2.6f), Quaternion.identity); // 현재 점프 플랫폼 생성
            }

            CreateBox(parent, "VerticalPlatform_TopLanding", new Vector3(0f, 4.45f, 0f), new Vector3(5f, 0.4f, 4f), Quaternion.identity); // 최상단 Landing 생성
        }

        private static void CreateBottomRingLanding(Transform parent) // Down Socket 진입용 중앙 구멍 Landing 생성
        {
            CreateBox(parent, "Landing_North", new Vector3(0f, FloorCenterY, 3.25f), new Vector3(10f, WallThickness, 3.5f), Quaternion.identity); // 북쪽 Landing 생성
            CreateBox(parent, "Landing_South", new Vector3(0f, FloorCenterY, -3.25f), new Vector3(10f, WallThickness, 3.5f), Quaternion.identity); // 남쪽 Landing 생성
            CreateBox(parent, "Landing_East", new Vector3(3.25f, FloorCenterY, 0f), new Vector3(3.5f, WallThickness, 3f), Quaternion.identity); // 동쪽 Landing 생성
            CreateBox(parent, "Landing_West", new Vector3(-3.25f, FloorCenterY, 0f), new Vector3(3.5f, WallThickness, 3f), Quaternion.identity); // 서쪽 Landing 생성
        }

        private static void CreateRampBetween(Transform parent, string objectName, Vector3 start, Vector3 end, float width) // 두 점 사이 Ramp Cube 생성
        {
            Vector3 direction = end - start; // Ramp 진행 벡터 계산
            float length = direction.magnitude; // Ramp 길이 계산
            Vector3 center = (start + end) * 0.5f; // Ramp 중심 계산
            Quaternion rotation = Quaternion.LookRotation(direction.normalized, Vector3.up); // 진행 방향 회전 계산
            CreateBox(parent, objectName, center, new Vector3(width, 0.4f, length), rotation); // Ramp 생성
        }

        private static void CreateFaceGeometry(Transform parent, MapModuleFaceDirection direction) // 10m 정육면체 Closed Face 생성
        {
            float half = ModuleSize * 0.5f; // 모듈 절반 크기 계산
            float inset = half - WallThickness * 0.5f; // Face 중심 위치 계산

            switch (direction) // 방향별 Geometry 생성
            {
                case MapModuleFaceDirection.North: CreateBox(parent, "Wall_North", new Vector3(0f, 0f, inset), new Vector3(ModuleSize, ModuleSize, WallThickness), Quaternion.identity); break; // 북쪽 벽 생성
                case MapModuleFaceDirection.South: CreateBox(parent, "Wall_South", new Vector3(0f, 0f, -inset), new Vector3(ModuleSize, ModuleSize, WallThickness), Quaternion.identity); break; // 남쪽 벽 생성
                case MapModuleFaceDirection.East: CreateBox(parent, "Wall_East", new Vector3(inset, 0f, 0f), new Vector3(WallThickness, ModuleSize, ModuleSize), Quaternion.identity); break; // 동쪽 벽 생성
                case MapModuleFaceDirection.West: CreateBox(parent, "Wall_West", new Vector3(-inset, 0f, 0f), new Vector3(WallThickness, ModuleSize, ModuleSize), Quaternion.identity); break; // 서쪽 벽 생성
                case MapModuleFaceDirection.Up: CreateBox(parent, "Ceiling", new Vector3(0f, inset, 0f), new Vector3(ModuleSize, WallThickness, ModuleSize), Quaternion.identity); break; // 천장 생성
                case MapModuleFaceDirection.Down: CreateBox(parent, "Floor", new Vector3(0f, -inset, 0f), new Vector3(ModuleSize, WallThickness, ModuleSize), Quaternion.identity); break; // 바닥 생성
            }
        }

        private static MapModuleSocket CreateSocket(Transform parent, MapModuleFaceDirection direction, MapModuleFaceState state) // 6방향 Socket 생성
        {
            GameObject socketObject = new GameObject("Socket_" + direction); // Socket 오브젝트 생성
            socketObject.transform.SetParent(parent, false); // Socket 부모 연결
            socketObject.transform.localPosition = MapModule.GetDirectionVector(direction) * (ModuleSize * 0.5f); // Socket을 ±5m Face 중심에 배치
            socketObject.transform.localRotation = GetSocketRotation(direction); // 외향 회전 적용
            MapModuleSocket socket = socketObject.AddComponent<MapModuleSocket>(); // Socket 컴포넌트 추가
            socket.Configure(direction, state); // 방향과 상태 설정
            return socket; // 생성 Socket 반환
        }

        private static Quaternion GetSocketRotation(MapModuleFaceDirection direction) // Socket 외향 회전 계산
        {
            if (direction == MapModuleFaceDirection.Up) // 위쪽 방향 검사
            {
                return Quaternion.LookRotation(Vector3.up, Vector3.forward); // 위쪽 회전 반환
            }

            if (direction == MapModuleFaceDirection.Down) // 아래쪽 방향 검사
            {
                return Quaternion.LookRotation(Vector3.down, Vector3.forward); // 아래쪽 회전 반환
            }

            return Quaternion.LookRotation(MapModule.GetDirectionVector(direction), Vector3.up); // 수평 방향 회전 반환
        }

        private static GameObject CreateBox(Transform parent, string objectName, Vector3 localPosition, Vector3 localScale, Quaternion localRotation) // Greybox Cube 생성
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube); // Unity Cube 생성
            box.name = objectName; // 오브젝트 이름 설정
            box.transform.SetParent(parent, false); // 부모 연결
            box.transform.localPosition = localPosition; // 로컬 위치 적용
            box.transform.localRotation = localRotation; // 로컬 회전 적용
            box.transform.localScale = localScale; // 로컬 크기 적용
            SetGroundLayer(box); // Ground 레이어 적용
            return box; // 생성 Cube 반환
        }

        private static Transform CreateEmptyChild(Transform parent, string objectName) // 빈 자식 생성
        {
            GameObject child = new GameObject(objectName); // 빈 GameObject 생성
            child.transform.SetParent(parent, false); // 부모 연결
            return child.transform; // Transform 반환
        }

        private static void CreateMarker(Transform parent, string objectName, Vector3 localPosition, Vector3 localScale) // 배치 영역 Marker 생성
        {
            GameObject marker = new GameObject(objectName); // Marker 생성
            marker.transform.SetParent(parent, false); // 부모 연결
            marker.transform.localPosition = localPosition; // 위치 기록
            marker.transform.localScale = localScale; // 범위 크기 기록
        }

        private static void SetGroundLayer(GameObject target) // Ground 레이어 적용
        {
            int groundLayer = LayerMask.NameToLayer("Ground"); // Ground 레이어 번호 조회

            if (groundLayer >= 0) // Ground 레이어 존재 검사
            {
                target.layer = groundLayer; // Ground 레이어 적용
            }
        }

        private static Dictionary<MapModuleFaceDirection, MapModuleFaceState> CreateStates(params object[] values) // 6방향 Face 상태 Dictionary 생성
        {
            Dictionary<MapModuleFaceDirection, MapModuleFaceState> result = new Dictionary<MapModuleFaceDirection, MapModuleFaceState>(); // 상태 Dictionary 생성

            for (int index = 0; index < AllDirections.Length; index++) // 모든 방향 순회
            {
                result.Add(AllDirections[index], MapModuleFaceState.Closed); // Closed 기본값 적용
            }

            for (int index = 0; index < values.Length; index += 2) // 전달 상태 쌍 순회
            {
                MapModuleFaceDirection direction = (MapModuleFaceDirection)values[index]; // 방향 변환
                MapModuleFaceState state = (MapModuleFaceState)values[index + 1]; // 상태 변환
                result[direction] = state; // 상태 적용
            }

            return result; // 완성 Dictionary 반환
        }

        private static Dictionary<MapModuleFaceDirection, MapModuleFaceState> CreateLegacyStates(LegacyModuleKind kind) // Day25 Face 상태 복원
        {
            switch (kind) // 기존 모듈 종류 선택
            {
                case LegacyModuleKind.Straight: return CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit); // 직선 상태 반환
                case LegacyModuleKind.Corner: return CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.East, MapModuleFaceState.Exit); // 코너 상태 반환
                case LegacyModuleKind.Vertical: return CreateStates(MapModuleFaceDirection.Down, MapModuleFaceState.Entrance, MapModuleFaceDirection.Up, MapModuleFaceState.Exit); // 수직 상태 반환
                case LegacyModuleKind.Branch: return CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit, MapModuleFaceDirection.East, MapModuleFaceState.Exit); // 분기 상태 반환
                case LegacyModuleKind.Merge: return CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.West, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit); // 합류 상태 반환
                case LegacyModuleKind.Drop: return CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.North, MapModuleFaceState.Exit, MapModuleFaceDirection.East, MapModuleFaceState.Drop); // 낙하 상태 반환
                case LegacyModuleKind.Start: return CreateStates(MapModuleFaceDirection.South, MapModuleFaceState.Entrance, MapModuleFaceDirection.Up, MapModuleFaceState.Exit); // 시작 상태 반환
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null); // 잘못된 종류 예외 출력
            }
        }

        private static TraversalStyle GetLegacyTraversalStyle(LegacyModuleKind kind) // Day25 내부 이동 구조 선택
        {
            if (kind == LegacyModuleKind.Vertical) // 수직 모듈 검사
            {
                return TraversalStyle.VerticalRamp; // 수직 Ramp 반환
            }

            if (kind == LegacyModuleKind.Start) // 시작 모듈 검사
            {
                return TraversalStyle.SouthToUpRamp; // 남쪽에서 위쪽 Ramp 반환
            }

            return TraversalStyle.None; // 기본 평면 반환
        }

        private static void DeleteDay144ModulesInternal() // Day144 전용 폴더만 삭제
        {
            if (AssetDatabase.IsValidFolder(Day144RootPath)) // Day144 폴더 존재 검사
            {
                AssetDatabase.DeleteAsset(Day144RootPath); // Day144 폴더 삭제
            }
        }

        private static void EnsureFolder(string folderPath) // Unity 폴더 생성
        {
            string[] pathParts = folderPath.Split('/'); // 경로 단계 분리
            string currentPath = pathParts[0]; // Assets 루트 저장

            for (int index = 1; index < pathParts.Length; index++) // 하위 경로 순회
            {
                string nextPath = currentPath + "/" + pathParts[index]; // 다음 경로 계산

                if (!AssetDatabase.IsValidFolder(nextPath)) // 폴더 누락 검사
                {
                    AssetDatabase.CreateFolder(currentPath, pathParts[index]); // 누락 폴더 생성
                }

                currentPath = nextPath; // 현재 경로 갱신
            }
        }

        private static void EnsureFolderWithGuid(string folderPath, string expectedGuid) // 기존 폴더 GUID 복구
        {
            string absoluteFolderPath = Path.GetFullPath(folderPath); // 실제 폴더 경로 계산
            string metaPath = absoluteFolderPath + ".meta"; // Meta 경로 계산

            if (!Directory.Exists(absoluteFolderPath)) // 실제 폴더 누락 검사
            {
                Directory.CreateDirectory(absoluteFolderPath); // 실제 폴더 생성
            }

            string currentGuid = AssetDatabase.AssetPathToGUID(folderPath); // 현재 GUID 조회

            if (string.IsNullOrEmpty(currentGuid) || !string.Equals(currentGuid, expectedGuid, StringComparison.OrdinalIgnoreCase)) // GUID 불일치 검사
            {
                File.WriteAllText(metaPath, BuildFolderMeta(expectedGuid)); // 원래 GUID Meta 기록
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate); // 변경 상태 즉시 반영
            }
        }

        private static void EnsurePrefabMetaGuid(string prefabPath, string expectedGuid) // 기존 Prefab GUID 유지
        {
            string absolutePrefabPath = Path.GetFullPath(prefabPath); // Prefab 실제 경로 계산
            string metaPath = absolutePrefabPath + ".meta"; // Meta 실제 경로 계산

            if (File.Exists(metaPath)) // 기존 Meta 존재 검사
            {
                string existingText = File.ReadAllText(metaPath); // 기존 Meta 읽기

                if (existingText.IndexOf("guid: " + expectedGuid, StringComparison.OrdinalIgnoreCase) >= 0) // 예상 GUID 일치 검사
                {
                    return; // 기존 Meta 유지
                }
            }

            File.WriteAllText(metaPath, BuildPrefabMeta(expectedGuid)); // 예상 GUID Meta 기록
        }

        private static string BuildFolderMeta(string guid) // 폴더 Meta 문자열 생성
        {
            return "fileFormatVersion: 2\n" + "guid: " + guid + "\n" + "folderAsset: yes\n" + "DefaultImporter:\n" + "  externalObjects: {}\n" + "  userData: \n" + "  assetBundleName: \n" + "  assetBundleVariant: \n"; // Unity 폴더 Meta 반환
        }

        private static string BuildPrefabMeta(string guid) // Prefab Meta 문자열 생성
        {
            return "fileFormatVersion: 2\n" + "guid: " + guid + "\n" + "PrefabImporter:\n" + "  externalObjects: {}\n" + "  userData: \n" + "  assetBundleName: \n" + "  assetBundleVariant: \n"; // Unity Prefab Meta 반환
        }

        private enum TraversalStyle // 내부 이동 형태 목록
        {
            None, // 기본 평면
            NarrowLane, // 좁은 직선
            Slalom, // 슬라럼
            Pillars, // 기둥
            CornerNarrowEast, // 좁은 우회전
            CornerNarrowWest, // 좁은 좌회전
            JumpSingle, // 단일 점프
            JumpDouble, // 2연속 점프
            JumpStones, // 징검다리
            NarrowBridge, // 좁은 다리
            LowCenter, // 중앙 앉기 통로
            LowLeft, // 좌측 앉기 통로
            LowRight, // 우측 앉기 통로
            RaisedCenter, // 중앙 상승 지형
            VerticalRamp, // 수직 Ramp
            VerticalStairs, // 수직 계단
            VerticalPlatforms, // 수직 발판
            SouthToUpRamp, // 남쪽에서 위쪽 Ramp
            EastToUpRamp, // 동쪽에서 위쪽 Ramp
            DownLanding, // 아래쪽 진입 Landing
            SideRampEast, // 우측 측면 Ramp
            SideRampWest // 좌측 측면 Ramp
        }

        private enum LegacyModuleKind // Day25 모듈 종류
        {
            Straight, // 직선
            Corner, // 코너
            Vertical, // 수직
            Branch, // 분기
            Merge, // 합류
            Drop, // 낙하
            Start // 시작
        }

        private readonly struct LegacyPrefabSpec // Day25 Prefab 복구 정보
        {
            public string ModuleId { get; } // 기존 Module ID
            public string Guid { get; } // 기존 Prefab GUID
            public LegacyModuleKind Kind { get; } // 기존 모듈 종류

            public LegacyPrefabSpec(string moduleId, string guid, LegacyModuleKind kind) // 복구 정보 생성
            {
                ModuleId = moduleId; // Module ID 저장
                Guid = guid; // GUID 저장
                Kind = kind; // 종류 저장
            }
        }
    }
}

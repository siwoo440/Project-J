using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay141ThirdPartyReviewResolver
    {
        private const string ThirdPartyRoot = "Assets/ProjectJ/ThirdParty";

        private sealed class MoveRule
        {
            public readonly string Source;
            public readonly string Destination;

            public MoveRule(
                string source,
                string destination
            )
            {
                Source = source; // 원본 경로
                Destination = destination; // 정식 경로
            }
        }

        private static readonly MoveRule[] MoveRules =
        {
            new MoveRule("Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Blue 2 Base.mat", "Assets/ProjectJ/Art/Characters/Materials/Imported/Blue 2 Base.mat"),
            new MoveRule("Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Grey 1 Dark.mat", "Assets/ProjectJ/Art/Characters/Materials/Imported/Grey 1 Dark.mat"),
            new MoveRule("Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Grey 2 Base.mat", "Assets/ProjectJ/Art/Characters/Materials/Imported/Grey 2 Base.mat"),
            new MoveRule("Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Pink 2 Base.mat", "Assets/ProjectJ/Art/Characters/Materials/Imported/Pink 2 Base.mat"),
            new MoveRule("Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Purple 1 Dark.mat", "Assets/ProjectJ/Art/Characters/Materials/Imported/Purple 1 Dark.mat"),
            new MoveRule("Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Turquoise 2 Base.mat", "Assets/ProjectJ/Art/Characters/Materials/Imported/Turquoise 2 Base.mat"),
            new MoveRule("Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Yellow 1 Dark.mat", "Assets/ProjectJ/Art/Characters/Materials/Imported/Yellow 1 Dark.mat"),
            new MoveRule("Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/ColorPalette.mat", "Assets/ProjectJ/Art/Characters/Materials/Imported/ColorPalette.mat"),
            new MoveRule("Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/ColorPalette.png", "Assets/ProjectJ/Art/Characters/Textures/Imported/ColorPalette.png"),
            new MoveRule("Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Face Images/face 1.png", "Assets/ProjectJ/Art/Characters/Textures/Imported/face 1.png"),
            new MoveRule("Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Face Images/face 2.png", "Assets/ProjectJ/Art/Characters/Textures/Imported/face 2.png"),
            new MoveRule("Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Face/face 1.mat", "Assets/ProjectJ/Art/Characters/Materials/Imported/face 1.mat"),
            new MoveRule("Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Face/face 2.mat", "Assets/ProjectJ/Art/Characters/Materials/Imported/face 2.mat"),
        };

        private static readonly string[] DeleteCandidates =
        {
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Icons/lock_off.png",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Icons/lock_on.png",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Icons/promo_image.png",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Icons/reset_cam.png",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Blue 1 Dark.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Blue 3 Light.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Brown 1 Dark.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Brown 2 Base.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Brown 3 Light.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Cream 1 Dark.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Cream 2 Base.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Cream 3 Light.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Cyan 1 Dark.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Cyan 2 Base.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Cyan 3 Light.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Green 1 Dark.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Green 2 Base.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Green 3 Light.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Grey 3 Light.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Orange 1 Dark.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Orange 2 Base.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Orange 3 Light.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Pink 1 Dark.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Pink 3 Light.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Purple 2 Base.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Purple 3 Light.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Red 1 Dark.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Red 2 Base.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Red 3 Light.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Turquoise 1 Dark.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Turquoise 3 Light.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Yellow 2 Base.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Body/Yellow 3 Light.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Face Images/face 3.png",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Face/face 3.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Materials/Ground.mat",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Prefabs/character_default.prefab",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Prefabs/Hats/chef hat.prefab",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Prefabs/Hats/orange fedora.prefab",
            "Assets/ProjectJ/ThirdParty/FREE/Pack_FREE_PartyCharacters/Resources/Prefabs/Hats/party hat.prefab",
        };

        [MenuItem(
            "Project J/Day141/ThirdParty/2C. Resolve Remaining Resources"
        )]
        private static void ResolveRemainingResources()
        {
            if (!AssetDatabase.IsValidFolder(ThirdPartyRoot))
            {
                Debug.LogWarning(
                    "[Project J/Day141] ThirdParty 폴더가 없습니다."
                ); // ThirdParty 없음
                return;
            }

            string previewPath = WritePreviewReport(); // 처리 계획 저장

            bool confirmed = EditorUtility.DisplayDialog(
                "Day141 Remaining Resources 정리",
                "현재 검증 결과의 남은 PartyCharacters Resources를 처리합니다.\n\n" +
                "외부 참조 13개는 GUID를 유지하며 정식 폴더로 이동하고,\n" +
                "외부 참조가 없는 40개는 재검증 후 삭제합니다.\n\n" +
                "이전 2B의 자기 자신 오탐을 수정하여 Editor Script는 " +
                "Resources.Load 검사 대상에서 제외합니다.\n\n" +
                $"계획 보고서: {previewPath}",
                "2C 실행",
                "취소"
            ); // 사용자 확인

            if (!confirmed)
            {
                return;
            }

            List<string> moved = new List<string>(); // 이동 성공
            List<string> deleted = new List<string>(); // 삭제 성공
            List<string> blocked = new List<string>(); // 안전 차단
            List<string> failures = new List<string>(); // 처리 실패

            foreach (MoveRule rule in MoveRules)
            {
                ResolveMove(
                    rule,
                    moved,
                    blocked,
                    failures
                ); // 실제 사용 Resource 승격
            }

            AssetDatabase.SaveAssets(); // 이동 결과 저장
            AssetDatabase.Refresh(); // 이동 결과 반영

            foreach (string assetPath in DeleteCandidates)
            {
                ResolveDelete(
                    assetPath,
                    deleted,
                    blocked,
                    failures
                ); // 미사용 Resource 정리
            }

            AssetDatabase.SaveAssets(); // 삭제 결과 저장
            AssetDatabase.Refresh(); // 삭제 결과 반영

            string resultPath = WriteResultReport(
                moved,
                deleted,
                blocked,
                failures
            ); // 최종 결과 저장

            if (failures.Count > 0)
            {
                Debug.LogError(
                    $"[Project J/Day141] 2C 처리 완료 - " +
                    $"이동 {moved.Count}, 삭제 {deleted.Count}, " +
                    $"차단 {blocked.Count}, 실패 {failures.Count}\n" +
                    $"{resultPath}"
                ); // 실패 결과
                return;
            }

            if (blocked.Count > 0)
            {
                Debug.LogWarning(
                    $"[Project J/Day141] 2C 처리 완료 - " +
                    $"이동 {moved.Count}, 삭제 {deleted.Count}, " +
                    $"차단 {blocked.Count}\n" +
                    $"{resultPath}\n" +
                    "컴파일/임포트 완료 후 3. Verify References를 다시 실행하세요."
                ); // 일부 차단 결과
                return;
            }

            Debug.Log(
                $"[Project J/Day141] 2C 처리 완료 - " +
                $"이동 {moved.Count}, 삭제 {deleted.Count}\n" +
                $"{resultPath}\n" +
                "컴파일/임포트 완료 후 3. Verify References를 다시 실행하세요."
            ); // 처리 결과
        }

        private static void ResolveMove(
            MoveRule rule,
            List<string> moved,
            List<string> blocked,
            List<string> failures
        )
        {
            if (AssetDatabase.LoadMainAssetAtPath(rule.Source) == null)
            {
                blocked.Add(
                    $"MOVE SKIP - 원본 없음: {rule.Source}"
                ); // 이미 처리된 대상
                return;
            }

            if (TryFindRuntimeResourcesLoadReference(
                    rule.Source,
                    out string runtimeScript
                ))
            {
                blocked.Add(
                    $"MOVE BLOCKED - Runtime Resources.Load 참조: " +
                    $"{rule.Source} <- {runtimeScript}"
                ); // 실제 런타임 문자열 로드 보호
                return;
            }

            string destination = PrepareDestinationPath(
                rule.Source,
                rule.Destination
            ); // 대상 경로 결정

            EnsureAssetFolder(
                Path.GetDirectoryName(destination)
                    ?.Replace('\\', '/')
            ); // 정식 폴더 생성

            string error = AssetDatabase.MoveAsset(
                rule.Source,
                destination
            ); // GUID 유지 이동

            if (!string.IsNullOrEmpty(error))
            {
                failures.Add(
                    $"MOVE FAILED - {rule.Source} -> {destination}: {error}"
                ); // 이동 실패
                return;
            }

            moved.Add(
                $"{rule.Source} -> {destination}"
            ); // 이동 성공
        }

        private static void ResolveDelete(
            string assetPath,
            List<string> deleted,
            List<string> blocked,
            List<string> failures
        )
        {
            if (AssetDatabase.LoadMainAssetAtPath(assetPath) == null)
            {
                return; // 이미 없는 대상
            }

            List<string> externalReferences =
                FindExternalReferences(assetPath); // 외부 Asset 참조 확인

            if (externalReferences.Count > 0)
            {
                blocked.Add(
                    $"DELETE BLOCKED - 외부 참조: {assetPath} <- " +
                    string.Join(", ", externalReferences)
                ); // 실제 Asset 참조 보호
                return;
            }

            if (TryFindRuntimeResourcesLoadReference(
                    assetPath,
                    out string runtimeScript
                ))
            {
                blocked.Add(
                    $"DELETE BLOCKED - Runtime Resources.Load 참조: " +
                    $"{assetPath} <- {runtimeScript}"
                ); // 실제 런타임 문자열 로드 보호
                return;
            }

            bool deletedAsset = AssetDatabase.DeleteAsset(
                assetPath
            ); // 미사용 Asset 삭제

            if (!deletedAsset)
            {
                failures.Add(
                    $"DELETE FAILED - {assetPath}"
                ); // 삭제 실패
                return;
            }

            deleted.Add(assetPath); // 삭제 성공
        }

        private static List<string> FindExternalReferences(
            string targetPath
        )
        {
            List<string> references = new List<string>(); // 외부 참조 목록
            string[] allPaths = AssetDatabase.GetAllAssetPaths(); // 전체 Asset

            foreach (string sourcePath in allPaths)
            {
                if (!sourcePath.StartsWith(
                        "Assets/",
                        StringComparison.OrdinalIgnoreCase
                    ) ||
                    IsUnderThirdParty(sourcePath) ||
                    AssetDatabase.IsValidFolder(sourcePath))
                {
                    continue;
                }

                string[] dependencies;

                try
                {
                    dependencies = AssetDatabase.GetDependencies(
                        sourcePath,
                        true
                    ); // 의존성 조회
                }
                catch
                {
                    continue;
                }

                if (dependencies.Any(
                        dependency =>
                            dependency.Equals(
                                targetPath,
                                StringComparison.OrdinalIgnoreCase
                            )
                    ))
                {
                    references.Add(sourcePath); // 외부 참조 추가
                }
            }

            return references;
        }

        private static bool TryFindRuntimeResourcesLoadReference(
            string assetPath,
            out string scriptPath
        )
        {
            scriptPath = null; // 기본 결과
            string resourceKey = GetResourcesKey(assetPath); // Resources 키
            string fileStem = Path.GetFileNameWithoutExtension(
                assetPath
            ); // 파일명 키

            if (string.IsNullOrEmpty(resourceKey))
            {
                return false;
            }

            foreach (string absolutePath in Directory.EnumerateFiles(
                         Application.dataPath,
                         "*.cs",
                         SearchOption.AllDirectories
                     ))
            {
                string normalized = absolutePath.Replace('\\', '/'); // 경로 정규화

                if (normalized.Contains(
                        "/Assets/ProjectJ/ThirdParty/"
                    ) ||
                    normalized.Contains(
                        "/Editor/"
                    ))
                {
                    continue; // ThirdParty 및 Editor Script 제외
                }

                string content;

                try
                {
                    content = File.ReadAllText(
                        absolutePath
                    ); // Runtime Script 읽기
                }
                catch
                {
                    continue;
                }

                if (!ContainsActualResourcesLoad(
                        content,
                        resourceKey,
                        fileStem
                    ))
                {
                    continue;
                }

                scriptPath = MakeProjectRelativePath(
                    absolutePath
                ); // 실제 Runtime 참조 Script
                return true;
            }

            return false;
        }

        private static bool ContainsActualResourcesLoad(
            string content,
            string resourceKey,
            string fileStem
        )
        {
            MatchCollection matches = Regex.Matches(
                content,
                @"Resources\.Load(?:Async)?(?:<[^>]+>)?\s*\(\s*""([^""]+)""",
                RegexOptions.CultureInvariant
            ); // 직접 문자열 Resources.Load 호출 추출

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                string loadedKey = match.Groups[1].Value; // 로드 문자열

                if (loadedKey.Equals(
                        resourceKey,
                        StringComparison.OrdinalIgnoreCase
                    ) ||
                    loadedKey.Equals(
                        fileStem,
                        StringComparison.OrdinalIgnoreCase
                    ))
                {
                    return true; // 실제 문자열 로드 일치
                }
            }

            return false;
        }

        private static string GetResourcesKey(
            string assetPath
        )
        {
            string normalized = assetPath.Replace('\\', '/'); // 경로 정규화
            const string marker = "/Resources/"; // Resources 경계
            int index = normalized.IndexOf(
                marker,
                StringComparison.OrdinalIgnoreCase
            ); // Resources 위치

            if (index < 0)
            {
                return string.Empty;
            }

            string relative = normalized.Substring(
                index + marker.Length
            ); // Resources 상대 경로
            string extension = Path.GetExtension(relative); // 확장자

            if (!string.IsNullOrEmpty(extension))
            {
                relative = relative.Substring(
                    0,
                    relative.Length - extension.Length
                ); // Resources.Load 키 형식
            }

            return relative;
        }

        private static string PrepareDestinationPath(
            string sourcePath,
            string desiredPath
        )
        {
            UnityEngine.Object existing =
                AssetDatabase.LoadMainAssetAtPath(
                    desiredPath
                ); // 대상 충돌 확인

            if (existing == null)
            {
                return desiredPath;
            }

            string sourceGuid = AssetDatabase.AssetPathToGUID(
                sourcePath
            ); // 원본 GUID
            string targetGuid = AssetDatabase.AssetPathToGUID(
                desiredPath
            ); // 대상 GUID

            if (!string.IsNullOrEmpty(sourceGuid) &&
                sourceGuid == targetGuid)
            {
                return desiredPath;
            }

            return AssetDatabase.GenerateUniqueAssetPath(
                desiredPath
            ); // 충돌 경로 생성
        }

        private static void EnsureAssetFolder(
            string folderPath
        )
        {
            if (string.IsNullOrEmpty(folderPath) ||
                AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string normalized = folderPath
                .Replace('\\', '/')
                .TrimEnd('/'); // 폴더 경로 정규화
            string parent = Path.GetDirectoryName(
                normalized
            )?.Replace('\\', '/'); // 부모 폴더
            string folderName = Path.GetFileName(
                normalized
            ); // 현재 폴더명

            if (string.IsNullOrEmpty(parent) ||
                string.IsNullOrEmpty(folderName))
            {
                return;
            }

            EnsureAssetFolder(parent); // 부모 폴더 우선 생성

            if (!AssetDatabase.IsValidFolder(normalized))
            {
                AssetDatabase.CreateFolder(
                    parent,
                    folderName
                ); // Unity 폴더 생성
            }
        }

        private static bool IsUnderThirdParty(
            string assetPath
        )
        {
            return assetPath.Equals(
                    ThirdPartyRoot,
                    StringComparison.OrdinalIgnoreCase
                ) ||
                assetPath.StartsWith(
                    ThirdPartyRoot + "/",
                    StringComparison.OrdinalIgnoreCase
                ); // ThirdParty 하위 판별
        }

        private static string WritePreviewReport()
        {
            StringBuilder text = new StringBuilder(); // 계획 보고서
            text.AppendLine("Project J Day141 ThirdParty 2C Preview");
            text.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            text.AppendLine();
            text.AppendLine($"MOVE: {MoveRules.Length}");
            text.AppendLine($"DELETE CANDIDATES: {DeleteCandidates.Length}");
            text.AppendLine();

            text.AppendLine("=== MOVE ===");

            foreach (MoveRule rule in MoveRules)
            {
                text.AppendLine(
                    $"{rule.Source} -> {rule.Destination}"
                );
            }

            text.AppendLine();
            text.AppendLine("=== DELETE CANDIDATES ===");

            foreach (string assetPath in DeleteCandidates)
            {
                text.AppendLine(assetPath);
            }

            return WriteReport(
                "ThirdParty2CPreview.txt",
                text.ToString()
            ); // 계획 보고서 저장
        }

        private static string WriteResultReport(
            List<string> moved,
            List<string> deleted,
            List<string> blocked,
            List<string> failures
        )
        {
            StringBuilder text = new StringBuilder(); // 결과 보고서
            text.AppendLine("Project J Day141 ThirdParty 2C Result");
            text.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            text.AppendLine();
            text.AppendLine($"MOVED: {moved.Count}");
            text.AppendLine($"DELETED: {deleted.Count}");
            text.AppendLine($"BLOCKED: {blocked.Count}");
            text.AppendLine($"FAILED: {failures.Count}");
            text.AppendLine();

            AppendSection(text, "MOVED", moved);
            AppendSection(text, "DELETED", deleted);
            AppendSection(text, "BLOCKED", blocked);
            AppendSection(text, "FAILED", failures);

            return WriteReport(
                "ThirdParty2CResult.txt",
                text.ToString()
            ); // 결과 보고서 저장
        }

        private static void AppendSection(
            StringBuilder text,
            string title,
            List<string> entries
        )
        {
            text.AppendLine($"=== {title} ===");

            foreach (string entry in entries)
            {
                text.AppendLine(entry);
            }

            text.AppendLine();
        }

        private static string WriteReport(
            string fileName,
            string content
        )
        {
            string directory = Path.Combine(
                GetProjectRoot(),
                "Day141Reports"
            ); // 보고서 폴더
            Directory.CreateDirectory(directory);

            string path = Path.Combine(
                directory,
                fileName
            ); // 보고서 경로

            File.WriteAllText(
                path,
                content,
                new UTF8Encoding(false)
            ); // UTF-8 저장

            return path;
        }

        private static string GetProjectRoot()
        {
            string projectRoot = Directory.GetParent(
                Application.dataPath
            )?.FullName; // Unity 프로젝트 루트

            return string.IsNullOrEmpty(projectRoot)
                ? Directory.GetCurrentDirectory()
                : projectRoot;
        }

        private static string MakeProjectRelativePath(
            string absolutePath
        )
        {
            string root = GetProjectRoot()
                .Replace('\\', '/'); // 프로젝트 루트
            string normalized = absolutePath
                .Replace('\\', '/'); // 절대 경로

            if (!normalized.StartsWith(
                    root,
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                return normalized;
            }

            return normalized
                .Substring(root.Length)
                .TrimStart('/'); // 프로젝트 상대 경로
        }
    }
}

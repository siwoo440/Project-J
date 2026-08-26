using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectJ.EditorTools
{
    public static class ProjectJDay141ThirdPartyMigration
    {
        private const string ThirdPartyRoot = "Assets/ProjectJ/ThirdParty";
        private const string ProjectRoot = "Assets/ProjectJ";

        private static readonly string[] DiscardWords =
        {
            "/demo/", "/demos/", "/sample/", "/samples/",
            "/example/", "/examples/", "/documentation/", "/docs/",
            "/tutorial/", "/tutorials/", "/preview/", "/previews/",
            "/screenshot/", "/screenshots/", "/readme", "/license",
            "/licenses/", "/changelog"
        };

        private static readonly string[] RiskWords =
        {
            "/resources/", "/streamingassets/", "/editor/"
        };

        private static readonly HashSet<string> ProductionExtensions =
            new HashSet<string>(
                new[]
                {
                    ".fbx", ".obj", ".dae", ".blend",
                    ".png", ".jpg", ".jpeg", ".tga", ".tif", ".tiff",
                    ".psd", ".exr", ".hdr", ".dds",
                    ".mat", ".prefab",
                    ".anim", ".controller", ".overridecontroller", ".mask",
                    ".wav", ".mp3", ".ogg", ".aiff", ".aif",
                    ".shader", ".shadergraph", ".shadersubgraph", ".compute",
                    ".ttf", ".otf", ".spriteatlas", ".spriteatlasv2",
                    ".uxml", ".uss", ".vfx"
                },
                StringComparer.OrdinalIgnoreCase
            );

        private static readonly HashSet<string> DocumentExtensions =
            new HashSet<string>(
                new[] { ".md", ".pdf", ".rtf", ".doc", ".docx", ".url", ".unitypackage" },
                StringComparer.OrdinalIgnoreCase
            );

        private enum ActionType
        {
            Promote,
            Discard,
            Review
        }

        private enum Category
        {
            Environment,
            Props,
            Characters,
            Items,
            UI,
            VFX
        }

        private sealed class Record
        {
            public string Path;
            public ActionType Action;
            public Category Category;
            public string Reason;
            public List<string> References = new List<string>();
        }

        [MenuItem("Project J/Day141/ThirdParty/1. Analyze ThirdParty")]
        private static void Analyze()
        {
            List<Record> records = BuildRecords(); // 전체 분석
            string report = WriteAnalysis(records); // 보고서 저장

            Debug.Log(
                $"[Project J/Day141] 분석 완료 - " +
                $"PROMOTE {records.Count(x => x.Action == ActionType.Promote)}, " +
                $"DISCARD {records.Count(x => x.Action == ActionType.Discard)}, " +
                $"REVIEW {records.Count(x => x.Action == ActionType.Review)}\n{report}"
            ); // 분석 결과
        }

        [MenuItem("Project J/Day141/ThirdParty/2. Promote Production Assets")]
        private static void Promote()
        {
            List<Record> targets =
                BuildRecords()
                    .Where(x => x.Action == ActionType.Promote)
                    .ToList(); // 승격 대상

            if (targets.Count == 0)
            {
                Debug.Log("[Project J/Day141] 승격 대상이 없습니다.");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Day141 Production Asset 승격",
                    $"{targets.Count}개 Asset을 ProjectJ 정식 폴더로 이동합니다.\n" +
                    "AssetDatabase.MoveAsset을 사용해 GUID를 유지합니다.",
                    "승격 실행",
                    "취소"
                ))
            {
                return;
            }

            List<string> failures = new List<string>(); // 실패 목록

            try
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    Record record = targets[i]; // 현재 대상
                    EditorUtility.DisplayProgressBar(
                        "Day141 ThirdParty 승격",
                        record.Path,
                        (float)i / targets.Count
                    ); // 진행 표시

                    string destination = GetUniqueDestination(record); // 대상 경로
                    EnsureFolder(
                        Path.GetDirectoryName(destination)?.Replace('\\', '/')
                    ); // 대상 폴더

                    string error =
                        AssetDatabase.MoveAsset(record.Path, destination); // GUID 유지 이동

                    if (!string.IsNullOrEmpty(error))
                    {
                        failures.Add($"{record.Path} -> {destination}: {error}");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string report = WriteAnalysis(BuildRecords()); // 이동 후 재분석

            if (failures.Count > 0)
            {
                string failureReport = WriteText(
                    "ThirdPartyMigrationFailures.txt",
                    string.Join(Environment.NewLine, failures)
                ); // 실패 보고서

                Debug.LogError(
                    $"[Project J/Day141] 일부 승격 실패\n{failureReport}"
                );
                return;
            }

            Debug.Log($"[Project J/Day141] 승격 완료\n{report}");
        }

        [MenuItem("Project J/Day141/ThirdParty/3. Verify References")]
        private static void VerifyMenu()
        {
            Verify(true); // 참조 검증
        }

        [MenuItem("Project J/Day141/ThirdParty/4. Delete ThirdParty")]
        private static void DeleteThirdParty()
        {
            if (!AssetDatabase.IsValidFolder(ThirdPartyRoot))
            {
                Debug.Log("[Project J/Day141] ThirdParty 폴더가 없습니다.");
                return;
            }

            if (!Verify(false))
            {
                EditorUtility.DisplayDialog(
                    "삭제 차단",
                    "외부 참조 또는 REVIEW 대상이 남아 있습니다.\n" +
                    "Day141Reports/ThirdPartyVerification.txt를 확인하세요.",
                    "확인"
                );
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "ThirdParty 최종 삭제",
                    "ThirdParty 전체를 삭제합니다. README와 LICENSE도 함께 삭제됩니다.\n\n" +
                    "라이선스 파일을 삭제해도 해당 에셋의 라이선스 의무는 유지됩니다.\n" +
                    "Unity 실제 플레이 검증을 완료한 뒤 실행하세요.",
                    "삭제",
                    "취소"
                ))
            {
                return;
            }

            bool deleted = AssetDatabase.DeleteAsset(ThirdPartyRoot); // 최종 삭제
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!deleted)
            {
                Debug.LogError("[Project J/Day141] ThirdParty 삭제 실패");
                return;
            }

            Debug.Log(
                "[Project J/Day141] ThirdParty 삭제 완료. " +
                "Scene/Prefab/Material/Host/Client/Bot을 다시 검증하세요."
            );
        }

        [MenuItem("Project J/Day141/ThirdParty/Open Report Folder")]
        private static void OpenReportFolder()
        {
            string path = GetReportDirectory(); // 보고서 폴더
            Directory.CreateDirectory(path);
            EditorUtility.RevealInFinder(path);
        }

        private static bool Verify(bool log)
        {
            List<Record> records = BuildRecords(); // 최신 분석
            int referenced = records.Count(x => x.References.Count > 0);
            int review = records.Count(x => x.Action == ActionType.Review);
            bool safe = referenced == 0 && review == 0; // 삭제 조건

            string report = WriteVerification(records, safe);

            if (log)
            {
                if (safe)
                {
                    Debug.Log($"[Project J/Day141] 검증 통과\n{report}");
                }
                else
                {
                    Debug.LogWarning(
                        $"[Project J/Day141] 삭제 보류 - " +
                        $"외부 참조 {referenced}, REVIEW {review}\n{report}"
                    );
                }
            }

            return safe;
        }

        private static List<Record> BuildRecords()
        {
            List<string> assets =
                AssetDatabase.GetAllAssetPaths()
                    .Where(
                        x => IsThirdParty(x) &&
                        !AssetDatabase.IsValidFolder(x)
                    )
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToList(); // ThirdParty 파일

            Dictionary<string, List<string>> references =
                FindExternalReferences(assets); // 외부 역참조

            List<Record> records = new List<Record>();

            foreach (string path in assets)
            {
                references.TryGetValue(path, out List<string> refs);
                records.Add(Classify(path, refs ?? new List<string>()));
            }

            return records;
        }

        private static Dictionary<string, List<string>> FindExternalReferences(
            List<string> thirdPartyAssets
        )
        {
            HashSet<string> targets =
                new HashSet<string>(
                    thirdPartyAssets,
                    StringComparer.OrdinalIgnoreCase
                ); // 대상 집합

            Dictionary<string, List<string>> result =
                new Dictionary<string, List<string>>(
                    StringComparer.OrdinalIgnoreCase
                ); // 역참조 결과

            string[] all = AssetDatabase.GetAllAssetPaths();

            try
            {
                for (int i = 0; i < all.Length; i++)
                {
                    string source = all[i];

                    if (!source.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) ||
                        IsThirdParty(source) ||
                        AssetDatabase.IsValidFolder(source))
                    {
                        continue;
                    }

                    if (i % 25 == 0)
                    {
                        EditorUtility.DisplayProgressBar(
                            "Day141 ThirdParty 참조 분석",
                            source,
                            (float)i / all.Length
                        );
                    }

                    string[] dependencies;

                    try
                    {
                        dependencies = AssetDatabase.GetDependencies(source, true);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning(
                            $"[Project J/Day141] 의존성 조회 실패: {source}\n" +
                            exception.Message
                        );
                        continue;
                    }

                    foreach (string dependency in dependencies)
                    {
                        if (!targets.Contains(dependency))
                        {
                            continue;
                        }

                        if (!result.TryGetValue(dependency, out List<string> referencers))
                        {
                            referencers = new List<string>();
                            result.Add(dependency, referencers);
                        }

                        if (!referencers.Any(
                                x => x.Equals(
                                    source,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            ))
                        {
                            referencers.Add(source);
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return result;
        }

        private static Record Classify(
            string path,
            List<string> references
        )
        {
            string normalized = path.Replace('\\', '/').ToLowerInvariant();
            string extension = Path.GetExtension(path).ToLowerInvariant();
            Category category = InferCategory(normalized);
            bool used = references.Count > 0;

            if (IsDiscardPath(normalized) && !used)
            {
                return NewRecord(
                    path,
                    ActionType.Discard,
                    category,
                    "Demo/Sample/문서/README/LICENSE 미사용 파일",
                    references
                );
            }

            if (RiskWords.Any(normalized.Contains))
            {
                return NewRecord(
                    path,
                    ActionType.Review,
                    category,
                    "Resources/StreamingAssets/Editor 경로 수동 검토 필요",
                    references
                );
            }

            if (ProductionExtensions.Contains(extension))
            {
                return NewRecord(
                    path,
                    ActionType.Promote,
                    category,
                    used
                        ? "ProjectJ 외부 참조가 있는 Production Asset"
                        : "재사용 가능한 Production Asset",
                    references
                );
            }

            if (DocumentExtensions.Contains(extension) && !used)
            {
                return NewRecord(
                    path,
                    ActionType.Discard,
                    category,
                    "문서/패키지 미사용 파일",
                    references
                );
            }

            return NewRecord(
                path,
                ActionType.Review,
                category,
                BuildReviewReason(extension, used),
                references
            );
        }

        private static Record NewRecord(
            string path,
            ActionType action,
            Category category,
            string reason,
            List<string> references
        )
        {
            return new Record
            {
                Path = path,
                Action = action,
                Category = category,
                Reason = reason,
                References = references
            };
        }

        private static string BuildReviewReason(
            string extension,
            bool used
        )
        {
            string prefix = used ? "외부 참조 존재. " : string.Empty;

            if (extension == ".cs" ||
                extension == ".asmdef" ||
                extension == ".asmref" ||
                extension == ".dll")
            {
                return prefix + "Script/Assembly 코드 의존성 수동 확인 필요";
            }

            if (extension == ".asset" ||
                extension == ".json" ||
                extension == ".xml" ||
                extension == ".bytes" ||
                extension == ".inputactions")
            {
                return prefix + "데이터/경로 기반 로드 여부 수동 확인 필요";
            }

            if (extension == ".unity")
            {
                return prefix + "Scene 사용 여부 수동 확인 필요";
            }

            return prefix + $"알 수 없는 확장자 '{extension}' 수동 확인 필요";
        }

        private static bool IsDiscardPath(string normalized)
        {
            if (DiscardWords.Any(normalized.Contains))
            {
                return true;
            }

            string name =
                Path.GetFileNameWithoutExtension(normalized);

            return name == "readme" ||
                name.StartsWith("readme_") ||
                name == "license" ||
                name.StartsWith("license_") ||
                name.StartsWith("licence") ||
                name.StartsWith("changelog");
        }

        private static Category InferCategory(string value)
        {
            if (Has(value, "colorful_ui", "gameinputcontrollericons", "/ui/", "/icons/", "sprite"))
            {
                return Category.UI;
            }

            if (Has(value, "/vfx/", "particle", "effect", "/fx/"))
            {
                return Category.VFX;
            }

            if (Has(value, "character", "/player/", "avatar", "human", "humanoid"))
            {
                return Category.Characters;
            }

            if (Has(value, "/item/", "/items/", "pickup", "weapon", "consumable"))
            {
                return Category.Items;
            }

            if (Has(
                    value,
                    "platform",
                    "playground",
                    "environment",
                    "/map/",
                    "terrain",
                    "ground",
                    "building",
                    "shipping container",
                    "shipping_container",
                    "/road/"
                ))
            {
                return Category.Environment;
            }

            return Category.Props;
        }

        private static bool Has(
            string value,
            params string[] words
        )
        {
            return words.Any(value.Contains);
        }

        private static string GetUniqueDestination(Record record)
        {
            string directory = GetDestinationDirectory(record);
            string name = Path.GetFileName(record.Path);
            string target = $"{directory}/{name}";

            if (AssetDatabase.LoadMainAssetAtPath(target) == null)
            {
                return target;
            }

            string vendor = GetVendor(record.Path);
            string renamed =
                $"{Path.GetFileNameWithoutExtension(name)}__{Sanitize(vendor)}" +
                $"{Path.GetExtension(name)}";

            return AssetDatabase.GenerateUniqueAssetPath(
                $"{directory}/{renamed}"
            );
        }

        private static string GetDestinationDirectory(Record record)
        {
            string extension =
                Path.GetExtension(record.Path).ToLowerInvariant();

            if (extension == ".wav" ||
                extension == ".mp3" ||
                extension == ".ogg" ||
                extension == ".aiff" ||
                extension == ".aif")
            {
                return $"{ProjectRoot}/Audio/Imported";
            }

            if (extension == ".shader" ||
                extension == ".shadergraph" ||
                extension == ".shadersubgraph" ||
                extension == ".compute")
            {
                return $"{ProjectRoot}/Art/Shaders/Imported";
            }

            if (record.Category == Category.UI)
            {
                if (extension == ".prefab")
                {
                    return $"{ProjectRoot}/Prefabs/Props/UI/Imported";
                }

                return $"{ProjectRoot}/Art/UI/Imported";
            }

            if (record.Category == Category.VFX)
            {
                if (extension == ".prefab")
                {
                    return $"{ProjectRoot}/Prefabs/Props/VFX/Imported";
                }

                return $"{ProjectRoot}/Art/VFX/Imported";
            }

            if (extension == ".anim" ||
                extension == ".controller" ||
                extension == ".overridecontroller" ||
                extension == ".mask")
            {
                return $"{ProjectRoot}/Art/Characters/Animations/Imported";
            }

            if (extension == ".prefab")
            {
                switch (record.Category)
                {
                    case Category.Characters:
                        return $"{ProjectRoot}/Prefabs/Player/Imported";

                    case Category.Items:
                        return $"{ProjectRoot}/Prefabs/Items/Imported";

                    case Category.Environment:
                        return $"{ProjectRoot}/Prefabs/Map/Imported";

                    default:
                        return $"{ProjectRoot}/Prefabs/Props/Imported";
                }
            }

            string root = $"{ProjectRoot}/Art/{record.Category}";

            if (extension == ".fbx" ||
                extension == ".obj" ||
                extension == ".dae" ||
                extension == ".blend")
            {
                return $"{root}/Meshes/Imported";
            }

            if (extension == ".mat")
            {
                return $"{root}/Materials/Imported";
            }

            if (extension == ".png" ||
                extension == ".jpg" ||
                extension == ".jpeg" ||
                extension == ".tga" ||
                extension == ".tif" ||
                extension == ".tiff" ||
                extension == ".psd" ||
                extension == ".exr" ||
                extension == ".hdr" ||
                extension == ".dds")
            {
                return $"{root}/Textures/Imported";
            }

            return $"{root}/Imported";
        }

        private static string GetVendor(string path)
        {
            string relative =
                path.Replace('\\', '/')
                    .Substring((ThirdPartyRoot + "/").Length);

            int slash = relative.IndexOf('/');
            return slash >= 0
                ? relative.Substring(0, slash)
                : relative;
        }

        private static string Sanitize(string value)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            StringBuilder builder = new StringBuilder();

            foreach (char character in value)
            {
                builder.Append(
                    invalid.Contains(character) || char.IsWhiteSpace(character)
                        ? '_'
                        : character
                );
            }

            return builder.ToString();
        }

        private static void EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder) ||
                AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string normalized = folder.Replace('\\', '/').TrimEnd('/');
            string parent =
                Path.GetDirectoryName(normalized)?.Replace('\\', '/');
            string name = Path.GetFileName(normalized);

            if (string.IsNullOrEmpty(parent) ||
                string.IsNullOrEmpty(name))
            {
                return;
            }

            EnsureFolder(parent);

            if (!AssetDatabase.IsValidFolder(normalized))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static bool IsThirdParty(string path)
        {
            return path.Equals(
                    ThirdPartyRoot,
                    StringComparison.OrdinalIgnoreCase
                ) ||
                path.StartsWith(
                    ThirdPartyRoot + "/",
                    StringComparison.OrdinalIgnoreCase
                );
        }

        private static string WriteAnalysis(List<Record> records)
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("Project J Day141 ThirdParty Analysis");
            text.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            text.AppendLine();
            text.AppendLine($"PROMOTE: {records.Count(x => x.Action == ActionType.Promote)}");
            text.AppendLine($"DISCARD: {records.Count(x => x.Action == ActionType.Discard)}");
            text.AppendLine($"REVIEW : {records.Count(x => x.Action == ActionType.Review)}");
            text.AppendLine();

            foreach (Record record in records)
            {
                text.AppendLine(
                    $"[{record.Action}] [{record.Category}] {record.Path}"
                );
                text.AppendLine($"  Reason: {record.Reason}");

                if (record.Action == ActionType.Promote)
                {
                    text.AppendLine(
                        $"  Target: {GetDestinationDirectory(record)}"
                    );
                }

                foreach (string referencer in record.References)
                {
                    text.AppendLine($"  <- {referencer}");
                }

                text.AppendLine();
            }

            return WriteText("ThirdPartyAnalysis.txt", text.ToString());
        }

        private static string WriteVerification(
            List<Record> records,
            bool safe
        )
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("Project J Day141 ThirdParty Verification");
            text.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            text.AppendLine(
                $"Assets with external references: " +
                $"{records.Count(x => x.References.Count > 0)}"
            );
            text.AppendLine(
                $"Manual review blockers: " +
                $"{records.Count(x => x.Action == ActionType.Review)}"
            );
            text.AppendLine(
                $"Safe to delete ThirdParty: {(safe ? "YES" : "NO")}"
            );
            text.AppendLine();

            foreach (Record record in records.Where(
                         x => x.References.Count > 0 ||
                         x.Action == ActionType.Review
                     ))
            {
                text.AppendLine(
                    $"[{record.Action}] {record.Path}"
                );
                text.AppendLine($"  Reason: {record.Reason}");

                foreach (string referencer in record.References)
                {
                    text.AppendLine($"  <- {referencer}");
                }

                text.AppendLine();
            }

            return WriteText(
                "ThirdPartyVerification.txt",
                text.ToString()
            );
        }

        private static string WriteText(
            string fileName,
            string content
        )
        {
            string directory = GetReportDirectory();
            Directory.CreateDirectory(directory);

            string path = Path.Combine(directory, fileName);
            File.WriteAllText(
                path,
                content,
                new UTF8Encoding(false)
            );

            return path;
        }

        private static string GetReportDirectory()
        {
            string project =
                Directory.GetParent(Application.dataPath)?.FullName;

            if (string.IsNullOrEmpty(project))
            {
                project = Directory.GetCurrentDirectory();
            }

            return Path.Combine(project, "Day141Reports");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlaneLedger.Data.Models;
using PlaneLedger.Data.SO;
using UnityEditor;
using UnityEngine;

namespace PlaneLedger.Editor.Tools
{
    /// <summary>
    /// CSV → ScriptableObject 一键导入工具。
    /// 菜单: PlaneLedger → CSV 导入工具
    /// CSV 路径: Assets/_Project/Data/CSV/
    /// SO 输出路径: Assets/_Project/ScriptableObjects/{类型}/
    /// </summary>
    public class CSVImporter : EditorWindow
    {
        private const string CSV_ROOT = "Assets/_Project/Data/CSV";
        private const string SO_ROOT = "Assets/_Project/ScriptableObjects";

        private Vector2 scrollPos;
        private string lastMessage = "";
        private MessageType lastMessageType = MessageType.None;

        [MenuItem("PlaneLedger/CSV 导入工具")]
        public static void ShowWindow()
        {
            var window = GetWindow<CSVImporter>("CSV 导入工具");
            window.minSize = new Vector2(400, 320);
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.LabelField("CSV → ScriptableObject 一键导入", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "CSV 文件位于: Assets/_Project/Data/CSV/\n" +
                "SO 输出到: Assets/_Project/ScriptableObjects/{类型}/\n\n" +
                "已有同 Id 的 SO 会被更新（不会丢失手动设置的 Icon/Sprite 等引用）。",
                MessageType.Info);
            EditorGUILayout.Space(10);

            if (GUILayout.Button("导入 装饰品 (decorations.csv)", GUILayout.Height(30)))
            {
                ImportDecorations();
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("导入 残章卡片 (wisdom_cards.csv)", GUILayout.Height(30)))
            {
                ImportWisdomCards();
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("导入 成就 (achievements.csv)", GUILayout.Height(30)))
            {
                ImportAchievements();
            }

            EditorGUILayout.Space(15);

            if (GUILayout.Button("全部导入", GUILayout.Height(40)))
            {
                ImportDecorations();
                ImportWisdomCards();
                ImportAchievements();
            }

            EditorGUILayout.Space(10);

            if (!string.IsNullOrEmpty(lastMessage))
            {
                EditorGUILayout.HelpBox(lastMessage, lastMessageType);
            }

            EditorGUILayout.EndScrollView();
        }

        // ─────────────────────────── 装饰品 ───────────────────────────

        private void ImportDecorations()
        {
            string csvPath = Path.Combine(CSV_ROOT, "decorations.csv");
            var rows = ReadCSV(csvPath);
            if (rows == null) return;

            string outputDir = Path.Combine(SO_ROOT, "Decorations");
            EnsureDirectory(outputDir);

            // 索引已有 SO，按 Id 查找
            var existing = LoadExistingSOs<DecorationSO>(outputDir);

            int created = 0, updated = 0;

            foreach (var row in rows)
            {
                string id = GetValue(row, "Id");
                if (string.IsNullOrEmpty(id)) continue;

                DecorationSO so;
                if (existing.TryGetValue(id, out so))
                {
                    updated++;
                }
                else
                {
                    so = CreateInstance<DecorationSO>();
                    created++;
                }

                // 填充字段
                so.Id = id;
                so.DisplayName = GetValue(row, "DisplayName");
                so.VisualDescription = GetValue(row, "VisualDescription");
                so.Slot = ParseEnum<DecorationSlot>(GetValue(row, "Slot"));
                so.Style = ParseEnum<DecorationStyle>(GetValue(row, "Style"));
                so.Rarity = ParseEnum<Rarity>(GetValue(row, "Rarity"));
                so.CanDropFromPool = ParseBool(GetValue(row, "CanDropFromPool"));
                so.TokenPrice = ParseInt(GetValue(row, "TokenPrice"));
                so.UnlockAchievementId = GetValue(row, "UnlockAchievementId");
                so.HasSpecialEffect = ParseBool(GetValue(row, "HasSpecialEffect"));
                so.Version = GetValue(row, "Version", "1.0");

                // Description 使用 VisualDescription（装饰品CSV无单独Description列）
                so.Description = so.VisualDescription;

                SaveSO(so, outputDir, id, existing.ContainsKey(id));
            }

            SetMessage($"装饰品导入完成：新建 {created}，更新 {updated}", MessageType.Info);
        }

        // ─────────────────────────── 残章卡片 ───────────────────────────

        private void ImportWisdomCards()
        {
            string csvPath = Path.Combine(CSV_ROOT, "wisdom_cards.csv");
            var rows = ReadCSV(csvPath);
            if (rows == null) return;

            string outputDir = Path.Combine(SO_ROOT, "WisdomCards");
            EnsureDirectory(outputDir);

            var existing = LoadExistingSOs<WisdomCardSO>(outputDir);

            int created = 0, updated = 0;

            foreach (var row in rows)
            {
                string id = GetValue(row, "Id");
                if (string.IsNullOrEmpty(id)) continue;

                WisdomCardSO so;
                if (existing.TryGetValue(id, out so))
                {
                    updated++;
                }
                else
                {
                    so = CreateInstance<WisdomCardSO>();
                    created++;
                }

                so.Id = id;
                so.DisplayName = GetValue(row, "DisplayName");
                so.BookSource = GetValue(row, "BookSource");
                so.Faction = ParseEnum<CardFaction>(GetValue(row, "Faction"));
                so.Rarity = ParseEnum<Rarity>(GetValue(row, "Rarity"));
                so.TokenPrice = ParseInt(GetValue(row, "TokenPrice"), 30);
                so.OriginalQuote = GetValue(row, "OriginalQuote");
                so.PlaneAnnotation = GetValue(row, "PlaneAnnotation");

                SaveSO(so, outputDir, id, existing.ContainsKey(id));
            }

            SetMessage($"残章卡片导入完成：新建 {created}，更新 {updated}", MessageType.Info);
        }

        // ─────────────────────────── 成就 ───────────────────────────

        private void ImportAchievements()
        {
            string csvPath = Path.Combine(CSV_ROOT, "achievements.csv");
            var rows = ReadCSV(csvPath);
            if (rows == null) return;

            string outputDir = Path.Combine(SO_ROOT, "Achievements");
            EnsureDirectory(outputDir);

            var existing = LoadExistingSOs<AchievementSO>(outputDir);

            int created = 0, updated = 0;

            foreach (var row in rows)
            {
                string id = GetValue(row, "Id");
                if (string.IsNullOrEmpty(id)) continue;

                AchievementSO so;
                if (existing.TryGetValue(id, out so))
                {
                    updated++;
                }
                else
                {
                    so = CreateInstance<AchievementSO>();
                    created++;
                }

                so.Id = id;
                so.SteamApiName = id.Replace("-", "_"); // A-001 → A_001
                so.DisplayName = GetValue(row, "DisplayName");
                so.Description = GetValue(row, "Description");
                so.Category = ParseEnum<AchievementCategory>(GetValue(row, "Category"));
                so.CheckTiming = ParseEnum<AchievementCheckTiming>(GetValue(row, "CheckTiming"));
                so.ConditionType = ParseEnum<AchievementConditionType>(GetValue(row, "ConditionType"));
                so.TargetValue = ParseInt(GetValue(row, "TargetValue"), 1);
                so.IsHidden = ParseBool(GetValue(row, "IsHidden"));
                so.TokenReward = ParseInt(GetValue(row, "TokenReward"));
                so.DecorationRewardId = GetValue(row, "DecorationRewardId");
                so.Version = GetValue(row, "Version", "1.0");

                SaveSO(so, outputDir, id, existing.ContainsKey(id));
            }

            SetMessage($"成就导入完成：新建 {created}，更新 {updated}", MessageType.Info);
        }

        // ─────────────────────────── CSV 解析 ───────────────────────────

        /// <summary>
        /// 读取 CSV 文件，返回每行的 header→value 字典列表。
        /// 支持带引号的字段（处理字段内逗号）。
        /// </summary>
        private List<Dictionary<string, string>> ReadCSV(string assetPath)
        {
            string fullPath = Path.Combine(Application.dataPath, "..", assetPath);
            if (!File.Exists(fullPath))
            {
                SetMessage($"找不到文件: {assetPath}", MessageType.Error);
                return null;
            }

            var lines = File.ReadAllLines(fullPath, System.Text.Encoding.UTF8);
            if (lines.Length < 2)
            {
                SetMessage($"CSV 文件为空或只有表头: {assetPath}", MessageType.Warning);
                return null;
            }

            var headers = ParseCSVLine(lines[0]);
            var result = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var values = ParseCSVLine(line);
                var dict = new Dictionary<string, string>();

                for (int j = 0; j < headers.Count; j++)
                {
                    dict[headers[j]] = j < values.Count ? values[j] : "";
                }

                result.Add(dict);
            }

            return result;
        }

        /// <summary>解析一行 CSV，支持双引号包围的字段</summary>
        private List<string> ParseCSVLine(string line)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // 检查是否是转义的双引号 ""
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        fields.Add(current.ToString().Trim());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            fields.Add(current.ToString().Trim());
            return fields;
        }

        // ─────────────────────────── 工具方法 ───────────────────────────

        private string GetValue(Dictionary<string, string> row, string key, string fallback = "")
        {
            string val;
            if (row.TryGetValue(key, out val) && !string.IsNullOrEmpty(val))
                return val;
            return fallback;
        }

        private T ParseEnum<T>(string value) where T : struct
        {
            if (string.IsNullOrEmpty(value)) return default;
            T result;
            if (Enum.TryParse(value, true, out result))
                return result;
            Debug.LogWarning($"[CSVImporter] 无法解析枚举 {typeof(T).Name}: '{value}'");
            return default;
        }

        private bool ParseBool(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value == "1";
        }

        private int ParseInt(string value, int fallback = 0)
        {
            if (string.IsNullOrEmpty(value)) return fallback;
            int result;
            return int.TryParse(value, out result) ? result : fallback;
        }

        /// <summary>扫描目录下所有指定类型的 SO，建立 Id→SO 索引</summary>
        private Dictionary<string, T> LoadExistingSOs<T>(string dir) where T : ScriptableObject
        {
            var dict = new Dictionary<string, T>();
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { dir });

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<T>(path);
                if (so == null) continue;

                // 通过反射获取 Id 字段
                var idField = typeof(T).GetField("Id");
                if (idField != null)
                {
                    string id = idField.GetValue(so) as string;
                    if (!string.IsNullOrEmpty(id))
                        dict[id] = so;
                }
            }

            return dict;
        }

        private void EnsureDirectory(string assetDir)
        {
            string fullPath = Path.Combine(Application.dataPath, "..", assetDir);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }
        }

        private void SaveSO(ScriptableObject so, string dir, string id, bool alreadyExists)
        {
            if (alreadyExists)
            {
                EditorUtility.SetDirty(so);
            }
            else
            {
                // 文件名：去掉特殊字符，用 Id 命名
                string safeName = id.Replace("/", "_").Replace("\\", "_");
                string path = Path.Combine(dir, $"{safeName}.asset");
                AssetDatabase.CreateAsset(so, path);
            }

            AssetDatabase.SaveAssets();
        }

        private void SetMessage(string msg, MessageType type)
        {
            lastMessage = msg;
            lastMessageType = type;
            Debug.Log($"[CSVImporter] {msg}");
            Repaint();
        }
    }
}

using System;
using System.IO;
using System.Text;
using PlaneLedger.Data.Models;
using UnityEngine;

namespace PlaneLedger.Core
{
    /// <summary>
    /// 存档系统。负责 JSON 序列化/反序列化、自动备份、版本迁移。
    /// 存档路径使用 Application.persistentDataPath，兼容 Steam Auto-Cloud。
    /// </summary>
    public class SaveSystem
    {
        private const string SAVE_FILE = "save_profile.json";
        private const string BACKUP_FILE = "save_profile_backup.json";
        private const string SETTINGS_FILE = "settings.json";
        private const int CURRENT_VERSION = 1;

        private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE);
        private string BackupPath => Path.Combine(Application.persistentDataPath, BACKUP_FILE);
        private string SettingsPath => Path.Combine(Application.persistentDataPath, SETTINGS_FILE);

        private SaveData _cachedData;

        /// <summary>
        /// 加载存档。如果不存在或损坏，尝试加载备份；都失败则创建新存档。
        /// </summary>
        public SaveData Load()
        {
            // 尝试加载主存档
            _cachedData = TryLoadFromFile(SavePath);

            // 主存档失败，尝试备份
            if (_cachedData == null)
            {
                Debug.LogWarning("[SaveSystem] 主存档加载失败，尝试加载备份...");
                _cachedData = TryLoadFromFile(BackupPath);

                if (_cachedData != null)
                {
                    Debug.Log("[SaveSystem] 从备份恢复成功");
                    Save(_cachedData); // 用备份覆盖损坏的主存档
                }
            }

            // 都失败，创建新存档
            if (_cachedData == null)
            {
                Debug.Log("[SaveSystem] 创建新存档");
                _cachedData = CreateNewSave();
                Save(_cachedData);
            }

            // 版本迁移
            if (_cachedData.Version < CURRENT_VERSION)
            {
                MigrateVersion(_cachedData);
                Save(_cachedData);
            }

            return _cachedData;
        }

        /// <summary>
        /// 保存存档。自动备份上一份主存档。
        /// </summary>
        public void Save(SaveData data)
        {
            try
            {
                // 备份当前主存档
                if (File.Exists(SavePath))
                {
                    File.Copy(SavePath, BackupPath, true);
                }

                // 写入新存档
                data.LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 保存失败: {e.Message}");
            }
        }

        /// <summary>
        /// 获取缓存的存档数据（避免重复读文件）。
        /// </summary>
        public SaveData GetCachedData()
        {
            return _cachedData ?? Load();
        }

        /// <summary>
        /// 快速保存缓存的数据（供自动保存使用）。
        /// </summary>
        public void SaveCached()
        {
            if (_cachedData != null)
            {
                Save(_cachedData);
            }
        }

        /// <summary>
        /// 导出存档到指定路径（用户手动备份）。
        /// </summary>
        public bool ExportSave(string targetPath)
        {
            try
            {
                if (_cachedData != null)
                {
                    string json = JsonUtility.ToJson(_cachedData, true);
                    File.WriteAllText(targetPath, json, Encoding.UTF8);
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 导出失败: {e.Message}");
            }
            return false;
        }

        /// <summary>
        /// 从指定路径导入存档。
        /// </summary>
        public SaveData ImportSave(string sourcePath)
        {
            var data = TryLoadFromFile(sourcePath);
            if (data != null)
            {
                _cachedData = data;
                Save(_cachedData);
            }
            return data;
        }

        private SaveData TryLoadFromFile(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;

                string json = File.ReadAllText(path, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json)) return null;

                var data = JsonUtility.FromJson<SaveData>(json);
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 读取文件失败 {path}: {e.Message}");
                return null;
            }
        }

        private SaveData CreateNewSave()
        {
            var data = new SaveData
            {
                Version = CURRENT_VERSION,
                LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            data.Profile.RegisterDate = DateTime.Now.ToString("yyyy-MM-dd");
            data.Settlement.LastLoginDate = "";
            data.Settlement.LastSettlementDate = "";

            // 装备默认装饰
            data.Collection.EquippedDecorations[0] = "DEC_BOOK_DEFAULT";    // 破旧账本
            data.Collection.EquippedDecorations[1] = "DEC_CURTAIN_DEFAULT"; // 素白薄纱
            data.Collection.EquippedDecorations[2] = "DEC_WINDOW_DEFAULT";  // 平静湖山
            data.Collection.EquippedDecorations[3] = "DEC_CARPET_DEFAULT";  // 素色木地板
            data.Collection.EquippedDecorations[4] = "DEC_LAMP_DEFAULT";    // 普通吊灯
            data.Collection.EquippedDecorations[5] = "DEC_FURN_DEFAULT";    // 普通书桌
            data.Collection.EquippedDecorations[6] = "";                     // 无特效

            // 默认装饰视为已解锁
            for (int i = 0; i < 6; i++)
            {
                data.Collection.UnlockedDecorationIds.Add(data.Collection.EquippedDecorations[i]);
            }

            return data;
        }

        /// <summary>
        /// 版本迁移链。按版本号逐步升级。
        /// </summary>
        private void MigrateVersion(SaveData data)
        {
            // 示例：未来版本升级时在此添加迁移逻辑
            // if (data.Version < 2) { MigrateV1ToV2(data); data.Version = 2; }
            // if (data.Version < 3) { MigrateV2ToV3(data); data.Version = 3; }

            data.Version = CURRENT_VERSION;
            Debug.Log($"[SaveSystem] 存档已迁移到版本 {CURRENT_VERSION}");
        }
    }
}

using UnityEngine;

namespace PlaneLedger.Steam
{
    /// <summary>
    /// Steam 初始化器。在 Boot 场景 GameManager 之前执行。
    /// 需要导入 Steamworks.NET 包后取消注释 Steam API 调用。
    /// </summary>
    public class SteamInitializer : MonoBehaviour
    {
        // TODO: 导入 Steamworks.NET 后取消下面的注释

        // [SerializeField] private uint _appId = 480; // 替换为实际 AppID (480 是 Spacewar 测试ID)

        private void Awake()
        {
            // try
            // {
            //     if (!SteamAPI.Init())
            //     {
            //         Debug.LogError("[Steam] SteamAPI.Init() 失败。请确认 Steam 客户端正在运行。");
            //         return;
            //     }
            //     Debug.Log("[Steam] 初始化成功");
            // }
            // catch (System.DllNotFoundException e)
            // {
            //     Debug.LogWarning($"[Steam] Steamworks 未安装: {e.Message}");
            // }

            Debug.Log("[Steam] SteamInitializer 就绪（Steamworks.NET 待导入）");
        }

        private void Update()
        {
            // SteamAPI.RunCallbacks();
        }

        private void OnApplicationQuit()
        {
            // SteamAPI.Shutdown();
        }
    }
}

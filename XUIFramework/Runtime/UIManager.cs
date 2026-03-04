using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using XAsset.Runtime;
using XAsset.Tools;

namespace XUIFramework
{
    /// <summary>
    /// UI 管理器
    /// 负责 UI 的加载、缓存、层级管理和生命周期调度
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {

        #region 配置 

        private Transform uiRoot;
        private UIConfig uiConfig; // UI 配置文件引用

        // 基础 SortingOrder，每个新窗口在此基础上递增
        private const int BaseSortingOrder = 100;
        // 每个窗口占用的 Order 跨度
        private const int SortingOrderStep = 10;
        
        public Camera uiCamera;
        public EventSystem  eventSystem;

        #endregion

        #region 运行时数据

        // 缓存所有已加载的 UI (Key: UI资源路径, Value: UI实例)
        private Dictionary<string, XUIBase> _loadedWindows = new Dictionary<string, XUIBase>();
        
        // 正在显示的窗口列表 (用于管理层级和 Update)
        // 顺序即层级，索引越大层级越高
        private List<XUIBase> _activeWindows = new List<XUIBase>();
        
        /// <summary>
        /// 用于消息队列UI
        /// </summary>
        private Queue<WindowQueueItem> _messageQueue = new Queue<WindowQueueItem>();
        
        /// <summary>
        /// 是否开始弹出UI
        /// </summary>
        private bool startPopWindow = false;

        #endregion

        #region 生命周期
        /// <summary>
        /// 初始化接口
        /// </summary>
        public void OnInit()
        {
            if (uiRoot == null)
            {
                // 尝试查找或创建默认 UIRoot
                GameObject rootGo = GameObject.Find("UIRoot");
                if (rootGo == null)
                {
                    rootGo = new GameObject("UIRoot");
                    rootGo.transform.position = Vector3.one;
                    GameObject.DontDestroyOnLoad(rootGo);
                }
                uiRoot = rootGo.transform;
            }
   
            if (uiConfig == null)
            {
                GameObject cameraObj = Resources.Load<GameObject>("UICamera");
                cameraObj = GameObject.Instantiate(cameraObj);
                GameObject.DontDestroyOnLoad(cameraObj);
                uiCamera =  cameraObj.GetComponent<Camera>();
            }

            if (eventSystem == null)
            {
                GameObject eventSystemObj = Resources.Load<GameObject>("EventSystem");
                eventSystemObj =  GameObject.Instantiate(eventSystemObj);
                GameObject.DontDestroyOnLoad(eventSystemObj);
                eventSystem = eventSystemObj.GetComponent<EventSystem>();
            }
            
            #if UNITY_EDITOR
            uiConfig = AssetDatabase.LoadAssetAtPath<UIConfig>("Assets/XLHFrameWork/XUIFramework/Config/UIConfig.asset");
            if (uiConfig == null)
            {
                Debug.LogError("uiConfig  is null!!!!!");
                return;
            }
            uiConfig.Initialize();
#endif
        }

        /// <summary>
        /// 渲染帧更新接口
        /// </summary>
        private void OnUpdate()
        {
            float dt = Time.deltaTime;
            // 轮询驱动所有激活窗口的 Update
            // 倒序遍历以防在 Update 中关闭窗口导致集合修改异常
            for (int i = _activeWindows.Count - 1; i >= 0; i--)
            {
                var window = _activeWindows[i];
                if (window.IsVisible)
                {
                    window.OnUpdate(dt);
                }
            }
        }

        #endregion

        #region 公开接口 

        /// <summary>
        /// 加载 UI 预制体 (可重写以支持非 XAsset 模式)
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        protected virtual async UniTask<GameObject> LoadUIPrefabAsync(string assetPath)
        {
#if UNITY_EDITOR
            // 增加容错：如果 Application.isPlaying 为 true 但 XAssetFrameWork 未初始化，也走 AssetDatabase
            // 或者如果在 Edit Mode 下运行测试
            bool useXAsset = Application.isPlaying && XAssetFrameWork.Instance.IsInitialized;
            
            if (!useXAsset)
            {
                // 注意：这里需要确保 assetPath 是完整的项目路径 (Assets/...)
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab != null)
                {
                    // 在编辑器非运行模式下不能使用 Instantiate(prefab, parent)，但在运行模式下可以
                    if (Application.isPlaying)
                    {
                        return GameObject.Instantiate(prefab, uiRoot);
                    }
                    else
                    {
                        // 非运行模式下的实例化 (例如编辑器工具中预览)
                        return (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, uiRoot);
                    }
                }
                else
                {
                    Debug.LogWarning($"[UIManager] AssetDatabase load failed: {assetPath}");
                }
            }
#endif
            // 默认使用 XAsset 加载
            return await XAssetFrameWork.Instance.InstantiateAsync(assetPath, uiRoot);
        }

        /// <summary>
        /// 异步打开窗口
        /// </summary>
        /// <typeparam name="T">窗口逻辑类类型</typeparam>
        /// <param name="uiName">UI 名称 (定义在 UIConfig 中)</param>
        /// <param name="args">传递给 OnOpen 的参数</param>
        public async UniTask<T> OpenWindowAsync<T>(params object[] args) where T : XUIBase, new()
        {
            string uiName =  typeof(T).Name;
            if (uiConfig == null)
            {
                Debug.LogError("[UIManager] UIConfig is not assigned!");
                return null;
            }

            // 获取 UI 配置信息
            UIConfig.UIInfo uiInfo = uiConfig.GetUIInfo(uiName);
            if (uiInfo == null || string.IsNullOrEmpty(uiInfo.assetPath))
            {
                return null;
            }
            
            string assetPath = uiInfo.assetPath;
            XUIBase window;

            // 1. 检查缓存 (使用 assetPath 作为 Key)
            if (!_loadedWindows.TryGetValue(assetPath, out window))
            {
                // 2. 加载资源
                GameObject uiGo = await LoadUIPrefabAsync(assetPath);
                if (uiGo == null)
                {
                    Debug.LogError($"[UIManager] Failed to load UI: {assetPath}");
                    return null;
                }

                // 3. 实例化逻辑类
                window = new T();
                window.Init(uiGo);
                
                _loadedWindows.Add(assetPath, window);
            }

            // 4. 显示窗口 
            await ShowWindowInternal(window, args);

            return window as T;
        }
        
        private async UniTask<XUIBase> OpenWindowAsync(XUIBase inputWindow, object[] args)
        {
            if (uiConfig == null)
            {
                Debug.LogError("[UIManager] UIConfig is not assigned!");
                return null;
            }

            // 获取 UI 配置信息
            UIConfig.UIInfo uiInfo = uiConfig.GetUIInfo(inputWindow.Name);
            if (uiInfo == null || string.IsNullOrEmpty(uiInfo.assetPath))
            {
                return null;
            }
            
            string assetPath = uiInfo.assetPath;
            XUIBase window;

            // 1. 检查缓存 (使用 assetPath 作为 Key)
            if (_loadedWindows.TryGetValue(assetPath, out window))
            {
                // 如果缓存中已有实例，复用它，并同步 isPopQueue 状态
                window.isPopQueue = inputWindow.isPopQueue;
            }
            else
            {
                // 如果缓存中没有，使用传入的新实例
                window = inputWindow;
                
                // 2. 加载资源
                GameObject uiGo = await LoadUIPrefabAsync(assetPath);
                if (uiGo == null)
                {
                    Debug.LogError($"[UIManager] Failed to load UI: {assetPath}");
                    return null;
                }

                // 3. 实例化逻辑类
                window.Init(uiGo);
                
                _loadedWindows.Add(assetPath, window);
            }

            // 4. 显示窗口 
            await ShowWindowInternal(window, args);

            return window;
        }

        /// <summary>
        /// 加入并且开始弹出UI
        /// </summary>
        /// <param name="setTop"></param>
        /// <typeparam name="T"></typeparam>
        public async UniTask<T> PushAndPopWindowAsync<T>(object[] args = null, bool setTop = false)  where T : XUIBase, new()
        {
            XUIBase window = new T();
            window.Name =  typeof(T).Name;
            window.isPopQueue = true;
            if (startPopWindow)
            {
                _messageQueue.Enqueue(new WindowQueueItem { Window = window, Args = args });
                return null;
            }
            else
            {
                startPopWindow = true;
                return await OpenWindowAsync(window, args) as T;
            }
        }

        private async UniTask PopNextMessageWindow()
        {
            if(_messageQueue.Count != 0)
            {
                var item = _messageQueue.Dequeue();
                await OpenWindowAsync(item.Window, item.Args);
            }
            else
            {
                startPopWindow = false;
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="uiName">UI 名称</param>
        public async UniTask CloseWindow<T>() where T : XUIBase, new()
        {
            if (uiConfig == null) return;
            
            string uiName =  typeof(T).Name;
            
            UIConfig.UIInfo uiInfo = uiConfig.GetUIInfo(uiName);
            if (uiInfo == null) return;
            string assetPath = uiInfo.assetPath;

            if (_loadedWindows.TryGetValue(assetPath, out XUIBase window))
            {
                bool isPopMessage = window.isPopQueue;
                await CloseWindow(window);
                if (isPopMessage)
                {
                   await PopNextMessageWindow();
                }
            }
        }

        /// <summary>
        /// 关闭指定窗口实例
        /// </summary>
        public async UniTask CloseWindow(XUIBase window)
        {
            if (window == null || !window.IsVisible) return;

            // 1. 调用关闭流程 (播放动画 -> SetActive(false))
            await window.OnClose();

            // 2. 从激活列表中移除
            if (_activeWindows.Contains(window))
            {
                _activeWindows.Remove(window);
            }
        }

        /// <summary>
        /// 关闭所有窗口
        /// </summary>
        public async UniTask CloseAll()
        {
            // 倒序关闭
            for (int i = _activeWindows.Count - 1; i >= 0; i--)
            {
                await CloseWindow(_activeWindows[i]);
            }
        }

        /// <summary>
        /// 销毁窗口 (释放资源)
        /// </summary>
        /// <param name="uiName">UI 名称</param>
        public void DestroyWindow<T>() where T : XUIBase, new()
        {
            string uiName =  typeof(T).Name;
            if (uiConfig == null) return;
            string assetPath = uiConfig.GetUIInfo(uiName).assetPath;
            if (string.IsNullOrEmpty(assetPath)) return;

            if (_loadedWindows.TryGetValue(assetPath, out XUIBase window))
            {
                // 如果正在显示，先关闭 (使用 Forget 因为 Destroy 通常不等待动画)
                if (window.IsVisible)
                {
                    CloseWindow(window).Forget();
                }

                // 释放 GameObject
                if (window.GameObject != null)
                {
                    if(XAssetFrameWork.Instance.IsInitialized)
                        XAssetFrameWork.Instance.ReleaseGameObject(window.GameObject);
                    else
                    {
                        GameObject.Destroy(window.GameObject);
                    }
                    
                    window.OnDestroy();
                }

                _loadedWindows.Remove(assetPath);
            }
            else
            {
                Debug.LogWarning($"{uiName} already destroyed!");
            }
        }

        /// <summary>
        /// 清空所有UI窗口
        /// </summary>
        public void DestroyAllWindows()
        {
            foreach (var window in _loadedWindows.Values)
            {
                if (window.GameObject != null)
                {
                    if(XAssetFrameWork.Instance.IsInitialized)
                        XAssetFrameWork.Instance.ReleaseGameObject(window.GameObject);
                    else
                    {
                        GameObject.Destroy(window.GameObject);
                    }
                    window.OnDestroy();
                }
            }
            Resources.UnloadUnusedAssets();
            _loadedWindows.Clear();
            _activeWindows.Clear();
            _messageQueue.Clear();
            startPopWindow = false;
        }

        /// <summary>
        /// 获取已加载的窗口实例
        /// </summary>
        public T GetWindow<T>() where T : XUIBase
        {
            string uiName =  typeof(T).Name;
            if (uiConfig == null) return null;
            string assetPath = uiConfig.GetUIInfo(uiName).assetPath;
            if (string.IsNullOrEmpty(assetPath)) return null;

            if (_loadedWindows.TryGetValue(assetPath, out XUIBase window))
            {
                return window as T;
            }
            return null;
        }

        #endregion

        #region 内部逻辑

        private async UniTask ShowWindowInternal(XUIBase window, object[] args)
        {
            // 如果已经在激活列表中，将其移至顶层
            if (_activeWindows.Contains(window))
            {
                _activeWindows.Remove(window);
                _activeWindows.Add(window);
            }
            else
            {
                _activeWindows.Add(window);
            }

            // 更新层级 (基于 Canvas SortingOrder 对 UIRoot 下的子节点进行同级排序)
            UpdateSortingOrders(); 

            // 调用打开流程 (SetActive(true) -> 播放动画)
            await window.OnOpen(args);
        }

        /// <summary>
        /// 刷新所有激活窗口的显示顺序 (SiblingIndex)
        /// 确保 Canvas SortingOrder 大的显示在上面
        /// 如果 Order 相同，则保持打开顺序
        /// </summary>
        private void UpdateSortingOrders()
        {
            // 使用 LINQ 的 OrderBy 进行稳定排序：
            // 1. 先按 Canvas.sortingOrder 升序
            // 2. Order 相同则保持原列表顺序 (因为原列表是按打开时间 Append 的)
            var sortedList = _activeWindows.OrderBy(w => w.Canvas != null ? w.Canvas.sortingOrder : 0).ToList();

            _activeWindows = sortedList;

            // 根据排序后的列表，重新设置 SiblingIndex，确保 Hierarchy 顺序与 Order 一致
            // 这样相同 Order 的 UI，后打开的会在下面 (显示在最上层)
            for (int i = 0; i < _activeWindows.Count; i++)
            {
                var window = _activeWindows[i];
                if (window.Transform != null)
                {
                    window.Transform.SetSiblingIndex(i);
                }
            }
        }

        #endregion
    }
}

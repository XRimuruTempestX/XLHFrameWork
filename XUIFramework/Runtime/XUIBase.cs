using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace XUIFramework
{
    /// <summary>
    /// UI 逻辑基类，不继承 MonoBehaviour
    /// 实现逻辑与渲染分离，负责 UI 的生命周期管理和基础信息记录
    /// </summary>
    public abstract class XUIBase
    {
        #region 基础属性

        /// <summary>
        /// UI 对应的 GameObject
        /// </summary>
        public GameObject GameObject { get; private set; }

        /// <summary>
        /// UI 对应的 Transform
        /// </summary>
        public Transform Transform { get; private set; }

        /// <summary>
        /// UI 对应的 RectTransform (UI 元素通常使用 RectTransform)
        /// </summary>
        public RectTransform RectTransform { get; private set; }

        /// <summary>
        /// UI面板 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 设置标志 是否是消息队列弹出
        /// </summary>
        public bool isPopQueue = false;

        #endregion

        #region 组件引用

        /// <summary>
        /// 面板上的 Canvas 组件，用于控制渲染层级
        /// </summary>
        public Canvas Canvas { get; private set; }

        /// <summary>
        /// 面板上的 GraphicRaycaster 组件，用于控制交互
        /// </summary>
        public GraphicRaycaster GraphicRaycaster { get; private set; }

        public CanvasScaler CanvasScaler { get; private set; }

        #endregion

        #region 状态管理

        /// <summary>
        /// 是否处于显示状态
        /// </summary>
        private bool IsActive { get; set; }

        /// <summary>
        /// 是否正在显示
        /// </summary>
        public bool IsVisible => IsActive;

        #endregion

        #region 绑定UI组件监听

        private List<Button> buttonList = new List<Button>();

        private List<Slider> sliderList = new List<Slider>();

        private List<Toggle> toggleList = new List<Toggle>();

        private List<InputField> inputFieldList = new List<InputField>();

        private List<Scrollbar> scrollbarList = new List<Scrollbar>();

        private List<Dropdown> dropdownList = new List<Dropdown>();

        private List<ScrollRect> scrollRectList = new List<ScrollRect>();
        
        private List<UIBehaviour> customUIList = new List<UIBehaviour>();

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化 UI，绑定 GameObject 和组件
        /// </summary>
        /// <param name="go">实例化的 GameObject</param>
        public virtual void Init(GameObject go)
        {
            if (go == null)
            {
                Debug.LogError($"[XUIBase] Init failed: GameObject is null!");
                return;
            }

            GameObject = go;
            Transform = go.transform;
            RectTransform = go.GetComponent<RectTransform>();
            Name = go.name;
            // 获取 Canvas 和 Raycaster，如果没有则尝试添加或报错，视需求而定
            // 这里假设每个 Panel 根节点都应该有 Canvas 以便独立管理层级
            Canvas = go.GetComponent<Canvas>();
            GraphicRaycaster = go.GetComponent<GraphicRaycaster>();
            CanvasScaler = go.GetComponent<CanvasScaler>();
            // 如果没有 Canvas，可能需要根据具体项目规范决定是否自动添加
            if (Canvas == null)
            {
                Debug.LogError($"[XUIBase] {Name} does not have a Canvas component on root.");
            }
            // 调试日志：检查 RenderMode 和 Root Canvas 状态
       //     Debug.LogError($"[XUIBase] {Name} Canvas.renderMode = {Canvas.renderMode}, isRootCanvas = {Canvas.isRootCanvas}");
            
            // 警告：如果是嵌套 Canvas，RenderMode 会继承自父 Canvas，此时自身的设置无效
            /*if (!Canvas.isRootCanvas)
            {
                Debug.LogWarning($"[XUIBase] Warning: {Name} is nested under parent Canvas '{Canvas.rootCanvas.name}' ({Canvas.rootCanvas.renderMode}). Prefab RenderMode settings are ignored.");
            }*/

            // 如果当前是 Camera 模式，或者预制体期望是 Camera 但因缺少 Camera 而变成了 Overlay (针对 Root Canvas)
            // 尝试赋值 Camera
            /*Debug.Log(Canvas.renderMode);
            if (Canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // Debug.LogError("UIManager.Instance.uiCamera = " + UIManager.Instance.uiCamera);
                Canvas.worldCamera = UIManager.Instance.uiCamera;
            }*/
            Canvas.worldCamera = UIManager.Instance.uiCamera;
            // 默认初始化时隐藏，避免加载后闪烁，等待 OnOpen 调用
            SetActive(false);

            // 调用子类初始化
            OnInit();
        }

        #endregion

        #region 生命周期

        //OnInit -> PlayOpenAnimation  -> OnOpen -> PlayCloseAnimation -> OnClose -> OnDestroy

        /// <summary>
        /// 初始化回调，仅执行一次
        /// 用于查找子节点组件、添加事件监听等
        /// </summary>
        protected virtual void OnInit()
        {
        }

        /// <summary>
        /// 播放打开动画
        /// </summary>
        /// <returns></returns>
        public virtual async UniTask PlayOpenAnimation()
        {
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// 播放关闭动画
        /// </summary>
        /// <returns></returns>
        public virtual async UniTask PlayCloseAnimation()
        {
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// 打开 UI 时调用
        /// </summary>
        /// <param name="args">可变参数，用于传递打开 UI 时的数据</param>
        public virtual async UniTask OnOpen(params object[] args)
        {
            SetActive(true);
            await PlayOpenAnimation();
        }

        /// <summary>
        /// 关闭 UI 时调用
        /// </summary>
        public virtual async UniTask OnClose()
        {
            await PlayCloseAnimation();
            SetActive(false);
            isPopQueue = false;
        }

        /// <summary>
        /// 轮询更新，由 UI 管理器统一驱动
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public virtual void OnUpdate(float deltaTime)
        {
        }

        /// <summary>
        /// 销毁时调用，用于释放资源、移除事件
        /// </summary>
        public virtual void OnDestroy()
        {
            GameObject = null;
            Transform = null;
            RectTransform = null;
            Canvas = null;
            GraphicRaycaster = null;
            CanvasScaler = null;
            RemoveAllUIListeners();
        }

        #endregion

        #region 操作方法

        /// <summary>
        /// 设置 UI 激活/失活
        /// </summary>
        /// <param name="active">是否激活</param>
        public virtual void SetActive(bool active)
        {
            if (GameObject != null && IsActive != active)
            {
                Canvas.enabled = active;
                IsActive = active;
            }
        }

        #endregion

        #region UI事件监听

        public void AddButtonListener(Button button, UnityAction<Button> callback)
        {
            if (button != null && !buttonList.Contains(button))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => { callback?.Invoke(button); });
                buttonList.Add(button);
            }
        }

        public void AddSliderListener(Slider slider, UnityAction<Slider, float> callback)
        {
            if (slider != null && !sliderList.Contains(slider))
            {
                slider.onValueChanged.RemoveAllListeners();
                slider.onValueChanged.AddListener((value) => { callback?.Invoke(slider, value); });
                sliderList.Add(slider);
            }
        }

        public void AddToggleListener(Toggle toggle, UnityAction<Toggle, bool> callback)
        {
            if (toggle != null && !toggleList.Contains(toggle))
            {
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((isOn) => { callback?.Invoke(toggle, isOn); });
                toggleList.Add(toggle);
            }
        }

        public void AddInputFieldListener(InputField inputField, UnityAction<InputField, string> callback)
        {
            if (inputField != null && !inputFieldList.Contains(inputField))
            {
                inputField.onValueChanged.RemoveAllListeners();
                inputField.onValueChanged.AddListener((text) => { callback?.Invoke(inputField, text); });
                inputFieldList.Add(inputField);
            }
        }

        public void AddInputFieldEndEditListener(InputField inputField, UnityAction<InputField, string> callback)
        {
            if (inputField != null && !inputFieldList.Contains(inputField))
            {
                inputField.onEndEdit.RemoveAllListeners();
                inputField.onEndEdit.AddListener((text) => { callback?.Invoke(inputField, text); });
                // 注意：如果已经添加过 onValueChanged，这里不需要重复 Add 到 List，但为了保险起见还是判断一下
                if (!inputFieldList.Contains(inputField))
                    inputFieldList.Add(inputField);
            }
        }

        public void AddScrollbarListener(Scrollbar scrollbar, UnityAction<Scrollbar, float> callback)
        {
            if (scrollbar != null && !scrollbarList.Contains(scrollbar))
            {
                scrollbar.onValueChanged.RemoveAllListeners();
                scrollbar.onValueChanged.AddListener((value) => { callback?.Invoke(scrollbar, value); });
                scrollbarList.Add(scrollbar);
            }
        }

        public void AddDropdownListener(Dropdown dropdown, UnityAction<Dropdown, int> callback)
        {
            if (dropdown != null && !dropdownList.Contains(dropdown))
            {
                dropdown.onValueChanged.RemoveAllListeners();
                dropdown.onValueChanged.AddListener((value) => { callback?.Invoke(dropdown, value); });
                dropdownList.Add(dropdown);
            }
        }

        public void AddScrollRectListener(ScrollRect scrollRect, UnityAction<ScrollRect, Vector2> callback)
        {
            if (scrollRect != null && !scrollRectList.Contains(scrollRect))
            {
                scrollRect.onValueChanged.RemoveAllListeners();
                scrollRect.onValueChanged.AddListener((value) =>
                {
                    callback?.Invoke(scrollRect, value);
                });
                scrollRectList.Add(scrollRect);
            }
        }
        
        /// <summary>
        /// 为任意 UI 控件添加 EventTrigger 监听 (如 Click, Down, Up, Enter, Exit)
        /// </summary>
        public void AddCustomEventListener(UIBehaviour ui, EventTriggerType eventType, UnityAction<BaseEventData> callback)
        {
            if (ui == null) return;
            
            // 获取或添加 EventTrigger 组件
            EventTrigger trigger = ui.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = ui.gameObject.AddComponent<EventTrigger>();
            }

            // 创建 Entry
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = eventType;
            entry.callback.AddListener(callback);

            trigger.triggers.Add(entry);

            if (!customUIList.Contains(ui))
            {
                customUIList.Add(ui);
            }
        }

        public void RemoveAllButtonListener()
        {
            foreach (var btn in buttonList)
            {
                if (btn != null) btn.onClick.RemoveAllListeners();
            }

            buttonList.Clear();
        }

        public void RemoveAllSliderListener()
        {
            foreach (var slider in sliderList)
            {
                if (slider != null) slider.onValueChanged.RemoveAllListeners();
            }

            sliderList.Clear();
        }

        public void RemoveAllToggleListener()
        {
            foreach (var toggle in toggleList)
            {
                if (toggle != null) toggle.onValueChanged.RemoveAllListeners();
            }

            toggleList.Clear();
        }

        public void RemoveAllInputFieldListener()
        {
            foreach (var input in inputFieldList)
            {
                if (input != null)
                {
                    input.onValueChanged.RemoveAllListeners();
                    input.onEndEdit.RemoveAllListeners();
                }
            }

            inputFieldList.Clear();
        }

        public void RemoveAllScrollbarListener()
        {
            foreach (var scrollbar in scrollbarList)
            {
                if (scrollbar != null) scrollbar.onValueChanged.RemoveAllListeners();
            }

            scrollbarList.Clear();
        }

        public void RemoveAllDropdownListener()
        {
            foreach (var dropdown in dropdownList)
            {
                if (dropdown != null) dropdown.onValueChanged.RemoveAllListeners();
            }

            dropdownList.Clear();
        }

        public void RemoveAllScrollRectListener()
        {
            foreach (var scrollRect in scrollRectList)
            {
                if(scrollRect != null) scrollRect.onValueChanged.RemoveAllListeners();
            }
            scrollRectList.Clear();
        }
        
        public void RemoveAllCustomListener()
        {
            foreach (var ui in customUIList)
            {
                if (ui != null)
                {
                    var trigger = ui.GetComponent<EventTrigger>();
                    if (trigger != null)
                    {
                        trigger.triggers.Clear();
                    }
                }
            }
            customUIList.Clear();
        }
        
        /// <summary>
        /// 移除所有注册的 UI 事件监听
        /// </summary>
        private void RemoveAllUIListeners()
        {
            RemoveAllButtonListener();
            RemoveAllSliderListener();
            RemoveAllToggleListener();
            RemoveAllInputFieldListener();
            RemoveAllScrollbarListener();
            RemoveAllDropdownListener();
            RemoveAllScrollRectListener();
            RemoveAllCustomListener();
        }

        #endregion
    }
}
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace XUIFramework.Tests
{
    public class TestWindow : XUIBase
    {
        private TestWindowComponent _view;

        #region 生命周期

        protected override void OnInit()
        {
            base.OnInit();
            
            // 1. 获取 View 组件引用
            _view = GameObject.GetComponent<TestWindowComponent>();
            if (_view == null)
            {
                Debug.LogError($"[TestWindow] Missing TestWindowComponent on {Name}");
                return;
            }

            // 2. 绑定事件
            AddButtonListener(_view.clickButton, OnClickButton);
            AddSliderListener(_view.musicSlider, OnMusicSliderChanged);
        }

        public override async UniTask PlayOpenAnimation()
        {
            await base.PlayOpenAnimation();
        }

        public override async UniTask PlayCloseAnimation()
        {
            await base.PlayCloseAnimation();
        }

        public override async UniTask OnOpen(params object[] args)
        {
            await base.OnOpen(args);
            Debug.Log($"[TestWindow] OnOpen Called. Args: {(args != null && args.Length > 0 ? args[0] : "null")}");
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion

        #region 事件回调

        private void OnClickButton(Button button) 
        {
            Debug.Log($"[TestWindow] Button Clicked: {button.name}");
        }

        private void OnMusicSliderChanged(Slider slider, float value)
        {
            Debug.Log($"[TestWindow] Slider Changed: {value}");
        }

        #endregion
    }
}

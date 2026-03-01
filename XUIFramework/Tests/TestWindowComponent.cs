using UnityEngine;
using UnityEngine.UI;

namespace XUIFramework.Tests
{
    // View 层：只负责持有 UI 组件引用，不包含任何业务逻辑
    public class TestWindowComponent : MonoBehaviour
    {
        public Button clickButton;
        public Slider musicSlider;
    }
}

using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace XUIFramework.Editor
{
    public class UICodeGenerator : EditorWindow
    {
        private const string SAVE_PATH_KEY = "XUIFramework_CodeGen_SavePath";
        private const string PREFAB_PATH_KEY = "XUIFramework_CodeGen_PrefabPath";
        private const string COMPONENT_NAME_KEY = "XUIFramework_CodeGen_ComponentName";

        private string _savePath = "Assets/Scripts/UI/Generated"; // 默认保存路径
        
        [MenuItem("GameObject/XUIFramework/Generate UI Code", false, 0)]
        public static void GenerateUICode()
        {
            GameObject selectedObj = Selection.activeGameObject;
            if (selectedObj == null)
            {
                Debug.LogError("Please select a UI GameObject in Hierarchy first!");
                return;
            }
            
            // 获取上次保存路径，如果为空则使用默认路径
            string lastSavePath = EditorPrefs.GetString(SAVE_PATH_KEY, "Assets");
            if (!Directory.Exists(lastSavePath))
            {
                lastSavePath = Application.dataPath;
            }

            // 每次都弹出选择文件夹对话框
            string path = EditorUtility.OpenFolderPanel("Select Save Folder", lastSavePath, "");
            
            // 如果用户取消了选择，直接返回
            if (string.IsNullOrEmpty(path)) return;

            // 转换为相对路径
            string savePath;
            if (path.StartsWith(Application.dataPath))
            {
                savePath = "Assets" + path.Substring(Application.dataPath.Length);
                // 保存本次选择的路径，供下次使用
                EditorPrefs.SetString(SAVE_PATH_KEY, savePath);
            }
            else
            {
                Debug.LogError("Please select a folder inside Assets.");
                return;
            }

            // 记录当前操作的 Prefab 信息，用于编译后回调
            // 注意：如果是场景中的实例，我们需要找到它对应的 Prefab Asset 路径，或者记录 InstanceID
            // 为了稳健，我们建议用户在 Prefab Mode 下操作，或者直接记录 InstanceID
            // 如果是 Scene 中的物体，我们需要确保它是 Prefab 实例
            
            string prefabAssetPath = null;
            if (PrefabUtility.IsPartOfAnyPrefab(selectedObj))
            {
                 // 获取 Prefab Asset 路径
                 prefabAssetPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(selectedObj));
            }
            else
            {
                // 如果不是 Prefab，可能只是场景里的临时物体，也可以生成，但无法自动保存回 Prefab
                Debug.LogWarning("Selected object is not a Prefab instance. Changes will only apply to the scene object.");
            }

            // 记录 InstanceID 以便编译后找回对象
            EditorPrefs.SetInt(PREFAB_PATH_KEY, selectedObj.GetInstanceID());
            EditorPrefs.SetString(COMPONENT_NAME_KEY, selectedObj.name + "Component");

            Generate(selectedObj, savePath);
        }

        [MenuItem("XUIFramework/Setting/Set UI Code Gen Path")]
        public static void SetSavePath()
        {
             string path = EditorUtility.OpenFolderPanel("Select Save Folder", Application.dataPath, "");
             if (string.IsNullOrEmpty(path)) return;
             
             if (path.StartsWith(Application.dataPath))
             {
                 string savePath = "Assets" + path.Substring(Application.dataPath.Length);
                 EditorPrefs.SetString(SAVE_PATH_KEY, savePath);
                 Debug.Log($"UI Code Save Path Set to: {savePath}");
             }
             else
             {
                 Debug.LogError("Please select a folder inside Assets.");
             }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            // 检查是否有待处理的自动挂载任务
            int instanceID = EditorPrefs.GetInt(PREFAB_PATH_KEY, 0);
            if (instanceID == 0) return;

            // 清除标记，防止重复执行 (立即清除)
            EditorPrefs.DeleteKey(PREFAB_PATH_KEY);
            
            string componentName = EditorPrefs.GetString(COMPONENT_NAME_KEY, "");
            EditorPrefs.DeleteKey(COMPONENT_NAME_KEY);
            
            if (string.IsNullOrEmpty(componentName)) return;

            // 尝试找回 GameObject
            GameObject targetObj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (targetObj == null)
            {
                // 可能是场景切换了，或者对象被销毁了
                return;
            }

            // 获取组件类型
            System.Type componentType = null;
            // 遍历所有程序集查找类型
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                componentType = assembly.GetType(componentName);
                if (componentType != null) break;
            }

            if (componentType == null)
            {
                Debug.LogError($"[UICodeGenerator] Could not find type: {componentName}. Maybe compilation failed?");
                return;
            }

            // 挂载组件
            Component component = targetObj.GetComponent(componentType);
            if (component == null)
            {
                component = targetObj.AddComponent(componentType);
            }

            // 自动赋值
            SerializedObject serializedObj = new SerializedObject(component);
            serializedObj.Update();

            // 扫描所有字段
            var fields = componentType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                // 根据字段名查找子节点
                // 约定：字段名 == 节点名
                Transform child = FindChildRecursive(targetObj.transform, field.Name);
                if (child != null)
                {
                    // 数组类型处理
                    if (field.FieldType.IsArray)
                    {
                        // 获取数组元素的类型 (e.g., Button[] -> Button)
                        System.Type elementType = field.FieldType.GetElementType();
                        
                        // 获取所有符合条件的子节点组件
                        // 策略：获取 child 节点下的所有直接子节点，并获取它们身上的 elementType 组件
                        List<Object> components = new List<Object>();
                        foreach (Transform subChild in child)
                        {
                            if (elementType == typeof(GameObject))
                            {
                                components.Add(subChild.gameObject);
                            }
                            else
                            {
                                Component subComp = subChild.GetComponent(elementType);
                                if (subComp != null)
                                {
                                    components.Add(subComp);
                                }
                            }
                        }

                        // 创建数组并赋值给 SerializedProperty
                        SerializedProperty prop = serializedObj.FindProperty(field.Name);
                        if (prop != null)
                        {
                            prop.ClearArray();
                            prop.arraySize = components.Count;
                            for (int i = 0; i < components.Count; i++)
                            {
                                SerializedProperty elementProp = prop.GetArrayElementAtIndex(i);
                                elementProp.objectReferenceValue = components[i];
                            }
                        }
                    }
                    // 单个元素处理
                    else
                    {
                         // 特殊处理 GameObject 类型
                        if (field.FieldType == typeof(GameObject))
                        {
                            SerializedProperty prop = serializedObj.FindProperty(field.Name);
                            if (prop != null)
                            {
                                prop.objectReferenceValue = child.gameObject;
                            }
                        }
                        else
                        {
                            // 尝试获取对应类型的组件
                            Component childComp = child.GetComponent(field.FieldType);
                            if (childComp != null)
                            {
                                SerializedProperty prop = serializedObj.FindProperty(field.Name);
                                if (prop != null)
                                {
                                    prop.objectReferenceValue = childComp;
                                }
                            }
                        }
                    }
                }
            }

            serializedObj.ApplyModifiedProperties();
            
            // 标记脏数据
            EditorUtility.SetDirty(targetObj);
            
            Debug.Log($"[UICodeGenerator] Successfully attached and bound {componentName} to {targetObj.name}");
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            // 广度优先搜索
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
            }
            
            foreach (Transform child in parent)
            {
                Transform result = FindChildRecursive(child, name);
                if (result != null) return result;
            }
            
            return null;
        }

        private static void Generate(GameObject prefab, string savePath)
        {
            string uiName = prefab.name;
            string componentClassName = uiName + "Component";
            string windowClassName = uiName;

            // 创建子文件夹
            string componentFolder = Path.Combine(savePath, "Component");
            string designerFolder = Path.Combine(savePath, "Designer");
            string logicFolder = Path.Combine(savePath, "Window");

            if (!Directory.Exists(componentFolder)) Directory.CreateDirectory(componentFolder);
            if (!Directory.Exists(designerFolder)) Directory.CreateDirectory(designerFolder);
            if (!Directory.Exists(logicFolder)) Directory.CreateDirectory(logicFolder);

            // 1. 扫描组件
            List<ComponentInfo> componentList = ScanComponents(prefab);

            // 2. 生成 Component 代码 (View)
            string componentCode = GenerateComponentCode(uiName, componentClassName, componentList);
            string componentPath = Path.Combine(componentFolder, componentClassName + ".cs");
            File.WriteAllText(componentPath, componentCode);
            Debug.Log($"Generated: {componentPath}");

            // 3. 生成 Window 代码 (Controller)
            string windowPath = Path.Combine(logicFolder, windowClassName + ".cs");
            string designerPath = Path.Combine(designerFolder, windowClassName + ".Designer.cs");

            // 3.1 生成 Designer 代码 (每次覆盖)
            string designerCode = GenerateWindowDesignerCode(uiName, windowClassName, componentClassName, componentList);
            File.WriteAllText(designerPath, designerCode);
            Debug.Log($"Generated: {designerPath}");

            // 3.2 生成或更新 Logic 代码 (增量更新)
            if (!File.Exists(windowPath))
            {
                // 如果文件不存在，生成完整的类结构
                string windowCode = GenerateWindowLogicCode(uiName, windowClassName, componentList);
                File.WriteAllText(windowPath, windowCode);
                Debug.Log($"Generated: {windowPath}");
            }
            else
            {
                // 如果文件已存在，尝试追加缺失的方法
                string existingCode = File.ReadAllText(windowPath);
                string updatedCode = UpdateWindowLogicCode(existingCode, componentList);
                if (updatedCode != existingCode)
                {
                    File.WriteAllText(windowPath, updatedCode);
                    Debug.Log($"Updated: {windowPath}");
                }
                else
                {
                    Debug.Log($"No changes needed for: {windowPath}");
                }
            }

            // 4. 刷新 AssetDatabase 并自动挂载组件 (延迟一帧等待编译完成比较复杂，这里先做生成)
            AssetDatabase.Refresh();
            
            // 提示用户手动挂载或后续实现自动挂载
            Debug.Log("UI Code Generation Finished. Please wait for compilation and then attach the Component script to your Prefab.");
        }

        private struct ComponentInfo
        {
            public string Name;
            public string Type;
            public string Path; // 相对于根节点的路径
            public bool IsArray;
        }

        private static List<ComponentInfo> ScanComponents(GameObject root)
        {
            List<ComponentInfo> list = new List<ComponentInfo>();
            
            // 递归遍历所有子节点
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            
            // 使用 HashSet 防止数组元素被重复添加为单元素
            // 实际上，如果父节点被标记为数组容器，其子节点不应该再被单独识别（除非它们也有特殊命名）
            // 这里我们采用简单策略：先扫描数组容器，再扫描普通节点
            
            foreach (var trans in transforms)
            {
                if (trans == root.transform) continue;

                string name = trans.name;
                string type = "";
                bool isArray = false;

                // 数组容器识别 (复数前缀)
                // Btns_ -> Button[]
                // Txts_ -> Text[]
                // Tmps_ -> TextMeshProUGUI[]
                // Imgs_ -> Image[]
                // Inputs_ -> InputField[]
                // TmpInputs_ -> TMP_InputField[]
                // Sliders_ -> Slider[]
                // Togs_ -> Toggle[]
                // Gos_ -> GameObject[]

                if (name.StartsWith("Btns_")) { type = "Button"; isArray = true; }
                else if (name.StartsWith("Txts_")) { type = "Text"; isArray = true; }
                else if (name.StartsWith("Tmps_")) { type = "TextMeshProUGUI"; isArray = true; }
                else if (name.StartsWith("Imgs_")) { type = "Image"; isArray = true; }
                else if (name.StartsWith("Inputs_")) { type = "InputField"; isArray = true; }
                else if (name.StartsWith("TmpInputs_")) { type = "TMP_InputField"; isArray = true; }
                else if (name.StartsWith("Sliders_")) { type = "Slider"; isArray = true; }
                else if (name.StartsWith("Togs_")) { type = "Toggle"; isArray = true; }
                else if (name.StartsWith("Gos_")) { type = "GameObject"; isArray = true; }
                
                // 单个元素识别 (单数前缀)
                else if (name.StartsWith("Btn_")) type = "Button";
                else if (name.StartsWith("Txt_")) type = "Text";
                else if (name.StartsWith("Tmp_")) type = "TextMeshProUGUI";
                else if (name.StartsWith("Img_")) type = "Image";
                else if (name.StartsWith("Input_")) type = "InputField";
                else if (name.StartsWith("TmpInput_")) type = "TMP_InputField";
                else if (name.StartsWith("Slider_")) type = "Slider";
                else if (name.StartsWith("Tog_")) type = "Toggle";
                else if (name.StartsWith("Scroll_")) type = "ScrollRect";
                else if (name.StartsWith("Go_")) type = "GameObject";
                
                if (!string.IsNullOrEmpty(type))
                {
                     list.Add(new ComponentInfo
                     {
                         Name = name,
                         Type = type,
                         Path = GetRelativePath(root.transform, trans),
                         IsArray = isArray
                     });
                }
            }

            return list;
        }

        private static string GetRelativePath(Transform root, Transform child)
        {
            StringBuilder path = new StringBuilder(child.name);
            while (child.parent != root && child.parent != null)
            {
                child = child.parent;
                path.Insert(0, child.name + "/");
            }
            return path.ToString();
        }

        private static string GenerateComponentCode(string uiName, string className, List<ComponentInfo> list)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine("using TMPro;");
            sb.AppendLine("using XUIFramework;");
            sb.AppendLine("");
            sb.AppendLine($"// Auto Generated Code for {uiName}");
            sb.AppendLine($"public class {className} : MonoBehaviour");
            sb.AppendLine("{");

            foreach (var info in list)
            {
                if (info.IsArray)
                {
                    sb.AppendLine($"    public {info.Type}[] {info.Name};");
                }
                else
                {
                    sb.AppendLine($"    public {info.Type} {info.Name};");
                }
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string GenerateWindowDesignerCode(string uiName, string className, string componentClassName, List<ComponentInfo> list)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine("using TMPro;");
            sb.AppendLine("using XUIFramework;");
            sb.AppendLine("using Cysharp.Threading.Tasks;");
            sb.AppendLine("");
            sb.AppendLine($"public partial class {className} : XUIBase");
            sb.AppendLine("{");
            sb.AppendLine($"    private {componentClassName} _view;");
            sb.AppendLine("");
            sb.AppendLine("    protected override void OnInit()");
            sb.AppendLine("    {");
            sb.AppendLine("        base.OnInit();");
            sb.AppendLine($"        _view = GameObject.GetComponent<{componentClassName}>();");
            sb.AppendLine("        if (_view == null)");
            sb.AppendLine("        {");
            sb.AppendLine($"            Debug.LogError($\"[{className}] Missing {componentClassName} on {{Name}}\");");
            sb.AppendLine("            return;");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("        // Auto Bind Listeners");

            foreach (var info in list)
            {
                if (info.IsArray)
                {
                    // 数组类型的绑定
                    if (info.Type == "Button")
                    {
                        sb.AppendLine($"        if (_view.{info.Name} != null)");
                        sb.AppendLine("        {");
                        sb.AppendLine($"            foreach (var btn in _view.{info.Name})");
                        sb.AppendLine("            {");
                        sb.AppendLine($"                AddButtonListener(btn, On{info.Name}Click);");
                        sb.AppendLine("            }");
                        sb.AppendLine("        }");
                    }
                    else if (info.Type == "Slider")
                    {
                         sb.AppendLine($"        if (_view.{info.Name} != null)");
                         sb.AppendLine("        {");
                         sb.AppendLine($"            foreach (var slider in _view.{info.Name})");
                         sb.AppendLine("            {");
                         sb.AppendLine($"                AddSliderListener(slider, On{info.Name}Changed);");
                         sb.AppendLine("            }");
                         sb.AppendLine("        }");
                    }
                    // ... 其他数组类型的绑定逻辑类似，按需添加 ...
                    else if (info.Type == "Toggle")
                    {
                         sb.AppendLine($"        if (_view.{info.Name} != null)");
                         sb.AppendLine("        {");
                         sb.AppendLine($"            foreach (var tog in _view.{info.Name})");
                         sb.AppendLine("            {");
                         sb.AppendLine($"                AddToggleListener(tog, On{info.Name}Changed);");
                         sb.AppendLine("            }");
                         sb.AppendLine("        }");
                    }
                }
                else
                {
                    // 单个类型的绑定
                    if (info.Type == "Button")
                    {
                        sb.AppendLine($"        AddButtonListener(_view.{info.Name}, On{info.Name}Click);");
                    }
                    else if (info.Type == "Slider")
                    {
                        sb.AppendLine($"        AddSliderListener(_view.{info.Name}, On{info.Name}Changed);");
                    }
                    else if (info.Type == "Toggle")
                    {
                         sb.AppendLine($"        AddToggleListener(_view.{info.Name}, On{info.Name}Changed);");
                    }
                    else if (info.Type == "InputField")
                    {
                         sb.AppendLine($"        AddInputFieldListener(_view.{info.Name}, On{info.Name}Changed);");
                    }
                    else if (info.Type == "TMP_InputField")
                    {
                         sb.AppendLine($"        AddTMPInputFieldListener(_view.{info.Name}, On{info.Name}Changed);");
                    }
                }
            }
            
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string GenerateWindowLogicCode(string uiName, string className, List<ComponentInfo> list)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine("using TMPro;");
            sb.AppendLine("using XUIFramework;");
            sb.AppendLine("using Cysharp.Threading.Tasks;");
            sb.AppendLine("");
            sb.AppendLine($"public partial class {className}");
            sb.AppendLine("{");
            sb.AppendLine("    #region 生命周期");
            sb.AppendLine("");
            sb.AppendLine("    public override async UniTask PlayOpenAnimation()");
            sb.AppendLine("    {");
            sb.AppendLine("        await base.PlayOpenAnimation();");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine("    public override async UniTask PlayCloseAnimation()");
            sb.AppendLine("    {");
            sb.AppendLine("        await base.PlayCloseAnimation();");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine("    public override async UniTask OnOpen(params object[] args)");
            sb.AppendLine("    {");
            sb.AppendLine("        await base.OnOpen(args);");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine("    public override async UniTask OnClose()");
            sb.AppendLine("    {");
            sb.AppendLine("        await base.OnClose();");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine("    public override void OnUpdate(float deltaTime)");
            sb.AppendLine("    {");
            sb.AppendLine("        base.OnUpdate(deltaTime);");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine("    public override void OnDestroy()");
            sb.AppendLine("    {");
            sb.AppendLine("        base.OnDestroy();");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine("    #endregion");
            sb.AppendLine("");
            sb.AppendLine("    // Add your logic here");
            sb.AppendLine("");
            
            foreach (var info in list)
            {
                sb.Append(GenerateMethodCode(info));
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string UpdateWindowLogicCode(string existingCode, List<ComponentInfo> list)
        {
            // 找到最后一个 '}' 的位置，准备插入
            int lastBraceIndex = existingCode.LastIndexOf('}');
            if (lastBraceIndex == -1) return existingCode; // 格式错误

            StringBuilder newMethods = new StringBuilder();
            bool hasNewMethods = false;

            foreach (var info in list)
            {
                string methodName = "";
                if (info.Type == "Button") methodName = $"On{info.Name}Click";
                else if (info.Type == "Slider" || info.Type == "Toggle" || info.Type == "InputField" || info.Type == "TMP_InputField") methodName = $"On{info.Name}Changed";

                if (!string.IsNullOrEmpty(methodName) && !existingCode.Contains(methodName))
                {
                    newMethods.AppendLine();
                    newMethods.Append("    " + GenerateMethodCode(info).Replace("\n", "\n    ")); // 增加缩进
                    hasNewMethods = true;
                }
            }

            if (hasNewMethods)
            {
                return existingCode.Insert(lastBraceIndex, newMethods.ToString());
            }

            return existingCode;
        }

        private static string GenerateMethodCode(ComponentInfo info)
        {
            StringBuilder sb = new StringBuilder();
            if (info.Type == "Button")
            {
                sb.AppendLine($"    private void On{info.Name}Click(Button btn)");
                sb.AppendLine("    {");
                sb.AppendLine($"        Debug.Log(\"{info.Name} Clicked\");");
                sb.AppendLine("    }");
                sb.AppendLine("");
            }
            else if (info.Type == "Slider")
            {
                sb.AppendLine($"    private void On{info.Name}Changed(Slider slider, float value)");
                sb.AppendLine("    {");
                sb.AppendLine("    }");
                sb.AppendLine("");
            }
            else if (info.Type == "Toggle")
            {
                sb.AppendLine($"    private void On{info.Name}Changed(Toggle toggle, bool isOn)");
                sb.AppendLine("    {");
                sb.AppendLine("    }");
                sb.AppendLine("");
            }
            else if (info.Type == "InputField")
            {
                sb.AppendLine($"    private void On{info.Name}Changed(InputField input, string text)");
                sb.AppendLine("    {");
                sb.AppendLine("    }");
                sb.AppendLine("");
            }
            else if (info.Type == "TMP_InputField")
            {
                sb.AppendLine($"    private void On{info.Name}Changed(TMP_InputField input, string text)");
                sb.AppendLine("    {");
                sb.AppendLine("    }");
                sb.AppendLine("");
            }
            return sb.ToString();
        }
    }
}

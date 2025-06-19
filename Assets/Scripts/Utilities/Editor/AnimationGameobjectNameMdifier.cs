using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

/// <summary>
/// 动画引用名称修改器
/// 用于修改动画剪辑中的指定引用名称，将引用路径中的名字从A改为B
/// </summary>
public class AnimationGameobjectNameMdifier : EditorWindow
{
    private GameObject targetGameObject;
    private string oldObjectName = "";
    private string newObjectName = "";
    private bool includeChildren = true;
    private bool showPreview = false;
    private List<AnimationClip> foundClips = new List<AnimationClip>();
    private List<string> previewResults = new List<string>();
    private Vector2 scrollPosition;
    
    [MenuItem("工具/动画引用名称修改器", false, 100)]
    public static void ShowWindow()
    {
        GetWindow<AnimationGameobjectNameMdifier>("动画引用名称修改器");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("动画引用名称修改器", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("此工具用于修改动画剪辑中的指定引用名称。\n" +
                               "使用方法：\n" +
                               "1. 选择包含动画的目标物体\n" +
                               "2. 输入要替换的引用名称（从A改为B）\n" +
                               "3. 点击预览查看将要修改的动画引用\n" +
                               "4. 确认无误后点击执行修改", MessageType.Info);
        
        GUILayout.Space(10);
        
        // 目标物体选择
        EditorGUILayout.LabelField("目标设置", EditorStyles.boldLabel);
        targetGameObject = (GameObject)EditorGUILayout.ObjectField("目标物体", targetGameObject, typeof(GameObject), true);
        
        GUILayout.Space(5);
        
        // 名称输入
        EditorGUILayout.LabelField("引用名称修改", EditorStyles.boldLabel);
        oldObjectName = EditorGUILayout.TextField("原引用名称 (A)", oldObjectName);
        newObjectName = EditorGUILayout.TextField("新引用名称 (B)", newObjectName);
        
        EditorGUILayout.HelpBox("将动画剪辑中所有包含'原引用名称'的路径替换为'新引用名称'。\n" +
                               "例如：将路径 'Parent/OldName/Child' 中的 'OldName' 替换为 'NewName'。", MessageType.None);
        
        GUILayout.Space(5);
        
        // 选项
        EditorGUILayout.LabelField("搜索选项", EditorStyles.boldLabel);
        includeChildren = EditorGUILayout.Toggle("搜索子物体动画", includeChildren);
        
        GUILayout.Space(10);
        
        // 按钮区域
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = IsInputValid();
        if (GUILayout.Button("预览修改", GUILayout.Height(30)))
        {
            PreviewChanges();
        }
        
        GUI.enabled = IsInputValid();
        if (GUILayout.Button("执行修改", GUILayout.Height(30)))
        {
            string message = foundClips.Count > 0 ? 
                $"即将修改 {foundClips.Count} 个动画剪辑中的引用。\n" :
                "未找到包含指定引用名称的动画剪辑。\n";
            
            if (foundClips.Count > 0 && EditorUtility.DisplayDialog("确认修改", 
                message +
                $"将动画引用中的 '{oldObjectName}' 替换为 '{newObjectName}'。\n\n" +
                "此操作不可撤销，是否继续？", "确认", "取消"))
            {
                ExecuteRename();
            }
            else if (foundClips.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "未找到包含指定引用名称的动画剪辑。", "确定");
            }
        }
        
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // 预览结果显示
        if (showPreview && previewResults.Count > 0)
        {
            EditorGUILayout.LabelField("预览结果", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            foreach (string result in previewResults)
            {
                EditorGUILayout.LabelField(result, EditorStyles.wordWrappedLabel);
            }
            
            EditorGUILayout.EndScrollView();
        }
    }
    
    /// <summary>
    /// 检查输入是否有效
    /// </summary>
    private bool IsInputValid()
    {
        return targetGameObject != null && 
               !string.IsNullOrEmpty(oldObjectName) && 
               !string.IsNullOrEmpty(newObjectName) &&
               oldObjectName != newObjectName;
    }
    
    /// <summary>
    /// 预览将要进行的修改
    /// </summary>
    private void PreviewChanges()
    {
        foundClips.Clear();
        previewResults.Clear();
        
        // 查找所有相关的动画剪辑
        FindAnimationClips();
        
        if (foundClips.Count == 0)
        {
            previewResults.Add("未找到包含指定引用名称的动画剪辑。");
            previewResults.Add($"搜索的引用名称: '{oldObjectName}'");
            showPreview = true;
            return;
        }
        
        // 分析每个动画剪辑
        foreach (AnimationClip clip in foundClips)
        {
            AnalyzeAnimationClip(clip);
        }
        
        showPreview = true;
        
        Debug.Log($"预览完成：找到 {foundClips.Count} 个需要修复的动画剪辑。");
    }
    
    /// <summary>
    /// 查找所有相关的动画剪辑
    /// </summary>
    private void FindAnimationClips()
    {
        // 从目标物体的Animator组件查找
        Animator animator = targetGameObject.GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
            if (controller != null)
            {
                foreach (var clip in controller.animationClips)
                {
                    if (ClipContainsObjectName(clip, oldObjectName))
                    {
                        foundClips.Add(clip);
                    }
                }
            }
        }
        
        // 从Animation组件查找
        Animation animation = targetGameObject.GetComponent<Animation>();
        if (animation != null)
        {
            foreach (AnimationState state in animation)
            {
                if (ClipContainsObjectName(state.clip, oldObjectName))
                {
                    foundClips.Add(state.clip);
                }
            }
        }
        
        // 如果包含子物体，递归查找
        if (includeChildren)
        {
            FindAnimationClipsInChildren(targetGameObject.transform);
        }
    }
    
    /// <summary>
    /// 在子物体中查找动画剪辑
    /// </summary>
    private void FindAnimationClipsInChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // 检查Animator
            Animator animator = child.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
                if (controller != null)
                {
                    foreach (var clip in controller.animationClips)
                    {
                        if (ClipContainsObjectName(clip, oldObjectName) && !foundClips.Contains(clip))
                        {
                            foundClips.Add(clip);
                        }
                    }
                }
            }
            
            // 检查Animation
            Animation animation = child.GetComponent<Animation>();
            if (animation != null)
            {
                foreach (AnimationState state in animation)
                {
                    if (ClipContainsObjectName(state.clip, oldObjectName) && !foundClips.Contains(state.clip))
                    {
                        foundClips.Add(state.clip);
                    }
                }
            }
            
            // 递归查找
            FindAnimationClipsInChildren(child);
        }
    }
    
    /// <summary>
    /// 检查动画剪辑是否包含指定的物体名称
    /// </summary>
    private bool ClipContainsObjectName(AnimationClip clip, string objectName)
    {
        if (clip == null) return false;
        
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
        foreach (var binding in bindings)
        {
            if (binding.path.Contains(objectName))
            {
                return true;
            }
        }
        
        // 检查对象引用曲线
        EditorCurveBinding[] objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
        foreach (var binding in objectBindings)
        {
            if (binding.path.Contains(objectName))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 分析动画剪辑中的引用
    /// </summary>
    private void AnalyzeAnimationClip(AnimationClip clip)
    {
        previewResults.Add($"\n动画剪辑: {clip.name}");
        
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
        int foundCount = 0;
        
        foreach (var binding in bindings)
        {
            if (binding.path.Contains(oldObjectName))
            {
                string newPath = binding.path.Replace(oldObjectName, newObjectName);
                previewResults.Add($"  路径: {binding.path} -> {newPath}");
                previewResults.Add($"  属性: {binding.propertyName}");
                foundCount++;
            }
        }
        
        // 检查对象引用曲线
        EditorCurveBinding[] objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
        foreach (var binding in objectBindings)
        {
            if (binding.path.Contains(oldObjectName))
            {
                string newPath = binding.path.Replace(oldObjectName, newObjectName);
                previewResults.Add($"  对象引用路径: {binding.path} -> {newPath}");
                previewResults.Add($"  属性: {binding.propertyName}");
                foundCount++;
            }
        }
        
        previewResults.Add($"  找到 {foundCount} 个需要修复的引用");
    }
    
    /// <summary>
    /// 执行动画引用修改操作
    /// </summary>
    private void ExecuteRename()
    {
        // 修改动画引用
        int totalFixed = 0;
        foreach (AnimationClip clip in foundClips)
        {
            int fixedInClip = FixAnimationClip(clip);
            totalFixed += fixedInClip;
            
            // 标记资源为脏，确保保存
            EditorUtility.SetDirty(clip);
        }
        
        // 保存资源
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        string message = $"成功修改了 {foundClips.Count} 个动画剪辑中的 {totalFixed} 个引用。";
        
        EditorUtility.DisplayDialog("修改完成", message, "确定");
        
        Debug.Log($"动画引用修改完成：{message}");
        
        // 清空结果
        foundClips.Clear();
        previewResults.Clear();
        showPreview = false;
    }
    

    
    private int FixAnimationClip(AnimationClip clip)
    {
        int fixedCount = 0;
        
        // 处理普通曲线
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
        foreach (var binding in bindings)
        {
            if (binding.path.Contains(oldObjectName))
            {
                // 获取原始曲线
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                
                // 创建新的绑定
                EditorCurveBinding newBinding = binding;
                newBinding.path = binding.path.Replace(oldObjectName, newObjectName);
                
                // 移除旧曲线并添加新曲线
                AnimationUtility.SetEditorCurve(clip, binding, null);
                AnimationUtility.SetEditorCurve(clip, newBinding, curve);
                
                fixedCount++;
            }
        }
        
        // 处理对象引用曲线
        EditorCurveBinding[] objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
        foreach (var binding in objectBindings)
        {
            if (binding.path.Contains(oldObjectName))
            {
                // 获取原始对象引用曲线
                ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                
                // 创建新的绑定
                EditorCurveBinding newBinding = binding;
                newBinding.path = binding.path.Replace(oldObjectName, newObjectName);
                
                // 移除旧曲线并添加新曲线
                AnimationUtility.SetObjectReferenceCurve(clip, binding, null);
                AnimationUtility.SetObjectReferenceCurve(clip, newBinding, keyframes);
                
                fixedCount++;
            }
        }
        
        return fixedCount;
    }
}
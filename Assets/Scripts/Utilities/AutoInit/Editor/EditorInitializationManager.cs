using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class EditorInitializationManager
{
    static EditorInitializationManager()
    {
        // 延迟调用确保场景加载完成
        EditorApplication.delayCall += InitializeAllInScene;
    }

    /// <summary>
    /// 初始化场景中所有实现IAutoInitializable的对象
    /// </summary>
    public static void InitializeAllInScene()
    {
        var components = Object.FindObjectsOfType<MonoBehaviour>(true); // 包含隐藏对象

        int count = 0;
        
        foreach (var component in components)
        {
            if (component is IAutoInitializable initializable)
            {
                try
                {
                    if (!initializable.EditorInitialize(out string errorMessage))
                    {
                        Debug.LogError(errorMessage);
                        continue;
                    }
                    
                    count++;
                    Debug.Log($"{component.name}初始化完毕");
                }
                catch (Exception e)
                {
                    Debug.LogError($"初始化 {component.name} 失败: {e.Message}", component);
                }
            }
        }

        if (count > 0)
        {
            Debug.Log($"[Editor] 已初始化 {count} 个对象");
        }
    }
}

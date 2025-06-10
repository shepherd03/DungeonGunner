#if UNITY_EDITOR

public interface IAutoInitializable
{
    /// <summary>
    /// 编辑器中的初始化方法
    /// </summary>
    void EditorInitialize();
    
    bool NeedInitialize { get; }
}

#endif
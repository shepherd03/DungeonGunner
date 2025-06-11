#if UNITY_EDITOR

public interface IAutoInitializable
{
    /// <summary>
    /// 编辑器中的初始化方法
    /// </summary>
    bool EditorInitialize(out string errorMessage);
}

#endif
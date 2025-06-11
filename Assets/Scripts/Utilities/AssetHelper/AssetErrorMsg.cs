using System;
using Object = UnityEngine.Object;

public static class AssetErrorMsg
{
    public static string NotFound<T>() where T:Object
    {
        return $"没有找到类型为{typeof(T)}的文件";
    }

    public static string NotSingle<T>() where T:Object
    {
        return $"当前项目中的{typeof(T)}数量大于1,这可能是不合理的";
    }
}
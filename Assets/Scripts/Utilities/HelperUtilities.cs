using System.Collections;
using UnityEngine;

public class HelperUtilities
{
    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck)
    {
        if (string.IsNullOrEmpty(stringToCheck))
        {
            Debug.Log(fieldName + "为空，且必须包含一个值在对象" + thisObject.name + "中");
            return true;
        }

        return false;
    }

    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableToCheck)
    {
        bool error = false;
        int count = 0;

        if (enumerableToCheck == null)
        {
            Debug.Log($"{fieldName}在对象{thisObject.name}中为空");
            return true;
        }

        foreach (var item in enumerableToCheck)
        {
            if (item == null)
            {
                Debug.Log($"{fieldName}在对象{thisObject.name}中存在空值");
                error = true;
            }
            else
            {
                count++;
            }
        }

        if (count == 0)
        {
            Debug.Log($"{fieldName}在对象{thisObject.name}中没有任何值");
            error = true;
        }
        
        return error;
    }
}
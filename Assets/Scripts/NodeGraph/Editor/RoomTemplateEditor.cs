using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomTemplateSO))]
public class RoomTemplateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        RoomTemplateSO roomTemplate = (RoomTemplateSO) target;

        if (GUILayout.Button("自动设置边界"))
        {
            roomTemplate.GenerateBounds();
        }
    }
}

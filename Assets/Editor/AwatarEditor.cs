using UnityEngine;
using System.Collections;
using UnityEditor;

public class AwatarEditor : EditorWindow
{
    static AwatarEditor m_editorWindow;
    GameObject playerModel;
    GameObject playerModelClone;

    [MenuItem("Custom/Awatar %w")]
    static void ShowWindow()
    {
        m_editorWindow = (AwatarEditor)GetWindow<AwatarEditor>();
        m_editorWindow.Show();
    }

    bool CheckModelValid(GameObject model)
    {
        return !model.name.Contains("@");
    }

    void OnGUI()
    {
        GUILayout.Label("--- 选择模型拖放到下面 ---");
        GUILayout.BeginHorizontal();
        GameObject newModel = EditorGUILayout.ObjectField("FBX", playerModel,typeof(GameObject), true) as GameObject;

        if (newModel != playerModel)
        {
            if (CheckModelValid(newModel))
            {
                playerModelClone = GameObject.Instantiate(newModel) as GameObject;

                playerModel = newModel;
            }
            else
            {
                playerModel = null;
                EditorUtility.DisplayDialog("选择模型错误", "不能选择动画模型.", "Ok");
            }

        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        GUILayout.Label("--- 按照步奏进行拆解模型 ---");

        if (GUILayout.Button("1-创建材质"))
        {
            if (playerModel != null)
            {
                if (AwatarAssetManager.GenerateMaterials(playerModel))
                {
                    EditorUtility.DisplayDialog("Character Generator", "成功创建材质.", "Ok");
                }
                else
                {
                    EditorUtility.DisplayDialog("Character Generator", "创建材质失败", "Ok");
                }
            }
        }
    }

    void OnDestroy()
    {
        if (playerModelClone != null)
        {
            GameObject.DestroyImmediate(playerModelClone);
        }
    }
}

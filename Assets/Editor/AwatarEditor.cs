﻿using UnityEngine;
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
            else
            {
                EditorUtility.DisplayDialog("错误", "当前没有模型", "Ok");
            }
        }
        EditorGUILayout.Separator();
        if (GUILayout.Button("2-分解动作"))
        {
            if (playerModel != null)
            {
                if (AwatarAssetManager.SplitAnimations(playerModel))
                {
                    EditorUtility.DisplayDialog("Character Generator", "成功分解动作.", "Ok");
                }
                else
                {
                    EditorUtility.DisplayDialog("Character Generator", "分解动作失败", "Ok");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "当前没有模型", "Ok");
            }
        }
        EditorGUILayout.Separator();
        if (GUILayout.Button("3-创建模型基本组件"))
        {
            if (playerModel != null)
            {
                if (AwatarAssetManager.GenerateRoleBaseAsset(playerModel))
                {
                    EditorUtility.DisplayDialog("Character Generator", "成功创建模型基本组件.", "Ok");
                }
                else
                {
                    EditorUtility.DisplayDialog("Character Generator", "创建模型基本组件失败", "Ok");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "当前没有模型", "Ok");
            }
        }
        EditorGUILayout.Separator();
        if (GUILayout.Button("4-创建模型蒙皮"))
        {
            if (playerModel != null)
            {
                if (AwatarAssetManager.GenerateRoleSkinAsset(playerModel))
                {
                    EditorUtility.DisplayDialog("Character Generator", "成功创建模型蒙皮.", "Ok");
                }
                else
                {
                    EditorUtility.DisplayDialog("Character Generator", "创建模型蒙皮失败", "Ok");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "当前没有模型", "Ok");
            }
        }

        EditorGUILayout.Separator();
        if (GUILayout.Button("测试生成"))
        {
            if (playerModel != null)
            {
                GameObject.DestroyImmediate(playerModel);
            }
            playerModel = AwatarAssetManager.GenerateRole("ZhanShi", "ZhanShi05");
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

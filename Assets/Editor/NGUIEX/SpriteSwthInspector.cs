using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(SpriteSwith))]
public class SpriteSwthInspector : Editor{

	SpriteSwith m_TargetSpriteSwith;
	UISprite m_TargetSprite;
    int arrayID = -1;
    public override bool HasPreviewGUI()
    {
        return !serializedObject.isEditingMultipleObjects;
    }

    public override void OnPreviewGUI(Rect rect, GUIStyle background)
    {
        SpriteSwith sprite = target as SpriteSwith;
        if (sprite == null || sprite.m_SpriteList.Length < 0) return;
        if (this.arrayID < 0)
        {
            return;
        }
        m_TargetSprite.spriteName = sprite.m_SpriteList[this.arrayID];
        Texture2D tex = m_TargetSprite.mainTexture as Texture2D;
        if (tex == null) return;

        UISpriteData sd = m_TargetSprite.atlas.GetSprite(m_TargetSprite.spriteName);
        NGUIEditorTools.DrawSprite(tex, rect, sd, m_TargetSprite.color);
    }

	public override void OnInspectorGUI ()
	{
		EditorGUIUtility.LookLikeControls(80f);
		m_TargetSpriteSwith = target as SpriteSwith;
        m_TargetSpriteSwith.InitTargetSprite();
		EditorGUILayout.BeginHorizontal();

		m_TargetSpriteSwith.m_SpriteCount = EditorGUILayout.IntField("精灵数量",m_TargetSpriteSwith.m_SpriteCount);
		if(GUILayout.Button("确定"))
		{
            List<string> m_SpriteListGroup = new List<string>();
            foreach (var item in m_TargetSpriteSwith.m_SpriteList)
            {
                m_SpriteListGroup.Add(item);
            }

			m_TargetSpriteSwith.m_SpriteList = new string[m_TargetSpriteSwith.m_SpriteCount];
            for (int i = 0; i < m_SpriteListGroup.Count; i++)
            {
                if (i < m_TargetSpriteSwith.m_SpriteList.Length)
                {
                    m_TargetSpriteSwith.m_SpriteList[i] = m_SpriteListGroup[i];
                }
            }
		}
		EditorGUILayout.EndHorizontal();

		m_TargetSprite = EditorGUILayout.ObjectField("UISprite:",m_TargetSpriteSwith.m_TargetSprite,typeof(UISprite),true) as UISprite;
		if(m_TargetSpriteSwith.m_TargetSprite != m_TargetSprite)
		{
			m_TargetSpriteSwith.m_TargetSprite = m_TargetSprite;
		}

		if(m_TargetSprite != null)
		{
			ComponentSelector.Draw<UIAtlas>(m_TargetSprite.atlas, OnSelectAtlas, true);
			if(m_TargetSpriteSwith.m_SpriteList==null)
			{
				m_TargetSpriteSwith.m_SpriteList = new string[m_TargetSpriteSwith.m_SpriteCount];
			}
			for(int i = 0;i<m_TargetSpriteSwith.m_SpriteList.Length;i++)
			{
				string labelName = "Sprite "+i.ToString();
				string spriteName =m_TargetSpriteSwith.m_SpriteList[i] ==null?"Empty":m_TargetSpriteSwith.m_SpriteList[i].ToString();
				GUILayout.BeginHorizontal();
				GUILayout.Label(labelName, GUILayout.Width(76f));
				
				if (GUILayout.Button(spriteName, "MiniPullDown"))
				{
					NGUISettings.atlas = m_TargetSprite.atlas;
					NGUISettings.selectedSprite = spriteName;
					SpriteSelectorForSpriteSwith.Show(OnSelectSprite,i);
				}
				GUILayout.EndHorizontal();
			}
		}
	}

	void OnSelectAtlas(object obj)
	{
		if (m_TargetSpriteSwith.m_TargetSprite != null)
		{
			NGUIEditorTools.RegisterUndo("Atlas Selection", m_TargetSpriteSwith.m_TargetSprite);
			m_TargetSpriteSwith.m_TargetSprite.atlas = obj as UIAtlas;
           
			//m_TargetSpriteSwith.m_TargetSprite.MakePixelPerfect();
		}
	}

	void OnSelectSprite(string spriteName,int arrayID)
	{
        if (arrayID == 0)
        {
            m_TargetSpriteSwith.m_TargetSprite.spriteName = spriteName;
            //m_TargetSpriteSwith.m_TargetSprite.MakePixelPerfect();
        }
        this.arrayID = arrayID;
		m_TargetSpriteSwith.m_SpriteList[arrayID] = spriteName;
	}

}

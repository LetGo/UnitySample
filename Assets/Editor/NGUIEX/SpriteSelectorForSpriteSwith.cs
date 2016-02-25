using UnityEditor;
using UnityEngine;
using System.Collections;

public class SpriteSelectorForSpriteSwith : SpriteSelector {
	public delegate void SpriteSelectCallback(string sprite,int arrayID);

	SpriteSelectCallback m_SpriteSelectCallBack;
	public int m_ArrayID = 0;

	static public void Show (SpriteSelectCallback callback,int arrayID)
	{
		if (instance != null)
		{
			instance.Close();
			instance = null;
		}
		SpriteSelectorForSpriteSwith comp = ScriptableWizard.DisplayWizard<SpriteSelectorForSpriteSwith>("Select a Sprite");
		comp.m_ArrayID = arrayID;
		comp.m_SpriteSelectCallBack = callback;
		//comp.mCallback = comp.OnSelectAtlas;
	}

	public void OnSelectAtlas(string spriteName)
	{
		if(m_SpriteSelectCallBack!=null)
		{
			m_SpriteSelectCallBack(spriteName,m_ArrayID);
		}
	}

}

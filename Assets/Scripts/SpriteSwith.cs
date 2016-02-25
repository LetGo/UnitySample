using UnityEngine;
using System.Collections;
public class SpriteSwith : MonoBehaviour {

	public int m_SpriteCount = 0;
	public UISprite m_TargetSprite;
	public string[] m_SpriteList;

	public void InitTargetSprite()
	{
		if(m_TargetSprite == null)
		{
			m_TargetSprite = GetComponent<UISprite>();
		}
	}
	/// <summary>
	/// 切换图集，传入Inspector面板内相应的图集位置ID，位置从1开始，0为关闭图集显示
	/// </summary>
	/// <param name="spriteID">图集ID</param>
	public void ChangeSprite(int spriteID)
	{
		if(IsDisableSpriteID(spriteID))
		{
			gameObject.SetActive(false);
			return;
		}else if(!gameObject.activeSelf)
		{
			gameObject.SetActive(true);
		}
        if (m_TargetSprite == null)
            m_TargetSprite = GetComponent<UISprite>();

		m_TargetSprite.spriteName = m_SpriteList[spriteID -1];
	}

    public void ChangeSprite(uint id)
    {
        ChangeSprite((int)id);
    }

	bool IsDisableSpriteID(int spriteID)
	{
		return spriteID == 0;
	}

}

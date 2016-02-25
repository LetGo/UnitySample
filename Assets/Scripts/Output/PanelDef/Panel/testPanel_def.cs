//-----------------------------------------
//此文件自动生成，请勿手动修改
//生成日期: 2/25/2016 8:53:21 PM
//-----------------------------------------
using UnityEngine;


partial class testPanel : MonoBehaviour 
{

    UISprite      m_sprite_Sprite;

    UILabel       m_label_2;

    UILabel       m_label_Label2;

    UIButton      m_btn_Button2;


    //初始化控件变量
    protected void InitControls()
    {
        m_sprite_Sprite = transform.FindChild("Sprite").GetComponent<UISprite>();
        m_label_2 = transform.FindChild("Sprite/2").GetComponent<UILabel>();
        m_label_Label2 = transform.FindChild("Label2").GetComponent<UILabel>();
        m_btn_Button2 = transform.FindChild("Button2").GetComponent<UIButton>();
    }


    //注册控件事件处理函数
    protected void RegisterControlEvents()
    {
        UIEventListener.Get(m_btn_Button2.gameObject).onClick = _OnClick_Button2_Btn;
    }

    void _OnClick_Button2_Btn(GameObject go)
    {
		//OnClick_Button2_Btn( go );
    }


}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;
class WidgetMeta
{
    public string control_name;
    public string var_name;
    public string uitype;
    public string event_name;
    public string event_func;
    public string control_path;

}

public class PanelGenerator  
{
    static string ms_ouput_dir;

    static GameObject selecePanel;

    static Dictionary<string, WidgetMeta> ms_widget_dict;

    [MenuItem("NGUI/生成Panel代码")]
    static void Excute() {
        Debug.ClearDeveloperConsole();

        selecePanel = Selection.activeGameObject;

        if (selecePanel == null)
        {
            Debug.LogError("请选择一个UIPanel物体节点");
            return;
        }

        if (selecePanel.GetComponent<UIPanel>() == null)
        {
            Debug.LogError("请先选择一个带有UIPanel物体节点");
            return;
        }
        string path;

        if (PrefabUtility.GetPrefabType(selecePanel) == PrefabType.PrefabInstance)
        {
            UnityEngine.Object parentObject = PrefabUtility.GetPrefabParent(selecePanel);
            path = AssetDatabase.GetAssetPath(parentObject);
        }
        else if (PrefabUtility.GetPrefabType(selecePanel) == PrefabType.Prefab)
        {
            path = AssetDatabase.GetAssetPath(selecePanel);
        }
        else
        {
            Debug.LogError("请先保存到一个Prefab");
            return;
        }

        string[] subdirs = path.Split(new char[] { '/' });
        if (subdirs.Length < 3)
        {
            Debug.LogError("路径有误");
            return;
        }

        ms_ouput_dir = Application.dataPath + "/Scripts/Output/PanelDef/" + subdirs[subdirs.Length - 2] + "/";

        if (!Directory.Exists(ms_ouput_dir))
        {
            Directory.CreateDirectory(ms_ouput_dir);
        }

        ms_widget_dict = new Dictionary<string, WidgetMeta>();

        FindTagWidgets(selecePanel.transform);

        GenerateCode(selecePanel.gameObject);
    }


    static void FindTagWidgets(Transform root)
    {
        //只寻找带有此标签的组件
        if (root.tag == "ui_control")
        {
            WidgetMeta wm = new WidgetMeta();
            wm.uitype = GetControlType(root);

            GetEventParam(root,wm);

            GetWidgetPath(root.gameObject, ref wm.control_path);

            try
            {
                ms_widget_dict.Add(wm.control_name, wm);
                Debug.Log("add :" + wm.control_name );
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                Debug.LogError("控件名有重复: " + wm.control_name);
                return;
            }
        }

        foreach (Transform child in root)
        {
            FindTagWidgets(child);
        }
    }


    static void GenerateCode(GameObject panel) {
        string code_path = ms_ouput_dir + panel.name + "_def.cs";
        FileStream fs = File.Open(code_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
        fs.SetLength(0);
        StreamWriter sw = new StreamWriter(fs);

        sw.WriteLine("//-----------------------------------------");
        sw.WriteLine("//此文件自动生成，请勿手动修改");
        sw.WriteLine("//生成日期: " + DateTime.Now);
        sw.WriteLine("//-----------------------------------------");

        sw.WriteLine("using UnityEngine;");
        sw.WriteLine("");
        sw.WriteLine("");

        sw.WriteLine("partial class " + panel.name + " : MonoBehaviour \n{");

        sw.WriteLine();

        //控件变量声明 例 UIButton      m_btn_enter;
        foreach (WidgetMeta meta in ms_widget_dict.Values)
        {
            string line = "    " + meta.uitype;

            //对齐处理
            int space_num = 18 - line.Length;
            line += new string(' ', space_num);

            line += meta.var_name + ";";
            sw.WriteLine(line);
            sw.WriteLine();
        }

        //控件变量初始化函数
        sw.WriteLine("");
        sw.WriteLine("    //初始化控件变量");
        sw.WriteLine("    protected void InitControls()");
        sw.WriteLine("    {");
        foreach (WidgetMeta meta in ms_widget_dict.Values)
        {
            sw.WriteLine("        " + meta.var_name + " = transform.FindChild(\"" + meta.control_path + "\").GetComponent<" + meta.uitype + ">();");
        }

        sw.WriteLine("    }");
        sw.WriteLine("");

        //控件事件关联函数
        sw.WriteLine("");
        sw.WriteLine("    //注册控件事件处理函数");
        sw.WriteLine("    protected void RegisterControlEvents()");
        sw.WriteLine("    {");
        foreach (WidgetMeta meta in ms_widget_dict.Values)
        {
            if (string.IsNullOrEmpty(meta.event_name) == false)
            {
                if (string.IsNullOrEmpty(meta.event_func))
                {
                    meta.event_func = GetFirstUpperStr(meta.event_name)+ "_" + GetFirstUpperStr(meta.control_name);

                    if (meta.uitype == "UIButton")
                    {
                        meta.event_func += "_Btn";
                    }
                }

                string line = "        UIEventListener.Get(" + meta.var_name + ".gameObject" + ")." + meta.event_name + " = _" + meta.event_func + ";";
                sw.WriteLine(line);
            }
        }
        sw.WriteLine("    }");
        sw.WriteLine("");

        //处理函数
        foreach (WidgetMeta meta in ms_widget_dict.Values)
        {

            if (string.IsNullOrEmpty(meta.event_func) == false)
            {
                sw.WriteLine("    void _" + meta.event_func + "(GameObject go)");
                sw.WriteLine("    {");
                sw.WriteLine("\t\t//" + meta.event_func + "( go );");
                sw.WriteLine("    }");

                sw.WriteLine("");
            }
        }

        sw.WriteLine("");
        sw.WriteLine("}");

        sw.Close();
        fs.Close();

        Debug.Log("生成代码文件: " + code_path);
        AssetDatabase.Refresh();
    }

    static void GetWidgetPath(GameObject root,ref string path) {
        while (root != selecePanel)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = "/" + root.name;
            }
            else
            {
                path = root.name + path;
            }

            root = root.transform.parent.gameObject;
        }
        if (path[0] == '/')
        {
            path = path.Substring(1);
        }
    }

    static string GetControlType(Transform control)
    {
        //button 优先级为最高
        if (control.GetComponent<UIButton>() != null)
        {
            return "UIButton";
        }
        if (control.GetComponent<UIInput>() != null)
        {
            return "UIInput";
        }
        if (control.GetComponent<UIToggle>() != null)
        {
            return "UIToggle";
        }
        if (control.GetComponent<UIScrollView>() != null)
        {
            return "UIScrollView";
        }

        if (control.GetComponent<UIPlayAnimation>() != null)
        {
            return "UIPlayAnimation";
        }

        if (control.GetComponent<UITextList>() != null)
        {
            return "UITextList";
        }

        if (control.GetComponent<UITooltip>() != null)
        {
            return "UITooltip";
        }

        {
            UIWidget w = control.GetComponent<UIWidget>();
            if (w != null)
            {
                return w.GetType().ToString();
            }
        }

        {
            UIWidgetContainer w = control.GetComponent<UIWidgetContainer>();
            if (w != null)
            {
                return w.GetType().ToString();
            }
        }

        return "Transform";
    }

    static void GetEventParam(Transform control, WidgetMeta meta)
    {
        meta.control_name = control.name;


        UIControlMeta meta_def = control.GetComponent<UIControlMeta>();
        if (meta_def != null)
        {
            meta.event_name = meta_def.trigger.ToString();
            meta.event_func = meta_def.FunctionName;
            if (meta_def.VarName != null)
            {
                meta_def.VarName = meta_def.VarName.Trim();
                if (string.IsNullOrEmpty(meta_def.VarName) == false)
                    meta.var_name = meta_def.VarName;
            }
        }
        else
        {
            if (meta.uitype == "UIButton")
            {
                //按钮默认是有单击事件的
                meta.event_name = "onClick";
            }
        }

        if (string.IsNullOrEmpty(meta.var_name))
        {
            string prefix = GetVarPrefix(meta.uitype);
            if (prefix != null)
                meta.var_name = "m_" + prefix + "_" + control.name;
            else
                meta.var_name = "m_" + control.name;
        }
    }

    static string GetVarPrefix(string type)
    {
        if (type == "UISprite")
        {
            return "sprite";
        }
        if (type == "UIWidget")
        {
            return "widget";
        }
        else if (type == "UIButton")
        {
            return "btn";
        }
        else if (type == "UIScrollView")
        {
            return "scrollview";
        }
        else if (type == "UILabel")
        {
            return "label";
        }
        else if (type == "UILabel")
        {
            return "label";
        }
        else if (type == "Transform")
        {
            return "trans";
        }
        else if (type == "UIInput")
        {
            return "input";
        }
        else if (type == "UIGrid")
        {
            return "grid";
        }
        else if (type == "UISlider")
        {
            return "slider";
        }
        else if (type == "UIProgressBar")
        {
            return "progress";
        }
        else if (type == "UIToggle")
        {
            return "toggle";
        }

        return "";
    }

    static string GetFirstUpperStr(string s)
    {
        if (!string.IsNullOrEmpty(s))
        {
            if (s.Length > 1)
            {
                return char.ToUpper(s[0]) + s.Substring(1);
            }
            return char.ToUpper(s[0]).ToString();
        }

        return null;
    }
}

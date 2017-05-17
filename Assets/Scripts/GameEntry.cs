using UnityEngine;
using System.Collections;
using System;

public class GameEntry : MonoBehaviour {

	// Use this for initialization
	void Start () {
        SetResDir();
	}

    private void SetResDir()
    {
        string strResDir;
        if (Application.isEditor == false)
        {
            strResDir = Application.persistentDataPath + "/assets/";
        }
        else
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                strResDir = Application.dataPath + "/../Resource/android/";
            }
            else
            {
                strResDir = Application.dataPath + "/../Resource/ios/";
            }
        }


        Debug.Log("设置资源路径:"+  strResDir);
        Debug.Log(System.IO.Path.GetDirectoryName(strResDir));
        
        Utility.FileUtils.Instance.ResourcePath = strResDir;

        Debug.Log(Utility.FileUtils.Instance.ReadTextFile("test.txt"));


    }


	// Update is called once per frame
	void Update () 
    {
	
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

public class AwatarAssetManager
{
    private static string RoleBaseAssetPath = Application.dataPath + "/Resources/Players/";
    private static string MaterialPath = "Mats/";

    public static bool GenerateMaterials(GameObject playermodel)
    {
        string matPath = RoleBaseAssetPath + MaterialPath;
        matPath += playermodel.name + "/";

        CheckCreateDir(matPath);

        DeleteSpecFiles(matPath, ".mat");

        string texturePath = GetModelRootPath(playermodel) + "/textures";

        List<Texture2D> textureList = CollectAll<Texture2D>(texturePath);

        var smrs = playermodel.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        foreach (var smr in smrs)
        {
            foreach (var t in textureList)
            {

            }
        }

        return true;
    }


    static void CheckCreateDir(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    static void DeleteSpecFiles(string dirPath, string filterName)
    {
        var fileNames = Directory.GetFiles(dirPath);
        fileNames = fileNames.Where(P => !P.EndsWith(".meta") && (string.IsNullOrEmpty(filterName) ? true : P.Contains(filterName))).ToArray();
        
        foreach (string fileName in fileNames)
        {
            File.Delete(fileName);
        }
    }

    static string GetModelRootPath(GameObject model)
    {
        string path = AssetDatabase.GetAssetPath(model);
        int index = path.LastIndexOf('/');
        path = path.Substring(0, index);
        return path;
    }


    public static List<T> CollectAll<T>(string path) where T : UnityEngine.Object
    {
        List<T> l = new List<T>();
        string[] files = Directory.GetFiles(path);
        for (int i = 0, imax = files.Length; i < imax; i++ )
        {
            if (files[i].EndsWith(".meta")) continue;
            T asset = AssetDatabase.LoadAssetAtPath(files[i], typeof(T)) as T;
            if (asset == null) throw new System.Exception("Asset is not "+ typeof(T) +":" + files[i]);
            l.Add(asset);
        }
        return l;
    }
}

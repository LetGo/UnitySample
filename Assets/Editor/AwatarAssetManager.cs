using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

public class AwatarAssetManager
{
    private const string RoleBaseAssetPath ="Assets/Resources/Players/";
    private const string MaterialPath = RoleBaseAssetPath  + "Mats/";
    private const string AnimationPath = RoleBaseAssetPath + "Animations/";
    private const string RoleBasePath = RoleBaseAssetPath + "RoleBase/";
    private const string SkinPath = RoleBaseAssetPath + "Skins/";

    public static bool GenerateMaterials(GameObject playermodel)
    {
        string matPath = MaterialPath;
        matPath += playermodel.name + "/";

        CheckCreateDir(matPath);

        DeleteSpecFiles(matPath, ".mat");

        string texturePath = GetModelRootPath(playermodel) + "/textures";

        List<Texture2D> textureList = CollectAll<Texture2D>(texturePath);

        var smrs = playermodel.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        foreach (var smr in smrs)
        {
            //TODO NORMAL TEXTURE  

            foreach (var t in textureList)
            {
                if (t.name.ToLower().Contains("normal")) continue;
                if (!t.name.ToLower().Contains(smr.name.ToLower())) continue;

                string matSavePath = matPath + smr.name.ToLower() + ".mat";

                Material m = new Material(Shader.Find("Transparent/Diffuse"));
                m.SetColor("_COlor", new Color(0.6f, 0.6f, 0.6f));
                m.SetTexture("_MainTex", t);

                if (m != null)
                {
                    AssetDatabase.CreateAsset(m,matSavePath.Replace(Application.dataPath +"/",""));
                }
            }
        }

        return true;
    }

    public static bool SplitAnimations(GameObject playerModel) 
    {
        string animPath = AnimationPath + playerModel.name + "/";

        CheckCreateDir(animPath);
        DeleteSpecFiles(animPath,".anim");

        List<GameObject> animaGOs = GetAllAnimGameObject( GetModelRootPath(playerModel), playerModel.name);

        List<AnimationClip> animationClips = new List<AnimationClip>();
        foreach (var item in animaGOs)
        {
            animationClips.AddRange(GetAnimationClipFromGO(item));
        }

        foreach (var item in animationClips)
        {
            var clipName = animPath + item.name;
            string clipPath = clipName + ".anim";

            AssetDatabase.CreateAsset(item, clipPath);
        }
        return true;
    }

    public static bool GenerateRoleBaseAsset(GameObject plaerModel)
    {
        string basePath = RoleBasePath;
        CheckCreateDir(basePath);
        DeleteSpecFiles(basePath, plaerModel.name);

        var modelClone = GameObject.Instantiate(plaerModel) as GameObject;

        if (modelClone != null)
        {
            var anim = modelClone.GetComponent<Animation>();
            if(anim != null) Object.DestroyImmediate(anim);
            var animtor = modelClone.GetComponent<Animator>();
            if (animtor != null) Object.DestroyImmediate(animtor);

            var mes = modelClone.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var item in mes)
            {
                Object.DestroyImmediate(item.gameObject);
            }
            CreatePrefab(modelClone, basePath + plaerModel.name + ".prefab");
        }
        return true;
    }

    public static bool GenerateRoleSkinAsset(GameObject plaerModel)
    {
        string roleSkin = SkinPath + plaerModel.name +"/";
        CheckCreateDir(roleSkin);
        DeleteSpecFiles(roleSkin,"");
        List<Material> materials = CollectAll<Material>(MaterialPath + plaerModel.name);

        foreach (var smr in plaerModel.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            SkinMeshRenderHolder smrHolder = ScriptableObject.CreateInstance<SkinMeshRenderHolder>();
            smrHolder.Mats = new List<Material>();
            smrHolder.Bones = new List<string>();

            smrHolder.sharedMesh = smr.sharedMesh;

            foreach (Material m in materials)
            {
                if (m.name.Contains(smr.name.ToLower()))
                {
                    smrHolder.Mats.Add(m);
                }
            }
            foreach (Transform t in smr.bones)
                smrHolder.Bones.Add(t.name);

            AssetDatabase.CreateAsset(smrHolder, roleSkin + smr.name + ".asset");
        }
        return true;
    }

    public static GameObject GenerateRole(string roleName, string avatarName)
    {
        string basePath = RoleBasePath + roleName + ".prefab";
        var avatarPath = SkinPath + roleName + "/" + avatarName + ".asset";

        var basePrefab = AssetDatabase.LoadAssetAtPath(basePath, typeof(GameObject)) as GameObject;
        if (basePrefab != null)
        {
            var avatar = AssetDatabase.LoadAssetAtPath(avatarPath, typeof(SkinMeshRenderHolder)) as SkinMeshRenderHolder;
            if (avatar != null)
            {
                GameObject baseClone = GameObject.Instantiate(basePrefab) as GameObject;

                List<CombineInstance> combineInstances = new List<CombineInstance>();
                List<Transform> bones = new List<Transform>();
                Transform[] transforms = baseClone.GetComponentsInChildren<Transform>();

                for (int sub = 0; sub < avatar.sharedMesh.subMeshCount; sub++)
                {
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = avatar.sharedMesh;
                    ci.subMeshIndex = sub;
                    combineInstances.Add(ci);
                }

                foreach (string bone in avatar.Bones)
                {
                    foreach (Transform transform in transforms)
                    {
                        if (transform.name != bone) continue;
                        bones.Add(transform);
                        break;
                    }
                }

                SkinnedMeshRenderer r = baseClone.GetComponent<SkinnedMeshRenderer>();
                if (r == null) r = baseClone.AddComponent<SkinnedMeshRenderer>();
                r.sharedMesh = new Mesh();
                r.sharedMesh.CombineMeshes(combineInstances.ToArray(), false, false);
                r.bones = bones.ToArray();
                r.materials = avatar.Mats.ToArray();

                AttachAnimation(baseClone, roleName, "run", "run", "idle", "atkIdle");

                return baseClone;
            }
        }
        return null;
    }

    private static void AttachAnimation(GameObject root, string roleName, string defaultAnim, params string[] anims)
    {
        Animation anim = root.GetComponent<Animation>();
        if (anim == null)
        {
            anim = root.AddComponent<Animation>();
        }
        else
        {
            List<string> stateName = new List<string>();
            foreach (AnimationState state in anim)
            {
                stateName.Add(state.name);
            }
            foreach (string clip in stateName)
            {
                anim.RemoveClip(clip);
            }
        }

        foreach (var s in anims)
        {
            string aniPath = "Players/Animations/" + roleName + "/" + s;
            var aniClip = Resources.Load(aniPath) as AnimationClip;
            if (aniClip != null)
            {
                root.animation.AddClip(aniClip, s);
            }
            else
            {
                Debug.LogError("load anim failed path :" + aniPath);
            }
        }
        root.animation.CrossFade(defaultAnim);
    }

    static Object CreatePrefab(GameObject go, string path) 
    {
        Object prefab = PrefabUtility.CreatePrefab(path, go, ReplacePrefabOptions.ReplaceNameBased);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static List<AnimationClip> GetAnimationClipFromGO(GameObject model) 
    {
        List<AnimationClip> clips = new List<AnimationClip>();

        GameObject go = GameObject.Instantiate(model) as GameObject;
        if (go != null)
        {
            var getClips = AnimationUtility.GetAnimationClips(go);

            foreach (var item in getClips)
            {
                var tempClip = Object.Instantiate(item) as AnimationClip;
                tempClip.name = item.name;
                clips.Add(tempClip);
            }
            GameObject.DestroyImmediate(go);
        }
        return clips;
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

    private static List<GameObject> GetAllAnimGameObject(string dirPath,string modelName) 
    {
        List<GameObject> l = CollectAll<GameObject>(dirPath);

        return l.FindAll(C => C.name.StartsWith(modelName +"@"));
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

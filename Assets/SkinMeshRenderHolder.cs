using System.Collections.Generic;
using UnityEngine;

public class SkinMeshRenderHolder : ScriptableObject
{
    public List<Material> Mats;
    public List<string> Bones;
    public Mesh sharedMesh;
}

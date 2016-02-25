using UnityEngine;
using System.Collections;

[AddComponentMenu("NGUI/Custom/UIControlMeta")]
public class UIControlMeta : MonoBehaviour
{
    public string VarName;
    public UIControlMeta.Trigger trigger;
    public string FunctionName;

    public enum Trigger
    {
        onClick = 0,
        onPress = 1,
        onRelease = 2,
        onDoubleClick = 3,
    }
}

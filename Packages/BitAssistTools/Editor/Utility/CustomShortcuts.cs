using UnityEngine;
using UnityEditor;
using PlacemenAssistance;

public class CustomShortcutWindow : EditorWindow
{
    CommonFunctions CommonFunctionsInstance;

    [MenuItem("Bit Assist Tools/カスタム ショートカット/グループを生成 %g", priority = 100)]
    static void ParentCenterAlignerShortcuts()
    {
        CommonFunctions CommonFunctionsInstance = new CommonFunctions();
        CommonFunctions.ParentCenterAligner(Selection.transforms);
    }
}

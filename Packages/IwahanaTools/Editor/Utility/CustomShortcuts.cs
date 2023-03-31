using UnityEngine;
using UnityEditor;
using PlacemenAssistance;

public class CustomShortcutWindow : EditorWindow
{
    private CommonFunctions CommonFunctionsInstance;

    [MenuItem("Iwahana Tools/カスタム ショートカット/グループを生成 %g", priority = 100)]
    private static void ParentCenterAlignerShortcuts()
    {
        CommonFunctions CommonFunctionsInstance = new CommonFunctions();
        CommonFunctions.ParentCenterAligner(Selection.transforms);
    }
}

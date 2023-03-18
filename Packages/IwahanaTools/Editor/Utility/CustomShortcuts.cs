using UnityEngine;
using UnityEditor;
using PlacemenAssistance;

public class CustomShortcutWindow : EditorWindow
{
    private CommonFunctions CommonFunctionsInstance;

    [MenuItem("Iwahana Tools/カスタム ショートカット/グループを生成 %g", priority = 50)]
    private static void ParentCenterAlignerShortcuts()
    {
        CustomShortcutWindow window = (CustomShortcutWindow)EditorWindow.GetWindow(typeof(CustomShortcutWindow));
        window.CommonFunctionsInstance = new CommonFunctions();
        CommonFunctions.ParentCenterAligner(Selection.transforms);
    }
}

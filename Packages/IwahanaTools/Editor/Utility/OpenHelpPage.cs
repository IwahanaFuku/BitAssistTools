using UnityEditor;
using UnityEngine;

public class HelpMenu : EditorWindow
{
    private const string HELP_URL = "https://github.com/IwahanaFuku/UnityEditor";

    [MenuItem("Iwahana Tools/ヘルプページ",priority = 100)]
    public static void OpenHelpPage()
    {
        Application.OpenURL(HELP_URL);
    }
}

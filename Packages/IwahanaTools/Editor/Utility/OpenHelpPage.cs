using UnityEditor;
using UnityEngine;

public class HelpMenu : EditorWindow
{
    private const string HELP_URL = "https://scrapbox.io/IwahanaTools/Iwahana_Tools%E3%81%AB%E3%81%A4%E3%81%84%E3%81%A6";

    [MenuItem("Iwahana Tools/ヘルプ",priority = 200)]
    public static void OpenHelpPage()
    {
        Application.OpenURL(HELP_URL);
    }
}

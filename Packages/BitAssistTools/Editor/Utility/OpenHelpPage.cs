using UnityEditor;
using UnityEngine;

public class HelpMenu : EditorWindow
{
    const string HELP_URL = "https://scrapbox.io/BitAssistTools/Bit_Assist_Tools%E3%81%AB%E3%81%A4%E3%81%84%E3%81%A6";

    [MenuItem("Bit Assist Tools/ヘルプ",priority = 210)]
    public static void OpenHelpPage()
    {
        Application.OpenURL(HELP_URL);
    }
}

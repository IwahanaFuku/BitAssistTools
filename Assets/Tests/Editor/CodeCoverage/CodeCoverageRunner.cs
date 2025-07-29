using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

/// <summary>
/// Code Coverage自動実行用スクリプト
/// Unity 2019.4対応版
/// </summary>
public static class CodeCoverageRunner
{
    [MenuItem("Bit Assist Tools/テスト/カバレッジ付きテスト実行")]
    public static void RunTestsWithCoverage()
    {
        Debug.Log("コードカバレッジ付きでテストを開始します...");
        
        // Unity 2019のCode Coverage有効化
        EditorPrefs.SetBool("CodeCoverageEnabled", true);
        
        // Test Runnerの起動
        var testRunnerWindowType = typeof(UnityEditor.TestTools.TestRunner.TestRunnerWindow);
        var window = EditorWindow.GetWindow(testRunnerWindowType);
        window.Show();
        
        Debug.Log("Test Runnerを開きました。Code Coverage有効でテストを手動実行してください。");
    }
    
    [MenuItem("Bit Assist Tools/テスト/カバレッジレポートを開く")]
    public static void OpenCoverageReport()
    {
        var reportPath = Path.Combine(Application.dataPath, "../CodeCoverage/Report/index.html");
        if (File.Exists(reportPath))
        {
            System.Diagnostics.Process.Start(reportPath);
            Debug.Log("カバレッジレポートをブラウザで開きました。");
        }
        else
        {
            Debug.LogWarning("カバレッジレポートが見つかりません: " + reportPath);
            Debug.LogWarning("まずカバレッジ付きでテストを実行し、Code CoverageウィンドウでHTMLレポートを生成してください。");
        }
    }
    
    [MenuItem("Bit Assist Tools/テスト/カバレッジ統計表示")]
    public static void ShowCoverageStats()
    {
        var packagesPath = Path.Combine(Application.dataPath, "../Packages/BitAssistTools");
        
        if (!Directory.Exists(packagesPath))
        {
            Debug.LogError("BitAssistToolsパッケージが見つかりません: " + packagesPath);
            return;
        }
        
        // テストファイルを検索
        var testFiles = Directory.GetFiles(packagesPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => f.Contains("Tests")).ToArray();
            
        // ソースファイルを検索（テスト以外）
        var sourceFiles = Directory.GetFiles(packagesPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Tests") && !f.Contains(".meta")).ToArray();
        
        var fileBasedCoverage = sourceFiles.Length > 0 ? (double)testFiles.Length / sourceFiles.Length * 100 : 0;
        
        Debug.Log("=== BitAssistTools カバレッジ統計 ===");
        Debug.Log($"テストファイル数: {testFiles.Length}");
        Debug.Log($"ソースファイル数: {sourceFiles.Length}");
        Debug.Log($"ファイル数ベースカバレッジ: {fileBasedCoverage:F1}%");
        Debug.Log("────────────────────────────────");
        Debug.Log("テストファイル:");
        foreach (var file in testFiles.Take(10)) // 最初の10件のみ表示
        {
            var fileName = Path.GetFileName(file);
            Debug.Log($"  • {fileName}");
        }
        if (testFiles.Length > 10)
        {
            Debug.Log($"  ... 他 {testFiles.Length - 10} 個のテストファイル");
        }
        Debug.Log("────────────────────────────────");
        Debug.Log("詳細な行カバレッジを確認するには:");
        Debug.Log("1. Window → Analysis → Code Coverage");
        Debug.Log("2. Enable Code Coverage にチェック");
        Debug.Log("3. Window → General → Test Runner → Run All");
        Debug.Log("4. Code CoverageウィンドウでHTMLレポート生成");
    }
    
    [MenuItem("Bit Assist Tools/テスト/Code Coverageウィンドウ")]
    public static void OpenCodeCoverageWindow()
    {
        // Unity 2019のCode Coverageウィンドウを開く
        EditorApplication.ExecuteMenuItem("Window/Analysis/Code Coverage");
    }
    
    [MenuItem("Bit Assist Tools/テスト/Test Runnerウィンドウ")]
    public static void OpenTestRunner()
    {
        EditorApplication.ExecuteMenuItem("Window/General/Test Runner");
    }
    
    [MenuItem("Bit Assist Tools/テスト/カバレッジ設定ガイド")]
    public static void QuickSetup()
    {
        Debug.Log("=== コードカバレッジ設定ガイド ===");
        Debug.Log("BitAssistTools用のCode Coverageを設定します...");
        
        // Code Coverageウィンドウを開く
        EditorApplication.ExecuteMenuItem("Window/Analysis/Code Coverage");
        
        Debug.Log("設定手順:");
        Debug.Log("1. Code Coverageウィンドウで 'Enable Code Coverage' にチェック");
        Debug.Log("2. Included Assemblies 設定: +BitAssistTools.* -BitAssistTools.Tests.*");
        Debug.Log("3. Included Paths設定: +<assets>/Packages/BitAssistTools/** -<assets>/Packages/BitAssistTools/Tests/**");
        Debug.Log("4. Test Runnerを開く (Window → General → Test Runner)");
        Debug.Log("5. EditModeタブで 'Run All' をクリック");
        Debug.Log("6. Code CoverageウィンドウでHTMLレポートを生成");
        Debug.Log("════════════════════════════════");
    }
}
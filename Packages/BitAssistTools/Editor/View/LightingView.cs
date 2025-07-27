using UnityEngine;
using UnityEditor;
using BitAssistTools.Application.ViewModels;
using BitAssistTools.Domain.Models.Lighting;

namespace BitAssistTools.Editor.Lighting
{
    /// <summary>
    /// Lighting機能のView
    /// ViewModelを使用してUI状態とビジネスロジックを分離
    /// </summary>
    public class LightingView : EditorWindow
    {
        private LightingViewModel _viewModel;

        [MenuItem("Bit Assist Tools/ライティング支援ツール", priority = 14)]
        public static void ShowWindow()
        {
            var window = GetWindow<LightingView>("ライティング支援ツール");
            window.Initialize();
        }

        private void Initialize()
        {
            _viewModel = new LightingViewModel();
            
            // イベント購読
            _viewModel.OnUIStateChanged += Repaint;
            _viewModel.OnError += ShowError;
            _viewModel.OnSuccess += ShowSuccess;
        }

        private void OnDestroy()
        {
            // イベント購読解除
            if (_viewModel != null)
            {
                _viewModel.OnUIStateChanged -= Repaint;
                _viewModel.OnError -= ShowError;
                _viewModel.OnSuccess -= ShowSuccess;
                _viewModel.Dispose();
            }
        }

        private void OnGUI()
        {
            if (_viewModel == null)
            {
                Initialize();
            }

            DrawLightIntensityPanel();
        }

        /// <summary>
        /// ライト強度変更パネルの描画
        /// </summary>
        private void DrawLightIntensityPanel()
        {
            _viewModel.UI.IntensityCalculation = EditorGUILayout.Foldout(
                _viewModel.UI.IntensityCalculation, 
                "Intensityの変更"
            );

            if (_viewModel.UI.IntensityCalculation)
            {
                DrawIntensityCalculationControls();
            }
        }

        /// <summary>
        /// 強度計算コントロールの描画
        /// </summary>
        private void DrawIntensityCalculationControls()
        {
            EditorGUILayout.Space();

            // 操作種類選択
            DrawOperationSelection();

            // 強度値入力
            DrawIntensityValueInput();

            EditorGUILayout.Space();

            // 現在の選択状況表示
            DrawCurrentSelectionInfo();

            EditorGUILayout.Space();

            // 実行ボタン
            DrawExecuteButton();
        }

        /// <summary>
        /// 操作種類選択の描画
        /// </summary>
        private void DrawOperationSelection()
        {
            string[] operationLabels = { "加算", "乗算" };
            int currentIndex = (int)_viewModel.LightOperation.Operation;
            
            int newIndex = GUILayout.SelectionGrid(currentIndex, operationLabels, 2);
            
            if (newIndex != currentIndex)
            {
                _viewModel.UpdateOperation((ArithmeticOperation)newIndex);
            }
        }

        /// <summary>
        /// 強度値入力の描画
        /// </summary>
        private void DrawIntensityValueInput()
        {
            float newValue = EditorGUILayout.FloatField(
                "Intensity Value", 
                _viewModel.LightOperation.IntensityValue
            );
            
            if (!Mathf.Approximately(newValue, _viewModel.LightOperation.IntensityValue))
            {
                _viewModel.UpdateIntensityValue(newValue);
            }
        }

        /// <summary>
        /// 現在の選択状況表示
        /// </summary>
        private void DrawCurrentSelectionInfo()
        {
            var stats = _viewModel.GetCurrentStats();
            
            if (stats.HasLights)
            {
                EditorGUILayout.HelpBox(
                    $"選択中: {stats}", 
                    MessageType.Info
                );
                
                // プレビュー表示
                string preview = $"操作: {_viewModel.GetOperationPreview()}";
                EditorGUILayout.LabelField("プレビュー", preview);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Lightコンポーネントを持つGameObjectを選択してください。", 
                    MessageType.Warning
                );
            }
        }

        /// <summary>
        /// 実行ボタンの描画
        /// </summary>
        private void DrawExecuteButton()
        {
            bool canExecute = _viewModel.CanExecuteLightOperation();
            
            EditorGUI.BeginDisabledGroup(!canExecute);
            
            if (GUILayout.Button("適用"))
            {
                _viewModel.ExecuteLightIntensityOperation();
            }
            
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// エラー表示
        /// </summary>
        private void ShowError(string message)
        {
            EditorUtility.DisplayDialog("エラー", message, "OK");
        }

        /// <summary>
        /// 成功表示
        /// </summary>
        private void ShowSuccess(string message)
        {
            Debug.Log(message);
            // 必要に応じてステータスバーや通知で表示可能
        }
    }
}
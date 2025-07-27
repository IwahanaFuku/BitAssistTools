#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using BitAssistTools.Application.Services;
using BitAssistTools.Domain.Models.Texture;
using BitAssistTools.Domain.Models.Shared;
using BitAssistTools.Infrastructure.Unity;

namespace BitAssistTools.Editor.View
{
    /// <summary>
    /// テクスチャツールのエディターウィンドウ
    /// </summary>
    public class TextureToolsView : EditorWindow
    {
        [MenuItem("Bit Assist Tools/テクスチャ編集支援ツール", priority = 18)]
        public static void Open() => GetWindow<TextureToolsView>("テクスチャツール");

        #region UI State Variables

        // 共通設定
        private string saveBasePathNoExt = "Assets/Textures/Result";
        private bool useGpu = true;
        private ComputeShader computeShader;

        // PNG変換
        private Texture2D convertTarget;
        private bool deleteOriginal = true;

        // 反転
        private Texture2D invertTarget;
        private bool invertR = true, invertG = true, invertB = true, invertA = true;

        // 法線マップ
        private Texture2D normalTarget;
        private bool normalInvertR = false, normalInvertG = true;
        private float normalStrength = 1.0f;

        // マスク統合
        private Texture2D rSource, gSource, bSource, aSource;
        private Channel rChannel = Channel.R, gChannel = Channel.G, bChannel = Channel.B, aChannel = Channel.A;
        private int outputWidth = 1024, outputHeight = 1024;
        private bool rInvert = false, gInvert = false, bInvert = false, aInvert = false;

        #endregion

        #region Service Dependencies

        private UnityTextureRepository _repository;
        private UnityTextureProcessingService _cpuProcessor;
        private UnityGpuTextureOperationService _gpuProcessor;

        #endregion

        private void OnEnable()
        {
            _repository = new UnityTextureRepository();
            _cpuProcessor = new UnityTextureProcessingService();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            DrawGeneralSection();

            EditorGUILayout.Space();
            DrawConvertSection();

            EditorGUILayout.Space();
            DrawInvertSection();

            EditorGUILayout.Space();
            DrawNormalSection();

            EditorGUILayout.Space();
            DrawMergeSection();
        }

        #region UI Drawing Methods

        private void DrawGeneralSection()
        {
            EditorGUILayout.LabelField("全般設定", EditorStyles.boldLabel);
            useGpu = EditorGUILayout.ToggleLeft("GPUを使用（対応環境のみ）", useGpu);
            computeShader = (ComputeShader)EditorGUILayout.ObjectField(
                new GUIContent("コンピュートシェーダ"), computeShader, typeof(ComputeShader), false);
            saveBasePathNoExt = EditorGUILayout.TextField(
                new GUIContent("保存先ベースパス（拡張子なし）"), saveBasePathNoExt);

            // GPU処理サービスの更新
            if (useGpu && computeShader != null)
                _gpuProcessor = new UnityGpuTextureOperationService(computeShader);
            else
                _gpuProcessor = null;
        }

        private void DrawConvertSection()
        {
            EditorGUILayout.LabelField("PNGへ変換", EditorStyles.boldLabel);
            convertTarget = (Texture2D)EditorGUILayout.ObjectField(
                new GUIContent("対象テクスチャ"), convertTarget, typeof(Texture2D), false);
            deleteOriginal = EditorGUILayout.ToggleLeft("元のアセットを削除（元に戻せません）", deleteOriginal);

            if (GUILayout.Button("変換"))
            {
                ExecutePngConversion();
            }
        }

        private void DrawInvertSection()
        {
            EditorGUILayout.LabelField("チャンネル反転", EditorStyles.boldLabel);
            invertTarget = (Texture2D)EditorGUILayout.ObjectField(
                new GUIContent("対象テクスチャ"), invertTarget, typeof(Texture2D), false);

            using (new EditorGUILayout.HorizontalScope())
            {
                invertR = GUILayout.Toggle(invertR, "R", "Button");
                invertG = GUILayout.Toggle(invertG, "G", "Button");
                invertB = GUILayout.Toggle(invertB, "B", "Button");
                invertA = GUILayout.Toggle(invertA, "A", "Button");
            }

            if (GUILayout.Button("反転してPNG保存"))
            {
                ExecuteTextureInversion();
            }
        }

        private void DrawNormalSection()
        {
            EditorGUILayout.LabelField("法線マップツール", EditorStyles.boldLabel);
            normalTarget = (Texture2D)EditorGUILayout.ObjectField(
                new GUIContent("対象テクスチャ（法線マップ）"), normalTarget, typeof(Texture2D), false);

            using (new EditorGUILayout.HorizontalScope())
            {
                normalInvertR = GUILayout.Toggle(normalInvertR, "Rを反転", "Button");
                normalInvertG = GUILayout.Toggle(normalInvertG, "Gを反転", "Button");
            }
            normalStrength = EditorGUILayout.Slider("強度", normalStrength, 0.0f, 2.0f);

            if (GUILayout.Button("適用してPNG保存"))
            {
                ExecuteNormalMapAdjustment();
            }
        }

        private void DrawMergeSection()
        {
            EditorGUILayout.LabelField("マスク統合（RGBAパッキング）", EditorStyles.boldLabel);

            // R チャンネル
            rSource = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Rの入力"), rSource, typeof(Texture2D), false);
            using (new EditorGUILayout.HorizontalScope())
            {
                rChannel = (Channel)EditorGUILayout.EnumPopup("Rに割り当てる元チャンネル", rChannel);
                rInvert = EditorGUILayout.ToggleLeft("R入力を反転", rInvert, GUILayout.Width(110));
            }

            // G チャンネル
            gSource = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Gの入力"), gSource, typeof(Texture2D), false);
            using (new EditorGUILayout.HorizontalScope())
            {
                gChannel = (Channel)EditorGUILayout.EnumPopup("Gに割り当てる元チャンネル", gChannel);
                gInvert = EditorGUILayout.ToggleLeft("G入力を反転", gInvert, GUILayout.Width(110));
            }

            // B チャンネル
            bSource = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Bの入力"), bSource, typeof(Texture2D), false);
            using (new EditorGUILayout.HorizontalScope())
            {
                bChannel = (Channel)EditorGUILayout.EnumPopup("Bに割り当てる元チャンネル", bChannel);
                bInvert = EditorGUILayout.ToggleLeft("B入力を反転", bInvert, GUILayout.Width(110));
            }

            // A チャンネル
            aSource = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Aの入力"), aSource, typeof(Texture2D), false);
            using (new EditorGUILayout.HorizontalScope())
            {
                aChannel = (Channel)EditorGUILayout.EnumPopup("Aに割り当てる元チャンネル", aChannel);
                aInvert = EditorGUILayout.ToggleLeft("A入力を反転", aInvert, GUILayout.Width(110));
            }

            outputWidth = EditorGUILayout.IntField("出力幅", outputWidth);
            outputHeight = EditorGUILayout.IntField("出力高さ", outputHeight);

            if (GUILayout.Button("統合してPNG保存"))
            {
                ExecuteMaskMerge();
            }
        }

        #endregion

        #region Execution Methods

        private void ExecutePngConversion()
        {
            if (convertTarget == null)
            {
                EditorUtility.DisplayDialog("エラー", "テクスチャが選択されていません。", "OK");
                return;
            }

            if (deleteOriginal && !EditorUtility.DisplayDialog("削除の確認",
                "元のアセットを削除しますか？この操作は元に戻せません。", "削除する", "キャンセル"))
            {
                return;
            }

            var result = TextureConversionService.ConvertToPng(convertTarget, deleteOriginal, _repository);
            DisplayResult(result, "PNG変換");
        }

        private void ExecuteTextureInversion()
        {
            if (invertTarget == null)
            {
                EditorUtility.DisplayDialog("エラー", "テクスチャが選択されていません。", "OK");
                return;
            }

            var result = TextureInversionService.InvertTexture(
                invertTarget, invertR, invertG, invertB, invertA, saveBasePathNoExt,
                _repository, _cpuProcessor, _gpuProcessor);
            DisplayResult(result, "テクスチャ反転");
        }

        private void ExecuteNormalMapAdjustment()
        {
            if (normalTarget == null)
            {
                EditorUtility.DisplayDialog("エラー", "テクスチャが選択されていません。", "OK");
                return;
            }

            var result = NormalMapService.AdjustNormalMap(
                normalTarget, normalInvertR, normalInvertG, normalStrength, saveBasePathNoExt,
                _repository, _cpuProcessor, _gpuProcessor);
            DisplayResult(result, "法線マップ調整");
        }

        private void ExecuteMaskMerge()
        {
            var maskMapping = new MaskMapping(
                rSource ? new MaskSource(rSource, rChannel, rInvert) : null,
                gSource ? new MaskSource(gSource, gChannel, gInvert) : null,
                bSource ? new MaskSource(bSource, bChannel, bInvert) : null,
                aSource ? new MaskSource(aSource, aChannel, aInvert) : null
            );

            var result = MaskMergeService.MergeMasks(
                maskMapping, outputWidth, outputHeight, saveBasePathNoExt,
                _repository, _cpuProcessor, _gpuProcessor);
            DisplayResult(result, "マスク統合");
        }

        #endregion

        #region Helper Methods

        private void DisplayResult<T>(Result<T> result, string operationName)
        {
            if (result.IsOk)
            {
                EditorUtility.DisplayDialog("成功", $"{operationName}が完了しました。", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("エラー", $"{operationName}に失敗しました:\n{result.Error}", "OK");
            }
        }

        #endregion
    }
}
#endif
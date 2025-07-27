#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TextureTools.Infrastructure;
using TextureTools.Infrastructure.Gpu;
using TextureTools.Domain.Services;
using TextureTools.Domain.Model;
using TextureTools.Application.Services;
using TextureTools.Application.Ports;

namespace TextureTools.UI
{
    public class TextureToolsWindow : EditorWindow
    {
        [MenuItem("ツール/テクスチャツール（DDD）")]
        public static void Open() => GetWindow<TextureToolsWindow>("テクスチャツール");

        // 共通状態
        private string saveBasePathNoExt = "Assets/Textures/Result";
        private bool useGpu = true;
        private ComputeShader computeShader;
        private IGpuTextureOps gpuOps;

        // Convert
        private Texture2D convertTarget;
        private bool deleteOriginal = true;

        // Invert
        private Texture2D invTarget;
        private bool invR = true, invG = true, invB = true, invA = true;

        // Normal
        private Texture2D nrmTarget;
        private bool nrmInvR = false, nrmInvG = true;
        private float nrmStrength = 1.0f;

        // Merge
        private Texture2D rSrc, gSrc, bSrc, aSrc;
        private Channel rCh = Channel.R, gCh = Channel.G, bCh = Channel.B, aCh = Channel.A;
        private int outW = 1024, outH = 1024;

        // DI（簡易）
        private UnityTextureRepository repo;
        private TextureProcessor proc;

        private void OnEnable()
        {
            repo = new UnityTextureRepository();
            proc = new TextureProcessor();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("全般", EditorStyles.boldLabel);
            useGpu = EditorGUILayout.ToggleLeft("GPUを使用（対応環境のみ）", useGpu);
            computeShader = (ComputeShader)EditorGUILayout.ObjectField(new GUIContent("コンピュートシェーダ"), computeShader, typeof(ComputeShader), false);
            saveBasePathNoExt = EditorGUILayout.TextField(new GUIContent("保存先ベースパス（拡張子なし）"), saveBasePathNoExt);

            if (useGpu && computeShader)
                gpuOps = new UnityComputeTextureOps(computeShader);
            else
                gpuOps = null;

            EditorGUILayout.Space();
            DrawConvertSection();

            EditorGUILayout.Space();
            DrawInvertSection();

            EditorGUILayout.Space();
            DrawNormalSection();

            EditorGUILayout.Space();
            DrawMergeSection();
        }

        private void DrawConvertSection()
        {
            EditorGUILayout.LabelField("PNGへ変換（必要に応じて元アセット削除）", EditorStyles.boldLabel);
            convertTarget = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("対象テクスチャ"), convertTarget, typeof(Texture2D), false);
            deleteOriginal = EditorGUILayout.ToggleLeft("元のアセットを削除（元に戻せません）", deleteOriginal);
            if (GUILayout.Button("変換"))
            {
                if (convertTarget == null)
                {
                    EditorUtility.DisplayDialog("エラー", "テクスチャが選択されていません。", "OK");
                    return;
                }
                var uc = new ConvertToPngUseCase(repo);
                if (deleteOriginal && !EditorUtility.DisplayDialog("削除の確認",
                    "元のアセットを削除しますか？この操作は元に戻せません。", "削除する", "キャンセル"))
                {
                    return;
                }
                var r = uc.Execute(convertTarget, deleteOriginal);
                if (!r.IsOk) EditorUtility.DisplayDialog("エラー", r.Error, "OK");
            }
        }

        private void DrawInvertSection()
        {
            EditorGUILayout.LabelField("反転（Invert）", EditorStyles.boldLabel);
            invTarget = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("対象テクスチャ"), invTarget, typeof(Texture2D), false);
            using (new EditorGUILayout.HorizontalScope())
            {
                invR = GUILayout.Toggle(invR, "R", "Button");
                invG = GUILayout.Toggle(invG, "G", "Button");
                invB = GUILayout.Toggle(invB, "B", "Button");
                invA = GUILayout.Toggle(invA, "A", "Button");
            }
            if (GUILayout.Button("反転してPNG保存"))
            {
                if (invTarget == null)
                {
                    EditorUtility.DisplayDialog("エラー", "テクスチャが選択されていません。", "OK");
                    return;
                }
                var uc = new InvertTextureUseCase(repo, proc);
                var r = uc.Execute(invTarget, invR, invG, invB, invA, saveBasePathNoExt,
                    (useGpu && gpuOps != null && gpuOps.IsSupported) ? gpuOps : null);
                if (!r.IsOk) EditorUtility.DisplayDialog("エラー", r.Error, "OK");
            }
        }

        private void DrawNormalSection()
        {
            EditorGUILayout.LabelField("法線マップ用ツール", EditorStyles.boldLabel);
            nrmTarget = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("対象テクスチャ（法線マップ）"), nrmTarget, typeof(Texture2D), false);
            using (new EditorGUILayout.HorizontalScope())
            {
                nrmInvR = GUILayout.Toggle(nrmInvR, "Rを反転", "Button");
                nrmInvG = GUILayout.Toggle(nrmInvG, "Gを反転", "Button");
            }
            nrmStrength = EditorGUILayout.Slider("強度", nrmStrength, 0.0f, 2.0f);

            if (GUILayout.Button("適用してPNG保存"))
            {
                if (nrmTarget == null)
                {
                    EditorUtility.DisplayDialog("エラー", "テクスチャが選択されていません。", "OK");
                    return;
                }
                var uc = new AdjustNormalMapUseCase(repo, proc);
                var r = uc.Execute(nrmTarget, nrmInvR, nrmInvG, nrmStrength, saveBasePathNoExt,
                    (useGpu && gpuOps != null && gpuOps.IsSupported) ? gpuOps : null);
                if (!r.IsOk) EditorUtility.DisplayDialog("エラー", r.Error, "OK");
            }
        }

        private void DrawMergeSection()
        {
            EditorGUILayout.LabelField("マスク統合（RGBAパッキング）", EditorStyles.boldLabel);
            rSrc = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Rの入力"), rSrc, typeof(Texture2D), false);
            rCh = (Channel)EditorGUILayout.EnumPopup("Rに割り当てる元チャンネル", rCh);
            gSrc = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Gの入力"), gSrc, typeof(Texture2D), false);
            gCh = (Channel)EditorGUILayout.EnumPopup("Gに割り当てる元チャンネル", gCh);
            bSrc = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Bの入力"), bSrc, typeof(Texture2D), false);
            bCh = (Channel)EditorGUILayout.EnumPopup("Bに割り当てる元チャンネル", bCh);
            aSrc = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Aの入力"), aSrc, typeof(Texture2D), false);
            aCh = (Channel)EditorGUILayout.EnumPopup("Aに割り当てる元チャンネル", aCh);

            outW = EditorGUILayout.IntField("出力幅", outW);
            outH = EditorGUILayout.IntField("出力高さ", outH);

            if (GUILayout.Button("統合してPNG保存"))
            {
                var mapping = new MaskMapping
                {
                    OutR = rSrc ? new MaskSource { Source = rSrc, SourceChannel = rCh } : null,
                    OutG = gSrc ? new MaskSource { Source = gSrc, SourceChannel = gCh } : null,
                    OutB = bSrc ? new MaskSource { Source = bSrc, SourceChannel = bCh } : null,
                    OutA = aSrc ? new MaskSource { Source = aSrc, SourceChannel = aCh } : null,
                };
                var uc = new MergeMasksUseCase(repo, proc);
                var r = uc.Execute(mapping, outW, outH, saveBasePathNoExt,
                    (useGpu && gpuOps != null && gpuOps.IsSupported) ? gpuOps : null);
                if (!r.IsOk) EditorUtility.DisplayDialog("エラー", r.Error, "OK");
            }
        }
    }
}
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BitAssistTools.Application.Services;
using BitAssistTools.Domain.Models.Texture;
using BitAssistTools.Domain.Models.Shared;
using BitAssistTools.Infrastructure.Unity;

namespace BitAssistTools.Application.ViewModels
{
    /// <summary>
    /// テクスチャツールのViewModelクラス
    /// </summary>
    public class TextureToolsViewModel
    {
        #region Events
        public event Action OnStateChanged;
        public event Action<string> OnNotification;
        public event Action<string> OnError;
        
        /// <summary>
        /// UI状態変更を通知
        /// </summary>
        public void NotifyStateChanged()
        {
            OnStateChanged?.Invoke();
        }
        #endregion

        #region Services
        private readonly UnityTextureRepository _repository;
        private readonly UnityTextureProcessingService _cpuProcessor;
        private UnityGpuTextureOperationService _gpuProcessor;
        #endregion

        #region UI State
        public int SelectedTab { get; set; } = 0; // 0=レシピ, 1=詳細
        public int SelectedSection { get; set; } = 0; // 詳細タブの選択セクション

        // Foldout状態
        public bool ConvertFoldout { get; set; } = true;
        public bool InvertFoldout { get; set; } = false;
        public bool NormalFoldout { get; set; } = false;
        public bool MergeFoldout { get; set; } = false;

        // 共通設定
        public GpuMode GpuMode { get; set; } = GpuMode.Auto;
        public ComputeShader ComputeShader { get; set; }
        public SaveMode SaveMode { get; set; } = SaveMode.SameFolderWithSuffix;
        public bool OverwriteOriginal { get; set; } = false;

        // 対象テクスチャ（自動設定対応）
        public Texture2D TargetTexture { get; set; }

        // PNG変換
        public bool DeleteOriginal { get; set; } = false;

        // 反転
        public bool InvertR { get; set; } = true;
        public bool InvertG { get; set; } = true;
        public bool InvertB { get; set; } = true;
        public bool InvertA { get; set; } = true;

        // 法線マップ
        public bool NormalInvertR { get; set; } = false;
        public bool NormalInvertG { get; set; } = true; // デフォルトON（OpenGL/Y）
        public float NormalStrength { get; set; } = 1.0f;

        // マスク統合
        public MaskChannelData[] MaskChannels { get; set; } = new MaskChannelData[4];
        public int OutputWidth { get; set; } = 1024;
        public int OutputHeight { get; set; } = 1024;

        // プレビュー
        public bool ShowPreview { get; set; } = true;
        public bool ShowBeforeAfter { get; set; } = false;
        public PreviewMode PreviewMode { get; set; } = PreviewMode.RGB;
        
        // レシピ選択
        public RecipeType SelectedRecipe { get; set; } = RecipeType.None;
        
        // 詳細タブの機能選択
        public DetailFunction SelectedDetailFunction { get; set; } = DetailFunction.None;
        
        // プレビューテクスチャ（レシピ適用結果）
        public Texture2D PreviewTexture { get; private set; }
        
        // Read/Write設定の自動管理用
        private readonly Dictionary<string, bool> _originalReadableStates = new Dictionary<string, bool>();
        #endregion

        public TextureToolsViewModel()
        {
            _repository = new UnityTextureRepository();
            _cpuProcessor = new UnityTextureProcessingService();
            
            // マスクチャンネルデータ初期化
            for (int i = 0; i < MaskChannels.Length; i++)
            {
                MaskChannels[i] = new MaskChannelData();
            }
            
            LoadPreferences();
        }

        #region Recipe Execution
        
        /// <summary>
        /// 選択されたレシピを実行
        /// </summary>
        public void ExecuteSelectedRecipe()
        {
            switch (SelectedRecipe)
            {
                case RecipeType.PngConversion:
                    ExecutePngConversionRecipe();
                    break;
                case RecipeType.InvertAll:
                    ExecuteInvertAllRecipe();
                    break;
                case RecipeType.NormalGInvert:
                    ExecuteNormalGInvertRecipe();
                    break;
                case RecipeType.NormalStrength15:
                    ExecuteNormalStrength15Recipe();
                    break;
                case RecipeType.ORMMerge:
                    ExecuteORMRecipe();
                    break;
                case RecipeType.MRAMerge:
                    ApplyMRAPreset();
                    ExecuteCustomMaskMerge();
                    break;
                case RecipeType.None:
                default:
                    OnError?.Invoke("レシピが選択されていません。");
                    break;
            }
        }
        
        /// <summary>
        /// 選択されたレシピの説明を取得
        /// </summary>
        public string GetSelectedRecipeDescription()
        {
            switch (SelectedRecipe)
            {
                case RecipeType.PngConversion:
                    return "テクスチャをPNG形式に変換し、元ファイルを削除します。";
                case RecipeType.InvertAll:
                    return "全チャンネル（RGBA）を反転します。";
                case RecipeType.NormalGInvert:
                    return "法線マップのGチャンネルを反転（OpenGL→DirectX）します。";
                case RecipeType.NormalStrength15:
                    return "法線マップの強度を1.5倍に調整します。";
                case RecipeType.ORMMerge:
                    return "ORM形式（AO+Roughness+Metallic）でマスクを統合します。";
                case RecipeType.MRAMerge:
                    return "MRA形式（Metallic+Roughness+AO）でマスクを統合します。";
                case RecipeType.None:
                default:
                    return "レシピを選択してください。";
            }
        }
        
        #endregion
        
        #region Recipe Methods
        
        /// <summary>
        /// PNG変換レシピ実行
        /// </summary>
        public void ExecutePngConversionRecipe()
        {
            if (TargetTexture == null)
            {
                OnError?.Invoke("テクスチャが選択されていません。");
                return;
            }

            var result = TextureConversionService.ConvertToPng(TargetTexture, true, _repository);
            HandleResult(result, "PNG変換");
        }

        /// <summary>
        /// 全チャンネル反転レシピ実行
        /// </summary>
        public void ExecuteInvertAllRecipe()
        {
            if (TargetTexture == null)
            {
                OnError?.Invoke("テクスチャが選択されていません。");
                return;
            }

            var result = TextureInversionService.InvertAllChannels(
                TargetTexture, GetSavePath(), _repository, _cpuProcessor, GetGpuProcessor());
            HandleResult(result, "全チャンネル反転");
        }

        /// <summary>
        /// 法線G反転レシピ実行
        /// </summary>
        public void ExecuteNormalGInvertRecipe()
        {
            if (TargetTexture == null)
            {
                OnError?.Invoke("テクスチャが選択されていません。");
                return;
            }

            var result = NormalMapService.AdjustNormalMap(
                TargetTexture, false, true, 1.0f, GetSavePath(),
                _repository, _cpuProcessor, GetGpuProcessor());
            HandleResult(result, "法線G反転");
        }

        /// <summary>
        /// 法線強度1.5レシピ実行
        /// </summary>
        public void ExecuteNormalStrength15Recipe()
        {
            if (TargetTexture == null)
            {
                OnError?.Invoke("テクスチャが選択されていません。");
                return;
            }

            var result = NormalMapService.AdjustNormalMap(
                TargetTexture, false, true, 1.5f, GetSavePath(),
                _repository, _cpuProcessor, GetGpuProcessor());
            HandleResult(result, "法線強度1.5");
        }

        /// <summary>
        /// ORMマスク統合レシピ実行
        /// </summary>
        public void ExecuteORMRecipe()
        {
            var mapping = CreateORMMapping();
            if (mapping == null)
            {
                OnError?.Invoke("ORM用のテクスチャが設定されていません。");
                return;
            }

            var result = MaskMergeService.MergeMasks(
                mapping, OutputWidth, OutputHeight, GetSavePath(),
                _repository, _cpuProcessor, GetGpuProcessor());
            HandleResult(result, "ORM統合");
        }

        #endregion

        #region Detailed Operation Methods

        /// <summary>
        /// カスタムPNG変換実行
        /// </summary>
        public void ExecuteCustomPngConversion()
        {
            if (TargetTexture == null)
            {
                OnError?.Invoke("テクスチャが選択されていません。");
                return;
            }

            var result = TextureConversionService.ConvertToPng(TargetTexture, DeleteOriginal, _repository);
            HandleResult(result, "PNG変換");
        }

        /// <summary>
        /// カスタム反転実行
        /// </summary>
        public void ExecuteCustomInvert()
        {
            if (TargetTexture == null)
            {
                OnError?.Invoke("テクスチャが選択されていません。");
                return;
            }

            var result = TextureInversionService.InvertTexture(
                TargetTexture, InvertR, InvertG, InvertB, InvertA, GetSavePath(),
                _repository, _cpuProcessor, GetGpuProcessor());
            HandleResult(result, "チャンネル反転");
        }

        /// <summary>
        /// カスタム法線マップ調整実行
        /// </summary>
        public void ExecuteCustomNormalAdjust()
        {
            if (TargetTexture == null)
            {
                OnError?.Invoke("テクスチャが選択されていません。");
                return;
            }

            var result = NormalMapService.AdjustNormalMap(
                TargetTexture, NormalInvertR, NormalInvertG, NormalStrength, GetSavePath(),
                _repository, _cpuProcessor, GetGpuProcessor());
            HandleResult(result, "法線マップ調整");
        }

        /// <summary>
        /// カスタムマスク統合実行
        /// </summary>
        public void ExecuteCustomMaskMerge()
        {
            var mapping = CreateMaskMapping();
            if (mapping == null || !mapping.HasValidSources)
            {
                OnError?.Invoke("有効なマスクソースが設定されていません。");
                return;
            }

            var result = MaskMergeService.MergeMasks(
                mapping, OutputWidth, OutputHeight, GetSavePath(),
                _repository, _cpuProcessor, GetGpuProcessor());
            HandleResult(result, "マスク統合");
        }

        #endregion

        #region Preset Methods

        /// <summary>
        /// ORMプリセット適用
        /// </summary>
        public void ApplyORMPreset()
        {
            // ORM = AO(R), Roughness(G), Metallic(B)
            var channels = MaskChannels;
            channels[0].SourceChannel = Channel.R; // AO -> R
            channels[1].SourceChannel = Channel.G; // Roughness -> G
            channels[2].SourceChannel = Channel.B; // Metallic -> B
            channels[3].Source = null; // A未使用
            
            NotifyStateChanged();
        }

        /// <summary>
        /// MRAプリセット適用
        /// </summary>
        public void ApplyMRAPreset()
        {
            // MRA = Metallic(R), Roughness(G), AO(B)
            var channels = MaskChannels;
            channels[0].SourceChannel = Channel.B; // Metallic -> R
            channels[1].SourceChannel = Channel.G; // Roughness -> G  
            channels[2].SourceChannel = Channel.R; // AO -> B
            channels[3].Source = null; // A未使用
            
            NotifyStateChanged();
        }

        #endregion

        #region Preview Methods

        /// <summary>
        /// 選択されたレシピのプレビューを生成
        /// </summary>
        public void GenerateRecipePreview()
        {
            if (TargetTexture == null)
            {
                PreviewTexture = null;
                NotifyStateChanged();
                return;
            }

            // テクスチャが読み取り可能かチェック、必要に応じて自動設定
            if (!IsTextureReadable(TargetTexture))
            {
                if (!EnsureTextureReadable(TargetTexture))
                {
                    OnNotification?.Invoke($"プレビュー表示するには、テクスチャ '{TargetTexture.name}' のImport Settingsで 'Read/Write Enabled' をオンにしてください。");
                    PreviewTexture = TargetTexture; // 元テクスチャをそのまま表示
                    NotifyStateChanged();
                    return;
                }
            }

            try
            {
                switch (SelectedRecipe)
                {
                    case RecipeType.None:
                        PreviewTexture = null;
                        break;
                    case RecipeType.PngConversion:
                        // PNG変換の場合は元テクスチャをそのまま表示
                        PreviewTexture = TargetTexture;
                        break;
                    case RecipeType.InvertAll:
                        PreviewTexture = GenerateInvertAllPreview();
                        break;
                    case RecipeType.NormalGInvert:
                        PreviewTexture = GenerateNormalGInvertPreview();
                        break;
                    case RecipeType.NormalStrength15:
                        PreviewTexture = GenerateNormalStrengthPreview(1.5f);
                        break;
                    case RecipeType.ORMMerge:
                        PreviewTexture = GenerateORMPreview();
                        break;
                    case RecipeType.MRAMerge:
                        PreviewTexture = GenerateMRAPreview();
                        break;
                }
                
                NotifyStateChanged();
            }
            catch (System.Exception ex)
            {
                OnError?.Invoke($"プレビュー生成に失敗しました: {ex.Message}");
                PreviewTexture = null;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// 選択された詳細機能のプレビューを生成
        /// </summary>
        public void GenerateDetailPreview()
        {
            if (TargetTexture == null)
            {
                PreviewTexture = null;
                NotifyStateChanged();
                return;
            }

            // テクスチャが読み取り可能かチェック、必要に応じて自動設定
            if (!IsTextureReadable(TargetTexture))
            {
                if (!EnsureTextureReadable(TargetTexture))
                {
                    OnNotification?.Invoke($"プレビュー表示するには、テクスチャ '{TargetTexture.name}' のImport Settingsで 'Read/Write Enabled' をオンにしてください。");
                    PreviewTexture = TargetTexture; // 元テクスチャをそのまま表示
                    NotifyStateChanged();
                    return;
                }
            }

            try
            {
                switch (SelectedDetailFunction)
                {
                    case DetailFunction.None:
                        PreviewTexture = null;
                        break;
                    case DetailFunction.PngConvert:
                        // PNG変換の場合は元テクスチャをそのまま表示
                        PreviewTexture = TargetTexture;
                        break;
                    case DetailFunction.ChannelInvert:
                        PreviewTexture = GenerateCustomInvertPreview();
                        break;
                    case DetailFunction.NormalMap:
                        PreviewTexture = GenerateCustomNormalPreview();
                        break;
                    case DetailFunction.MaskMerge:
                        PreviewTexture = GenerateCustomMaskPreview();
                        break;
                }
                
                NotifyStateChanged();
            }
            catch (System.Exception ex)
            {
                OnError?.Invoke($"詳細機能プレビュー生成に失敗しました: {ex.Message}");
                PreviewTexture = null;
                NotifyStateChanged();
            }
        }

        private Texture2D GenerateInvertAllPreview()
        {
            var colors = TargetTexture.GetPixels();
            var previewColors = new Color[colors.Length];
            
            for (int i = 0; i < colors.Length; i++)
            {
                previewColors[i] = new Color(
                    1f - colors[i].r,
                    1f - colors[i].g,
                    1f - colors[i].b,
                    1f - colors[i].a
                );
            }
            
            var preview = new Texture2D(TargetTexture.width, TargetTexture.height);
            preview.SetPixels(previewColors);
            preview.Apply();
            return preview;
        }

        private Texture2D GenerateNormalGInvertPreview()
        {
            var colors = TargetTexture.GetPixels();
            var previewColors = new Color[colors.Length];
            
            for (int i = 0; i < colors.Length; i++)
            {
                previewColors[i] = new Color(
                    colors[i].r,
                    1f - colors[i].g, // G反転
                    colors[i].b,
                    colors[i].a
                );
            }
            
            var preview = new Texture2D(TargetTexture.width, TargetTexture.height);
            preview.SetPixels(previewColors);
            preview.Apply();
            return preview;
        }

        private Texture2D GenerateNormalStrengthPreview(float strength)
        {
            var colors = TargetTexture.GetPixels();
            var previewColors = new Color[colors.Length];
            
            for (int i = 0; i < colors.Length; i++)
            {
                // 法線ベクトルを展開
                var nx = colors[i].r * 2f - 1f;
                var ny = (1f - colors[i].g) * 2f - 1f; // G反転も適用
                var nz = colors[i].b * 2f - 1f;
                
                // XY成分に強度適用
                nx *= strength;
                ny *= strength;
                
                // 正規化
                var length = Mathf.Sqrt(nx * nx + ny * ny + nz * nz);
                if (length > 0f)
                {
                    nx /= length;
                    ny /= length;
                    nz /= length;
                }
                
                // 0-1範囲に戻す
                previewColors[i] = new Color(
                    nx * 0.5f + 0.5f,
                    (1f - ny) * 0.5f + 0.5f, // G反転を戻す
                    nz * 0.5f + 0.5f,
                    colors[i].a
                );
            }
            
            var preview = new Texture2D(TargetTexture.width, TargetTexture.height);
            preview.SetPixels(previewColors);
            preview.Apply();
            return preview;
        }

        private Texture2D GenerateORMPreview()
        {
            // 簡易プレビュー：現在設定されているマスクチャンネルから生成
            if (MaskChannels[0].Source == null && MaskChannels[1].Source == null && MaskChannels[2].Source == null)
            {
                return CreateSolidColorTexture(Color.black, 256, 256);
            }
            
            return CreateMaskPreview();
        }

        private Texture2D GenerateMRAPreview()
        {
            // 簡易プレビュー：現在設定されているマスクチャンネルから生成
            if (MaskChannels[0].Source == null && MaskChannels[1].Source == null && MaskChannels[2].Source == null)
            {
                return CreateSolidColorTexture(Color.black, 256, 256);
            }
            
            return CreateMaskPreview();
        }

        private Texture2D CreateMaskPreview()
        {
            var size = 256;
            var preview = new Texture2D(size, size);
            var colors = new Color[size * size];
            
            for (int i = 0; i < colors.Length; i++)
            {
                var r = GetChannelValue(0, i, size) ?? 0f;
                var g = GetChannelValue(1, i, size) ?? 0f;
                var b = GetChannelValue(2, i, size) ?? 0f;
                var a = GetChannelValue(3, i, size) ?? 1f;
                
                colors[i] = new Color(r, g, b, a);
            }
            
            preview.SetPixels(colors);
            preview.Apply();
            return preview;
        }

        private float? GetChannelValue(int outputChannel, int pixelIndex, int textureSize)
        {
            var maskData = MaskChannels[outputChannel];
            if (maskData.Source == null) return null;
            
            var sourceTexture = maskData.Source;
            
            // テクスチャが読み取り可能かチェック、必要に応じて自動設定
            if (!IsTextureReadable(sourceTexture))
            {
                if (!EnsureTextureReadable(sourceTexture))
                {
                    // 自動設定に失敗した場合はデフォルト値を返す
                    return outputChannel == 3 ? 1f : 0f; // Alpha = 1, RGB = 0
                }
            }
            
            var x = pixelIndex % textureSize;
            var y = pixelIndex / textureSize;
            
            // UV座標に変換してサンプリング
            var u = (float)x / textureSize;
            var v = (float)y / textureSize;
            
            var sourceX = Mathf.RoundToInt(u * sourceTexture.width);
            var sourceY = Mathf.RoundToInt(v * sourceTexture.height);
            
            if (sourceX >= sourceTexture.width) sourceX = sourceTexture.width - 1;
            if (sourceY >= sourceTexture.height) sourceY = sourceTexture.height - 1;
            
            var sourceColor = sourceTexture.GetPixel(sourceX, sourceY);
            float value = 0f;
            
            switch (maskData.SourceChannel)
            {
                case Channel.R: value = sourceColor.r; break;
                case Channel.G: value = sourceColor.g; break;
                case Channel.B: value = sourceColor.b; break;
                case Channel.A: value = sourceColor.a; break;
            }
            
            return maskData.Invert ? 1f - value : value;
        }

        /// <summary>
        /// テクスチャが読み取り可能かチェック
        /// </summary>
        private bool IsTextureReadable(Texture2D texture)
        {
            if (texture == null) return false;
            
            try
            {
                // 読み取り可能かどうかをテスト
                texture.GetPixel(0, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// テクスチャのRead/Write設定を自動管理してプレビューに必要な状態にする
        /// </summary>
        public bool EnsureTextureReadable(Texture2D texture)
        {
            if (texture == null) return false;
            
            var path = GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) return false;
            
            var importer = GetTextureImporter(texture);
            if (importer == null) return false;
            
            // 元の状態を記録（初回のみ）
            if (!_originalReadableStates.ContainsKey(path))
            {
                _originalReadableStates[path] = importer.isReadable;
            }
            
            // 既に読み取り可能な場合は何もしない
            if (importer.isReadable) return true;
            
            // Read/Write Enabledをオンにする
            importer.isReadable = true;
            importer.SaveAndReimport();
            
            OnNotification?.Invoke($"テクスチャ '{texture.name}' を一時的にRead/Write Enabledに設定しました。");
            return true;
        }

        /// <summary>
        /// 指定されたテクスチャのRead/Write設定を元に戻す
        /// </summary>
        public void RestoreTextureReadable(Texture2D texture)
        {
            if (texture == null) return;
            
            var path = GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) return;
            
            // 元の状態が記録されていない場合は何もしない
            if (!_originalReadableStates.ContainsKey(path)) return;
            
            var originalState = _originalReadableStates[path];
            var importer = GetTextureImporter(texture);
            if (importer == null) return;
            
            // 元の状態と現在の状態が異なる場合のみ復元
            if (importer.isReadable != originalState)
            {
                importer.isReadable = originalState;
                importer.SaveAndReimport();
                
                var stateText = originalState ? "Read/Write Enabled" : "Read/Write Disabled";
                OnNotification?.Invoke($"テクスチャ '{texture.name}' の設定を元の状態（{stateText}）に戻しました。");
            }
            
            // 記録を削除
            _originalReadableStates.Remove(path);
        }

        /// <summary>
        /// 全てのテクスチャのRead/Write設定を元に戻す
        /// </summary>
        public void RestoreAllTextureReadable()
        {
            var pathsToRestore = new List<string>(_originalReadableStates.Keys);
            
            foreach (var path in pathsToRestore)
            {
                var texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null)
                {
                    RestoreTextureReadable(texture);
                }
                else
                {
                    // テクスチャが見つからない場合は記録だけ削除
                    _originalReadableStates.Remove(path);
                }
            }
        }

        private string GetAssetPath(Texture2D texture)
        {
            return UnityEditor.AssetDatabase.GetAssetPath(texture);
        }

        private UnityEditor.TextureImporter GetTextureImporter(Texture2D texture)
        {
            var path = GetAssetPath(texture);
            return UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
        }

        /// <summary>
        /// 管理中のテクスチャがあるかチェック
        /// </summary>
        public bool HasManagedTextures()
        {
            return _originalReadableStates.Count > 0;
        }

        /// <summary>
        /// 管理中のテクスチャ情報を取得
        /// </summary>
        public List<string> GetManagedTextureInfo()
        {
            var info = new List<string>();
            foreach (var kvp in _originalReadableStates)
            {
                var texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(kvp.Key);
                var textureName = texture != null ? texture.name : System.IO.Path.GetFileNameWithoutExtension(kvp.Key);
                var originalState = kvp.Value ? "Read/Write Enabled" : "Read/Write Disabled";
                info.Add($"'{textureName}' - 元の状態: {originalState}");
            }
            return info;
        }

        private Texture2D CreateSolidColorTexture(Color color, int width, int height)
        {
            var texture = new Texture2D(width, height);
            var colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }

        private Texture2D GenerateCustomInvertPreview()
        {
            var colors = TargetTexture.GetPixels();
            var previewColors = new Color[colors.Length];
            
            for (int i = 0; i < colors.Length; i++)
            {
                previewColors[i] = new Color(
                    InvertR ? 1f - colors[i].r : colors[i].r,
                    InvertG ? 1f - colors[i].g : colors[i].g,
                    InvertB ? 1f - colors[i].b : colors[i].b,
                    InvertA ? 1f - colors[i].a : colors[i].a
                );
            }
            
            var preview = new Texture2D(TargetTexture.width, TargetTexture.height);
            preview.SetPixels(previewColors);
            preview.Apply();
            return preview;
        }

        private Texture2D GenerateCustomNormalPreview()
        {
            var colors = TargetTexture.GetPixels();
            var previewColors = new Color[colors.Length];
            
            for (int i = 0; i < colors.Length; i++)
            {
                // 法線ベクトルを展開
                var nx = colors[i].r * 2f - 1f;
                var ny = colors[i].g * 2f - 1f;
                var nz = colors[i].b * 2f - 1f;
                
                // 反転適用
                if (NormalInvertR) nx = -nx;
                if (NormalInvertG) ny = -ny;
                
                // 強度適用
                nx *= NormalStrength;
                ny *= NormalStrength;
                
                // 正規化
                var length = Mathf.Sqrt(nx * nx + ny * ny + nz * nz);
                if (length > 0f)
                {
                    nx /= length;
                    ny /= length;
                    nz /= length;
                }
                
                // 0-1範囲に戻す
                previewColors[i] = new Color(
                    nx * 0.5f + 0.5f,
                    ny * 0.5f + 0.5f,
                    nz * 0.5f + 0.5f,
                    colors[i].a
                );
            }
            
            var preview = new Texture2D(TargetTexture.width, TargetTexture.height);
            preview.SetPixels(previewColors);
            preview.Apply();
            return preview;
        }

        private Texture2D GenerateCustomMaskPreview()
        {
            // 現在の設定でマスクプレビューを生成
            return CreateMaskPreview();
        }

        #endregion

        #region Helper Methods

        private UnityGpuTextureOperationService GetGpuProcessor()
        {
            if (GpuMode == GpuMode.CpuOnly) return null;
            if (GpuMode == GpuMode.GpuOnly || (GpuMode == GpuMode.Auto && SystemInfo.supportsComputeShaders))
            {
                if (ComputeShader != null)
                {
                    if (_gpuProcessor == null)
                        _gpuProcessor = new UnityGpuTextureOperationService(ComputeShader);
                    return _gpuProcessor;
                }
            }
            return null;
        }

        private string GetSavePath()
        {
            if (TargetTexture == null) return "Assets/TextureResult";
            
            var assetPath = _repository.GetAssetPath(TargetTexture);
            if (string.IsNullOrEmpty(assetPath)) return "Assets/TextureResult";

            var directory = System.IO.Path.GetDirectoryName(assetPath);
            var fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            
            if (SaveMode == SaveMode.Overwrite)
            {
                return System.IO.Path.Combine(directory, fileName).Replace('\\', '/');
            }
            else
            {
                return System.IO.Path.Combine(directory, fileName + "_proc").Replace('\\', '/');
            }
        }

        private MaskMapping CreateMaskMapping()
        {
            return new MaskMapping(
                MaskChannels[0].Source != null ? new MaskSource(MaskChannels[0].Source, MaskChannels[0].SourceChannel, MaskChannels[0].Invert) : null,
                MaskChannels[1].Source != null ? new MaskSource(MaskChannels[1].Source, MaskChannels[1].SourceChannel, MaskChannels[1].Invert) : null,
                MaskChannels[2].Source != null ? new MaskSource(MaskChannels[2].Source, MaskChannels[2].SourceChannel, MaskChannels[2].Invert) : null,
                MaskChannels[3].Source != null ? new MaskSource(MaskChannels[3].Source, MaskChannels[3].SourceChannel, MaskChannels[3].Invert) : null
            );
        }

        private MaskMapping CreateORMMapping()
        {
            // 簡単な例: 最初の3つのチャンネルがAO, Roughness, Metallicと仮定
            var channels = MaskChannels;
            if (channels[0].Source == null && channels[1].Source == null && channels[2].Source == null)
                return null;

            return new MaskMapping(
                channels[0].Source != null ? new MaskSource(channels[0].Source, Channel.R, false) : null, // AO
                channels[1].Source != null ? new MaskSource(channels[1].Source, Channel.G, false) : null, // Roughness
                channels[2].Source != null ? new MaskSource(channels[2].Source, Channel.B, false) : null, // Metallic
                null // A未使用
            );
        }

        private void HandleResult<T>(Result<T> result, string operationName)
        {
            if (result.IsOk)
            {
                OnNotification?.Invoke($"{operationName}が完了しました。");
            }
            else
            {
                // PNG変換に関するエラーメッセージの特別処理
                if (operationName == "PNG変換" && result.Error.Contains("すでにPNG形式"))
                {
                    OnNotification?.Invoke("既にPNG形式のテクスチャです。変換は不要です。");
                }
                else
                {
                    OnError?.Invoke($"{operationName}に失敗しました: {result.Error}");
                }
            }
        }

        private void LoadPreferences()
        {
            // EditorPrefsから設定を読み込み
            ConvertFoldout = UnityEditor.EditorPrefs.GetBool("TextureTools.ConvertFoldout", true);
            InvertFoldout = UnityEditor.EditorPrefs.GetBool("TextureTools.InvertFoldout", false);
            NormalFoldout = UnityEditor.EditorPrefs.GetBool("TextureTools.NormalFoldout", false);
            MergeFoldout = UnityEditor.EditorPrefs.GetBool("TextureTools.MergeFoldout", false);
            
            GpuMode = (GpuMode)UnityEditor.EditorPrefs.GetInt("TextureTools.GpuMode", (int)GpuMode.Auto);
            SaveMode = (SaveMode)UnityEditor.EditorPrefs.GetInt("TextureTools.SaveMode", (int)SaveMode.SameFolderWithSuffix);
            
            OutputWidth = UnityEditor.EditorPrefs.GetInt("TextureTools.OutputWidth", 1024);
            OutputHeight = UnityEditor.EditorPrefs.GetInt("TextureTools.OutputHeight", 1024);
        }

        public void SavePreferences()
        {
            // EditorPrefsに設定を保存
            UnityEditor.EditorPrefs.SetBool("TextureTools.ConvertFoldout", ConvertFoldout);
            UnityEditor.EditorPrefs.SetBool("TextureTools.InvertFoldout", InvertFoldout);
            UnityEditor.EditorPrefs.SetBool("TextureTools.NormalFoldout", NormalFoldout);
            UnityEditor.EditorPrefs.SetBool("TextureTools.MergeFoldout", MergeFoldout);
            
            UnityEditor.EditorPrefs.SetInt("TextureTools.GpuMode", (int)GpuMode);
            UnityEditor.EditorPrefs.SetInt("TextureTools.SaveMode", (int)SaveMode);
            
            UnityEditor.EditorPrefs.SetInt("TextureTools.OutputWidth", OutputWidth);
            UnityEditor.EditorPrefs.SetInt("TextureTools.OutputHeight", OutputHeight);
            
            // テクスチャのRead/Write設定を元に戻す
            RestoreAllTextureReadable();
        }

        #endregion
    }

    /// <summary>
    /// マスクチャンネルデータ
    /// </summary>
    [System.Serializable]
    public class MaskChannelData
    {
        public Texture2D Source;
        public Channel SourceChannel = Channel.R;
        public bool Invert = false;
    }

    /// <summary>
    /// GPU使用モード
    /// </summary>
    public enum GpuMode
    {
        Auto,     // 自動（推奨）
        GpuOnly,  // GPU固定
        CpuOnly   // CPU固定
    }

    /// <summary>
    /// 保存モード
    /// </summary>
    public enum SaveMode
    {
        SameFolderWithSuffix, // 同フォルダに_procサフィックス
        Overwrite             // 同名上書き
    }

    /// <summary>
    /// プレビューモード
    /// </summary>
    public enum PreviewMode
    {
        RGB,    // RGB表示
        Alpha,  // A表示
        Normal, // 法線ビジュアライズ
        R,      // Rチャンネルのみ
        G,      // Gチャンネルのみ
        B       // Bチャンネルのみ
    }
    
    /// <summary>
    /// レシピタイプ
    /// </summary>
    public enum RecipeType
    {
        None,           // 未選択
        PngConversion,  // PNG変換+元削除
        InvertAll,      // 全チャンネル反転
        NormalGInvert,  // 法線G反転
        NormalStrength15, // 法線強度1.5
        ORMMerge,       // ORM統合
        MRAMerge        // MRA統合
    }
    
    /// <summary>
    /// 詳細タブの機能タイプ
    /// </summary>
    public enum DetailFunction
    {
        None,           // 未選択
        PngConvert,     // PNG変換
        ChannelInvert,  // チャンネル反転
        NormalMap,      // 法線マップ調整
        MaskMerge       // マスク統合
    }
}
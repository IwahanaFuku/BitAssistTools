using System;
using System.Collections.Generic;
using UnityEngine;
using BitAssistTools.Domain.Models.TextureTools;
using BitAssistTools.Domain.Models.Texture;

namespace BitAssistTools.Application.ViewModels.TextureTools
{
    /// <summary>
    /// 詳細機能ViewModel
    /// </summary>
    public class DetailFunctionViewModel
    {
        #region Private Fields
        private readonly Func<UnityEngine.Texture2D> _getCurrentTexture;
        private const bool ENABLE_VERBOSE_LOGGING = false; // 詳細ログの有効/無効
        #endregion

        #region Events
        public event Action OnFunctionChanged;
        public event Action OnSettingsChanged;
        public event Action<string> OnNotification;
        public event Action<string> OnError;
        #endregion

        #region Properties
        private DetailFunction _selectedFunction = DetailFunction.None;
        public DetailFunction SelectedFunction
        {
            get => _selectedFunction;
            set
            {
                if (_selectedFunction != value)
                {
                    _selectedFunction = value;
                    OnFunctionChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// 利用可能な詳細機能一覧
        /// </summary>
        public IReadOnlyList<DetailFunctionType> AvailableFunctions { get; } = new[]
        {
            new DetailFunctionType(DetailFunction.PngConvert, "PNG変換", "PNG形式に変換します"),
            new DetailFunctionType(DetailFunction.ChannelInvert, "チャンネル反転", "指定したチャンネルを反転します"),
            new DetailFunctionType(DetailFunction.NormalMap, "法線マップ編集", "法線マップの反転や強度を調整します"),
            new DetailFunctionType(DetailFunction.MaskMerge, "マスク統合", "複数のマスクを1つのテクスチャに統合します")
        };

        // PNG変換設定
        public bool DeleteOriginal { get; set; } = false;

        // チャンネル反転設定
        public bool InvertR { get; set; } = true;
        public bool InvertG { get; set; } = true;
        public bool InvertB { get; set; } = true;
        public bool InvertA { get; set; } = true;

        // 法線マップ設定
        public bool NormalInvertR { get; set; } = false;
        public bool NormalInvertG { get; set; } = true;
        public float NormalStrength { get; set; } = 1.0f;

        // マスク統合設定
        public MaskChannelData[] MaskChannels { get; } = new MaskChannelData[4];
        public int OutputWidth { get; set; } = 1024;
        public int OutputHeight { get; set; } = 1024;
        public int PreviewSize { get; set; } = 128;
        public string OutputFileName { get; set; } = "mask_merged";
        
        // 出力サイズ参照設定
        public OutputSizeReference OutputSizeReference { get; set; } = OutputSizeReference.Raw;
        
        // ファイル接尾辞設定
        public FileSuffixSettings FileSuffixSettings { get; } = new FileSuffixSettings();
        #endregion

        #region Constructor
        public DetailFunctionViewModel(Func<UnityEngine.Texture2D> getCurrentTexture = null)
        {
            _getCurrentTexture = getCurrentTexture;
            
            // マスクチャンネルデータを初期化
            for (int i = 0; i < MaskChannels.Length; i++)
            {
                MaskChannels[i] = new MaskChannelData
                {
                    Source = null,
                    SourceChannel = (Channel)i,
                    Invert = false
                };
            }
            
            // 保存された設定を読み込み
            LoadSettings();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 設定変更を通知
        /// </summary>
        public void NotifySettingsChanged()
        {
            SaveSettings(); // 設定変更時に自動保存
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// 選択中のテクスチャに基づいてデフォルトファイル名を更新
        /// </summary>
        public void UpdateDefaultFileName()
        {
            var currentTexture = GetCurrentTexture();
            if (currentTexture != null)
            {
                var baseName = System.IO.Path.GetFileNameWithoutExtension(currentTexture.name);
                if (!string.IsNullOrEmpty(baseName))
                {
                    OutputFileName = $"{baseName}_mask_merged";
                }
            }
        }

        /// <summary>
        /// 詳細機能パラメータを取得
        /// </summary>
        public object GetFunctionParameters()
        {
            switch (_selectedFunction)
            {
                case DetailFunction.ChannelInvert:
                    return new ChannelInvertParameters
                    {
                        InvertR = InvertR,
                        InvertG = InvertG,
                        InvertB = InvertB,
                        InvertA = InvertA
                    };
                case DetailFunction.NormalMap:
                    return new NormalMapParameters
                    {
                        InvertR = NormalInvertR,
                        InvertG = NormalInvertG,
                        Strength = NormalStrength
                    };
                case DetailFunction.MaskMerge:
                    return this; // DetailFunctionViewModel自体を渡す
                default:
                    return null;
            }
        }


        /// <summary>
        /// 現在選択されているテクスチャを取得
        /// </summary>
        private UnityEngine.Texture2D GetCurrentTexture()
        {
            // 親ViewModelから提供された関数を使用してテクスチャを取得
            return _getCurrentTexture?.Invoke();
        }

        /// <summary>
        /// RMA → Unity Standardプリセットを適用
        /// </summary>
        public void ApplyRMAToUnityPreset()
        {
            // RMAテクスチャをUnity Standardシェーダ用に変換
            // UE4形式RMA: R=AO, G=Roughness, B=Metallic
            // Unity Standard: R=Metallic, G=AO, B=0, Alpha=Smoothness(1-Roughness)
            
            ClearMaskChannels();
            
            // 現在選択されているテクスチャを全チャンネルのソースとして設定
            var currentTexture = GetCurrentTexture();
            if (currentTexture != null)
            {
                // R出力 = Metallic (元のBチャンネル)
                MaskChannels[0].Source = currentTexture;
                MaskChannels[0].SourceChannel = Channel.B;
                MaskChannels[0].Invert = false;
                
                // G出力 = AO (元のRチャンネル)
                MaskChannels[1].Source = currentTexture;
                MaskChannels[1].SourceChannel = Channel.R;
                MaskChannels[1].Invert = false;
                
                // B出力 = 0 (使用しない)
                MaskChannels[2].Source = null;
                MaskChannels[2].SourceChannel = Channel.B;
                MaskChannels[2].Invert = false;
                
                // Alpha出力 = Smoothness (元のGチャンネルを反転)
                MaskChannels[3].Source = currentTexture;
                MaskChannels[3].SourceChannel = Channel.G;
                MaskChannels[3].Invert = true; // Roughness → Smoothness
            }
            
            OnNotification?.Invoke("RMA → Unity Standardプリセットを適用しました");
            NotifySettingsChanged();
        }

        /// <summary>
        /// マスクチャンネルをクリア
        /// </summary>
        public void ClearMaskChannels()
        {
            for (int i = 0; i < MaskChannels.Length; i++)
            {
                MaskChannels[i].Source = null;
                MaskChannels[i].SourceChannel = (Channel)i;
                MaskChannels[i].Invert = false;
            }
            NotifySettingsChanged();
        }

        /// <summary>
        /// 選択中の詳細機能を実行
        /// </summary>
        public void ExecuteSelectedFunction()
        {
            if (_selectedFunction == DetailFunction.None)
            {
                OnError?.Invoke("詳細機能が選択されていません");
                return;
            }

            var sourceTexture = GetCurrentTexture();
            if (sourceTexture == null)
            {
                OnError?.Invoke("対象テクスチャが選択されていません");
                return;
            }

            try
            {
                // 出力サイズ参照設定に基づいてテクスチャサイズを調整
                UnityEngine.Texture2D adjustedTexture = null;
                try
                {
                    adjustedTexture = GetAdjustedSizeTexture(sourceTexture);
                    if (adjustedTexture == null)
                    {
                        OnError?.Invoke("テクスチャサイズの調整に失敗しました");
                        return;
                    }
                }
                catch (Exception adjustEx)
                {
                    OnError?.Invoke($"テクスチャサイズ調整エラー: {adjustEx.Message}");
                    return;
                }
                
                // TexturePreviewServiceを使用して詳細機能を実行
                var parameters = GetFunctionParameters();
                var resultTexture = Services.TextureTools.TexturePreviewService.GenerateDetailPreview(adjustedTexture, _selectedFunction, parameters);
                
                if (resultTexture != null)
                {
                    // 結果テクスチャをファイルとして保存
                    SaveResultTexture(resultTexture, sourceTexture);
                    OnNotification?.Invoke($"詳細機能「{_selectedFunction}」を実行しました");
                }
                else
                {
                    OnError?.Invoke("詳細機能の実行に失敗しました");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"詳細機能実行に失敗しました: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 出力サイズ参照設定に基づいて調整されたテクスチャを取得
        /// </summary>
        private UnityEngine.Texture2D GetAdjustedSizeTexture(UnityEngine.Texture2D sourceTexture)
        {
            if (sourceTexture == null)
            {
                UnityEngine.Debug.LogError("[DetailFunctionViewModel] sourceTextureがnullです");
                return null;
            }

            // 全ての機能で出力サイズ参照設定を適用
            try
            {
                if (OutputSizeReference == OutputSizeReference.Raw)
                {
                    // Raw: 元画像サイズにリサイズしたテクスチャを作成
                    var rawTexture = GetRawSizeTexture(sourceTexture);
                    if (rawTexture == null)
                    {
                        UnityEngine.Debug.LogWarning("[DetailFunctionViewModel] Raw画像サイズの取得に失敗、元テクスチャを使用");
                        return sourceTexture;
                    }
                    return rawTexture;
                }
                else
                {
                    // Resize: Unityでリサイズされたサイズをそのまま使用
                    return sourceTexture;
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[DetailFunctionViewModel] GetAdjustedSizeTexture エラー: {ex.Message}");
                return sourceTexture; // フォールバック
            }
        }

        /// <summary>
        /// 元画像サイズにリサイズしたテクスチャを取得
        /// </summary>
        private UnityEngine.Texture2D GetRawSizeTexture(UnityEngine.Texture2D sourceTexture)
        {
            try
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(sourceTexture);
                if (string.IsNullOrEmpty(path)) return sourceTexture;

                // ファクトリーを使用して元画像サイズを取得
                var originalSize = Infrastructure.Unity.ImageParsers.ImageSizeParserFactory.ParseImageSize(path);
                if (!originalSize.HasValue) return sourceTexture;

                // 現在のサイズと同じ場合はそのまま返す
                if (originalSize.Value.width == sourceTexture.width && originalSize.Value.height == sourceTexture.height)
                {
                    return sourceTexture;
                }

                // 元サイズにリサイズしたテクスチャを作成
                return CreateResizedTexture(sourceTexture, originalSize.Value.width, originalSize.Value.height);
            }
            catch (System.Exception)
            {
                // エラー時は元のテクスチャをそのまま返す
                return sourceTexture;
            }
        }

        /// <summary>
        /// 指定サイズにリサイズしたテクスチャを作成
        /// </summary>
        private UnityEngine.Texture2D CreateResizedTexture(UnityEngine.Texture2D sourceTexture, int targetWidth, int targetHeight)
        {
            if (sourceTexture == null)
            {
                UnityEngine.Debug.LogError("[DetailFunctionViewModel] CreateResizedTexture: sourceTextureがnullです");
                return null;
            }

            if (targetWidth <= 0 || targetHeight <= 0)
            {
                UnityEngine.Debug.LogError($"[DetailFunctionViewModel] CreateResizedTexture: 無効なサイズ {targetWidth}x{targetHeight}");
                return null;
            }

            UnityEngine.RenderTexture renderTexture = null;
            var oldRT = UnityEngine.RenderTexture.active;
            
            try
            {
                renderTexture = UnityEngine.RenderTexture.GetTemporary(targetWidth, targetHeight, 0, UnityEngine.RenderTextureFormat.ARGB32);
                if (renderTexture == null)
                {
                    UnityEngine.Debug.LogError("[DetailFunctionViewModel] CreateResizedTexture: RenderTextureの作成に失敗");
                    return null;
                }

                UnityEngine.RenderTexture.active = renderTexture;
                UnityEngine.Graphics.Blit(sourceTexture, renderTexture);
                
                var resizedTexture = new UnityEngine.Texture2D(targetWidth, targetHeight, UnityEngine.TextureFormat.RGBA32, false);
                resizedTexture.ReadPixels(new UnityEngine.Rect(0, 0, targetWidth, targetHeight), 0, 0);
                resizedTexture.Apply();
                
                return resizedTexture;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[DetailFunctionViewModel] CreateResizedTexture エラー: {ex.Message}");
                return null;
            }
            finally
            {
                UnityEngine.RenderTexture.active = oldRT;
                if (renderTexture != null)
                {
                    UnityEngine.RenderTexture.ReleaseTemporary(renderTexture);
                }
            }
        }

        /// <summary>
        /// 結果テクスチャをファイルとして保存
        /// </summary>
        private void SaveResultTexture(UnityEngine.Texture2D resultTexture, UnityEngine.Texture2D sourceTexture)
        {
            if (resultTexture == null)
            {
                throw new System.ArgumentNullException(nameof(resultTexture), "結果テクスチャがnullです");
            }

            if (sourceTexture == null)
            {
                throw new System.ArgumentNullException(nameof(sourceTexture), "ソーステクスチャがnullです");
            }

            var sourcePath = UnityEditor.AssetDatabase.GetAssetPath(sourceTexture);
            if (string.IsNullOrEmpty(sourcePath))
            {
                throw new InvalidOperationException("ソーステクスチャのパスが取得できません");
            }

            var directory = System.IO.Path.GetDirectoryName(sourcePath);
            if (string.IsNullOrEmpty(directory))
            {
                throw new InvalidOperationException("出力ディレクトリが取得できません");
            }

            string outputFileName;
            
            // マスク統合の場合は設定されたファイル名を使用
            if (_selectedFunction == DetailFunction.MaskMerge)
            {
                outputFileName = string.IsNullOrEmpty(OutputFileName) ? "mask_merged" : OutputFileName;
            }
            else
            {
                var fileName = System.IO.Path.GetFileNameWithoutExtension(sourcePath);
                var functionName = GetFunctionSuffix(_selectedFunction);
                
                // 接尾辞が空白の場合はアンダースコアも付けない
                if (string.IsNullOrEmpty(functionName))
                {
                    outputFileName = fileName;
                }
                else
                {
                    outputFileName = $"{fileName}_{functionName}";
                }
            }
            
            var outputPath = System.IO.Path.Combine(directory, $"{outputFileName}.png");

            // PNG形式で保存 - Unity 2019.4対応
            byte[] pngData = null;
            bool needsFallback = false;
            
            // まずテクスチャが読み取り可能かチェック
            if (!IsTextureReadable(resultTexture))
            {
                if (ENABLE_VERBOSE_LOGGING)
                {
                    if (IsCompressedFormat(resultTexture.format))
                    {
                        UnityEngine.Debug.Log($"[DetailFunctionViewModel] 圧縮テクスチャフォーマット ({resultTexture.format}) のため読み取り可能テクスチャで処理");
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"[DetailFunctionViewModel] テクスチャが読み取り不可能 ({resultTexture.format})、読み取り可能テクスチャで処理");
                    }
                }
                needsFallback = true;
            }
            
            if (!needsFallback)
            {
                try
                {
                    pngData = resultTexture.EncodeToPNG();
                    if (pngData == null || pngData.Length == 0)
                    {
                        if (ENABLE_VERBOSE_LOGGING)
                        {
                            UnityEngine.Debug.Log($"[DetailFunctionViewModel] 直接エンコードが無効なデータを返しました、読み取り可能テクスチャで再試行");
                        }
                        needsFallback = true;
                    }
                    else
                    {
                        System.IO.File.WriteAllBytes(outputPath, pngData);
                    }
                }
                catch (System.Exception ex)
                {
                    if (ENABLE_VERBOSE_LOGGING)
                    {
                        UnityEngine.Debug.Log($"[DetailFunctionViewModel] 直接エンコードに失敗: {ex.Message}、読み取り可能テクスチャで再試行");
                    }
                    needsFallback = true;
                }
            }
            
            // フォールバック処理：読み取り可能テクスチャを作成してエンコード
            if (needsFallback)
            {
                var readableTexture = CreateReadableTexture(resultTexture);
                if (readableTexture == null)
                {
                    throw new InvalidOperationException("読み取り可能テクスチャの作成に失敗しました");
                }

                try
                {
                    pngData = readableTexture.EncodeToPNG();
                    if (pngData == null || pngData.Length == 0)
                    {
                        throw new InvalidOperationException("読み取り可能テクスチャのエンコードに失敗しました");
                    }
                    System.IO.File.WriteAllBytes(outputPath, pngData);
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(readableTexture);
                }
            }
            
            // Unityにアセットを再インポートさせる
            UnityEditor.AssetDatabase.ImportAsset(outputPath);
            UnityEditor.AssetDatabase.Refresh();
        }

        /// <summary>
        /// テクスチャが読み取り可能かチェック
        /// </summary>
        private bool IsTextureReadable(UnityEngine.Texture2D texture)
        {
            if (texture == null) return false;
            
            // 圧縮フォーマットかチェック
            if (IsCompressedFormat(texture.format))
            {
                return false;
            }
            
            try
            {
                // GetPixel(0, 0)を試してみて例外が発生しなければ読み取り可能
                texture.GetPixel(0, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 圧縮テクスチャフォーマットかチェック
        /// </summary>
        private bool IsCompressedFormat(UnityEngine.TextureFormat format)
        {
            switch (format)
            {
                // DXT圧縮
                case UnityEngine.TextureFormat.DXT1:
                case UnityEngine.TextureFormat.DXT5:
                // ETC圧縮
                case UnityEngine.TextureFormat.ETC_RGB4:
                case UnityEngine.TextureFormat.ETC2_RGB:
                case UnityEngine.TextureFormat.ETC2_RGBA1:
                case UnityEngine.TextureFormat.ETC2_RGBA8:
                case UnityEngine.TextureFormat.EAC_R:
                case UnityEngine.TextureFormat.EAC_R_SIGNED:
                case UnityEngine.TextureFormat.EAC_RG:
                case UnityEngine.TextureFormat.EAC_RG_SIGNED:
                // ASTC圧縮
                case UnityEngine.TextureFormat.ASTC_4x4:
                case UnityEngine.TextureFormat.ASTC_5x5:
                case UnityEngine.TextureFormat.ASTC_6x6:
                case UnityEngine.TextureFormat.ASTC_8x8:
                case UnityEngine.TextureFormat.ASTC_10x10:
                case UnityEngine.TextureFormat.ASTC_12x12:
                // PVRTC圧縮
                case UnityEngine.TextureFormat.PVRTC_RGB2:
                case UnityEngine.TextureFormat.PVRTC_RGBA2:
                case UnityEngine.TextureFormat.PVRTC_RGB4:
                case UnityEngine.TextureFormat.PVRTC_RGBA4:
#if UNITY_2020_1_OR_NEWER
                // BC圧縮（Unity 2020.1以降で利用可能）
                case UnityEngine.TextureFormat.BC4:
                case UnityEngine.TextureFormat.BC5:
                case UnityEngine.TextureFormat.BC6H:
                case UnityEngine.TextureFormat.BC7:
#endif
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 読み取り可能なテクスチャを作成
        /// </summary>
        private UnityEngine.Texture2D CreateReadableTexture(UnityEngine.Texture2D source)
        {
            if (source == null)
            {
                UnityEngine.Debug.LogError("[DetailFunctionViewModel] CreateReadableTexture: sourceがnullです");
                return null;
            }

            UnityEngine.RenderTexture renderTexture = null;
            var oldRT = UnityEngine.RenderTexture.active;
            
            try
            {
                renderTexture = UnityEngine.RenderTexture.GetTemporary(source.width, source.height, 0, UnityEngine.RenderTextureFormat.ARGB32);
                if (renderTexture == null)
                {
                    UnityEngine.Debug.LogError("[DetailFunctionViewModel] CreateReadableTexture: RenderTextureの取得に失敗");
                    return null;
                }

                UnityEngine.RenderTexture.active = renderTexture;
                UnityEngine.Graphics.Blit(source, renderTexture);
                
                // RGBA32フォーマットで確実に読み取り可能なテクスチャを作成
                var readableTexture = new UnityEngine.Texture2D(source.width, source.height, UnityEngine.TextureFormat.RGBA32, false);
                readableTexture.ReadPixels(new UnityEngine.Rect(0, 0, source.width, source.height), 0, 0);
                readableTexture.Apply();
                
                // 作成したテクスチャが実際に読み取り可能かテスト
                try
                {
                    readableTexture.GetPixel(0, 0);
                }
                catch (System.Exception testEx)
                {
                    UnityEngine.Debug.LogError($"[DetailFunctionViewModel] CreateReadableTexture: 作成したテクスチャが読み取り不可能: {testEx.Message}");
                    UnityEngine.Object.DestroyImmediate(readableTexture);
                    return null;
                }
                
                return readableTexture;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[DetailFunctionViewModel] CreateReadableTexture エラー: {ex.Message}");
                return null;
            }
            finally
            {
                UnityEngine.RenderTexture.active = oldRT;
                if (renderTexture != null)
                {
                    UnityEngine.RenderTexture.ReleaseTemporary(renderTexture);
                }
            }
        }

        /// <summary>
        /// 詳細機能タイプに応じたファイル名サフィックスを取得
        /// </summary>
        private string GetFunctionSuffix(DetailFunction functionType)
        {
            return FileSuffixSettings.GetSuffix(functionType);
        }

        /// <summary>
        /// 設定を保存
        /// </summary>
        private void SaveSettings()
        {
            Infrastructure.Unity.TextureToolsSettingsManager.SavePreviewSize(PreviewSize);
            Infrastructure.Unity.TextureToolsSettingsManager.SaveOutputSize(OutputWidth, OutputHeight);
            Infrastructure.Unity.TextureToolsSettingsManager.SaveOutputFileName(OutputFileName);
            Infrastructure.Unity.TextureToolsSettingsManager.SaveOutputSizeReference(OutputSizeReference);
            Infrastructure.Unity.TextureToolsSettingsManager.SaveFileSuffixSettings(FileSuffixSettings);
        }

        /// <summary>
        /// 設定を読み込み
        /// </summary>
        private void LoadSettings()
        {
            PreviewSize = Infrastructure.Unity.TextureToolsSettingsManager.LoadPreviewSize(PreviewSize);
            OutputWidth = Infrastructure.Unity.TextureToolsSettingsManager.LoadOutputWidth(OutputWidth);
            OutputHeight = Infrastructure.Unity.TextureToolsSettingsManager.LoadOutputHeight(OutputHeight);
            OutputFileName = Infrastructure.Unity.TextureToolsSettingsManager.LoadOutputFileName(OutputFileName);
            OutputSizeReference = Infrastructure.Unity.TextureToolsSettingsManager.LoadOutputSizeReference(OutputSizeReference);
            
            // ファイル接尾辞設定を読み込み
            var loadedSuffixSettings = Infrastructure.Unity.TextureToolsSettingsManager.LoadFileSuffixSettings();
            FileSuffixSettings.PngConvertSuffix = loadedSuffixSettings.PngConvertSuffix;
            FileSuffixSettings.ChannelInvertSuffix = loadedSuffixSettings.ChannelInvertSuffix;
            FileSuffixSettings.NormalMapSuffix = loadedSuffixSettings.NormalMapSuffix;
            FileSuffixSettings.MaskMergeSuffix = loadedSuffixSettings.MaskMergeSuffix;
        }
        #endregion
    }

    /// <summary>
    /// マスクチャンネルデータ
    /// </summary>
    public class MaskChannelData
    {
        public UnityEngine.Texture2D Source;
        public Channel SourceChannel = Channel.R;
        public bool Invert = false;
    }

    /// <summary>
    /// チャンネル反転パラメータ
    /// </summary>
    public class ChannelInvertParameters
    {
        public bool InvertR { get; set; }
        public bool InvertG { get; set; }
        public bool InvertB { get; set; }
        public bool InvertA { get; set; }
    }

    /// <summary>
    /// 法線マップパラメータ
    /// </summary>
    public class NormalMapParameters
    {
        public bool InvertR { get; set; }
        public bool InvertG { get; set; }
        public float Strength { get; set; }
    }
}
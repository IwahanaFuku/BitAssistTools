using System;
using System.Collections.Generic;
using BitAssistTools.Domain.Models.TextureTools;
using BitAssistTools.Domain.Models.Texture;

namespace BitAssistTools.Application.ViewModels.TextureTools
{
    /// <summary>
    /// 詳細機能ViewModel
    /// </summary>
    public class DetailFunctionViewModel
    {
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
            new DetailFunctionType(DetailFunction.NormalMap, "法線マップ調整", "法線マップの反転や強度を調整します"),
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
        #endregion

        #region Constructor
        public DetailFunctionViewModel()
        {
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
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 設定変更を通知
        /// </summary>
        public void NotifySettingsChanged()
        {
            OnSettingsChanged?.Invoke();
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
            // Selection.activeObjectから現在選択されているテクスチャを取得
            return UnityEditor.Selection.activeObject as UnityEngine.Texture2D;
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

            try
            {
                // TODO: 実際の詳細機能実行処理を実装
                OnNotification?.Invoke($"詳細機能を実行しました");
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"詳細機能実行に失敗しました: {ex.Message}");
            }
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
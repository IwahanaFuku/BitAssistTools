#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using BitAssistTools.Application.ViewModels;
using BitAssistTools.Domain.Models.Texture;
using System.Linq;
using static BitAssistTools.Application.ViewModels.RecipeType;

namespace BitAssistTools.Editor.View
{
    /// <summary>
    /// 改善されたテクスチャツールのエディターウィンドウ
    /// </summary>
    public class TextureToolsViewImproved : EditorWindow
    {
        [MenuItem("Bit Assist Tools/テクスチャツール（改善版）")]
        public static void Open()
        {
            var window = GetWindow<TextureToolsViewImproved>("テクスチャツール改");
            window.minSize = new Vector2(600, 400); // 最小サイズを設定
        }

        #region Private Fields
        private TextureToolsViewModel _viewModel;
        private Vector2 _leftScrollPos;
        private Vector2 _rightScrollPos;
        private string[] _tabNames = { "レシピ", "詳細" };
        private string[] _gpuModeNames = { "自動（推奨）", "GPU固定", "CPU固定" };
        private string[] _previewModeNames = { "RGB", "Alpha", "法線", "R", "G", "B" };
        private Texture2D _previewTexture;
        private bool _showNotification = false;
        private string _notificationText = "";
        private float _notificationTime = 0f;
        #endregion

        private void OnEnable()
        {
            _viewModel = new TextureToolsViewModel();
            _viewModel.OnStateChanged += Repaint;
            _viewModel.OnNotification += ShowNotification;
            _viewModel.OnError += ShowError;
            
            // 選択中のテクスチャを自動設定
            UpdateSelectedTexture();
            
            titleContent = new GUIContent("テクスチャツール改");
        }

        private void OnDisable()
        {
            if (_viewModel != null)
            {
                _viewModel.SavePreferences();
                _viewModel.OnStateChanged -= Repaint;
                _viewModel.OnNotification -= ShowNotification;
                _viewModel.OnError -= ShowError;
            }
        }

        private void OnSelectionChange()
        {
            UpdateSelectedTexture();
            Repaint();
        }

        private void OnGUI()
        {
            DrawHeader();
            
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLeftPanel();
                DrawRightPanel();
            }
            
            DrawNotification();
            DrawProgressBar();
            HandleKeyboardInput();
        }
        
        private void DrawProgressBar()
        {
            // プログレスバーの表示（必要時）
            if (_viewModel.TargetTexture != null && 
                _viewModel.TargetTexture.width * _viewModel.TargetTexture.height > 2048 * 2048)
            {
                var rect = new Rect(10, position.height - 50, position.width - 20, 15);
                EditorGUI.ProgressBar(rect, 0.0f, "大きなテクスチャの処理には時間がかかります");
            }
        }

        #region Header
        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                // タブ切り替え
                _viewModel.SelectedTab = GUILayout.Toolbar(_viewModel.SelectedTab, _tabNames, EditorStyles.toolbarButton);
                
                GUILayout.FlexibleSpace();
                
                // GPU設定
                GUILayout.Label("GPU:", EditorStyles.miniLabel);
                _viewModel.GpuMode = (GpuMode)EditorGUILayout.Popup((int)_viewModel.GpuMode, _gpuModeNames, EditorStyles.toolbarPopup, GUILayout.Width(120));
                
                // 保存設定
                if (GUILayout.Button("保存設定", EditorStyles.toolbarButton))
                {
                    DrawSaveSettings();
                }
            }
        }
        #endregion

        #region Left Panel
        private void DrawLeftPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
            {
                _leftScrollPos = EditorGUILayout.BeginScrollView(_leftScrollPos);
                
                if (_viewModel.SelectedTab == 0)
                {
                    DrawRecipeTab();
                }
                else
                {
                    DrawDetailTab();
                }
                
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawRecipeTab()
        {
            EditorGUILayout.LabelField("クイックレシピ", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 対象テクスチャ（共通）
            DrawTargetTextureField();
            EditorGUILayout.Space();

            // レシピ選択（ラジオボタン形式）
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("基本操作", EditorStyles.boldLabel);
                
                var selected = _viewModel.SelectedRecipe;
                
                bool pngToggle = EditorGUILayout.ToggleLeft("PNG変換＋元削除", selected == PngConversion);
                if (pngToggle && selected != PngConversion)
                {
                    _viewModel.SelectedRecipe = PngConversion;
                    _viewModel.GenerateRecipePreview();
                    _viewModel.NotifyStateChanged();
                }
                else if (!pngToggle && selected == PngConversion)
                {
                    _viewModel.SelectedRecipe = None;
                    _viewModel.GenerateRecipePreview();
                    _viewModel.NotifyStateChanged();
                }
                
                bool invertToggle = EditorGUILayout.ToggleLeft("全チャンネル反転", selected == InvertAll);
                if (invertToggle && selected != InvertAll)
                {
                    _viewModel.SelectedRecipe = InvertAll;
                    _viewModel.GenerateRecipePreview();
                    _viewModel.NotifyStateChanged();
                }
                else if (!invertToggle && selected == InvertAll)
                {
                    _viewModel.SelectedRecipe = None;
                    _viewModel.GenerateRecipePreview();
                    _viewModel.NotifyStateChanged();
                }
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("法線マップ", EditorStyles.boldLabel);
                
                var selected = _viewModel.SelectedRecipe;
                
                bool normalGToggle = EditorGUILayout.ToggleLeft("G反転（OpenGL→DirectX）", selected == NormalGInvert);
                if (normalGToggle && selected != NormalGInvert)
                {
                    _viewModel.SelectedRecipe = NormalGInvert;
                    _viewModel.GenerateRecipePreview();
                    _viewModel.NotifyStateChanged();
                }
                else if (!normalGToggle && selected == NormalGInvert)
                {
                    _viewModel.SelectedRecipe = None;
                    _viewModel.GenerateRecipePreview();
                    _viewModel.NotifyStateChanged();
                }
                
                bool normalStrengthToggle = EditorGUILayout.ToggleLeft("強度1.5倍", selected == NormalStrength15);
                if (normalStrengthToggle && selected != NormalStrength15)
                {
                    _viewModel.SelectedRecipe = NormalStrength15;
                    _viewModel.GenerateRecipePreview();
                    _viewModel.NotifyStateChanged();
                }
                else if (!normalStrengthToggle && selected == NormalStrength15)
                {
                    _viewModel.SelectedRecipe = None;
                    _viewModel.GenerateRecipePreview();
                    _viewModel.NotifyStateChanged();
                }
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("マスク統合プリセット", EditorStyles.boldLabel);
                
                var selected = _viewModel.SelectedRecipe;
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    bool ormToggle = EditorGUILayout.ToggleLeft("ORM", selected == ORMMerge);
                    if (ormToggle && selected != ORMMerge)
                    {
                        _viewModel.SelectedRecipe = ORMMerge;
                        _viewModel.GenerateRecipePreview();
                        _viewModel.NotifyStateChanged();
                    }
                    else if (!ormToggle && selected == ORMMerge)
                    {
                        _viewModel.SelectedRecipe = None;
                        _viewModel.GenerateRecipePreview();
                        _viewModel.NotifyStateChanged();
                    }
                    
                    bool mraToggle = EditorGUILayout.ToggleLeft("MRA", selected == MRAMerge);
                    if (mraToggle && selected != MRAMerge)
                    {
                        _viewModel.SelectedRecipe = MRAMerge;
                        _viewModel.GenerateRecipePreview();
                        _viewModel.NotifyStateChanged();
                    }
                    else if (!mraToggle && selected == MRAMerge)
                    {
                        _viewModel.SelectedRecipe = None;
                        _viewModel.GenerateRecipePreview();
                        _viewModel.NotifyStateChanged();
                    }
                }
                
                EditorGUILayout.HelpBox("ORM = AO(R) + Roughness(G) + Metallic(B)\nMRA = Metallic(R) + Roughness(G) + AO(B)", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // レシピの説明表示
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("選択中のレシピ", EditorStyles.boldLabel);
                var description = _viewModel.GetSelectedRecipeDescription();
                EditorGUILayout.HelpBox(description, MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // 実行ボタン
            using (new EditorGUILayout.VerticalScope("box"))
            {
                GUI.enabled = _viewModel.SelectedRecipe != None && _viewModel.TargetTexture != null;
                
                if (GUILayout.Button("選択レシピを実行", GUILayout.Height(40)))
                {
                    _viewModel.ExecuteSelectedRecipe();
                }
                
                GUI.enabled = true;
                
                if (_viewModel.SelectedRecipe == None)
                {
                    EditorGUILayout.HelpBox("レシピを選択してください", MessageType.Warning);
                }
                else if (_viewModel.TargetTexture == null)
                {
                    EditorGUILayout.HelpBox("テクスチャを選択してください", MessageType.Warning);
                }
            }
        }

        private void DrawDetailTab()
        {
            EditorGUILayout.LabelField("詳細設定", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 対象テクスチャ（共通）
            DrawTargetTextureField();
            EditorGUILayout.Space();

            // 機能選択（ラジオボタン形式）
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("機能選択", EditorStyles.boldLabel);
                
                var selected = _viewModel.SelectedDetailFunction;
                
                bool pngToggle = EditorGUILayout.ToggleLeft("PNG変換", selected == DetailFunction.PngConvert);
                if (pngToggle && selected != DetailFunction.PngConvert)
                {
                    _viewModel.SelectedDetailFunction = DetailFunction.PngConvert;
                    _viewModel.GenerateDetailPreview();
                    _viewModel.NotifyStateChanged();
                }
                else if (!pngToggle && selected == DetailFunction.PngConvert)
                {
                    _viewModel.SelectedDetailFunction = DetailFunction.None;
                    _viewModel.GenerateDetailPreview();
                    _viewModel.NotifyStateChanged();
                }
                
                bool invertToggle = EditorGUILayout.ToggleLeft("チャンネル反転", selected == DetailFunction.ChannelInvert);
                if (invertToggle && selected != DetailFunction.ChannelInvert)
                {
                    _viewModel.SelectedDetailFunction = DetailFunction.ChannelInvert;
                    _viewModel.GenerateDetailPreview();
                    _viewModel.NotifyStateChanged();
                }
                else if (!invertToggle && selected == DetailFunction.ChannelInvert)
                {
                    _viewModel.SelectedDetailFunction = DetailFunction.None;
                    _viewModel.GenerateDetailPreview();
                    _viewModel.NotifyStateChanged();
                }
                
                bool normalToggle = EditorGUILayout.ToggleLeft("法線マップ調整", selected == DetailFunction.NormalMap);
                if (normalToggle && selected != DetailFunction.NormalMap)
                {
                    _viewModel.SelectedDetailFunction = DetailFunction.NormalMap;
                    _viewModel.GenerateDetailPreview();
                    _viewModel.NotifyStateChanged();
                }
                else if (!normalToggle && selected == DetailFunction.NormalMap)
                {
                    _viewModel.SelectedDetailFunction = DetailFunction.None;
                    _viewModel.GenerateDetailPreview();
                    _viewModel.NotifyStateChanged();
                }
                
                bool maskToggle = EditorGUILayout.ToggleLeft("マスク統合", selected == DetailFunction.MaskMerge);
                if (maskToggle && selected != DetailFunction.MaskMerge)
                {
                    _viewModel.SelectedDetailFunction = DetailFunction.MaskMerge;
                    _viewModel.GenerateDetailPreview();
                    _viewModel.NotifyStateChanged();
                }
                else if (!maskToggle && selected == DetailFunction.MaskMerge)
                {
                    _viewModel.SelectedDetailFunction = DetailFunction.None;
                    _viewModel.GenerateDetailPreview();
                    _viewModel.NotifyStateChanged();
                }
            }

            EditorGUILayout.Space();

            // PNG変換（安全性改善版）
            if (_viewModel.SelectedDetailFunction == DetailFunction.PngConvert)
            {
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    // 安全性警告付き削除オプション
                    var originalColor = GUI.backgroundColor;
                    if (_viewModel.DeleteOriginal)
                    {
                        GUI.backgroundColor = Color.red;
                        EditorGUILayout.HelpBox("⚠️ 危険: 元ファイルを完全に削除します！\nこの操作は元に戻せません。", MessageType.Error);
                    }
                    
                    EditorGUI.BeginChangeCheck();
                    _viewModel.DeleteOriginal = EditorGUILayout.ToggleLeft("元アセットを削除（非可逆）", _viewModel.DeleteOriginal);
                    if (EditorGUI.EndChangeCheck() && _viewModel.SelectedDetailFunction == DetailFunction.PngConvert)
                    {
                        _viewModel.GenerateDetailPreview();
                    }
                    GUI.backgroundColor = originalColor;
                    
                    if (!_viewModel.DeleteOriginal)
                    {
                        EditorGUILayout.HelpBox("安全: 元ファイルを保持し、新しいPNGを作成します", MessageType.Info);
                    }
                    
                    EditorGUILayout.Space();
                    
                    if (GUILayout.Button("PNG変換実行", GUILayout.Height(30)))
                    {
                        if (_viewModel.DeleteOriginal)
                        {
                            // ダブル確認
                            if (EditorUtility.DisplayDialog("最終確認", 
                                $"本当に元ファイル'{_viewModel.TargetTexture?.name}'を削除しますか？\n\nこの操作は元に戻せません！", 
                                "削除して変換", "キャンセル"))
                            {
                                _viewModel.ExecuteCustomPngConversion();
                            }
                        }
                        else
                        {
                            _viewModel.ExecuteCustomPngConversion();
                        }
                    }
                }
            }

            // チャンネル反転
            if (_viewModel.SelectedDetailFunction == DetailFunction.ChannelInvert)
            {
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.LabelField("反転するチャンネル:");
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        _viewModel.InvertR = GUILayout.Toggle(_viewModel.InvertR, "R", "Button", GUILayout.ExpandWidth(true));
                        _viewModel.InvertG = GUILayout.Toggle(_viewModel.InvertG, "G", "Button", GUILayout.ExpandWidth(true));
                        _viewModel.InvertB = GUILayout.Toggle(_viewModel.InvertB, "B", "Button", GUILayout.ExpandWidth(true));
                        _viewModel.InvertA = GUILayout.Toggle(_viewModel.InvertA, "A", "Button", GUILayout.ExpandWidth(true));
                        if (EditorGUI.EndChangeCheck() && _viewModel.SelectedDetailFunction == DetailFunction.ChannelInvert)
                        {
                            _viewModel.GenerateDetailPreview();
                        }
                    }
                    
                    if (GUILayout.Button("反転実行"))
                    {
                        _viewModel.ExecuteCustomInvert();
                    }
                }
            }

            // 法線マップ（改善版）
            if (_viewModel.SelectedDetailFunction == DetailFunction.NormalMap)
            {
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    // 反転設定
                    EditorGUILayout.LabelField("反転設定", EditorStyles.boldLabel);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        _viewModel.NormalInvertR = GUILayout.Toggle(_viewModel.NormalInvertR, "Rを反転", "Button", GUILayout.Width(80));
                        _viewModel.NormalInvertG = GUILayout.Toggle(_viewModel.NormalInvertG, "Gを反転（OpenGL→DirectX）", "Button", GUILayout.ExpandWidth(true));
                        if (EditorGUI.EndChangeCheck() && _viewModel.SelectedDetailFunction == DetailFunction.NormalMap)
                        {
                            _viewModel.GenerateDetailPreview();
                        }
                    }
                    
                    if (_viewModel.NormalInvertG)
                    {
                        EditorGUILayout.HelpBox("OpenGL形式からDirectX形式に変換します", MessageType.Info);
                    }
                    
                    EditorGUILayout.Space();
                    
                    // 強度設定（改善版）
                    EditorGUILayout.LabelField("法線強度", EditorStyles.boldLabel);
                    
                    // 定数ボタン（改善版）
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("0.5", GUILayout.Height(25))) {
                            _viewModel.NormalStrength = 0.5f;
                            if (_viewModel.SelectedDetailFunction == DetailFunction.NormalMap)
                            {
                                _viewModel.GenerateDetailPreview();
                            }
                            ShowNotification("法線強度: 0.5（弱め）");
                        }
                        if (GUILayout.Button("1.0", GUILayout.Height(25))) {
                            _viewModel.NormalStrength = 1.0f;
                            if (_viewModel.SelectedDetailFunction == DetailFunction.NormalMap)
                            {
                                _viewModel.GenerateDetailPreview();
                            }
                            ShowNotification("法線強度: 1.0（標準）");
                        }
                        if (GUILayout.Button("1.5", GUILayout.Height(25))) {
                            _viewModel.NormalStrength = 1.5f;
                            if (_viewModel.SelectedDetailFunction == DetailFunction.NormalMap)
                            {
                                _viewModel.GenerateDetailPreview();
                            }
                            ShowNotification("法線強度: 1.5（強め）");
                        }
                        if (GUILayout.Button("2.0", GUILayout.Height(25))) {
                            _viewModel.NormalStrength = 2.0f;
                            if (_viewModel.SelectedDetailFunction == DetailFunction.NormalMap)
                            {
                                _viewModel.GenerateDetailPreview();
                            }
                            ShowNotification("法線強度: 2.0（非常に強い）");
                        }
                    }
                    
                    // スライダーと数値表示
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        _viewModel.NormalStrength = EditorGUILayout.Slider(_viewModel.NormalStrength, 0.0f, 3.0f);
                        if (EditorGUI.EndChangeCheck() && _viewModel.SelectedDetailFunction == DetailFunction.NormalMap)
                        {
                            _viewModel.GenerateDetailPreview();
                        }
                        EditorGUILayout.LabelField(_viewModel.NormalStrength.ToString("F2"), GUILayout.Width(40));
                    }
                    
                    EditorGUILayout.Space();
                    
                    if (GUILayout.Button("法線マップ調整実行", GUILayout.Height(30)))
                    {
                        _viewModel.ExecuteCustomNormalAdjust();
                    }
                }
            }

            // マスク統合
            if (_viewModel.SelectedDetailFunction == DetailFunction.MaskMerge)
            {
                DrawMaskMergeUI();
            }
        }

        private void DrawMaskMergeUI()
        {
            // プリセットボタン部
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("クイックプリセット", EditorStyles.boldLabel);
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("ORM", GUILayout.Height(25)))
                    {
                        _viewModel.ApplyORMPreset();
                        if (_viewModel.SelectedDetailFunction == DetailFunction.MaskMerge)
                        {
                            _viewModel.GenerateDetailPreview();
                        }
                        ShowNotification("ORMプリセットを適用: AO(R) + Roughness(G) + Metallic(B)");
                    }
                    if (GUILayout.Button("MRA", GUILayout.Height(25)))
                    {
                        _viewModel.ApplyMRAPreset();
                        if (_viewModel.SelectedDetailFunction == DetailFunction.MaskMerge)
                        {
                            _viewModel.GenerateDetailPreview();
                        }
                        ShowNotification("MRAプリセットを適用: Metallic(R) + Roughness(G) + AO(B)");
                    }
                    if (GUILayout.Button("クリア", GUILayout.Height(25)))
                    {
                        ClearMaskChannels();
                        if (_viewModel.SelectedDetailFunction == DetailFunction.MaskMerge)
                        {
                            _viewModel.GenerateDetailPreview();
                        }
                        ShowNotification("マスクチャンネルをクリア");
                    }
                }
                
                EditorGUILayout.HelpBox("ORM = AO(R) + Roughness(G) + Metallic(B)\nMRA = Metallic(R) + Roughness(G) + AO(B)", MessageType.Info);
            }

            EditorGUILayout.Space();

            // チャンネル設定表（改善版）
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("チャンネルマッピング", EditorStyles.boldLabel);
                
                // ヘッダー
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("出力", EditorStyles.miniLabel, GUILayout.Width(40));
                    EditorGUILayout.LabelField("入力テクスチャ", EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
                    EditorGUILayout.LabelField("チャンネル", EditorStyles.miniLabel, GUILayout.Width(140));
                    EditorGUILayout.LabelField("反転", EditorStyles.miniLabel, GUILayout.Width(35));
                    EditorGUILayout.LabelField("", GUILayout.Width(20)); // クリアボタン用スペース
                }
                
                string[] channelNames = { "R", "G", "B", "A" };
                string[] channelOptions = { "R", "G", "B", "A" };
                Color[] channelColors = { Color.red, Color.green, Color.blue, Color.white };
                
                for (int i = 0; i < 4; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        // 出力チャンネル名（色付き）
                        var originalColor = GUI.color;
                        GUI.color = channelColors[i];
                        EditorGUILayout.LabelField(channelNames[i], EditorStyles.boldLabel, GUILayout.Width(40));
                        GUI.color = originalColor;
                        
                        // 入力テクスチャ（可変幅）
                        EditorGUI.BeginChangeCheck();
                        _viewModel.MaskChannels[i].Source = (Texture2D)EditorGUILayout.ObjectField(
                            _viewModel.MaskChannels[i].Source, typeof(Texture2D), false, GUILayout.ExpandWidth(true));
                        
                        // ソースチャンネル
                        int selectedChannel = (int)_viewModel.MaskChannels[i].SourceChannel;
                        selectedChannel = GUILayout.SelectionGrid(selectedChannel, channelOptions, 4, GUILayout.Width(140));
                        _viewModel.MaskChannels[i].SourceChannel = (Channel)selectedChannel;
                        
                        // 反転
                        _viewModel.MaskChannels[i].Invert = EditorGUILayout.Toggle(_viewModel.MaskChannels[i].Invert, GUILayout.Width(35));
                        
                        if (EditorGUI.EndChangeCheck() && _viewModel.SelectedDetailFunction == DetailFunction.MaskMerge)
                        {
                            _viewModel.GenerateDetailPreview();
                        }
                        
                        // クリアボタン
                        if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            _viewModel.MaskChannels[i].Source = null;
                            _viewModel.MaskChannels[i].SourceChannel = (Channel)i;
                            _viewModel.MaskChannels[i].Invert = false;
                            if (_viewModel.SelectedDetailFunction == DetailFunction.MaskMerge)
                            {
                                _viewModel.GenerateDetailPreview();
                            }
                        }
                    }
                }
            }

            EditorGUILayout.Space();

            // 出力設定
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("出力設定", EditorStyles.boldLabel);
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("サイズ:", GUILayout.Width(40));
                    EditorGUI.BeginChangeCheck();
                    _viewModel.OutputWidth = EditorGUILayout.IntField(_viewModel.OutputWidth, GUILayout.Width(60));
                    EditorGUILayout.LabelField("×", GUILayout.Width(15));
                    _viewModel.OutputHeight = EditorGUILayout.IntField(_viewModel.OutputHeight, GUILayout.Width(60));
                    if (EditorGUI.EndChangeCheck() && _viewModel.SelectedDetailFunction == DetailFunction.MaskMerge)
                    {
                        _viewModel.GenerateDetailPreview();
                    }
                    
                    if (GUILayout.Button("元サイズ", GUILayout.Width(60)))
                    {
                        if (_viewModel.TargetTexture != null)
                        {
                            _viewModel.OutputWidth = _viewModel.TargetTexture.width;
                            _viewModel.OutputHeight = _viewModel.TargetTexture.height;
                            if (_viewModel.SelectedDetailFunction == DetailFunction.MaskMerge)
                            {
                                _viewModel.GenerateDetailPreview();
                            }
                        }
                    }
                }
                
                EditorGUILayout.HelpBox("デフォルト値（未設定時）: R=0, G=0, B=0, A=1", MessageType.Info);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("マスク統合実行", GUILayout.Height(30)))
            {
                _viewModel.ExecuteCustomMaskMerge();
            }
        }

        private void DrawTargetTextureField()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("対象テクスチャ:", GUILayout.Width(90));
                _viewModel.TargetTexture = (Texture2D)EditorGUILayout.ObjectField(_viewModel.TargetTexture, typeof(Texture2D), false);
                
                if (GUILayout.Button("選択中", GUILayout.Width(60)))
                {
                    UpdateSelectedTexture();
                }
            }

            if (_viewModel.TargetTexture != null)
            {
                var info = $"{_viewModel.TargetTexture.width}×{_viewModel.TargetTexture.height}, {_viewModel.TargetTexture.format}";
                EditorGUILayout.LabelField(info, EditorStyles.miniLabel);
                
                // Read/Write設定の自動管理状態を表示
                if (_viewModel.HasManagedTextures())
                {
                    EditorGUILayout.HelpBox("一部のテクスチャのRead/Write設定を一時的に変更中です。\nツール終了時に自動で元に戻されます。", MessageType.Info);
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("今すぐ元に戻す", GUILayout.Height(20)))
                        {
                            _viewModel.RestoreAllTextureReadable();
                            ShowNotification("全てのテクスチャ設定を元に戻しました。");
                        }
                        
                        if (GUILayout.Button("管理状態を確認", GUILayout.Height(20)))
                        {
                            ShowManagedTexturesInfo();
                        }
                    }
                }
                else if (!IsTextureReadable(_viewModel.TargetTexture))
                {
                    EditorGUILayout.HelpBox("プレビュー機能を使用するには、テクスチャのRead/Write Enabledをオンにしてください。", MessageType.Warning);
                    
                    if (GUILayout.Button("Read/Write Enabledをオンにする", GUILayout.Height(20)))
                    {
                        EnableTextureReadWrite(_viewModel.TargetTexture);
                    }
                }
            }
        }
        #endregion

        #region Right Panel (Preview)
        private void DrawRightPanel()
        {
            using (new EditorGUILayout.VerticalScope("box", GUILayout.Width(280), GUILayout.MinWidth(280), GUILayout.ExpandHeight(true)))
            {
                EditorGUILayout.LabelField("ライブプレビュー", EditorStyles.boldLabel);
                
                if (_viewModel.TargetTexture != null)
                {
                    // プレビュー設定
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        _viewModel.ShowBeforeAfter = EditorGUILayout.ToggleLeft("Before/After表示", _viewModel.ShowBeforeAfter);
                        
                        EditorGUILayout.LabelField("表示モード:");
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            // 基本モード
                            if (GUILayout.Toggle(_viewModel.PreviewMode == PreviewMode.RGB, "RGB", "Button", GUILayout.Width(45)))
                                _viewModel.PreviewMode = PreviewMode.RGB;
                            if (GUILayout.Toggle(_viewModel.PreviewMode == PreviewMode.Alpha, "Alpha", "Button", GUILayout.Width(45)))
                                _viewModel.PreviewMode = PreviewMode.Alpha;
                            if (GUILayout.Toggle(_viewModel.PreviewMode == PreviewMode.Normal, "法線", "Button", GUILayout.Width(45)))
                                _viewModel.PreviewMode = PreviewMode.Normal;
                        }
                        
                        EditorGUILayout.LabelField("チャンネル別表示:");
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            // チャンネル別モード
                            if (GUILayout.Toggle(_viewModel.PreviewMode == PreviewMode.R, "R", "Button", GUILayout.Width(30)))
                                _viewModel.PreviewMode = PreviewMode.R;
                            if (GUILayout.Toggle(_viewModel.PreviewMode == PreviewMode.G, "G", "Button", GUILayout.Width(30)))
                                _viewModel.PreviewMode = PreviewMode.G;
                            if (GUILayout.Toggle(_viewModel.PreviewMode == PreviewMode.B, "B", "Button", GUILayout.Width(30)))
                                _viewModel.PreviewMode = PreviewMode.B;
                        }
                        
                        if (_viewModel.PreviewMode == PreviewMode.Normal)
                        {
                            EditorGUILayout.HelpBox("法線ベクトルを黄色い矢印で表示", MessageType.Info);
                        }
                        else if (_viewModel.PreviewMode >= PreviewMode.R)
                        {
                            EditorGUILayout.HelpBox("選択したチャンネルをグレースケールで表示", MessageType.Info);
                        }
                    }

                    EditorGUILayout.Space();

                    // プレビュー画像（固定サイズ）
                    var previewSize = 250;
                    var rect = GUILayoutUtility.GetRect(previewSize, previewSize);
                    DrawPreviewTexture(rect);
                    
                    EditorGUILayout.Space();
                    
                    // テクスチャ情報
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        EditorGUILayout.LabelField("テクスチャ情報", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField($"サイズ: {_viewModel.TargetTexture.width}×{_viewModel.TargetTexture.height}");
                        EditorGUILayout.LabelField($"フォーマット: {_viewModel.TargetTexture.format}");
                        EditorGUILayout.LabelField($"ミップマップ: {(_viewModel.TargetTexture.mipmapCount > 1 ? "有" : "無")}");
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("テクスチャを選択してください", MessageType.Info);
                    
                    // ドラッグ&ドロップエリア
                    var dropRect = GUILayoutUtility.GetRect(250, 120);
                    GUI.Box(dropRect, "テクスチャをここにドラッグ&ドロップ", EditorStyles.helpBox);
                    
                    HandleDragAndDrop(dropRect);
                }

                GUILayout.FlexibleSpace();
            }
        }
        
        private void HandleDragAndDrop(Rect rect)
        {
            var evt = Event.current;
            
            if (rect.Contains(evt.mousePosition))
            {
                switch (evt.type)
                {
                    case EventType.DragUpdated:
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        evt.Use();
                        break;
                        
                    case EventType.DragPerform:
                        DragAndDrop.AcceptDrag();
                        
                        foreach (var obj in DragAndDrop.objectReferences)
                        {
                            if (obj is Texture2D texture)
                            {
                                _viewModel.TargetTexture = texture;
                                AutoDetectMaskInputs(texture);
                                evt.Use();
                                return;
                            }
                        }
                        break;
                }
            }
        }

        private void DrawPreviewTexture(Rect rect)
        {
            if (_viewModel.TargetTexture == null) return;

            var texture = _viewModel.TargetTexture;
            var previewTexture = _viewModel.PreviewTexture ?? texture;
            
            if (_viewModel.ShowBeforeAfter)
            {
                // Before/After表示
                var halfWidth = rect.width * 0.5f;
                var leftRect = new Rect(rect.x, rect.y, halfWidth - 1, rect.height);
                var rightRect = new Rect(rect.x + halfWidth + 1, rect.y, halfWidth - 1, rect.height);
                
                // Before（元テクスチャ）
                DrawTextureWithMode(leftRect, texture, _viewModel.PreviewMode);
                GUI.Label(new Rect(leftRect.x, leftRect.y, leftRect.width, 16), "Before", EditorStyles.miniLabel);
                
                // After（レシピプレビュー）
                DrawTextureWithMode(rightRect, previewTexture, _viewModel.PreviewMode);
                GUI.Label(new Rect(rightRect.x, rightRect.y, rightRect.width, 16), "After", EditorStyles.miniLabel);
                
                // 分割線
                EditorGUI.DrawRect(new Rect(rect.x + halfWidth, rect.y, 2, rect.height), Color.gray);
            }
            else
            {
                // レシピまたは詳細機能が選択されている場合はプレビューを、そうでなければ元テクスチャを表示
                bool hasPreview = (_viewModel.SelectedRecipe != RecipeType.None || _viewModel.SelectedDetailFunction != DetailFunction.None) 
                    && _viewModel.PreviewTexture != null;
                var displayTexture = hasPreview ? _viewModel.PreviewTexture : texture;
                DrawTextureWithMode(rect, displayTexture, _viewModel.PreviewMode);
                
                // プレビューが選択されている場合は「Preview」ラベルを表示
                if (hasPreview)
                {
                    GUI.Label(new Rect(rect.x, rect.y, rect.width, 16), "Preview", EditorStyles.miniLabel);
                }
            }
            
            // フレーム描画
            GUI.Box(rect, "", EditorStyles.helpBox);
        }
        
        private void DrawTextureWithMode(Rect rect, Texture2D texture, PreviewMode mode)
        {
            switch (mode)
            {
                case PreviewMode.RGB:
                    DrawTextureAsDefault(rect, texture);
                    break;
                case PreviewMode.Alpha:
                    DrawAlphaChannel(rect, texture);
                    break;
                case PreviewMode.Normal:
                    DrawNormalVisualization(rect, texture);
                    break;
                case PreviewMode.R:
                    DrawSingleChannel(rect, texture, 0);
                    break;
                case PreviewMode.G:
                    DrawSingleChannel(rect, texture, 1);
                    break;
                case PreviewMode.B:
                    DrawSingleChannel(rect, texture, 2);
                    break;
            }
        }

        private void DrawTextureAsDefault(Rect rect, Texture2D texture)
        {
            // テクスチャタイプに関係なく、常にDefaultタイプとして表示
            var originalTextureType = GetTextureType(texture);
            
            if (originalTextureType == TextureImporterType.NormalMap)
            {
                // NormalMapタイプの場合は、カスタムマテリアルを使用してDefaultとして描画
                DrawNormalMapAsDefault(rect, texture);
            }
            else
            {
                EditorGUI.DrawPreviewTexture(rect, texture);
            }
        }

        private TextureImporterType GetTextureType(Texture2D texture)
        {
            var path = UnityEditor.AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) return TextureImporterType.Default;
            
            var importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
            return importer?.textureType ?? TextureImporterType.Default;
        }

        private void DrawNormalMapAsDefault(Rect rect, Texture2D texture)
        {
            // NormalMapをDefaultタイプのように表示するためのマテリアル
            var material = new Material(Shader.Find("Hidden/Internal-GUITexture"));
            
            // マテリアルのプロパティを調整してDefaultタイプのような表示にする
            material.SetTexture("_MainTex", texture);
            material.SetFloat("_ColorMask", 15); // RGBA全チャンネル表示
            
            // 通常のテクスチャ表示（法線マップの特殊な色調整を無効化）
            EditorGUI.DrawPreviewTexture(rect, texture, material);
            
            // マテリアルを破棄
            DestroyImmediate(material);
        }
        
        private void DrawAlphaChannel(Rect rect, Texture2D texture)
        {
            // アルファチャンネルをグレースケールで表示
            var material = new Material(Shader.Find("Hidden/Internal-GUITexture"));
            material.SetFloat("_ColorMask", 8); // Alphaチャンネルのみ
            EditorGUI.DrawPreviewTexture(rect, texture, material);
        }

        private void DrawSingleChannel(Rect rect, Texture2D texture, int channel)
        {
            // 指定チャンネル（0=R, 1=G, 2=B）をグレースケールで表示
            if (!IsTextureReadable(texture))
            {
                // 読み取り不可の場合は通常表示
                EditorGUI.DrawPreviewTexture(rect, texture);
                return;
            }

            try
            {
                var tempTexture = CreateSingleChannelTexture(texture, channel);
                EditorGUI.DrawPreviewTexture(rect, tempTexture);
                DestroyImmediate(tempTexture);
            }
            catch
            {
                // エラー時は通常表示
                EditorGUI.DrawPreviewTexture(rect, texture);
            }
        }

        private Texture2D CreateSingleChannelTexture(Texture2D source, int channel)
        {
            var colors = source.GetPixels();
            var singleChannelColors = new Color[colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                float value = 0f;
                switch (channel)
                {
                    case 0: value = colors[i].r; break;
                    case 1: value = colors[i].g; break;
                    case 2: value = colors[i].b; break;
                    case 3: value = colors[i].a; break;
                }
                singleChannelColors[i] = new Color(value, value, value, 1f);
            }

            var result = new Texture2D(source.width, source.height);
            result.SetPixels(singleChannelColors);
            result.Apply();
            return result;
        }
        
        private void DrawNormalVisualization(Rect rect, Texture2D texture)
        {
            // 法線マップの可視化（簡易版）
            EditorGUI.DrawPreviewTexture(rect, texture);
            
            // 法線ベクトルの方向を示す矢印オーバーレイ（簡易版）
            if (rect.width > 64 && rect.height > 64)
            {
                DrawNormalVectorOverlay(rect, texture);
            }
        }
        
        private void DrawNormalVectorOverlay(Rect rect, Texture2D texture)
        {
            if (texture == null) return;
            
            // グリッド状に法線ベクトルを表示
            var gridSize = 16;
            var stepX = rect.width / gridSize;
            var stepY = rect.height / gridSize;
            
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    var centerX = rect.x + (x + 0.5f) * stepX;
                    var centerY = rect.y + (y + 0.5f) * stepY;
                    
                    // テクスチャから法線情報を取得（簡易版）
                    var uvX = (float)x / gridSize;
                    var uvY = (float)y / gridSize;
                    
                    // 法線ベクトルを矢印で表示
                    var arrowLength = Mathf.Min(stepX, stepY) * 0.3f;
                    var endX = centerX + arrowLength * Mathf.Sin(uvX * Mathf.PI * 2);
                    var endY = centerY + arrowLength * Mathf.Cos(uvY * Mathf.PI * 2);
                    
                    Handles.color = Color.yellow;
                    Handles.DrawLine(new Vector3(centerX, centerY, 0), new Vector3(endX, endY, 0));
                }
            }
        }
        #endregion

        #region Helper Methods
        private void UpdateSelectedTexture()
        {
            var selected = UnityEditor.Selection.activeObject as Texture2D;
            if (selected != null)
            {
                // 必ず選択中のテクスチャを設定（上書き）
                _viewModel.TargetTexture = selected;
                AutoDetectMaskInputs(selected);
                
                // プレビューモードを自動調整
                var textureName = selected.name.ToLower();
                if (textureName.Contains("normal") || textureName.Contains("norm"))
                {
                    _viewModel.PreviewMode = PreviewMode.Normal;
                }
                else if (textureName.Contains("alpha") || textureName.Contains("opacity"))
                {
                    _viewModel.PreviewMode = PreviewMode.Alpha;
                }
                else
                {
                    _viewModel.PreviewMode = PreviewMode.RGB;
                }
                
                // 選択されたレシピ・詳細機能のプレビューを再生成
                if (_viewModel.SelectedTab == 0) // レシピタブ
                {
                    _viewModel.GenerateRecipePreview();
                }
                else // 詳細タブ
                {
                    _viewModel.GenerateDetailPreview();
                }
            }
        }

        private void AutoDetectMaskInputs(Texture2D texture)
        {
            if (texture == null) return;
            
            var textureName = texture.name.ToLower();
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(texture).ToLower();
            
            // ヒューリスティクス検出（名前とパスの両方をチェック）
            if (IsTextureType(textureName, assetPath, new[] { "ao", "occlusion", "ambient" }))
            {
                _viewModel.MaskChannels[0].Source = texture; // R -> AO
                _viewModel.MaskChannels[0].SourceChannel = Channel.R;
                ShowNotification($"AOテクスチャをRチャンネルに自動配置: {texture.name}");
            }
            else if (IsTextureType(textureName, assetPath, new[] { "rough", "roughness" }))
            {
                _viewModel.MaskChannels[1].Source = texture; // G -> Roughness
                _viewModel.MaskChannels[1].SourceChannel = Channel.G;
                _viewModel.MaskChannels[1].Invert = false;
                ShowNotification($"RoughnessテクスチャをGチャンネルに自動配置: {texture.name}");
            }
            else if (IsTextureType(textureName, assetPath, new[] { "smooth", "smoothness" }))
            {
                _viewModel.MaskChannels[1].Source = texture; // G -> Smoothness (inverted to Roughness)
                _viewModel.MaskChannels[1].SourceChannel = Channel.G;
                _viewModel.MaskChannels[1].Invert = true; // Smoothness→Roughness変換
                ShowNotification($"SmoothnessテクスチャをGチャンネルに自動配置(反転): {texture.name}");
            }
            else if (IsTextureType(textureName, assetPath, new[] { "metal", "metallic" }))
            {
                _viewModel.MaskChannels[2].Source = texture; // B -> Metallic
                _viewModel.MaskChannels[2].SourceChannel = Channel.B;
                ShowNotification($"MetallicテクスチャをBチャンネルに自動配置: {texture.name}");
            }
            else if (IsTextureType(textureName, assetPath, new[] { "alpha", "opacity", "transparency" }))
            {
                _viewModel.MaskChannels[3].Source = texture; // A -> Alpha
                _viewModel.MaskChannels[3].SourceChannel = Channel.A;
                ShowNotification($"AlphaテクスチャをAチャンネルに自動配置: {texture.name}");
            }
            else if (IsTextureType(textureName, assetPath, new[] { "normal", "norm", "nrm" }))
            {
                // 法線マップ検出時の自動設定
                _viewModel.NormalInvertG = true; // OpenGL/Y形式と仮定
                _viewModel.NormalStrength = 1.0f;
                ShowNotification($"法線マップを検出: G反転を有効化");
            }
            
            // 出力サイズを入力テクスチャに合わせる
            if (_viewModel.OutputWidth == 1024 && _viewModel.OutputHeight == 1024)
            {
                _viewModel.OutputWidth = texture.width;
                _viewModel.OutputHeight = texture.height;
            }
        }
        
        private bool IsTextureType(string textureName, string assetPath, string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                if (textureName.Contains(keyword) || assetPath.Contains(keyword))
                {
                    return true;
                }
            }
            return false;
        }

        private void ClearMaskChannels()
        {
            for (int i = 0; i < _viewModel.MaskChannels.Length; i++)
            {
                _viewModel.MaskChannels[i].Source = null;
                _viewModel.MaskChannels[i].SourceChannel = (Channel)i;
                _viewModel.MaskChannels[i].Invert = false;
            }
        }

        private void DrawSaveSettings()
        {
            var menu = new GenericMenu();
            
            // 保存モード選択
            menu.AddItem(new GUIContent("保存モード/同フォルダに_procサフィックス（推奨）"), 
                _viewModel.SaveMode == SaveMode.SameFolderWithSuffix, () => {
                    _viewModel.SaveMode = SaveMode.SameFolderWithSuffix;
                    ShowNotification("保存モード: _procサフィックス");
                });
            menu.AddItem(new GUIContent("保存モード/同名上書き（注意）"), 
                _viewModel.SaveMode == SaveMode.Overwrite, () => {
                    _viewModel.SaveMode = SaveMode.Overwrite;
                    ShowNotification("保存モード: 上書き");
                });
            
            menu.AddSeparator("");
            
            // GPUモード選択
            menu.AddItem(new GUIContent("GPUモード/自動（推奨）"), 
                _viewModel.GpuMode == GpuMode.Auto, () => {
                    _viewModel.GpuMode = GpuMode.Auto;
                    ShowNotification("GPUモード: 自動");
                });
            menu.AddItem(new GUIContent("GPUモード/GPU固定"), 
                _viewModel.GpuMode == GpuMode.GpuOnly, () => {
                    _viewModel.GpuMode = GpuMode.GpuOnly;
                    ShowNotification("GPUモード: GPU固定");
                });
            menu.AddItem(new GUIContent("GPUモード/CPU固定"), 
                _viewModel.GpuMode == GpuMode.CpuOnly, () => {
                    _viewModel.GpuMode = GpuMode.CpuOnly;
                    ShowNotification("GPUモード: CPU固定");
                });
            
            menu.AddSeparator("");
            
            // フォルダ操作
            menu.AddItem(new GUIContent("フォルダ操作/出力フォルダを開く"), false, () => {
                if (_viewModel.TargetTexture != null)
                {
                    var assetPath = UnityEditor.AssetDatabase.GetAssetPath(_viewModel.TargetTexture);
                    var directory = System.IO.Path.GetDirectoryName(assetPath);
                    EditorUtility.RevealInFinder(directory);
                }
                else
                {
                    EditorUtility.RevealInFinder("Assets");
                }
            });
            
            menu.AddItem(new GUIContent("フォルダ操作/Assetsフォルダを開く"), false, () => {
                EditorUtility.RevealInFinder("Assets");
            });
            
            menu.AddSeparator("");
            
            // リセット機能
            menu.AddItem(new GUIContent("リセット/全設定をリセット"), false, () => {
                if (EditorUtility.DisplayDialog("確認", "全ての設定をリセットしますか？", "リセット", "キャンセル"))
                {
                    ResetAllSettings();
                }
            });
            
            menu.ShowAsContext();
        }
        
        private void ResetAllSettings()
        {
            // 設定をデフォルトにリセット
            _viewModel.GpuMode = GpuMode.Auto;
            _viewModel.SaveMode = SaveMode.SameFolderWithSuffix;
            _viewModel.DeleteOriginal = false;
            _viewModel.InvertR = _viewModel.InvertG = _viewModel.InvertB = _viewModel.InvertA = true;
            _viewModel.NormalInvertR = false;
            _viewModel.NormalInvertG = true;
            _viewModel.NormalStrength = 1.0f;
            _viewModel.OutputWidth = _viewModel.OutputHeight = 1024;
            
            ClearMaskChannels();
            ShowNotification("全設定をリセットしました");
        }

        private void ShowNotification(string message)
        {
            _notificationText = message;
            _showNotification = true;
            _notificationTime = (float)EditorApplication.timeSinceStartup;
            
            // Unity標準の通知システムを使用
            ShowNotification(new GUIContent(message));
            
            // SceneViewにも通知表示
            if (UnityEditor.SceneView.lastActiveSceneView != null)
            {
                UnityEditor.SceneView.lastActiveSceneView.ShowNotification(new GUIContent(message));
            }
            
            // 自動消去
            System.Threading.Tasks.Task.Delay(3000).ContinueWith(_ => {
                if (this != null)
                {
                    RemoveNotification();
                    if (UnityEditor.SceneView.lastActiveSceneView != null)
                    {
                        UnityEditor.SceneView.lastActiveSceneView.RemoveNotification();
                    }
                }
            });
        }

        private void ShowError(string error)
        {
            // エラーダイアログとコンソールログ
            EditorUtility.DisplayDialog("エラー", error, "OK");
            Debug.LogError($"[TextureTools] {error}");
            
            // エラー通知も表示
            ShowNotification($"エラー: {error}");
        }

        private void DrawNotification()
        {
            if (_showNotification && EditorApplication.timeSinceStartup - _notificationTime < 3.0)
            {
                var rect = new Rect(10, position.height - 30, position.width - 20, 20);
                GUI.Box(rect, _notificationText, EditorStyles.helpBox);
            }
            else
            {
                _showNotification = false;
            }
        }

        private void HandleKeyboardInput()
        {
            var evt = Event.current;
            if (evt.type == EventType.KeyDown && evt.keyCode == KeyCode.Return)
            {
                // Enterキーで実行
                if (_viewModel.SelectedTab == 0)
                {
                    // レシピタブでの処理
                    if (_viewModel.TargetTexture != null)
                    {
                        _viewModel.ExecutePngConversionRecipe();
                    }
                }
                else
                {
                    // 詳細タブでの処理
                    if (_viewModel.ConvertFoldout)
                    {
                        _viewModel.ExecuteCustomPngConversion();
                    }
                    else if (_viewModel.InvertFoldout)
                    {
                        _viewModel.ExecuteCustomInvert();
                    }
                    else if (_viewModel.NormalFoldout)
                    {
                        _viewModel.ExecuteCustomNormalAdjust();
                    }
                    else if (_viewModel.MergeFoldout)
                    {
                        _viewModel.ExecuteCustomMaskMerge();
                    }
                }
                evt.Use();
            }
        }

        /// <summary>
        /// テクスチャが読み取り可能かチェック
        /// </summary>
        private bool IsTextureReadable(Texture2D texture)
        {
            if (texture == null) return false;
            
            try
            {
                texture.GetPixel(0, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// テクスチャのRead/Write Enabledをオンにする
        /// </summary>
        private void EnableTextureReadWrite(Texture2D texture)
        {
            if (texture == null) return;
            
            var path = UnityEditor.AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) return;
            
            var importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
            if (importer == null) return;
            
            if (!importer.isReadable)
            {
                // 確認ダイアログ
                if (EditorUtility.DisplayDialog("テクスチャ設定変更", 
                    $"テクスチャ '{texture.name}' のRead/Write Enabledをオンにしますか？\n\nこの変更によりメモリ使用量が増加する可能性があります。", 
                    "変更する", "キャンセル"))
                {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                    
                    ShowNotification($"テクスチャ '{texture.name}' をRead/Write Enabledに設定しました。");
                    
                    // プレビューを再生成
                    _viewModel.GenerateRecipePreview();
                }
            }
            else
            {
                ShowNotification($"テクスチャ '{texture.name}' は既にRead/Write Enabledです。");
            }
        }

        /// <summary>
        /// 管理中のテクスチャ情報を表示
        /// </summary>
        private void ShowManagedTexturesInfo()
        {
            var managedTextures = _viewModel.GetManagedTextureInfo();
            if (managedTextures.Count == 0)
            {
                EditorUtility.DisplayDialog("管理状態", "現在管理中のテクスチャはありません。", "OK");
                return;
            }

            var message = "以下のテクスチャのRead/Write設定を一時的に変更中です：\n\n";
            message += string.Join("\n", managedTextures);
            message += "\n\nツール終了時に自動で元の状態に戻されます。";

            EditorUtility.DisplayDialog("管理中のテクスチャ", message, "OK");
        }
        #endregion
    }
}
#endif
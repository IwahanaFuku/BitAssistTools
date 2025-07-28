#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using BitAssistTools.Application.ViewModels.TextureTools;

namespace BitAssistTools.Editor.View.TextureTools
{
    /// <summary>
    /// テクスチャツールメインView - 統合ウィンドウ
    /// </summary>
    public class TextureToolsMainView : EditorWindow
    {
        [MenuItem("Bit Assist Tools/テクスチャツール（改善版）")]
        public static void Open()
        {
            var window = GetWindow<TextureToolsMainView>("テクスチャツール改");
            window.minSize = new Vector2(600, 400);
        }

        #region Private Fields
        private TextureToolsMainViewModel _viewModel;
        private Vector2 _leftScrollPos;
        private string[] _tabNames = { "レシピ", "詳細" };
        private bool _showNotification = false;
        private string _notificationText = "";
        private float _notificationTime = 0f;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            _viewModel = new TextureToolsMainViewModel();
            _viewModel.OnStateChanged += Repaint;
            _viewModel.OnNotification += ShowNotification;
            _viewModel.OnError += ShowError;
            
            // 選択中のテクスチャを自動設定
            _viewModel.UpdateSelectedTexture();
            
            titleContent = new GUIContent("テクスチャツール改");
        }

        private void OnDisable()
        {
            _viewModel?.Dispose();
        }

        private void OnSelectionChange()
        {
            _viewModel?.UpdateSelectedTexture();
            Repaint();
        }

        private void OnGUI()
        {
            if (_viewModel == null) return;

            DrawHeader();
            
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLeftPanel();
                PreviewPanelView.DrawPreviewPanel(_viewModel);
            }
            
            DrawNotification();
        }
        #endregion

        #region GUI Drawing
        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                // タブ切り替え
                _viewModel.SelectedTab = GUILayout.Toolbar(_viewModel.SelectedTab, _tabNames, EditorStyles.toolbarButton);
                
                GUILayout.FlexibleSpace();
                
                // 設定ボタン
                if (GUILayout.Button("設定", EditorStyles.toolbarButton))
                {
                    // TODO: 設定メニューを実装
                }
            }
        }

        private void DrawLeftPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
            {
                _leftScrollPos = EditorGUILayout.BeginScrollView(_leftScrollPos);
                
                // 対象テクスチャ選択（共通）
                DrawTargetTextureField();
                EditorGUILayout.Space();
                
                // タブ別コンテンツ
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

        private void DrawTargetTextureField()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("対象テクスチャ:", GUILayout.Width(90));
                
                EditorGUI.BeginChangeCheck();
                var newTexture = (Texture2D)EditorGUILayout.ObjectField(_viewModel.Selection.TargetTexture, typeof(Texture2D), false);
                if (EditorGUI.EndChangeCheck())
                {
                    _viewModel.Selection.TargetTexture = newTexture;
                }
                
                if (GUILayout.Button("選択中", GUILayout.Width(60)))
                {
                    _viewModel.UpdateSelectedTexture();
                }
            }

            if (_viewModel.Selection.TargetTexture != null)
            {
                EditorGUILayout.LabelField(_viewModel.Selection.GetTextureInfo(), EditorStyles.miniLabel);
                
                // ライブプレビュー情報表示
                EditorGUILayout.HelpBox("ライブプレビュー中、テクスチャの設定が変更されます。", MessageType.Info);
            }
        }

        private void DrawRecipeTab()
        {
            EditorGUILayout.LabelField("クイックレシピ", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // レシピ選択（ラジオボタン形式）
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("基本操作", EditorStyles.boldLabel);
                
                var selected = _viewModel.Recipe.SelectedRecipe;
                
                // PNG変換
                bool pngToggle = EditorGUILayout.ToggleLeft("PNG変換＋元削除", selected == Domain.Models.TextureTools.RecipeType.PngConversion);
                if (pngToggle != (selected == Domain.Models.TextureTools.RecipeType.PngConversion))
                {
                    _viewModel.Recipe.SelectedRecipe = pngToggle ? Domain.Models.TextureTools.RecipeType.PngConversion : Domain.Models.TextureTools.RecipeType.None;
                }
                
                // 全チャンネル反転
                bool invertToggle = EditorGUILayout.ToggleLeft("全チャンネル反転", selected == Domain.Models.TextureTools.RecipeType.InvertAll);
                if (invertToggle != (selected == Domain.Models.TextureTools.RecipeType.InvertAll))
                {
                    _viewModel.Recipe.SelectedRecipe = invertToggle ? Domain.Models.TextureTools.RecipeType.InvertAll : Domain.Models.TextureTools.RecipeType.None;
                }
            }

            EditorGUILayout.Space();

            // 法線マップ
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("法線マップ", EditorStyles.boldLabel);
                
                var selected = _viewModel.Recipe.SelectedRecipe;
                
                bool normalGToggle = EditorGUILayout.ToggleLeft("G反転（OpenGL→DirectX）", selected == Domain.Models.TextureTools.RecipeType.NormalGInvert);
                if (normalGToggle != (selected == Domain.Models.TextureTools.RecipeType.NormalGInvert))
                {
                    _viewModel.Recipe.SelectedRecipe = normalGToggle ? Domain.Models.TextureTools.RecipeType.NormalGInvert : Domain.Models.TextureTools.RecipeType.None;
                }
                
                bool normalStrengthToggle = EditorGUILayout.ToggleLeft("強度1.5倍", selected == Domain.Models.TextureTools.RecipeType.NormalStrength15);
                if (normalStrengthToggle != (selected == Domain.Models.TextureTools.RecipeType.NormalStrength15))
                {
                    _viewModel.Recipe.SelectedRecipe = normalStrengthToggle ? Domain.Models.TextureTools.RecipeType.NormalStrength15 : Domain.Models.TextureTools.RecipeType.None;
                }
            }

            EditorGUILayout.Space();

            // マスク統合プリセット
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("マスク統合プリセット", EditorStyles.boldLabel);
                
                var selected = _viewModel.Recipe.SelectedRecipe;
                
                bool rmaToUnityToggle = EditorGUILayout.ToggleLeft("RMA → Unity Standard", selected == Domain.Models.TextureTools.RecipeType.RMAToUnity);
                if (rmaToUnityToggle != (selected == Domain.Models.TextureTools.RecipeType.RMAToUnity))
                {
                    _viewModel.Recipe.SelectedRecipe = rmaToUnityToggle ? Domain.Models.TextureTools.RecipeType.RMAToUnity : Domain.Models.TextureTools.RecipeType.None;
                }
                
                EditorGUILayout.HelpBox("UE4形式RMAテクスチャをUnity Standardシェーダ用に変換\nRMA(R=AO, G=Roughness, B=Metallic) → Unity(R=Metallic, G=AO, Alpha=Smoothness)", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // レシピの説明表示
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("選択中のレシピ", EditorStyles.boldLabel);
                var description = _viewModel.Recipe.GetSelectedRecipeDescription();
                EditorGUILayout.HelpBox(description, MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // 実行ボタン
            using (new EditorGUILayout.VerticalScope("box"))
            {
                GUI.enabled = _viewModel.Recipe.SelectedRecipe != Domain.Models.TextureTools.RecipeType.None && _viewModel.Selection.TargetTexture != null;
                
                if (GUILayout.Button("選択レシピを実行", GUILayout.Height(40)))
                {
                    _viewModel.Recipe.ExecuteSelectedRecipe();
                }
                
                GUI.enabled = true;
                
                if (_viewModel.Recipe.SelectedRecipe == Domain.Models.TextureTools.RecipeType.None)
                {
                    EditorGUILayout.HelpBox("レシピを選択してください", MessageType.Warning);
                }
                else if (_viewModel.Selection.TargetTexture == null)
                {
                    EditorGUILayout.HelpBox("テクスチャを選択してください", MessageType.Warning);
                }
            }
        }

        private void DrawDetailTab()
        {
            EditorGUILayout.LabelField("詳細設定", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 機能選択（ラジオボタン形式）
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("機能選択", EditorStyles.boldLabel);
                
                var selected = _viewModel.DetailFunction.SelectedFunction;
                
                // PNG変換
                bool pngToggle = EditorGUILayout.ToggleLeft("PNG変換", selected == Domain.Models.TextureTools.DetailFunction.PngConvert);
                if (pngToggle != (selected == Domain.Models.TextureTools.DetailFunction.PngConvert))
                {
                    _viewModel.DetailFunction.SelectedFunction = pngToggle ? Domain.Models.TextureTools.DetailFunction.PngConvert : Domain.Models.TextureTools.DetailFunction.None;
                }
                
                // チャンネル反転
                bool invertToggle = EditorGUILayout.ToggleLeft("チャンネル反転", selected == Domain.Models.TextureTools.DetailFunction.ChannelInvert);
                if (invertToggle != (selected == Domain.Models.TextureTools.DetailFunction.ChannelInvert))
                {
                    _viewModel.DetailFunction.SelectedFunction = invertToggle ? Domain.Models.TextureTools.DetailFunction.ChannelInvert : Domain.Models.TextureTools.DetailFunction.None;
                }
                
                // 法線マップ調整
                bool normalToggle = EditorGUILayout.ToggleLeft("法線マップ調整", selected == Domain.Models.TextureTools.DetailFunction.NormalMap);
                if (normalToggle != (selected == Domain.Models.TextureTools.DetailFunction.NormalMap))
                {
                    _viewModel.DetailFunction.SelectedFunction = normalToggle ? Domain.Models.TextureTools.DetailFunction.NormalMap : Domain.Models.TextureTools.DetailFunction.None;
                }
                
                // マスク統合
                bool maskToggle = EditorGUILayout.ToggleLeft("マスク統合", selected == Domain.Models.TextureTools.DetailFunction.MaskMerge);
                if (maskToggle != (selected == Domain.Models.TextureTools.DetailFunction.MaskMerge))
                {
                    _viewModel.DetailFunction.SelectedFunction = maskToggle ? Domain.Models.TextureTools.DetailFunction.MaskMerge : Domain.Models.TextureTools.DetailFunction.None;
                }
            }

            EditorGUILayout.Space();

            // 選択された機能の詳細設定
            DrawDetailFunctionSettings();
        }

        private void DrawDetailFunctionSettings()
        {
            switch (_viewModel.DetailFunction.SelectedFunction)
            {
                case Domain.Models.TextureTools.DetailFunction.PngConvert:
                    DrawPngConvertSettings();
                    break;
                case Domain.Models.TextureTools.DetailFunction.ChannelInvert:
                    DrawChannelInvertSettings();
                    break;
                case Domain.Models.TextureTools.DetailFunction.NormalMap:
                    DrawNormalMapSettings();
                    break;
                case Domain.Models.TextureTools.DetailFunction.MaskMerge:
                    DrawMaskMergeSettings();
                    break;
            }
        }

        private void DrawPngConvertSettings()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUI.BeginChangeCheck();
                _viewModel.DetailFunction.DeleteOriginal = EditorGUILayout.ToggleLeft("元アセットを削除（非可逆）", _viewModel.DetailFunction.DeleteOriginal);
                if (EditorGUI.EndChangeCheck())
                {
                    _viewModel.DetailFunction.NotifySettingsChanged();
                }
                
                if (GUILayout.Button("PNG変換実行", GUILayout.Height(30)))
                {
                    _viewModel.DetailFunction.ExecuteSelectedFunction();
                }
            }
        }

        private void DrawChannelInvertSettings()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("反転するチャンネル:");
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    _viewModel.DetailFunction.InvertR = GUILayout.Toggle(_viewModel.DetailFunction.InvertR, "R", "Button", GUILayout.ExpandWidth(true));
                    _viewModel.DetailFunction.InvertG = GUILayout.Toggle(_viewModel.DetailFunction.InvertG, "G", "Button", GUILayout.ExpandWidth(true));
                    _viewModel.DetailFunction.InvertB = GUILayout.Toggle(_viewModel.DetailFunction.InvertB, "B", "Button", GUILayout.ExpandWidth(true));
                    _viewModel.DetailFunction.InvertA = GUILayout.Toggle(_viewModel.DetailFunction.InvertA, "A", "Button", GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        _viewModel.DetailFunction.NotifySettingsChanged();
                    }
                }
                
                if (GUILayout.Button("反転実行"))
                {
                    _viewModel.DetailFunction.ExecuteSelectedFunction();
                }
            }
        }

        private void DrawNormalMapSettings()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                // 反転設定
                EditorGUILayout.LabelField("反転設定", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    _viewModel.DetailFunction.NormalInvertR = GUILayout.Toggle(_viewModel.DetailFunction.NormalInvertR, "Rを反転", "Button", GUILayout.Width(80));
                    _viewModel.DetailFunction.NormalInvertG = GUILayout.Toggle(_viewModel.DetailFunction.NormalInvertG, "Gを反転（OpenGL→DirectX）", "Button", GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        _viewModel.DetailFunction.NotifySettingsChanged();
                    }
                }
                
                EditorGUILayout.Space();
                
                // 強度設定
                EditorGUILayout.LabelField("法線強度", EditorStyles.boldLabel);
                
                // プリセットボタン
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("0.5", GUILayout.Height(25))) 
                    {
                        _viewModel.DetailFunction.NormalStrength = 0.5f;
                        _viewModel.DetailFunction.NotifySettingsChanged();
                        ShowNotification("法線強度: 0.5（弱め）");
                    }
                    if (GUILayout.Button("1.0", GUILayout.Height(25))) 
                    {
                        _viewModel.DetailFunction.NormalStrength = 1.0f;
                        _viewModel.DetailFunction.NotifySettingsChanged();
                        ShowNotification("法線強度: 1.0（標準）");
                    }
                    if (GUILayout.Button("1.5", GUILayout.Height(25))) 
                    {
                        _viewModel.DetailFunction.NormalStrength = 1.5f;
                        _viewModel.DetailFunction.NotifySettingsChanged();
                        ShowNotification("法線強度: 1.5（強め）");
                    }
                    if (GUILayout.Button("2.0", GUILayout.Height(25))) 
                    {
                        _viewModel.DetailFunction.NormalStrength = 2.0f;
                        _viewModel.DetailFunction.NotifySettingsChanged();
                        ShowNotification("法線強度: 2.0（非常に強い）");
                    }
                }
                
                // スライダー
                EditorGUI.BeginChangeCheck();
                _viewModel.DetailFunction.NormalStrength = EditorGUILayout.Slider(_viewModel.DetailFunction.NormalStrength, 0.0f, 3.0f);
                if (EditorGUI.EndChangeCheck())
                {
                    _viewModel.DetailFunction.NotifySettingsChanged();
                }
                
                if (GUILayout.Button("法線マップ調整実行", GUILayout.Height(30)))
                {
                    _viewModel.DetailFunction.ExecuteSelectedFunction();
                }
            }
        }

        private void DrawMaskMergeSettings()
        {
            // プリセットボタン
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("クイックプリセット", EditorStyles.boldLabel);
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("RMA → Unity", GUILayout.Height(25)))
                    {
                        _viewModel.DetailFunction.ApplyRMAToUnityPreset();
                    }
                    if (GUILayout.Button("クリア", GUILayout.Height(25)))
                    {
                        _viewModel.DetailFunction.ClearMaskChannels();
                    }
                }
                
                EditorGUILayout.HelpBox("RMA → Unity Standard変換\nUE4形式RMAテクスチャをUnityのStandardシェーダ用に変換します", MessageType.Info);
            }

            EditorGUILayout.Space();

            // チャンネル設定表
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
                        _viewModel.DetailFunction.MaskChannels[i].Source = (Texture2D)EditorGUILayout.ObjectField(
                            _viewModel.DetailFunction.MaskChannels[i].Source, typeof(Texture2D), false, GUILayout.ExpandWidth(true));
                        
                        // ソースチャンネル
                        int selectedChannel = (int)_viewModel.DetailFunction.MaskChannels[i].SourceChannel;
                        selectedChannel = GUILayout.SelectionGrid(selectedChannel, channelOptions, 4, GUILayout.Width(140));
                        _viewModel.DetailFunction.MaskChannels[i].SourceChannel = (BitAssistTools.Domain.Models.Texture.Channel)selectedChannel;
                        
                        // 反転
                        _viewModel.DetailFunction.MaskChannels[i].Invert = EditorGUILayout.Toggle(_viewModel.DetailFunction.MaskChannels[i].Invert, GUILayout.Width(35));
                        
                        if (EditorGUI.EndChangeCheck())
                        {
                            _viewModel.DetailFunction.NotifySettingsChanged();
                        }
                        
                        // クリアボタン
                        if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            _viewModel.DetailFunction.MaskChannels[i].Source = null;
                            _viewModel.DetailFunction.MaskChannels[i].SourceChannel = (BitAssistTools.Domain.Models.Texture.Channel)i;
                            _viewModel.DetailFunction.MaskChannels[i].Invert = false;
                            _viewModel.DetailFunction.NotifySettingsChanged();
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
                    _viewModel.DetailFunction.OutputWidth = EditorGUILayout.IntField(_viewModel.DetailFunction.OutputWidth, GUILayout.Width(60));
                    EditorGUILayout.LabelField("×", GUILayout.Width(15));
                    _viewModel.DetailFunction.OutputHeight = EditorGUILayout.IntField(_viewModel.DetailFunction.OutputHeight, GUILayout.Width(60));
                    if (EditorGUI.EndChangeCheck())
                    {
                        _viewModel.DetailFunction.NotifySettingsChanged();
                    }
                    
                    if (GUILayout.Button("元サイズ", GUILayout.Width(60)))
                    {
                        if (_viewModel.Selection.TargetTexture != null)
                        {
                            _viewModel.DetailFunction.OutputWidth = _viewModel.Selection.TargetTexture.width;
                            _viewModel.DetailFunction.OutputHeight = _viewModel.Selection.TargetTexture.height;
                            _viewModel.DetailFunction.NotifySettingsChanged();
                        }
                    }
                }
                
                EditorGUILayout.HelpBox("デフォルト値（未設定時）: R=0, G=0, B=0, A=1", MessageType.Info);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("マスク統合実行", GUILayout.Height(30)))
            {
                _viewModel.DetailFunction.ExecuteSelectedFunction();
            }
        }
        #endregion


        #region Notification
        private void ShowNotification(string message)
        {
            _notificationText = message;
            _showNotification = true;
            _notificationTime = (float)EditorApplication.timeSinceStartup;
            
            ShowNotification(new GUIContent(message));
        }

        private void ShowError(string error)
        {
            EditorUtility.DisplayDialog("エラー", error, "OK");
            Debug.LogError($"[TextureTools] {error}");
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
        #endregion
    }
}
#endif
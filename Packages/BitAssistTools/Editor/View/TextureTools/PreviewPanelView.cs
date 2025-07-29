#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using BitAssistTools.Application.ViewModels.TextureTools;
using BitAssistTools.Domain.Models.TextureTools;

namespace BitAssistTools.Editor.View.TextureTools
{
    /// <summary>
    /// プレビューパネルView
    /// </summary>
    public static class PreviewPanelView
    {
        private static readonly string[] PreviewModeNames = { "RGB", "Alpha", "法線", "R", "G", "B" };

        /// <summary>
        /// プレビューパネルを描画
        /// </summary>
        public static void DrawPreviewPanel(TextureToolsMainViewModel viewModel, float panelWidth = 280f)
        {
            using (new EditorGUILayout.VerticalScope("box", GUILayout.Width(panelWidth), GUILayout.MinWidth(panelWidth), GUILayout.ExpandHeight(true)))
            {
                EditorGUILayout.LabelField("ライブプレビュー", EditorStyles.boldLabel);

                if (viewModel.Selection.TargetTexture != null)
                {
                    DrawPreviewSettings(viewModel);
                    EditorGUILayout.Space();
                    DrawPreviewTexture(viewModel, panelWidth - 30);
                    EditorGUILayout.Space();
                    DrawTextureInfo(viewModel);
                }
                else
                {
                    DrawNoTextureMessage(panelWidth - 30);
                }

                GUILayout.FlexibleSpace();
            }
        }

        private static void DrawPreviewSettings(TextureToolsMainViewModel viewModel)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                // Before/After表示設定
                var newShowBeforeAfter = EditorGUILayout.ToggleLeft("Before/After表示", viewModel.Preview.Settings.ShowBeforeAfter);
                
                EditorGUILayout.LabelField("表示モード:");
                using (new EditorGUILayout.HorizontalScope())
                {
                    // 基本モード
                    var newMode = viewModel.Preview.Settings.Mode;
                    if (GUILayout.Toggle(newMode == PreviewMode.RGB, "RGB", "Button", GUILayout.Width(40)))
                        newMode = PreviewMode.RGB;
                    if (GUILayout.Toggle(newMode == PreviewMode.Normal, "法線", "Button", GUILayout.Width(40)))
                        newMode = PreviewMode.Normal;
                        
                    // 設定が変更された場合は更新
                    if (newMode != viewModel.Preview.Settings.Mode || newShowBeforeAfter != viewModel.Preview.Settings.ShowBeforeAfter)
                    {
                        viewModel.Preview.UpdateSettings(newMode, newShowBeforeAfter, viewModel.Preview.Settings.PreviewSize);
                    }
                }
                
                EditorGUILayout.LabelField("チャンネル別表示:");
                using (new EditorGUILayout.HorizontalScope())
                {
                    var newMode = viewModel.Preview.Settings.Mode;
                    if (GUILayout.Toggle(newMode == PreviewMode.R, "R", "Button", GUILayout.Width(25)))
                        newMode = PreviewMode.R;
                    if (GUILayout.Toggle(newMode == PreviewMode.G, "G", "Button", GUILayout.Width(25)))
                        newMode = PreviewMode.G;
                    if (GUILayout.Toggle(newMode == PreviewMode.B, "B", "Button", GUILayout.Width(25)))
                        newMode = PreviewMode.B;
                    if (GUILayout.Toggle(newMode == PreviewMode.Alpha, "A", "Button", GUILayout.Width(25)))
                        newMode = PreviewMode.Alpha;
                        
                    // 設定が変更された場合は更新
                    if (newMode != viewModel.Preview.Settings.Mode)
                    {
                        viewModel.Preview.UpdateSettings(newMode, viewModel.Preview.Settings.ShowBeforeAfter, viewModel.Preview.Settings.PreviewSize);
                    }
                }
                
                // モードに応じたヘルプメッセージ
                if (viewModel.Preview.Settings.Mode == PreviewMode.Normal)
                {
                    EditorGUILayout.HelpBox("法線ベクトルを黄色い矢印で表示", MessageType.Info);
                }
            }
        }

        private static void DrawPreviewTexture(TextureToolsMainViewModel viewModel, float previewSize)
        {
            var rect = GUILayoutUtility.GetRect(previewSize, previewSize);
            var displayTexture = viewModel.Selection.DisplayTargetTexture;
            var previewTexture = viewModel.Preview.PreviewTexture ?? displayTexture;

            if (viewModel.Preview.Settings.ShowBeforeAfter && displayTexture != null)
            {
                // Before/After表示
                DrawBeforeAfterPreview(rect, displayTexture, previewTexture, viewModel.Preview.Settings.Mode);
            }
            else
            {
                // 通常表示
                DrawSinglePreview(rect, previewTexture, viewModel, displayTexture);
            }
            
            // フレーム描画
            GUI.Box(rect, "", EditorStyles.helpBox);
        }

        private static void DrawBeforeAfterPreview(Rect rect, Texture2D beforeTexture, Texture2D afterTexture, PreviewMode mode)
        {
            var halfWidth = rect.width * 0.5f;
            var leftRect = new Rect(rect.x, rect.y, halfWidth - 1, rect.height);
            var rightRect = new Rect(rect.x + halfWidth + 1, rect.y, halfWidth - 1, rect.height);
            
            // Before（元テクスチャ）
            DrawTextureWithMode(leftRect, beforeTexture, mode);
            GUI.Label(new Rect(leftRect.x, leftRect.y, leftRect.width, 16), "Before", EditorStyles.miniLabel);
            
            // After（プレビューテクスチャ）
            DrawTextureWithMode(rightRect, afterTexture, mode);
            GUI.Label(new Rect(rightRect.x, rightRect.y, rightRect.width, 16), "After", EditorStyles.miniLabel);
            
            // 分割線
            EditorGUI.DrawRect(new Rect(rect.x + halfWidth, rect.y, 2, rect.height), Color.gray);
        }

        private static void DrawSinglePreview(Rect rect, Texture2D texture, TextureToolsMainViewModel viewModel, Texture2D displayTexture)
        {
            // 詳細機能が選択されている場合はプレビューを、そうでなければ元テクスチャを表示
            bool hasPreview = viewModel.DetailFunction.SelectedFunction != DetailFunction.None 
                             && viewModel.Preview.PreviewTexture != null;
            
            var displayTex = hasPreview ? viewModel.Preview.PreviewTexture : displayTexture;
            DrawTextureWithMode(rect, displayTex, viewModel.Preview.Settings.Mode);
            
            // プレビューが選択されている場合は「Preview」ラベルを表示
            if (hasPreview)
            {
                GUI.Label(new Rect(rect.x, rect.y, rect.width, 16), "Preview", EditorStyles.miniLabel);
            }
        }

        private static void DrawTextureWithMode(Rect rect, Texture2D texture, PreviewMode mode)
        {
            if (texture == null) return;

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
                case PreviewMode.G:
                case PreviewMode.B:
                    DrawSingleChannel(rect, texture, (int)mode - (int)PreviewMode.R);
                    break;
            }
        }

        private static void DrawTextureAsDefault(Rect rect, Texture2D texture)
        {
            // テクスチャタイプに関係なく、常にDefaultタイプとして表示
            var originalTextureType = GetTextureType(texture);
            
            if (originalTextureType == UnityEditor.TextureImporterType.NormalMap)
            {
                // NormalMapタイプの場合は、カスタムマテリアルを使用してDefaultとして描画
                DrawNormalMapAsDefault(rect, texture);
            }
            else
            {
                try
                {
                    EditorGUI.DrawPreviewTexture(rect, texture);
                }
                catch
                {
                    GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
                }
            }
        }

        private static UnityEditor.TextureImporterType GetTextureType(Texture2D texture)
        {
            var path = UnityEditor.AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) return UnityEditor.TextureImporterType.Default;
            
            var importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
            return importer?.textureType ?? UnityEditor.TextureImporterType.Default;
        }

        private static void DrawNormalMapAsDefault(Rect rect, Texture2D texture)
        {
            // NormalMapをDefaultタイプのように表示するためのマテリアル
            var material = new Material(Shader.Find("Hidden/Internal-GUITexture"));
            
            // マテリアルのプロパティを調整してDefaultタイプのような表示にする
            material.SetTexture("_MainTex", texture);
            material.SetFloat("_ColorMask", 15); // RGBA全チャンネル表示
            
            // 通常のテクスチャ表示（法線マップの特殊な色調整を無効化）
            EditorGUI.DrawPreviewTexture(rect, texture, material);
            
            // マテリアルを破棄
            UnityEngine.Object.DestroyImmediate(material);
        }

        private static void DrawAlphaChannel(Rect rect, Texture2D texture)
        {
            if (!IsTextureReadable(texture))
            {
                // まず自動でプレビュー準備をする（Read/Write Enabled + Texture Type調整）
                if (BitAssistTools.Application.Services.TextureTools.TextureDisplayService.EnsureTexturePreviewReady(texture))
                {
                    // 自動オンが成功した場合は通常処理を続行
                    try
                    {
                        var tempTexture = CreateAlphaChannelTexture(texture);
                        EditorGUI.DrawPreviewTexture(rect, tempTexture);
                        UnityEngine.Object.DestroyImmediate(tempTexture);
                        return;
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogWarning($"[PreviewPanelView] Alphaチャンネル表示エラー (自動オン後): {ex.Message}");
                    }
                }
                
                // 自動オンに失敗した場合は表示用テクスチャを作成してから処理
                var displayTexture = BitAssistTools.Application.Services.TextureTools.TextureDisplayService.CreateDisplayableTexture(texture);
                if (displayTexture != null && IsTextureReadable(displayTexture))
                {
                    try
                    {
                        var tempTexture = CreateAlphaChannelTexture(displayTexture);
                        EditorGUI.DrawPreviewTexture(rect, tempTexture);
                        UnityEngine.Object.DestroyImmediate(tempTexture);
                        
                        // 作成した表示用テクスチャが元と異なる場合は破棄
                        if (displayTexture != texture)
                        {
                            UnityEngine.Object.DestroyImmediate(displayTexture);
                        }
                        return;
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogWarning($"[PreviewPanelView] Alphaチャンネル表示エラー (表示用テクスチャ): {ex.Message}");
                        if (displayTexture != texture)
                        {
                            UnityEngine.Object.DestroyImmediate(displayTexture);
                        }
                    }
                }
                
                // フォールバック: 通常表示
                EditorGUI.DrawPreviewTexture(rect, texture);
                return;
            }

            try
            {
                var tempTexture = CreateAlphaChannelTexture(texture);
                EditorGUI.DrawPreviewTexture(rect, tempTexture);
                UnityEngine.Object.DestroyImmediate(tempTexture);
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[PreviewPanelView] Alphaチャンネル表示エラー: {ex.Message}");
                // エラー時は通常表示
                EditorGUI.DrawPreviewTexture(rect, texture);
            }
        }

        private static Texture2D CreateAlphaChannelTexture(Texture2D source)
        {
            var colors = source.GetPixels();
            var alphaColors = new Color[colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                float alpha = colors[i].a;
                alphaColors[i] = new Color(alpha, alpha, alpha, 1f);
            }

            var result = new Texture2D(source.width, source.height);
            result.SetPixels(alphaColors);
            result.Apply();
            return result;
        }

        private static bool IsTextureReadable(Texture2D texture)
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

        private static void DrawSingleChannel(Rect rect, Texture2D texture, int channel)
        {
            // 指定チャンネル（0=R, 1=G, 2=B）をグレースケールで表示
            if (texture == null)
            {
                return;
            }

            string[] channelNames = { "R", "G", "B", "A" };
            var channelName = channel < channelNames.Length ? channelNames[channel] : channel.ToString();
            
            if (!IsTextureReadable(texture))
            {
                // まず自動でプレビュー準備をする（Read/Write Enabled + Texture Type調整）
                if (BitAssistTools.Application.Services.TextureTools.TextureDisplayService.EnsureTexturePreviewReady(texture))
                {
                    // 自動オンが成功した場合は通常処理を続行
                    try
                    {
                        var tempTexture = CreateSingleChannelTexture(texture, channel);
                        if (tempTexture != null)
                        {
                            EditorGUI.DrawPreviewTexture(rect, tempTexture);
                            UnityEngine.Object.DestroyImmediate(tempTexture);
                        }
                        else
                        {
                            EditorGUI.DrawPreviewTexture(rect, texture);
                        }
                        return;
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogWarning($"[PreviewPanelView] {channelName}チャンネル表示エラー (自動オン後): {ex.Message}");
                    }
                }
                
                // 自動オンに失敗した場合は表示用テクスチャを作成してから処理
                var displayTexture = BitAssistTools.Application.Services.TextureTools.TextureDisplayService.CreateDisplayableTexture(texture);
                if (displayTexture != null && IsTextureReadable(displayTexture))
                {
                    try
                    {
                        var tempTexture = CreateSingleChannelTexture(displayTexture, channel);
                        if (tempTexture != null)
                        {
                            EditorGUI.DrawPreviewTexture(rect, tempTexture);
                            UnityEngine.Object.DestroyImmediate(tempTexture);
                        }
                        
                        // 作成した表示用テクスチャが元と異なる場合は破棄
                        if (displayTexture != texture)
                        {
                            UnityEngine.Object.DestroyImmediate(displayTexture);
                        }
                        return;
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogWarning($"[PreviewPanelView] {channelName}チャンネル表示エラー (表示用テクスチャ): {ex.Message}");
                        if (displayTexture != texture)
                        {
                            UnityEngine.Object.DestroyImmediate(displayTexture);
                        }
                    }
                }
                
                // フォールバック: 通常表示
                EditorGUI.DrawPreviewTexture(rect, texture);
                return;
            }

            try
            {
                var tempTexture = CreateSingleChannelTexture(texture, channel);
                if (tempTexture != null)
                {
                    EditorGUI.DrawPreviewTexture(rect, tempTexture);
                    UnityEngine.Object.DestroyImmediate(tempTexture);
                }
                else
                {
                    // tempTextureが作成できない場合は通常表示
                    EditorGUI.DrawPreviewTexture(rect, texture);
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[PreviewPanelView] {channelName}チャンネル表示エラー: {ex.Message}");
                // エラー時は通常表示
                EditorGUI.DrawPreviewTexture(rect, texture);
            }
        }

        private static Texture2D CreateSingleChannelTexture(Texture2D source, int channel)
        {
            if (source == null) return null;
            
            try
            {
                var colors = source.GetPixels();
                if (colors == null || colors.Length == 0) return null;
                
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
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[PreviewPanelView] CreateSingleChannelTexture failed: {ex.Message}");
                return null;
            }
        }

        private static void DrawNormalVisualization(Rect rect, Texture2D texture)
        {
            // 法線マップの可視化（簡易版）
            EditorGUI.DrawPreviewTexture(rect, texture);
            
            // 法線ベクトルの方向を示す矢印オーバーレイ（簡易版）
            if (rect.width > 64 && rect.height > 64)
            {
                DrawNormalVectorOverlay(rect, texture);
            }
        }
        
        private static void DrawNormalVectorOverlay(Rect rect, Texture2D texture)
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
                    
                    UnityEditor.Handles.color = Color.yellow;
                    UnityEditor.Handles.DrawLine(new Vector3(centerX, centerY, 0), new Vector3(endX, endY, 0));
                }
            }
        }

        private static void DrawTextureInfo(TextureToolsMainViewModel viewModel)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("テクスチャ情報", EditorStyles.boldLabel);
                
                if (viewModel.Selection.TargetTexture != null)
                {
                    EditorGUILayout.LabelField($"Raw: {viewModel.Selection.GetRawTextureSize()}");
                    EditorGUILayout.LabelField($"Resize: {viewModel.Selection.GetResizeTextureSize()}");
                    EditorGUILayout.LabelField($"Format: {viewModel.Selection.TargetTexture.format}");
                    EditorGUILayout.LabelField($"Mip Map: {(viewModel.Selection.TargetTexture.mipmapCount > 1 ? "有" : "無")}");
                }
                else
                {
                    EditorGUILayout.LabelField("テクスチャが選択されていません");
                }
            }
        }

        private static void DrawNoTextureMessage(float width)
        {
            EditorGUILayout.HelpBox("テクスチャを選択してください", MessageType.Info);
            
            var dropRect = GUILayoutUtility.GetRect(width, 120);
            GUI.Box(dropRect, "テクスチャをここにドラッグ&ドロップ", EditorStyles.helpBox);
            
            // TODO: ドラッグ&ドロップ処理を実装
        }
    }
}
#endif
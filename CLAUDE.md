# BitAssistTools

Unity Editor拡張ツール群 - DDD（Domain-Driven Design）アーキテクチャベース

## 📋 プロジェクト概要

Unity Editorでの作業効率を向上させるツール群。配置支援、選択支援、コライダー配置支援、ライティング支援、リネーミング、プロジェクトメンテナンス機能を提供。

## 🏗️ アーキテクチャ

### DDD（Domain-Driven Design）構造
```
BitAssistTools/
├── Domain/                    # ドメイン層（ビジネスロジック）
│   ├── Models/               # エンティティ・値オブジェクト
│   ├── Services/             # ドメインサービス
│   └── Repositories/         # リポジトリインターフェース
├── Application/              # アプリケーション層
│   ├── Services/             # アプリケーションサービス
│   └── ViewModels/           # MVVM ViewModel
├── Infrastructure/           # インフラストラクチャ層
│   ├── Unity/               # Unity API実装
│   └── Package/             # パッケージ管理
└── Editor/                  # プレゼンテーション層
    ├── View/                # EditorWindow (MVVM View)
    └── EditorCommands/      # MenuItem機能
```

## 🛠️ 実装されたツール

| ツール | 機能 | DDD化 | MVVM | Undo対応 |
|--------|------|-------|------|----------|
| PlacementAssistance | オブジェクト配置・複製・整列 | ✅ | ✅ | ✅ |
| ColliderPlacementAssistance | コライダー配置・管理 | ✅ | ✅ | ✅ |
| SelectionAssistance | 選択履歴・階層選択 | ✅ | ✅ | ✅ |
| LightingAssistance | ライト強度調整 | ✅ | ✅ | ✅ |
| Renamer | オブジェクト・アセットリネーム | ✅ | ✅ | ✅ |
| ProjectMaintenance | Missing Script削除 | ✅ | ✅ | ✅ |
| AssetNavigation | アセットパス選択 | ✅ | ✅ | ✅ |

## 📐 実装方針

### 1. DDD原則の適用
- **Domain層**: Unity APIに依存しない純粋なビジネスロジック
- **Application層**: ユースケースの実装、ドメインの調整
- **Infrastructure層**: Unity API、外部システムとの統合
- **Editor層**: UI表示、ユーザー操作の処理

### 2. MVVM パターン
- **ViewModel**: UI状態管理、ビジネスロジックとの分離
- **View**: 純粋なUI描画、ViewModelとのデータバインディング
- **Model**: Domainモデルを使用

### 3. 不変性と型安全性
- 値オブジェクトは `readonly struct` で実装
- エンティティは不変なコピーを作成するメソッドを提供
- 強い型付けでコンパイル時エラー検出

## ⚠️ 実装時の注意事項

### コーディング規約
- **名前空間競合**: `Selection`, `Application` 等は `UnityEditor.Selection`, `UnityEngine.Application` として明示的修飾
- **Undo対応**: 全ての変更操作で `Undo.SetCurrentGroupName()` と `Undo.CollapseUndoOperations()` を使用
- **エラーハンドリング**: try-catch でラップし、適切なログ出力
- **Unity 2019.4互換**: C# 7.x構文を使用（C# 8.0以降の機能は使用しない）

### DDD実装パターン
```csharp
// 値オブジェクトの例
public readonly struct TransformData : IEquatable<TransformData>
{
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;
    public readonly Vector3 Scale;
    
    public TransformData(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }
}

// アプリケーションサービスの例
public static class ObjectPlacementService
{
    public static void ExecutePlacement(PlacementRule rule)
    {
        Undo.SetCurrentGroupName("Object Placement");
        int undoGroup = Undo.GetCurrentGroup();
        
        // ビジネスロジック実行
        
        Undo.CollapseUndoOperations(undoGroup);
    }
}
```

### MVVM実装パターン
```csharp
// ViewModel
public class PlacementViewModel
{
    public event Action OnUIStateChanged;
    
    public void ExecutePlacement()
    {
        try
        {
            ObjectPlacementService.ExecutePlacement(GetCurrentRule());
        }
        catch (Exception ex)
        {
            OnError?.Invoke(ex.Message);
        }
    }
}

// View
public class PlacementView : EditorWindow
{
    private PlacementViewModel _viewModel;
    
    private void OnGUI()
    {
        if (GUILayout.Button("実行"))
        {
            _viewModel.ExecutePlacement();
        }
    }
}
```

## 🔄 残タスク

### 現在進行中
- [ ] Utilitiesフォルダの EditorCommands への名称変更
- [ ] 名前空間の統一確認

### 今後の改善項目
- [ ] 単体テストの追加（Domain層優先）
- [ ] EtoEテストの追加 
- [ ] パフォーマンス最適化
- [ ] ドキュメント整備
- [ ] CI/CD導入

## 📖 開発ガイド

### 新機能追加時の手順
1. Domain層でビジネスロジック設計
2. Application層でユースケース実装
3. ViewModel作成（MVVM）
4. View作成（EditorWindow）
5. Undo対応確認
6. エラーハンドリング追加

### デバッグ時の確認ポイント
- コンソールログでエラー詳細確認
- Undo操作が正常動作するか
- UI状態とViewModelの同期
- 名前空間競合によるコンパイルエラー
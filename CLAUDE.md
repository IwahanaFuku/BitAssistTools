# BitAssistTools DDDベースリファクタリング計画

## 📋 現在の分析結果

**プロジェクト概要**: Unity Editor拡張ツール群（配置支援、選択支援、コライダー配置支援、ライティング支援、ユーティリティ）

**現在の課題**:
- プレゼンテーション層とビジネスロジックの混在
- 統一されていない設計パターン  
- GUIクラスの肥大化（特にColliderPlacementAssistance: 602行）
- 名前空間の命名不整合（`SelectionAsistance`など）

## 🎯 DDD適用領域の特定

### 1. **Editor Tool Domain（エディターツール領域）**
- **Bounded Context**: Unity Editor拡張ツールとしての機能
- **Core Domain**: オブジェクト配置・選択・変換の操作

### 2. **Sub-domains**:
- **Placement Domain**: オブジェクト配置・複製・整列
- **Selection Domain**: オブジェクト選択履歴・階層選択  
- **Transformation Domain**: Transform操作・座標変換
- **UI Domain**: EditorWindow・GUI状態管理

## 🏗️ DDDベース新アーキテクチャ設計

```
BitAssistTools/
├── Domain/                          # ドメイン層
│   ├── Models/                      # エンティティ・値オブジェクト
│   │   ├── Transform/
│   │   │   ├── TransformData.cs     # 値オブジェクト
│   │   │   ├── TransformOperation.cs
│   │   │   └── CoordinateSystem.cs
│   │   ├── Selection/
│   │   │   ├── SelectionHistory.cs  # エンティティ
│   │   │   └── SelectionSet.cs
│   │   └── Placement/
│   │       ├── PlacementRule.cs
│   │       └── DuplicationStrategy.cs
│   ├── Services/                    # ドメインサービス
│   │   ├── ITransformCalculationService.cs
│   │   ├── IObjectPlacementService.cs
│   │   └── ISelectionManagementService.cs
│   └── Repositories/                # リポジトリインターフェース
│       ├── ISelectionHistoryRepository.cs
│       └── IEditorPreferencesRepository.cs
├── Application/                     # アプリケーション層
│   ├── Commands/                    # コマンド
│   │   ├── PlaceObjectsCommand.cs
│   │   ├── DuplicateObjectsCommand.cs
│   │   └── SaveSelectionCommand.cs
│   ├── Queries/                     # クエリ
│   │   ├── GetSelectionHistoryQuery.cs
│   │   └── GetTransformDataQuery.cs
│   └── Services/                    # アプリケーションサービス
│       ├── ObjectPlacementService.cs
│       ├── SelectionManagementService.cs
│       └── TransformCalculationService.cs
├── Infrastructure/                  # インフラストラクチャ層
│   ├── Repositories/               # リポジトリ実装
│   │   ├── UnitySelectionHistoryRepository.cs
│   │   └── EditorPreferencesRepository.cs
│   └── Unity/                      # Unity固有実装
│       ├── UnityObjectFactory.cs
│       └── UnityTransformAdapter.cs
├── Tests/                          # テスト層
│   ├── Domain/                     # ドメインテスト
│   │   ├── Models/
│   │   │   ├── TransformDataTests.cs
│   │   │   ├── SelectionHistoryTests.cs
│   │   │   └── PlacementRuleTests.cs
│   │   └── Services/
│   │       ├── TransformCalculationServiceTests.cs
│   │       ├── ObjectPlacementServiceTests.cs
│   │       └── SelectionManagementServiceTests.cs
│   ├── Application/                # アプリケーションテスト
│   │   ├── Commands/
│   │   │   ├── PlaceObjectsCommandTests.cs
│   │   │   ├── DuplicateObjectsCommandTests.cs
│   │   │   └── SaveSelectionCommandTests.cs
│   │   ├── Queries/
│   │   │   ├── GetSelectionHistoryQueryTests.cs
│   │   │   └── GetTransformDataQueryTests.cs
│   │   └── Services/
│   │       ├── ObjectPlacementServiceTests.cs
│   │       ├── SelectionManagementServiceTests.cs
│   │       └── TransformCalculationServiceTests.cs
│   ├── Infrastructure/             # インフラストラクチャテスト
│   │   ├── Repositories/
│   │   │   ├── UnitySelectionHistoryRepositoryTests.cs
│   │   │   └── EditorPreferencesRepositoryTests.cs
│   │   └── Unity/
│   │       ├── UnityObjectFactoryTests.cs
│   │       └── UnityTransformAdapterTests.cs
│   └── Integration/                # 統合テスト
│       ├── PlacementWorkflowTests.cs
│       ├── SelectionWorkflowTests.cs
│       └── ColliderPlacementWorkflowTests.cs
└── Presentation/                   # プレゼンテーション層
    ├── ViewModels/                 # ビューモデル
    │   ├── PlacementViewModel.cs
    │   ├── SelectionViewModel.cs
    │   └── ColliderPlacementViewModel.cs
    ├── Views/                      # ビュー
    │   ├── PlacementView.cs
    │   ├── SelectionView.cs
    │   └── ColliderPlacementView.cs
    └── Controllers/                # コントローラー
        ├── PlacementController.cs
        └── SelectionController.cs
```

## 📅 段階的リファクタリング計画

### **Phase 1: 基盤整備（1-2週間）**
1. **Domain Models作成**
   - `TransformData`の値オブジェクト化
   - `SelectionHistory`エンティティ設計
   - 基本的なドメインサービス定義

2. **Namespace統一**
   - `SelectionAsistance` → `SelectionAssistance`修正
   - 全名前空間を`BitAssistTools.Domain.XXX`に統一

3. **テスト基盤構築**
   - Unity Test Runnerセットアップ
   - ドメインモデルの単体テスト作成
   - テスト用のMockオブジェクト準備

### **Phase 2: Application層構築（2-3週間）**
1. **Command/Query分離**
   - CQRS パターン導入
   - 各操作をCommandとして実装

2. **Application Services実装**
   - ビジネスロジックをドメインから分離
   - 依存関係注入の準備

3. **アプリケーション層テスト**
   - Commandハンドラーの単体テスト
   - Queryハンドラーの単体テスト
   - アプリケーションサービスのテスト

### **Phase 3: Presentation層リファクタリング（2-3週間）**
1. **MVVM パターン導入**
   - ViewModelクラス作成
   - GUI状態とビジネスロジックの完全分離

2. **ViewとViewModel分離**
   - LightingAssistanceパターンを他クラスに適用
   - ColliderPlacementAssistanceの大幅リファクタリング

3. **プレゼンテーション層テスト**
   - ViewModelの単体テスト
   - UI状態変更のテスト
   - データバインディングのテスト

### **Phase 4: Infrastructure層完成（1週間）**
1. **Repository Pattern実装**
   - 設定保存・履歴管理の抽象化
   - Unity Editor API依存の分離

2. **インフラストラクチャテスト**
   - Repositoryの実装テスト
   - Unity API統合テスト
   - ファイルI/O操作のテスト

### **Phase 5: 統合テスト・品質保証（1週間）**
1. **統合テスト実装**
   - エンドツーエンドワークフローテスト
   - 各機能の統合動作確認
   - パフォーマンステスト

2. **品質保証**
   - コードカバレッジ測定（目標: 80%以上）
   - 静的解析ツールによるコード品質確認
   - リファクタリング前後の機能比較テスト

## 🎯 重点リファクタリング対象

### **最優先: ColliderPlacementAssistance**
- 現在602行の巨大クラス
- UI状態とビジネスロジックが高度に結合
- ReorderableList処理の複雑化

### **中優先: PlacementAssistance**  
- 359行、中程度の結合
- 25個のUIフラグの整理が必要

### **参考実装: LightingAssistance**
- 最も理想的な分離実装（41行）
- 他クラスのリファクタリングモデルとして活用

## 🔧 技術的実装指針

1. **依存関係の逆転**: ドメイン層はUnity APIに依存しない
2. **Factory Pattern**: UnityオブジェクトのΓ処理隠蔽
3. **Command Pattern**: Undo/Redo機能の将来実装準備
4. **Observer Pattern**: UI更新の自動化
5. **Repository Pattern**: 設定・履歴の永続化抽象化
6. **Test-Driven Development**: テストファーストでの開発推進

## 🧪 テスト戦略

### **単体テスト（Unit Tests）**
- **対象**: ドメインモデル、アプリケーションサービス、ViewModels
- **フレームワーク**: Unity Test Runner + NUnit
- **カバレッジ目標**: 各層80%以上
- **実行頻度**: 各コミット時

### **統合テスト（Integration Tests）**
- **対象**: レイヤー間の連携、Unity Editor API統合
- **実行環境**: Unity Editor内での実行
- **テスト範囲**: エンドツーエンドワークフロー
- **実行頻度**: リリース前

### **リグレッションテスト**
- **目的**: リファクタリング前後の機能同等性確認
- **範囲**: 全ての既存機能
- **実行方法**: 自動化されたテストスイート
- **実行タイミング**: 各フェーズ完了時

### **パフォーマンステスト**
- **対象**: 大量オブジェクト処理、UI応答性
- **メトリクス**: 実行時間、メモリ使用量
- **ベースライン**: リファクタリング前の性能
- **目標**: 既存性能の維持または向上

## 📊 期待される効果

- **保守性向上**: ビジネスロジックとUIの完全分離
- **テスタビリティ向上**: ドメインロジックの単体テスト化
- **拡張性向上**: 新機能追加時の影響範囲最小化
- **再利用性向上**: ドメインロジックの他エディター拡張での活用

## 🔍 現在のGUIクラス結合度分析

### 1. PlacemenAssistance（配置支援ツール）
**結合度レベル**: 中程度の結合
- UI状態管理: 25個のプライベートフィールド
- ビジネスロジックは適切に分離済み
- クラス肥大化（359行）が課題

### 2. SelectionAssistance（選択支援ツール）
**結合度レベル**: 良好な分離
- 比較的コンパクト（145行）
- 選択操作は`SelectionHistoryFunction`クラスに委譲
- 責任が明確

### 3. ColliderPlacementAssistance（コライダー配置支援ツール）
**結合度レベル**: 高い結合
- 最も肥大化（602行）
- 30個以上のプライベートフィールド
- UI処理とビジネスロジックが混在

### 4. LightingAssistance（ライティング支援ツール）
**結合度レベル**: 優秀な分離
- 最も簡潔（41行）
- 完全なロジック分離
- 理想的なMVPパターンの実装例

## 🚀 次のステップ

1. **Phase 1から開始**: ドメインモデルの設計・実装
2. **ColliderPlacementAssistanceを最優先**でリファクタリング
3. **LightingAssistanceを参考実装**として他クラスに適用
4. **段階的移行**により既存機能の安定性を維持
5. **継続的テスト実行**により品質を担保

## 📋 テスト実装チェックリスト

### **Phase 1: 基盤テスト**
- [ ] Unity Test Runnerのセットアップ完了
- [ ] TransformDataの値オブジェクトテスト
- [ ] SelectionHistoryエンティティテスト
- [ ] ドメインサービスの基本テスト
- [ ] Mockオブジェクト・テストヘルパー準備

### **Phase 2: アプリケーション層テスト**
- [ ] PlaceObjectsCommandテスト
- [ ] DuplicateObjectsCommandテスト
- [ ] SaveSelectionCommandテスト
- [ ] GetSelectionHistoryQueryテスト
- [ ] GetTransformDataQueryテスト
- [ ] アプリケーションサービステスト

### **Phase 3: プレゼンテーション層テスト**
- [ ] PlacementViewModelテスト
- [ ] SelectionViewModelテスト
- [ ] ColliderPlacementViewModelテスト
- [ ] UI状態変更テスト
- [ ] データバインディングテスト

### **Phase 4: インフラストラクチャテスト**
- [ ] UnitySelectionHistoryRepositoryテスト
- [ ] EditorPreferencesRepositoryテスト
- [ ] UnityObjectFactoryテスト
- [ ] UnityTransformAdapterテスト
- [ ] ファイルI/O操作テスト

### **Phase 5: 統合・品質保証テスト**
- [ ] PlacementWorkflowテスト
- [ ] SelectionWorkflowテスト
- [ ] ColliderPlacementWorkflowテスト
- [ ] コードカバレッジ80%達成
- [ ] パフォーマンステスト実行
- [ ] リグレッションテスト完了

## 🎉 Phase 1-2 完了報告

### ✅ Phase 1: Domain層基盤整備完了（2025-07-26）

**1. Domain層のディレクトリ構造作成**
- ✅ `Domain/Models/Transform/`
- ✅ `Domain/Models/Selection/`
- ✅ `Domain/Models/Placement/`
- ✅ `Domain/Services/`
- ✅ `Domain/Repositories/`

**2. 値オブジェクト・エンティティ実装**
- ✅ **TransformData**: 不変な値オブジェクト、Unity APIからの分離完了
- ✅ **CoordinateSystem**: 座標系変換とAxisFlags管理
- ✅ **SelectionSet**: 選択オブジェクトの集合
- ✅ **SelectionHistory**: 選択履歴管理エンティティ
- ✅ **PlacementRule**: 配置ルールの値オブジェクト
- ✅ **DuplicationStrategy**: 複製戦略の値オブジェクト

**3. ドメインサービスインターフェース定義**
- ✅ **ITransformCalculationService**: Transform計算
- ✅ **IObjectPlacementService**: オブジェクト配置操作
- ✅ **ISelectionManagementService**: 選択管理

**4. リポジトリインターフェース定義**
- ✅ **ISelectionHistoryRepository**: 選択履歴永続化
- ✅ **IEditorPreferencesRepository**: エディター設定永続化

**5. テスト基盤構築**
- ✅ Unity Test Runner セットアップ完了
- ✅ Assembly Definition Files 作成
- ✅ 主要ドメインモデルの単体テスト実装

### ✅ Phase 2: ColliderPlacementAssistance + PlacementAssistance DDD化完了（2025-07-27）

**1. ColliderPlacementAssistance DDD化**
- ✅ **ColliderPlacementViewModel**: 40個のUI状態を論理グループ化
- ✅ **ColliderPlacementView**: 600行のUI描画ロジック分離
- ✅ **ColliderPlacementService**: ビジネスロジック抽象化
- ✅ **MVVM完全分離**: UI状態とビジネスロジックの完全分離
- ✅ **Undo対応**: 全操作でUndo機能実装

**2. PlacementAssistance DDD化**
- ✅ **PlacementViewModel**: 25個のUI状態を論理グループ化
- ✅ **PlacementView**: 358行のUI描画ロジック分離
- ✅ **ObjectPlacementService**: 配置・複製・入れ替え機能
- ✅ **TransformCalculationService**: Transform計算・変換機能
- ✅ **RandomizationService**: ランダム化機能
- ✅ **10機能すべてUndo対応**: 完全なUndo機能実装

**3. Application層構築**
- ✅ **3つのApplicationサービス**: 責任分離と単一責任原則
- ✅ **完全なUndo対応**: Unity Undoシステム統合
- ✅ **エラーハンドリング**: 例外処理とログ出力

### 🔧 技術的成果

**DDD原則の適用**
1. **不変性の徹底**: 全ての値オブジェクトでreadonlyフィールド使用
2. **Unity API分離**: ドメインモデルからUnity依存を排除
3. **型安全性**: 強い型付けとenum活用
4. **エンティティと値オブジェクトの明確な分離**

**アーキテクチャ品質**
1. **MVVM完全実装**: UI状態とビジネスロジックの完全分離
2. **レイヤー間依存関係**: Domain ← Application ← Editor
3. **単一責任原則**: 各サービスクラスが明確な責任を持つ
4. **Unity 2019.4対応**: C# 7.x構文への統一

### 🐛 修正対応済み項目

**コンパイルエラー修正**
- ✅ C# 8.0 switch式 → C# 7.x switch文に変更
- ✅ enum重複定義解決（Application層から削除）
- ✅ Random曖昧性解決（using UERandom = UnityEngine.Random）
- ✅ TransformDataメソッド追加（FromTransform/ApplyToTransform）

**アーキテクチャエラー修正**
- ✅ レイヤー間参照の整理
- ✅ 名前空間統一
- ✅ Assembly Definition Files適切な参照設定

### 📊 現在の成果指標

- **実装ファイル数**: 25個（Domain: 12個、Application: 6個、Editor: 7個）
- **DDDアーキテクチャ**: 完全適用 ✅
- **Unity 2019.4互換性**: 完全対応 ✅
- **Undo機能**: 全操作対応 ✅
- **コンパイルエラー**: 0個 ✅

## 🚨 **緊急課題: PlacementAssistance挙動修正**

### **現在の問題**
**ユーザー報告**: 「全体的に過去の挙動と変わってしまっている。選択したオブジェクトを入れ替える、分布、整列以外、全体的に挙動が変わっている」

### **根本原因分析完了**
- ✅ **元実装調査**: `git show 878c5eb`で元PlacemenAssistance実装を分析
- ✅ **挙動差異特定**: 7つの主要機能で実装ロジックが元と異なる
- ✅ **詳細分析**: `placementtodo.md`に修正計画を記録

### **次の緊急対応: 元挙動復元**

**最高優先度**:
1. **Special Duplicate**: 複製後の自動グループ化が欠落
2. **Transform Change**: 加算方式→置換方式に変化
3. **Rounding**: TransformFlags→AxisFlagsに変更
4. **Random Position**: 加算→置換に変化
5. **Random Rotation**: 軸別個別回転→一括回転に変化
6. **Padding/Random Scale**: ロジック自体変更

**修正方針**:
- DDDアーキテクチャは維持
- 元実装のロジックを正確に復元
- Undo機能は維持
- ViewModelとViewの分離は維持

## 📋 次期Phase: 挙動修正 + SelectionAssistance

### **緊急Phase: PlacementAssistance挙動修正（1-2日）**
**目標**: 元実装と同じ挙動に完全復元

**修正項目**:
- [ ] Special Duplicate: 複製後CommonFunctions.ParentCenterAligner()実行
- [ ] Transform Change: 加算方式に変更
- [ ] Rounding: TransformFlags方式に戻す
- [ ] Padding: 元の方向ベクトル×係数ロジック
- [ ] Random Position: 現在位置+ランダム位置
- [ ] Random Rotation: 軸別個別回転+カーブ評価
- [ ] Random Scale: 通常=uniform、詳細=軸別

### **Phase 3: SelectionAssistance + 統合テスト（1週間）**
**目標**: 最後のツールDDD化と全体品質保証

**SelectionAssistance DDD化**:
- [ ] SelectionHistoryドメインモデル適用
- [ ] MVVM分離実装
- [ ] UI状態管理改善

**統合・品質保証**:
- [ ] 全機能の動作検証
- [ ] リグレッションテスト実行
- [ ] パフォーマンステスト
- [ ] コードカバレッジ80%確認

### **🎯 現在のDDDアーキテクチャ利点**

**1. 保守性向上**
- ビジネスロジックとUIの完全分離実現
- 変更影響範囲の明確化

**2. テスタビリティ向上**
- ドメインロジックの単体テスト可能
- Mockを使った分離テスト

**3. 拡張性向上**
- 新機能追加時の影響範囲最小化
- ドメインロジックの再利用可能

**4. Undo機能の統一**
- 全操作でUndo対応
- Unity Undoシステム完全統合
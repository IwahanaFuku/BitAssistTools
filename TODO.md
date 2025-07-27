# コライダー配置ツール リファクタリングTODO

## 提案事項

- **`ColliderPlacementService`の役割見直し**
  - クラス名を`ColliderPlacementLogic`や`ColliderFactory`など、役割をより明確に表す名前に変更する。
  - または、ステートレスなドメインサービスとして位置づけ、その旨をコメントで明記する。

- **ViewModelとServiceの連携強化**
  - `IColliderPlacementService`インターフェースを定義し、DI（依存関係の注入）を導入する。
  - ViewModelはコンストラクタでインターフェースを受け取るように変更する。

- **`PointObjectInfo`クラスの配置場所見直し**
  - ViewModelの内部クラスから`BitAssistTools.Application.ViewModels`名前空間直下に移動する。

- **`ReorderableList`の責務分離**
  - `onAddCallback`でのインデックス更新など、UIロジックをViewModelに移動する。
  - `onChangedCallback`はViewModelのイベントを購読する形に変更する。

- **ユーティリティ機能の責務分離**
  - `ConvertSelectedBoxCollidersToTransform`などのロジックを`ColliderPlacementService`に移動する。
  - ViewModelにコマンド（メソッド）を実装し、Viewから呼び出す形にする。
  - ViewModelからUnity API（`Selection.gameObjects`など）への直接依存を減らす。

## 次のステップ

- 旧版の動作を完全に再現することに注力する。
- 上記の提案は、再現完了後のさらなる改善フェーズで検討する。

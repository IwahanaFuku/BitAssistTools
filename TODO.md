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

---

# Domain層リファクタリングTODO

## 課題

現在のDomain層には、`UnityEngine`への依存が複数存在する。これはDDDの原則に反しており、ドメインロジックの純粋性を損なっている。

- **Models**: `Vector3`, `Quaternion` などのUnityの型に依存。
- **Services (Interfaces)**: `GameObject`, `Transform` などのUnityの型に依存。

## 改善方針

Domain層をUnity APIから完全に独立させるためのリファクタリングを行う。

### タスク一覧

1.  **ドメインプリミティブの導入**:
    - `Vector3`をラップする`VectorValue`を作成する。
    - `Quaternion`をラップする`QuaternionValue`を作成する。
    - `Domain/Models/Common`のような共有ディレクトリに配置する。

2.  **エンティティIDの導入**:
    - `GameObject`や`Transform`を直接扱う代わりに、`EntityId` (Guidやintをラップ) を使用してドメイン内のオブジェクトを一意に識別する。

3.  **インターフェースの抽象化**:
    - `IObjectPlacementService`などのドメインサービスのインターフェースを修正し、`GameObject`や`Transform`の代わりに`EntityId`や`TransformData`などのドメインモデルを使用するように変更する。

4.  **Infrastructure層での型変換**:
    - Infrastructure層に、Unityの型 (`GameObject`, `Transform`) とDomain層の型 (`EntityId`, `TransformData`, `VectorValue`など) を相互に変換する責務を持たせる。
    - `UnityGameObjectRepository`のようなリポジトリ実装を作成し、`EntityId`から`GameObject`を取得するロジックを実装する。
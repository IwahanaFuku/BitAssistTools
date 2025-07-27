using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BitAssistTools.Domain.Models.Transform;
using BitAssistTools.Domain.Models.Placement;
using BitAssistTools.Application.Services;

namespace BitAssistTools.Application.ViewModels
{
    /// <summary>
    /// ポイントオブジェクト情報を格納するクラス
    /// </summary>
    [System.Serializable]
    public class PointObjectInfo
    {
        public GameObject GameObject;
    }

    /// <summary>
    /// ColliderPlacement機能のViewModel
    /// UI状態とビジネスロジックを分離
    /// </summary>
    public class ColliderPlacementViewModel
    {
        // UI状態管理
        public class UIState
        {
            public bool IsAutoUpdate { get; set; } = true;
            public bool IsPointListExpanded { get; set; } = true;
            public bool IsLineSettingExpanded { get; set; } = false;
            public bool IsConvertBoxColliderToTransformExpanded { get; set; } = true;
            public bool IsConvertTransformToBoxColliderExpanded { get; set; } = false;
            public bool IsRotationalFrequencyExpanded { get; set; } = true;
            
            public Vector2 ConnectionScrollPos { get; set; }
            public Vector2 UtilityScrollPos { get; set; }
        }

        // ボックス接続設定
        public class BoxConnectionSettings
        {
            public GameObject PointListParent { get; set; }
            public GameObject BoxListParent { get; set; }
            public bool IsClosed { get; set; } = false;
            public bool IsGenerateCorner { get; set; } = false;
            public AbstractPosition BoxAbstractPosition { get; set; } = AbstractPosition.Center;
            
            // スケール設定
            public float EdgeWidth { get; set; } = 1f;
            public float BoxHeight { get; set; } = 1f;
            public float CornerWidth { get; set; } = 1f;
            
            // その他
            public float RotationalFrequency { get; set; } = 360f;
            public int SpacePixel { get; set; } = 8;
        }

        // データ管理
        public class ColliderData
        {
            public List<PointObjectInfo> Points { get; set; } = new List<PointObjectInfo>();
            public List<GameObject> Edges { get; set; } = new List<GameObject>();
            public List<GameObject> Corners { get; set; } = new List<GameObject>();
        }

        // イベント定義
        public event Action OnDataChanged;
        public event Action OnUIStateChanged;
        public event Action<string> OnError;

        // プロパティ
        public UIState UI { get; private set; }
        public BoxConnectionSettings BoxSettings { get; private set; }
        public ColliderData Data { get; private set; }

        public ColliderPlacementViewModel()
        {
            UI = new UIState();
            BoxSettings = new BoxConnectionSettings();
            Data = new ColliderData();
        }

        /// <summary>
        /// ポイントを追加（旧実装互換）
        /// </summary>
        public void AddPoint()
        {
            GameObject newGameObject = null;
            
            if (BoxSettings.PointListParent != null)
            {
                // 既存の子オブジェクトから穴を埋める番号を見つける
                string baseName = "Point_";
                int index = 0;
                
                while (true)
                {
                    string candidateName = baseName + index;
                    Transform existingChild = BoxSettings.PointListParent.transform.Find(candidateName);
                    
                    if (existingChild == null)
                    {
                        break; // 穴がない場合、ループを終了
                    }
                    
                    index++;
                }
                
                // 新しいゲームオブジェクトの名前を設定
                string newName = baseName + index;
                newGameObject = new GameObject(newName);
                
                // 新しいゲームオブジェクトをポイントリストペアレントの子に設定
                newGameObject.transform.SetParent(BoxSettings.PointListParent.transform);
                
                // 最後のポイントの位置に配置（旧実装互換）
                if (Data.Points.Count > 0 && Data.Points.Last().GameObject != null)
                {
                    newGameObject.transform.position = Data.Points.Last().GameObject.transform.position;
                }
                
                // Undo登録
                UnityEditor.Undo.RegisterCreatedObjectUndo(newGameObject, "Add Point");
            }
            
            // レコーダブルオブジェクトのリストにも新しい要素を追加
            Data.Points.Add(new PointObjectInfo { GameObject = newGameObject });
            
            // 自動アップデートが有効な場合はボックス接続を生成
            if (UI.IsAutoUpdate)
            {
                GenerateBoxConnections();
            }
            
            NotifyDataChanged();
        }

        /// <summary>
        /// ポイントを削除
        /// </summary>
        public void RemovePointAt(int index)
        {
            if (index >= 0 && index < Data.Points.Count)
            {
                Data.Points.RemoveAt(index);
                
                // 自動アップデートが有効な場合はボックス接続を再生成
                if (UI.IsAutoUpdate)
                {
                    GenerateBoxConnections();
                }
                
                NotifyDataChanged();
            }
        }

        /// <summary>
        /// ポイントを移動（削除済み - ReorderableListの自動並び替えを使用）
        /// 旧版互換のため、ReorderableListに完全に委ねる実装に変更
        /// </summary>
        [System.Obsolete("ReorderableListの自動並び替えを使用するため削除")]
        public void MovePoint(int fromIndex, int toIndex)
        {
            // 二重処理回避のため実装を削除
            // ReorderableListが自動的にリストを並び替えるため不要
        }

        /// <summary>
        /// ポイントのGameObjectを更新
        /// </summary>
        public void UpdatePointGameObject(int index, GameObject gameObject)
        {
            if (index >= 0 && index < Data.Points.Count)
            {
                Data.Points[index].GameObject = gameObject;
                
                // 自動アップデートが有効な場合はボックス接続を再生成
                if (UI.IsAutoUpdate)
                {
                    GenerateBoxConnections();
                }
                
                NotifyDataChanged();
            }
        }

        /// <summary>
        /// ポイントリストペアレントが変更された時の処理
        /// </summary>
        public void UpdatePointListParent(GameObject newParent)
        {
            if (BoxSettings.PointListParent != newParent)
            {
                BoxSettings.PointListParent = newParent;
                Data.Points.Clear();
                AddChildrenToPointsList();
                NotifyDataChanged();
            }
        }

        /// <summary>
        /// 子オブジェクトをポイントリストに追加
        /// </summary>
        private void AddChildrenToPointsList()
        {
            if (BoxSettings.PointListParent != null)
            {
                foreach (Transform child in BoxSettings.PointListParent.transform)
                {
                    Data.Points.Add(new PointObjectInfo { GameObject = child.gameObject });
                }
            }
        }

        /// <summary>
        /// ボックス設定が変更された時の処理
        /// </summary>
        public void UpdateBoxSettings(Action settingsUpdater)
        {
            settingsUpdater?.Invoke();
            
            if (UI.IsAutoUpdate)
            {
                // 設定変更時は常に再生成（既存更新ではなく）
                // 閉じる/開く、コーナー生成ON/OFFなどでボックス数が変わるため
                GenerateBoxConnections();
            }
            
            NotifyDataChanged();
        }

        /// <summary>
        /// 既存の接続を更新（旧ChangeListContentsTransform互換）
        /// </summary>
        public void UpdateExistingConnections()
        {
            if (Data.Edges != null && Data.Edges.Count > 0)
            {
                UpdateExistingEdges();
            }
            else
            {
                GenerateBoxConnections();
            }
        }

        /// <summary>
        /// 既存のエッジを更新
        /// </summary>
        private void UpdateExistingEdges()
        {
            var validPoints = Data.Points.Where(p => p.GameObject != null).Select(p => p.GameObject).ToList();
            
            if (validPoints.Count < 2)
            {
                ClearExistingConnections();
                return;
            }

            // エッジの更新
            UpdateEdgeTransforms(validPoints);
            
            // コーナーの更新
            if (BoxSettings.IsGenerateCorner)
            {
                UpdateCornerTransforms(validPoints);
            }
            else
            {
                // コーナー生成が無効になった場合は削除
                ClearGameObjectList(Data.Corners);
                Data.Corners = new List<GameObject>();
            }
        }

        /// <summary>
        /// エッジのTransformを更新
        /// </summary>
        private void UpdateEdgeTransforms(List<GameObject> validPoints)
        {
            if (Data.Edges == null) return;

            for (int i = 0; i < Data.Edges.Count && i < GetExpectedEdgeCount(validPoints.Count); i++)
            {
                var edge = Data.Edges[i];
                if (edge == null) continue;

                var (pos1, pos2) = GetEdgePositions(validPoints, i);
                UpdateEdgeTransform(edge, pos1, pos2);
            }
        }

        /// <summary>
        /// コーナーのTransformを更新
        /// </summary>
        private void UpdateCornerTransforms(List<GameObject> validPoints)
        {
            if (Data.Corners == null) 
            {
                Data.Corners = new List<GameObject>();
            }

            // 必要な数のコーナーがない場合は再生成
            int expectedCornerCount = GetExpectedCornerCount(validPoints.Count);
            if (Data.Corners.Count != expectedCornerCount)
            {
                ClearGameObjectList(Data.Corners);
                Data.Corners = ColliderPlacementService.GenerateCornerColliders(
                    validPoints,
                    BoxSettings.BoxListParent,
                    BoxSettings.IsClosed,
                    BoxSettings.CornerWidth,
                    BoxSettings.EdgeWidth,
                    BoxSettings.BoxHeight,
                    BoxSettings.BoxAbstractPosition
                );
                return;
            }

            // 既存のコーナーを更新
            for (int i = 0; i < Data.Corners.Count; i++)
            {
                var corner = Data.Corners[i];
                if (corner == null) continue;

                var (pos1, pos2, pos3) = GetCornerPositions(validPoints, i);
                UpdateCornerTransform(corner, pos1, pos2, pos3);
            }
        }

        /// <summary>
        /// エッジのTransformを更新
        /// </summary>
        private void UpdateEdgeTransform(GameObject edge, Vector3 pos1, Vector3 pos2)
        {
            float size = Vector3.Distance(pos1, pos2);
            
            edge.transform.position = (pos1 + pos2) / 2;
            edge.transform.LookAt(pos2);
            edge.transform.localScale = new Vector3(BoxSettings.EdgeWidth, BoxSettings.BoxHeight, size);

            edge.transform.position = pos1 + (edge.transform.forward * (size / 2.0f)) + (edge.transform.up * (BoxSettings.BoxHeight / 2.0f));

            switch (BoxSettings.BoxAbstractPosition)
            {
                case AbstractPosition.Leftside:
                    edge.transform.position -= edge.transform.right * (BoxSettings.EdgeWidth / 2.0f);
                    break;
                case AbstractPosition.Rightside:
                    edge.transform.position += edge.transform.right * (BoxSettings.EdgeWidth / 2.0f);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// コーナーのTransformを更新
        /// </summary>
        private void UpdateCornerTransform(GameObject corner, Vector3 beforeVec, Vector3 mainVec, Vector3 afterVec)
        {
            Vector3 vecA = (beforeVec - mainVec).normalized;
            Vector3 vecB = (afterVec - mainVec).normalized;
            float angle = Vector3.Angle(vecA, vecB) * Mathf.Deg2Rad / 2;
            float length = ColliderPlacementService.CalculateAdjacentSideLength(BoxSettings.CornerWidth / 2, angle);
            Vector3 middleVector = (vecA + vecB) / 2.0f;

            corner.transform.position = mainVec;

            if (length == Mathf.Infinity)
            {
                length = 0;
            }

            corner.transform.localScale = new Vector3(BoxSettings.CornerWidth, BoxSettings.BoxHeight, length);
            corner.transform.LookAt(corner.transform.position + middleVector);

            corner.transform.position += corner.transform.up * (BoxSettings.BoxHeight / 2.0f) + corner.transform.forward * length / 2;

            // 以下、旧実装と同じロジック...
            float hypotenuse = BoxSettings.EdgeWidth / Mathf.Sin(angle);

            if (beforeVec == mainVec || mainVec == afterVec || beforeVec == afterVec)
            {
                return;
            }

            if (!ColliderPlacementService.IsVector3COnRight(beforeVec, mainVec, afterVec))
            {
                switch (BoxSettings.BoxAbstractPosition)
                {
                    case AbstractPosition.Leftside:
                        break;
                    case AbstractPosition.Center:
                        corner.transform.position += corner.transform.forward * hypotenuse / 2;
                        break;
                    case AbstractPosition.Rightside:
                        corner.transform.position += corner.transform.forward * hypotenuse;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (BoxSettings.BoxAbstractPosition)
                {
                    case AbstractPosition.Leftside:
                        corner.transform.position += corner.transform.forward * hypotenuse;
                        break;
                    case AbstractPosition.Center:
                        corner.transform.position += corner.transform.forward * hypotenuse / 2;
                        break;
                    case AbstractPosition.Rightside:
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// エッジの期待される数を計算
        /// </summary>
        private int GetExpectedEdgeCount(int pointCount)
        {
            if (pointCount < 2) return 0;
            return BoxSettings.IsClosed ? pointCount : pointCount - 1;
        }

        /// <summary>
        /// コーナーの期待される数を計算
        /// </summary>
        private int GetExpectedCornerCount(int pointCount)
        {
            if (pointCount < 3) return 0;
            return BoxSettings.IsClosed ? pointCount : pointCount - 2;
        }

        /// <summary>
        /// エッジの位置を取得
        /// </summary>
        private (Vector3 pos1, Vector3 pos2) GetEdgePositions(List<GameObject> validPoints, int edgeIndex)
        {
            if (BoxSettings.IsClosed)
            {
                int fromIndex = edgeIndex;
                int toIndex = (edgeIndex + 1) % validPoints.Count;
                return (validPoints[fromIndex].transform.position, validPoints[toIndex].transform.position);
            }
            else
            {
                return (validPoints[edgeIndex].transform.position, validPoints[edgeIndex + 1].transform.position);
            }
        }

        /// <summary>
        /// コーナーの位置を取得
        /// </summary>
        private (Vector3 pos1, Vector3 pos2, Vector3 pos3) GetCornerPositions(List<GameObject> validPoints, int cornerIndex)
        {
            if (BoxSettings.IsClosed)
            {
                int prevIndex = (cornerIndex - 1 + validPoints.Count) % validPoints.Count;
                int mainIndex = cornerIndex;
                int nextIndex = (cornerIndex + 1) % validPoints.Count;
                return (
                    validPoints[prevIndex].transform.position,
                    validPoints[mainIndex].transform.position,
                    validPoints[nextIndex].transform.position
                );
            }
            else
            {
                int mainIndex = cornerIndex + 1; // コーナーは1番目から始まる
                return (
                    validPoints[mainIndex - 1].transform.position,
                    validPoints[mainIndex].transform.position,
                    validPoints[mainIndex + 1].transform.position
                );
            }
        }

        /// <summary>
        /// ボックス接続を生成（Undo対応・グループ化）
        /// </summary>
        public void GenerateBoxConnections()
        {
            try
            {
                // Undoグループ化開始
                UnityEditor.Undo.SetCurrentGroupName("Generate Box Connections");
                int undoGroup = UnityEditor.Undo.GetCurrentGroup();
                
                ClearExistingConnections();
                
                // ポイントが2個未満の場合は静かに終了（エラーメッセージなし）
                if (Data.Points.Count < 2)
                {
                    UnityEditor.Undo.CollapseUndoOperations(undoGroup);
                    return;
                }

                CreateEdgeConnections();
                
                if (BoxSettings.IsGenerateCorner)
                {
                    CreateCornerConnections();
                }
                
                // Undoグループ化終了
                UnityEditor.Undo.CollapseUndoOperations(undoGroup);
                
                NotifyDataChanged();
            }
            catch (Exception ex)
            {
                NotifyError($"ボックス接続生成中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 既存の接続をクリア
        /// </summary>
        private void ClearExistingConnections()
        {
            ClearGameObjectList(Data.Edges);
            ClearGameObjectList(Data.Corners);
        }

        /// <summary>
        /// GameObjectリストをクリア（Undo対応）
        /// </summary>
        private void ClearGameObjectList(List<GameObject> list)
        {
            if (list != null && list.Count > 0)
            {
                var objectsToDestroy = list.Where(obj => obj != null).ToArray();
                if (objectsToDestroy.Length > 0)
                {
                    UnityEditor.Undo.RecordObjects(objectsToDestroy, "Clear Box Connections");
                    foreach (var obj in objectsToDestroy)
                    {
                        UnityEditor.Undo.DestroyObjectImmediate(obj);
                    }
                }
                list.Clear();
            }
        }

        /// <summary>
        /// エッジ接続を作成
        /// </summary>
        private void CreateEdgeConnections()
        {
            var validPoints = Data.Points.Where(p => p.GameObject != null).Select(p => p.GameObject).ToList();
            
            if (validPoints.Count < 2)
                return;

            Data.Edges = ColliderPlacementService.GenerateEdgeColliders(
                validPoints,
                BoxSettings.BoxListParent,
                BoxSettings.IsClosed,
                BoxSettings.EdgeWidth,
                BoxSettings.BoxHeight,
                BoxSettings.BoxAbstractPosition
            );
        }

        /// <summary>
        /// コーナー接続を作成
        /// </summary>
        private void CreateCornerConnections()
        {
            var validPoints = Data.Points.Where(p => p.GameObject != null).Select(p => p.GameObject).ToList();
            
            if (validPoints.Count < 3)
                return;

            Data.Corners = ColliderPlacementService.GenerateCornerColliders(
                validPoints,
                BoxSettings.BoxListParent,
                BoxSettings.IsClosed,
                BoxSettings.CornerWidth,
                BoxSettings.EdgeWidth,
                BoxSettings.BoxHeight,
                BoxSettings.BoxAbstractPosition
            );
        }

        /// <summary>
        /// データ変更通知
        /// </summary>
        public void NotifyDataChanged()
        {
            OnDataChanged?.Invoke();
        }

        /// <summary>
        /// UI状態変更通知
        /// </summary>
        private void NotifyUIStateChanged()
        {
            OnUIStateChanged?.Invoke();
        }

        /// <summary>
        /// エラー通知
        /// </summary>
        private void NotifyError(string message)
        {
            OnError?.Invoke(message);
        }

        /// <summary>
        /// リソースのクリーンアップ
        /// </summary>
        public void Cleanup()
        {
            ClearExistingConnections();
        }
    }
}
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

namespace BitAssistTools.Editor.EditorCommands
{
    /// <summary>
    /// Transform操作に関する汎用ユーティリティ
    /// </summary>
    public static class TransformUtilities
    {
        /// <summary>
        /// リセット済みの空のGameObjectを作成
        /// </summary>
        [MenuItem("GameObject/Create Reset Empty", false, -1)]
        public static void CreateResetEmpty(MenuCommand menuCommand)
        {
            Transform parentTransform = null;

            // Prefabステージでの処理
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                parentTransform = prefabStage.prefabContentsRoot.transform;
            }
            
            // 選択されたオブジェクトを親にする
            if (UnityEditor.Selection.activeTransform != null)
            {
                GameObject parentObject = UnityEditor.Selection.activeGameObject;
                parentTransform = parentObject != null ? parentObject.transform : null;
            }

            // リセット済みGameObjectを作成
            GameObject gameObject = new GameObject("GameObject");
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;

            if (parentTransform != null)
            {
                gameObject.transform.SetParent(parentTransform, false);
            }

            UnityEditor.Selection.activeGameObject = gameObject;
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Reset Empty");
        }

        /// <summary>
        /// 親の位置のみをリセット（子は元の位置を維持）
        /// </summary>
        [MenuItem("CONTEXT/Transform/Reset Position Only Parent")]
        public static void ResetPositionOnlyParent(MenuCommand command)
        {
            var parent = command.context as Transform;
            if (parent == null) return;

            var tempParent = new GameObject("TempParent");
            var children = parent.Cast<Transform>().ToList();

            try
            {
                Undo.SetCurrentGroupName("Reset Position Only Parent");
                int undoGroup = Undo.GetCurrentGroup();

                // 子を一時親に移動
                foreach (var child in children)
                {
                    Undo.SetTransformParent(child, tempParent.transform, "Reset Position Only Parent");
                }

                // 親の位置をリセット
                Undo.RecordObject(parent, "Reset Position Only Parent");
                parent.position = Vector3.zero;

                // 子を元の親に戻す
                foreach (var child in children)
                {
                    Undo.SetTransformParent(child, parent, "Reset Position Only Parent");
                }

                Undo.CollapseUndoOperations(undoGroup);
            }
            finally
            {
                // 一時オブジェクトを削除
                if (tempParent != null)
                {
                    Object.DestroyImmediate(tempParent);
                }
            }
        }

        /// <summary>
        /// 選択されたTransformを中央でグループ化（ショートカット版）
        /// </summary>
        [MenuItem("Bit Assist Tools/カスタム ショートカット/グループを生成 %g", priority = 100)]
        public static void CreateCenterGroup()
        {
            var selectedTransforms = UnityEditor.Selection.transforms;
            if (selectedTransforms == null || selectedTransforms.Length == 0)
            {
                Debug.LogWarning("グループ化するTransformを選択してください。");
                return;
            }

            CreateCenterGroupFromTransforms(selectedTransforms);
        }

        /// <summary>
        /// 指定されたTransform配列を中央でグループ化
        /// </summary>
        public static void CreateCenterGroupFromTransforms(Transform[] transforms)
        {
            if (transforms == null || transforms.Length == 0) return;

            int firstIndex = transforms.Min(t => t.GetSiblingIndex());

            // 中心点計算
            Vector3 center = Vector3.zero;
            foreach (Transform transform in transforms)
            {
                center += transform.position;
            }
            center /= transforms.Length;

            // 親の座標系に変換
            var parent = UnityEditor.Selection.activeTransform?.parent;
            if (parent != null)
            {
                center = parent.InverseTransformPoint(center);
            }

            // グループ作成
            var group = new GameObject("Group");
            group.transform.position = center;
            Undo.RegisterCreatedObjectUndo(group, "Group Selected");
            group.transform.SetSiblingIndex(firstIndex);

            // 親設定とグループ化
            group.transform.SetParent(parent, false);
            foreach (var transform in transforms.OrderBy(t => t.GetSiblingIndex()))
            {
                Undo.SetTransformParent(transform, group.transform, "Group Selected");
            }
            
            UnityEditor.Selection.activeGameObject = group;
        }
    }
}
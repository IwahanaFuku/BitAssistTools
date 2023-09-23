using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

namespace ColliderPlacemenAssistancer
{
    // カスタムクラスを定義
    [System.Serializable]
    public class GameObjectInfo
    {
        [SerializeField] // シリアライズ属性を追加
        private GameObject _gameObject;

        // ゲッターとセッターを追加
        public GameObject GameObject
        {
            get { return _gameObject; }
            set { _gameObject = value; }
        }
    }

    public class WallColliderPlacemenAssistanceEditorWindow : EditorWindow
    {
        private bool isPointListExpanded = true;
        GameObject pointListParent = null;
        private ReorderableList _reorderableList;
        private List<GameObjectInfo> _gameObjectList = new List<GameObjectInfo>(); // 表示する要素

        [MenuItem("MyTools/ウォールコライダー配置支援ツール", priority = 11)]
        public static void ShowWindow()
        {
            GetWindow<WallColliderPlacemenAssistanceEditorWindow>("コライダー配置支援ツール");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(); // 間隔を空ける

            // シーン上のオブジェクトを選択可能にする
            GameObject newPointListParent = (GameObject)EditorGUILayout.ObjectField("ポイントリストペアレント", pointListParent, typeof(GameObject), true);

            if (newPointListParent != pointListParent)
            {
                // ポイントリストペアレントが変更された場合、既存のリストを破棄して新しいリストを構築
                pointListParent = newPointListParent;
                _gameObjectList.Clear(); // 既存のリストをクリア
                AddChildrenToGameObjectList(); // 新しいリストを構築
            }

            if (GUILayout.Button("ポイントリストペアレントを追加"))
            {
                CreatePointListParent();
            }

            isPointListExpanded = EditorGUILayout.Foldout(isPointListExpanded, "ポイントリスト");
            if (isPointListExpanded)
            {
                _reorderableList.DoLayoutList();
            }

            if (GUILayout.Button("決定"))
            {
                OutputListContents();
            }
        }

        private void OnEnable()
        {
            _reorderableList = new ReorderableList
                (
                elements: _gameObjectList,
                elementType: typeof(GameObjectInfo),
                draggable: true,
                displayHeader: false,
                displayAddButton: true,
                displayRemoveButton: true
                );

            _reorderableList.drawElementCallback = DrawElementCallback;
            _reorderableList.onAddCallback += Add;
            _reorderableList.onRemoveCallback += Remove;
        }

        private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            float originalWidth = rect.width;
            int ratio = 6;
            
            // オブジェクトを選択するフィールドの幅を1/2に設定
            rect.width = originalWidth * (ratio - 1) / ratio;
            rect.x += originalWidth / ratio;

            // オブジェクトを選択するフィールドを描画
            _gameObjectList[index].GameObject = (GameObject)EditorGUI.ObjectField(rect, _gameObjectList[index].GameObject, typeof(GameObject), true);

            // rectを元の幅に戻す
            rect.width = originalWidth;
        }

        private void Add(ReorderableList list)
        {
            string baseName = "Point_";
            int index = 0;

            if (pointListParent != null)
            {
                // 既存の子オブジェクトから穴を埋める番号を見つける
                while (true)
                {
                    string candidateName = baseName + index;
                    Transform existingChild = pointListParent.transform.Find(candidateName);

                    if (existingChild == null)
                    {
                        break; // 穴がない場合、ループを終了
                    }

                    index++;
                }

                // 新しいゲームオブジェクトの名前を設定
                string newName = baseName + index;
                GameObject newGameObject = new GameObject(newName);

                // 新しいゲームオブジェクトをポイントリストペアレントの子に設定
                newGameObject.transform.parent = pointListParent.transform;

                // レコーダブルオブジェクトのリストにも新しい要素を追加
                _gameObjectList.Add(new GameObjectInfo { GameObject = newGameObject });
            }
        }

        private void Remove(ReorderableList list)
        {
            int selectedIndex = _reorderableList.index;
            GameObjectInfo removedItem = _gameObjectList[selectedIndex];

            // ゲームオブジェクトをシーンから削除
            if (removedItem.GameObject != null)
            {
                Undo.DestroyObjectImmediate(removedItem.GameObject);
            }

            // リストから要素を削除
            _gameObjectList.RemoveAt(selectedIndex);

            if (selectedIndex > 0)
            {
                _reorderableList.index = selectedIndex - 1;
            }
            else if (_gameObjectList.Count > 0)
            {
                _reorderableList.index = 0;
            }
        }

        private void OutputListContents()
        {
            BoxColliderPlacementer.GenerateBoxesBetweenObjects(_gameObjectList.Select(info => info.GameObject).ToList());
            
            foreach (var gameObjectInfo in _gameObjectList)
            {
                Debug.Log("GameObject: " + gameObjectInfo.GameObject);
            }
        }


        private void CreatePointListParent()
        {
            GameObject newPointListParent = new GameObject("PointListParent");
            pointListParent = newPointListParent;
        }

        private void AddChildrenToGameObjectList()
        {
            if (pointListParent != null)
            {
                // ポイントリストペアレントの子オブジェクトをポイントリストに追加
                Transform[] children = pointListParent.GetComponentsInChildren<Transform>(true);

                foreach (var child in children)
                {
                    if (child.gameObject != pointListParent)
                    {
                        _gameObjectList.Add(new GameObjectInfo { GameObject = child.gameObject });
                    }
                }
            }
        }
    }
}

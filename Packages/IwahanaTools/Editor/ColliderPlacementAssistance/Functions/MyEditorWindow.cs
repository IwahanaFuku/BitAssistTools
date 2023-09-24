using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

namespace ColliderPlacemenAssistancer
{
    // カスタムクラスを定義
    [System.Serializable]
    public class PointObjectInfo
    {
        [SerializeField] // シリアライズ属性を追加
        GameObject _gameObject;

        // ゲッターとセッターを追加
        public GameObject GameObject
        {
            get { return _gameObject; }
            set { _gameObject = value; }
        }
    }
    public enum AbstractPosition
    {
        Inside,
        Center,
        Outside
    }

    public struct PlacementType
    {
        public bool Inside;
        public bool Outside;
    }

    public class WallColliderPlacemenAssistanceEditorWindow : EditorWindow
    {
        int spacePixel = 8;
        bool isAutoUpdate_ = true;
        bool isPointListExpanded_ = true;
        bool isLineSettingExpanded_ = false;
        bool isCornerSettingExpanded_ = false;
        PlacementType placementType = new PlacementType {Inside = false, Outside = false};
        GameObject pointListParent_ = null;
        GameObject boxListParent_ = null;
        ReorderableList reorderableList_;
        List<PointObjectInfo> pointObjectList_ = new List<PointObjectInfo>(); // 表示する要素
        bool isClosed_ = false;

        float boxWidth_ = 1f;
        float boxHeight_ = 1f;
        
        AbstractPosition boxColliderAbstractPosition_ = AbstractPosition.Center;

        [MenuItem("MyTools/ウォールコライダー配置支援ツール", priority = 11)]
        public static void ShowWindow()
        {
            GetWindow<WallColliderPlacemenAssistanceEditorWindow>("コライダー配置支援ツール");
        }

        void OnGUI()
        {
            isAutoUpdate_ = EditorGUILayout.Toggle("自動アップデート", isAutoUpdate_);

            GUILayout.Space(spacePixel);

            // シーン上のオブジェクトを選択可能にする
            GameObject pointListParent = (GameObject)EditorGUILayout.ObjectField("ポイントリストペアレント", pointListParent_, typeof(GameObject), true);

            if (pointListParent != pointListParent_)
            {
                // ポイントリストペアレントが変更された場合、既存のリストを破棄して新しいリストを構築
                pointListParent_ = pointListParent;
                pointObjectList_.Clear(); // 既存のリストをクリア
                AddChildrenToGameObjectList(); // 新しいリストを構築
            }

            if (GUILayout.Button("ポイントリストペアレントを追加"))
            {
                CreatepointListParent();
            }

            boxListParent_ = (GameObject)EditorGUILayout.ObjectField("ボックスリストペアレント", boxListParent_, typeof(GameObject), true);

            if (GUILayout.Button("ボックスペアレントを追加"))
            {
                CreateBoxListParent();
            }

            isPointListExpanded_ = EditorGUILayout.Foldout(isPointListExpanded_, "ポイントリスト");
            if (isPointListExpanded_)
            {
                reorderableList_.DoLayoutList();
            }

            isLineSettingExpanded_ = EditorGUILayout.Foldout(isLineSettingExpanded_, "ラインの設定");
            if (isLineSettingExpanded_)
            {
                GUILayout.Label("基準");
                boxColliderAbstractPosition_ = (AbstractPosition)GUILayout.SelectionGrid((int)boxColliderAbstractPosition_, 
                    new string[] { "左揃え", "中心", "右揃え" }, 3);

                GUILayout.Space(spacePixel);

                GUILayout.Label("ボックスのスケール");
                boxWidth_ = EditorGUILayout.Slider("  幅", boxWidth_, 0f, 2f);
                boxHeight_ = EditorGUILayout.Slider("  高さ", boxHeight_, 0f, 10f);

                GUILayout.Space(spacePixel);

                isClosed_ = EditorGUILayout.Toggle("始点と終点を閉じる", isClosed_);
            }

            isCornerSettingExpanded_ = EditorGUILayout.Foldout(isCornerSettingExpanded_, "角の設定");
            if (isCornerSettingExpanded_)
            {
                GUILayout.Label("配置オプション");
                placementType.Inside = EditorGUILayout.Toggle("  内側", placementType.Inside);
                placementType.Outside = EditorGUILayout.Toggle("  外側", placementType.Outside);
            }

            if (GUILayout.Button("決定"))
            {
                OutputListContents();
            }
        }

        void OnEnable()
        {
            reorderableList_ = new ReorderableList
                (
                elements: pointObjectList_,
                elementType: typeof(PointObjectInfo),
                draggable: true,
                displayHeader: false,
                displayAddButton: true,
                displayRemoveButton: true
                );

            reorderableList_.drawElementCallback = DrawElementCallback;
            reorderableList_.onAddCallback += Add;
            reorderableList_.onRemoveCallback += Remove;
        }

        void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            float originalWidth = rect.width;
            int ratio = 6;
            
            // オブジェクトを選択するフィールドの幅を1/2に設定
            rect.width = originalWidth * (ratio - 1) / ratio;
            rect.x += originalWidth / ratio;

            // オブジェクトを選択するフィールドを描画
            pointObjectList_[index].GameObject = (GameObject)EditorGUI.ObjectField(rect, pointObjectList_[index].GameObject, typeof(GameObject), true);

            // rectを元の幅に戻す
            rect.width = originalWidth;
        }

        void Add(ReorderableList list)
        {
            string baseName = "Point_";
            int index = 0;

            if (pointListParent_ != null)
            {
                // 既存の子オブジェクトから穴を埋める番号を見つける
                while (true)
                {
                    string candidateName = baseName + index;
                    Transform existingChild = pointListParent_.transform.Find(candidateName);

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
                newGameObject.transform.parent = pointListParent_.transform;

                // レコーダブルオブジェクトのリストにも新しい要素を追加
                pointObjectList_.Add(new PointObjectInfo { GameObject = newGameObject });

            }
        }

        void Remove(ReorderableList list)
        {
            int selectedIndex = reorderableList_.index;
            PointObjectInfo removedItem = pointObjectList_[selectedIndex];

            // ゲームオブジェクトをシーンから削除
            if (removedItem.GameObject != null)
            {
                Undo.DestroyObjectImmediate(removedItem.GameObject);
            }

            // リストから要素を削除
            pointObjectList_.RemoveAt(selectedIndex);

            if (selectedIndex > 0)
            {
                reorderableList_.index = selectedIndex - 1;
            }
            else if (pointObjectList_.Count > 0)
            {
                reorderableList_.index = 0;
            }
        }

        void OutputListContents()
        {
            List<GameObject> objects = pointObjectList_.Select(info => info.GameObject).ToList();
            BoxColliderPlacementer.GenerateBoxesBetweenObjects(objects, isClosed_, boxWidth_, boxHeight_, boxColliderAbstractPosition_, boxListParent_);
        }

        void CreatepointListParent()
        {
            GameObject pointListParent = new GameObject("pointListParent_");
            pointListParent_ = pointListParent;
        }
        
        void CreateBoxListParent()
        {
            GameObject newBoxListParent = new GameObject("BoxListParent");
            boxListParent_ = newBoxListParent;
            // ここで新しいボックスペアレントをカスタマイズできる場合、例えば位置の設定などを行うことができます。
        }


        void AddChildrenToGameObjectList()
        {
            if (pointListParent_ != null)
            {
                // ポイントリストペアレントの子オブジェクトをポイントリストに追加
                Transform[] children = pointListParent_.GetComponentsInChildren<Transform>(true);

                foreach (var child in children)
                {
                    if (child.gameObject != pointListParent_)
                    {
                        pointObjectList_.Add(new PointObjectInfo { GameObject = child.gameObject });
                    }
                }
            }
        }
    }
}

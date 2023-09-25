using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

namespace ColliderPlacemenAssistancer
{
    public class WallColliderPlacemenAssistanceEditorWindow : EditorWindow
    {
        int spacePixel = 8;
        bool isAutoUpdate_ = false;
        bool isPointListExpanded_ = true;
        bool isLineSettingExpanded_ = false;
        bool isCornerSettingExpanded_ = false;
        PlacementType placementType = new PlacementType {Inside = false, Outside = false};
        GameObject pointListParent_ = null;
        GameObject boxListParent_ = null;
        ReorderableList reorderableList_;
        List<PointObjectInfo> pointObjectList_ = new List<PointObjectInfo>(); // 表示する要素
        List<GameObject> boxColliderObjects_ = null;
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
                bool isClosed = isClosed_; // 変更前の値を保持
                isClosed_ = EditorGUILayout.Toggle("始点と終点を閉じる", isClosed_);
                if (isClosed_ != isClosed)
                {
                    OutputListContents();
                }

                GUILayout.Space(spacePixel);
                
                GUILayout.Label("基準");
                AbstractPosition boxColliderAbstractPosition = boxColliderAbstractPosition_;
                boxColliderAbstractPosition_ = (AbstractPosition)GUILayout.SelectionGrid((int)boxColliderAbstractPosition_, 
                    new string[] { "左揃え", "中心", "右揃え" }, 3);

                if (boxColliderAbstractPosition_ != boxColliderAbstractPosition)
                {
                    OutputListContents();
                }
                GUILayout.Space(spacePixel);

                GUILayout.Label("ボックスのスケール");
                float boxWidth = boxWidth_;
                boxWidth_ = EditorGUILayout.Slider("  幅", boxWidth_, 0f, 2f);
                if (boxWidth_ != boxWidth)
                {
                    OutputListContents();
                }

                float boxHeight = boxHeight_;
                boxHeight_ = EditorGUILayout.Slider("  高さ", boxHeight_, 0f, 10f);
                if (boxHeight_ != boxHeight)
                {
                    OutputListContents();
                }
            }

            isCornerSettingExpanded_ = EditorGUILayout.Foldout(isCornerSettingExpanded_, "角の設定");
            if (isCornerSettingExpanded_)
            {
                GUILayout.Label("配置オプション");
                placementType.Inside = EditorGUILayout.Toggle("  内側", placementType.Inside);
                placementType.Outside = EditorGUILayout.Toggle("  外側", placementType.Outside);
            }

            if (GUILayout.Button("生成"))
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
            reorderableList_.onChangedCallback += OnChanged;

            EditorApplication.update += CheckObjectPositions;
        }

        void OnDisable()
        {
            // エディタのアップデートイベントから位置確認関数を削除
            EditorApplication.update -= CheckObjectPositions;
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
                if(pointObjectList_.Count() > 0)
                {
                    newGameObject.transform.position = pointObjectList_.Last().GameObject.transform.position;
                }

                // レコーダブルオブジェクトのリストにも新しい要素を追加
                pointObjectList_.Add(new PointObjectInfo { GameObject = newGameObject });

            }
        }

        void OnChanged(ReorderableList list)
        {
            if(isAutoUpdate_)
            {
                OutputListContents();
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
            // boxColliderObjects_ の要素をすべて削除
            if (boxColliderObjects_ != null)
            {
                foreach (var box in boxColliderObjects_)
                {
                    if (box != null)
                    {
                        Undo.DestroyObjectImmediate(box);
                    }
                }
                boxColliderObjects_.Clear();
            }

            List<GameObject> objects = pointObjectList_.Select(info => info.GameObject).ToList();
            boxColliderObjects_ = BoxColliderPlacementer.GenerateBoxesBetweenObjects(objects, boxListParent_, isClosed_, boxWidth_, boxHeight_, boxColliderAbstractPosition_);
        }

        void CreatepointListParent()
        {
            GameObject pointListParent = new GameObject("PointListParent");
            pointListParent_ = pointListParent;
        }
        
        void CreateBoxListParent()
        {
            GameObject newBoxListParent = new GameObject("BoxListParent");
            boxListParent_ = newBoxListParent;
        }
        
        private void CheckObjectPositions()
        {
            if (! isAutoUpdate_)
                return;

            if(pointObjectList_ == null)
                return;

            if(pointObjectList_.Count() < 2)
                return;

            for(int i = 0; i < pointObjectList_.Count(); i++)
            {
                if (pointObjectList_[i].GameObject == null || boxColliderObjects_ == null)
                    continue;

                
                if (pointObjectList_[i].GameObject.transform.hasChanged)
                {
                    if(i == 0)
                    {
                        if(isClosed_)
                        {
                            /* Debug.Log("pointObjectList_ の要素数: " + pointObjectList_.Count);
                            Debug.Log("boxColliderObjects_ の要素数: " + boxColliderObjects_.Count);
                            Debug.Log(pointObjectList_[i].GameObject.name + " の位置が変更されました。"); */
                            BoxColliderPlacementer.DistanceBoxBetweenObjects
                            (

                                boxColliderObjects_[pointObjectList_.Count() - 1],
                                boxListParent_,
                                pointObjectList_[boxColliderObjects_.Count() - 1].GameObject.transform.position,
                                pointObjectList_[i].GameObject.transform.position,
                                boxWidth_,
                                boxHeight_,
                                boxColliderAbstractPosition_
                            );
                        }

                        BoxColliderPlacementer.DistanceBoxBetweenObjects
                        (
                            boxColliderObjects_[i],
                            boxListParent_,
                            pointObjectList_[i].GameObject.transform.position,
                            pointObjectList_[i + 1].GameObject.transform.position,
                            boxWidth_,
                            boxHeight_,
                            boxColliderAbstractPosition_
                        );
                    }
                    else if(i == pointObjectList_.Count() - 1)
                    {
                        BoxColliderPlacementer.DistanceBoxBetweenObjects
                        (
                            boxColliderObjects_[i - 1],
                            boxListParent_,
                            pointObjectList_[i - 1].GameObject.transform.position,
                            pointObjectList_[i].GameObject.transform.position,
                            boxWidth_,
                            boxHeight_,
                            boxColliderAbstractPosition_
                        );

                        if(isClosed_)
                        {
                            BoxColliderPlacementer.DistanceBoxBetweenObjects
                            (
                                boxColliderObjects_[i],
                                boxListParent_,
                                pointObjectList_[i].GameObject.transform.position,
                                pointObjectList_[0].GameObject.transform.position,
                                boxWidth_,
                                boxHeight_,
                                boxColliderAbstractPosition_
                            );
                        }
                    }
                    else
                    {
                        BoxColliderPlacementer.DistanceBoxBetweenObjects
                        (
                            boxColliderObjects_[i - 1],
                            boxListParent_,
                            pointObjectList_[i - 1].GameObject.transform.position,
                            pointObjectList_[i].GameObject.transform.position,
                            boxWidth_,
                            boxHeight_,
                            boxColliderAbstractPosition_
                        );

                        BoxColliderPlacementer.DistanceBoxBetweenObjects
                        (
                            boxColliderObjects_[i],
                            boxListParent_,
                            pointObjectList_[i].GameObject.transform.position,
                            pointObjectList_[i + 1].GameObject.transform.position,
                            boxWidth_,
                            boxHeight_,
                            boxColliderAbstractPosition_
                        );
                    }
                    pointObjectList_[i].GameObject.transform.hasChanged = false;
                }
            }
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

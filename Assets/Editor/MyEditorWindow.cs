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
        GameObject pointListParent_ = null;
        GameObject boxListParent_ = null;
        ReorderableList reorderableList_;
        List<PointObjectInfo> points_ = new List<PointObjectInfo>();
        List<GameObject> edges_ = null;
        List<GameObject> corners_ = null;
        bool isClosed_ = false;
        bool isGenerateCorner_ = false;

        float edgeWidth_ = 1f;
        float boxHeight_ = 1f;

        float cornerWidth_ = 1f;
        
        AbstractPosition boxrAbsPos_ = AbstractPosition.Center;

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
                points_.Clear(); // 既存のリストをクリア
                AddChildrenToGameObjectList(); // 新しいリストを構築
            }

            boxListParent_ = (GameObject)EditorGUILayout.ObjectField("ボックスリストペアレント", boxListParent_, typeof(GameObject), true);

            if (GUILayout.Button("ポイントとボックスの親を追加"))
            {
                CreatepointListParent();
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
                AbstractPosition boxColliderAbstractPosition = boxrAbsPos_;
                boxrAbsPos_ = (AbstractPosition)GUILayout.SelectionGrid((int)boxrAbsPos_, 
                    new string[] { "左揃え", "中心", "右揃え" }, 3);

                if (boxrAbsPos_ != boxColliderAbstractPosition)
                {
                    OutputListContents();
                }
                GUILayout.Space(spacePixel);

                GUILayout.Label("ボックスのスケール");

                float boxWidth = edgeWidth_;
                edgeWidth_ = EditorGUILayout.Slider("  幅", edgeWidth_, 0f, 2f);
                if (edgeWidth_ != boxWidth)
                {
                    ChangeListContentsTransform();
                }

                float boxHeight = boxHeight_;
                boxHeight_ = EditorGUILayout.Slider("  高さ", boxHeight_, 0f, 10f);
                if (boxHeight_ != boxHeight)
                {
                    ChangeListContentsTransform();
                }

                GUILayout.Space(spacePixel);

                bool isGenerateCorner = isGenerateCorner_;
                isGenerateCorner_ = EditorGUILayout.Toggle("角を生成", isGenerateCorner_);
                if (isGenerateCorner_ != isGenerateCorner)
                {
                    OutputListContents();
                }

                float cornerWidth = cornerWidth_;
                cornerWidth_ = EditorGUILayout.Slider("  幅", cornerWidth_, 0f, 10f);
                if (cornerWidth_ != cornerWidth)
                {
                    ChangeListContentsTransform();
                }
            }

            if (GUILayout.Button("生成"))
            {
                OutputListContents();
            }
        }

        void OnEnable()
        {
            reorderableList_ = new ReorderableList(
                elements: points_,
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


        void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            float originalWidth = rect.width;
            int ratio = 8;
            
            // オブジェクトを選択するフィールドの幅を1/2に設定
            rect.width = originalWidth * (ratio - 1) / ratio;
            rect.x += originalWidth / ratio;

            // オブジェクトを選択するフィールドを描画
            points_[index].GameObject = (GameObject)EditorGUI.ObjectField(rect, points_[index].GameObject, typeof(GameObject), true);

            // rectを元の幅に戻す
            rect.width = originalWidth;

            // 要素の番号を表示
            GUI.Label(new Rect(rect.x - 30, rect.y, 20, EditorGUIUtility.singleLineHeight), index.ToString());
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
                if(points_.Count() > 0)
                {
                    newGameObject.transform.position = points_.Last().GameObject.transform.position;
                }

                // レコーダブルオブジェクトのリストにも新しい要素を追加
                points_.Add(new PointObjectInfo { GameObject = newGameObject });

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
            PointObjectInfo removedItem = points_[selectedIndex];

            // ゲームオブジェクトをシーンから削除
            if (removedItem.GameObject != null)
            {
                Undo.DestroyObjectImmediate(removedItem.GameObject);
            }

            // リストから要素を削除
            points_.RemoveAt(selectedIndex);

            if (selectedIndex > 0)
            {
                reorderableList_.index = selectedIndex - 1;
            }
            else if (points_.Count > 0)
            {
                reorderableList_.index = 0;
            }
        }

        void OutputListContents()
        {
            removeObjects(edges_);
            removeObjects(corners_);

            List<GameObject> objects = points_.Select(info => info.GameObject).ToList();

            edges_ = BoxColliderPlacementer.GenerateEdgeCollider(objects, boxListParent_, isClosed_, edgeWidth_, boxHeight_, boxrAbsPos_);
            if(isGenerateCorner_)
            {
                corners_ = BoxColliderPlacementer.GenerateCornerollider(objects, boxListParent_, isClosed_, cornerWidth_, boxHeight_, boxrAbsPos_);
            }
        }

        void ChangeListContentsTransform()
        {
            for(int i = 0; i < points_.Count(); i++)
            {
                ChangeEdgeTransform(i);
                if(isGenerateCorner_)
                {
                    ChangeCornerTransform(i);
                }
            }
        }

        void removeObjects(List<GameObject> removeObjects)
        {
            if (removeObjects != null)
            {
                foreach (var box in removeObjects)
                {
                    if (box != null)
                    {
                        Undo.DestroyObjectImmediate(box);
                    }
                }
                removeObjects.Clear();
            }
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

            if(points_ == null)
                return;

            if(points_.Count() < 2)
                return;

            for(int i = 0; i < points_.Count(); i++)
            {
                if (points_[i].GameObject == null || edges_ == null)
                    continue;
                
                if (points_[i].GameObject.transform.hasChanged)
                {
                    ChangeEdgeTransform(i);
                    ChangeCornerTransform(i);

                    points_[i].GameObject.transform.hasChanged = false;
                }
            }
        }

        private void ChangeEdgeTransform(int i)
        {
            Vector3 beforVec, mainVec, afterVec;
            beforVec = GetElementLoop(points_.ToArray(), i - 1).GameObject.transform.position;
            mainVec = GetElementLoop(points_.ToArray(), i).GameObject.transform.position;
            afterVec = GetElementLoop(points_.ToArray(), i + 1).GameObject.transform.position;
            
            GameObject beforEdge, afterEdge;
            beforEdge = GetElementLoop(edges_.ToArray(), i - 1);
            afterEdge = GetElementLoop(edges_.ToArray(), i);

            BoxColliderPlacementer.GenerateEdgeBox(beforEdge, boxListParent_, beforVec, mainVec, edgeWidth_, boxHeight_, boxrAbsPos_);
            BoxColliderPlacementer.GenerateEdgeBox(afterEdge, boxListParent_, mainVec, afterVec, edgeWidth_, boxHeight_, boxrAbsPos_);
        }

        private void ChangeCornerTransform(int i)
        {
            for(int j = -1; j <= 1; j++)
            {
                Vector3 beforVec, mainVec, afterVec;
                beforVec = GetElementLoop(points_.ToArray(), i - 1 + j).GameObject.transform.position;
                mainVec = GetElementLoop(points_.ToArray(), i + j).GameObject.transform.position;
                afterVec = GetElementLoop(points_.ToArray(), i + 1 + j).GameObject.transform.position;

                GameObject mainCorner = GetElementLoop(corners_.ToArray(), i + j);

                BoxColliderPlacementer.GenerateCornerBox(mainCorner, boxListParent_, beforVec, mainVec, afterVec, cornerWidth_, boxHeight_, boxrAbsPos_);
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
                        points_.Add(new PointObjectInfo { GameObject = child.gameObject });
                    }
                }
            }
        }

        public static T GetElementLoop<T>(T[] array, int index)
        {
            if (array == null || array.Length == 0)
            {
                // 配列がnullまたは空の場合はデフォルトの値を返す
                return default(T);
            }

            if (index < 0)
            {
                // インデックスが負の場合、配列の最後の要素に戻る
                return array[array.Length + index];
            }
            else if (index >= array.Length)
            {
                // インデックスが配列の長さ以上の場合、配列の最初の要素に戻る
                return array[index % array.Length];
            }
            else
            {
                // 範囲内のインデックスに対しては通常の要素を返す
                return array[index];
            }
        }
    }
}

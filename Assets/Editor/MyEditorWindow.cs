using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

namespace ColliderPlacemenAssistancer
{
    public class WallColliderPlacemenAssistanceEditorWindow : EditorWindow
    {
        int m_spacePixel = 8;
        bool m_isAutoUpdate = false;
        bool m_isPointListExpanded = true;
        bool m_isLineSettingExpanded = false;
        GameObject m_pointListParent = null;
        GameObject m_boxListParent = null;
        ReorderableList m_reorderableList;
        List<PointObjectInfo> m_points = new List<PointObjectInfo>();
        List<GameObject> m_edges = null;
        List<GameObject> m_corners = null;
        bool m_isClosed = false;
        bool m_isGenerateCorner = false;

        float m_edgeWidth = 1f;
        float m_boxHeight = 1f;

        float m_cornerWidth = 1f;
        
        AbstractPosition m_boxrAbsPos = AbstractPosition.Center;

        [MenuItem("MyTools/ウォールコライダー配置支援ツール", priority = 11)]
        public static void ShowWindow()
        {
            GetWindow<WallColliderPlacemenAssistanceEditorWindow>("コライダー配置支援ツール");
        }
        

        void OnGUI()
        {
            m_isAutoUpdate = EditorGUILayout.Toggle("自動アップデート", m_isAutoUpdate);
            GUILayout.Space(m_spacePixel);

            GameObject pointListParent = (GameObject)EditorGUILayout.ObjectField("ポイントリストペアレント", m_pointListParent, typeof(GameObject), true);

            if (pointListParent != m_pointListParent)
            {

                m_pointListParent = pointListParent;
                m_points.Clear();
                AddChildrenToGameObjectList();
            }

            m_boxListParent = (GameObject)EditorGUILayout.ObjectField("ボックスリストペアレント", m_boxListParent, typeof(GameObject), true);

            if (GUILayout.Button("ポイントとボックスの親を追加"))
            {
                CreatepointListParent();
                CreateBoxListParent();
            }

            m_isPointListExpanded = EditorGUILayout.Foldout(m_isPointListExpanded, "ポイントリスト");
            if (m_isPointListExpanded)
            {
                m_reorderableList.DoLayoutList();
            }

            m_isLineSettingExpanded = EditorGUILayout.Foldout(m_isLineSettingExpanded, "ラインの設定");
            if (m_isLineSettingExpanded)
            {
                bool isClosed = m_isClosed;
                m_isClosed = EditorGUILayout.Toggle("始点と終点を閉じる", m_isClosed);
                if (m_isClosed != isClosed)
                {
                    OutputListContents();
                }

                GUILayout.Space(m_spacePixel);
                
                GUILayout.Label("基準");
                AbstractPosition boxColliderAbstractPosition = m_boxrAbsPos;
                m_boxrAbsPos = (AbstractPosition)GUILayout.SelectionGrid((int)m_boxrAbsPos, 
                    new string[] { "左揃え", "中心", "右揃え" }, 3);

                if (m_boxrAbsPos != boxColliderAbstractPosition)
                {
                    OutputListContents();
                }
                GUILayout.Space(m_spacePixel);

                GUILayout.Label("ボックスのスケール");

                float boxWidth = m_edgeWidth;
                m_edgeWidth = EditorGUILayout.Slider("  幅", m_edgeWidth, 0f, 2f);
                if (m_edgeWidth != boxWidth)
                {
                    ChangeListContentsTransform();
                }

                float boxHeight = m_boxHeight;
                m_boxHeight = EditorGUILayout.Slider("  高さ", m_boxHeight, 0f, 10f);
                if (m_boxHeight != boxHeight)
                {
                    ChangeListContentsTransform();
                }

                GUILayout.Space(m_spacePixel);

                bool isGenerateCorner = m_isGenerateCorner;
                m_isGenerateCorner = EditorGUILayout.Toggle("角を生成", m_isGenerateCorner);
                if (m_isGenerateCorner != isGenerateCorner)
                {
                    OutputListContents();
                }

                float cornerWidth = m_cornerWidth;
                m_cornerWidth = EditorGUILayout.Slider("  幅", m_cornerWidth, 0f, 10f);
                if (m_cornerWidth != cornerWidth)
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
            m_reorderableList = new ReorderableList(
                elements: m_points,
                elementType: typeof(PointObjectInfo),
                draggable: true,
                displayHeader: false,
                displayAddButton: true,
                displayRemoveButton: true
            );

            m_reorderableList.drawElementCallback = DrawElementCallback;
            m_reorderableList.onAddCallback += Add;
            m_reorderableList.onRemoveCallback += Remove;
            m_reorderableList.onChangedCallback += OnChanged;

            EditorApplication.update += CheckObjectPositions;
        }


        void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            float originalWidth = rect.width;
            int ratio = 8;

            rect.width = originalWidth * (ratio - 1) / ratio;
            rect.x += originalWidth / ratio;

            m_points[index].GameObject = (GameObject)EditorGUI.ObjectField(rect, m_points[index].GameObject, typeof(GameObject), true);

            rect.width = originalWidth;

            GUI.Label(new Rect(rect.x - 30, rect.y, 20, EditorGUIUtility.singleLineHeight), index.ToString());
        }

        void Add(ReorderableList list)
        {
            string baseName = "Point_";
            int index = 0;

            if (m_pointListParent != null)
            {
                // 既存の子オブジェクトから穴を埋める番号を見つける
                while (true)
                {
                    string candidateName = baseName + index;
                    Transform existingChild = m_pointListParent.transform.Find(candidateName);

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
                newGameObject.transform.parent = m_pointListParent.transform;
                if(m_points.Count() > 0)
                {
                    newGameObject.transform.position = m_points.Last().GameObject.transform.position;
                }

                // レコーダブルオブジェクトのリストにも新しい要素を追加
                m_points.Add(new PointObjectInfo { GameObject = newGameObject });

            }
        }

        void OnChanged(ReorderableList list)
        {
            if(m_isAutoUpdate)
            {
                OutputListContents();
            }
        }

        void Remove(ReorderableList list)
        {
            int selectedIndex = m_reorderableList.index;
            PointObjectInfo removedItem = m_points[selectedIndex];

            if (removedItem.GameObject != null)
            {
                Undo.DestroyObjectImmediate(removedItem.GameObject);
            }

            m_points.RemoveAt(selectedIndex);

            if (selectedIndex > 0)
            {
                m_reorderableList.index = selectedIndex - 1;
            }
            else if (m_points.Count > 0)
            {
                m_reorderableList.index = 0;
            }
        }

        void OutputListContents()
        {
            removeObjects(m_edges);
            removeObjects(m_corners);

            List<GameObject> objects = m_points.Select(info => info.GameObject).ToList();

            m_edges = BoxColliderPlacementer.GenerateEdgeCollider(objects, m_boxListParent, m_isClosed, m_edgeWidth, m_boxHeight, m_boxrAbsPos);
            for(int i = 0; i < m_edges.Count(); i++)
            {
                m_edges[i].name = "Edge_" + i;
            }

            if(m_isGenerateCorner)
            {
                m_corners = BoxColliderPlacementer.GenerateCornerCollider(objects, m_boxListParent, m_isClosed, m_cornerWidth, m_edgeWidth, m_boxHeight, m_boxrAbsPos);

                for(int i = 0; i < m_corners.Count(); i++)
                {
                    m_corners[i].name = "Corner_" + i;
                }
            }
        }

        void ChangeListContentsTransform()
        {
            for(int i = 0; i < m_points.Count(); i++)
            {
                ChangeEdgeTransform(i);
                if(m_isGenerateCorner)
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
            Undo.RegisterCreatedObjectUndo(pointListParent, "Create Parent");
            m_pointListParent = pointListParent;
        }
        
        void CreateBoxListParent()
        {
            GameObject newBoxListParent = new GameObject("BoxListParent");
            Undo.RegisterCreatedObjectUndo(newBoxListParent, "Create Parent");
            m_boxListParent = newBoxListParent;
        }
        
        private void CheckObjectPositions()
        {
            if (! m_isAutoUpdate)
                return;

            if(m_points == null)
                return;

            if(m_points.Count() < 2)
                return;

            for(int i = 0; i < m_points.Count(); i++)
            {
                if (m_points[i].GameObject == null || m_edges == null)
                    continue;
                
                if (m_points[i].GameObject.transform.hasChanged)
                {
                    ChangeEdgeTransform(i);
                    ChangeCornerTransform(i);

                    m_points[i].GameObject.transform.hasChanged = false;
                }
            }
        }

        private void ChangeEdgeTransform(int i)
        {
            Vector3 beforVec, mainVec, afterVec;
            beforVec = GetElementLoop(m_points.ToArray(), i - 1).GameObject.transform.position;
            mainVec = GetElementLoop(m_points.ToArray(), i).GameObject.transform.position;
            afterVec = GetElementLoop(m_points.ToArray(), i + 1).GameObject.transform.position;
            
            GameObject beforEdge, afterEdge;
            beforEdge = GetElementLoop(m_edges.ToArray(), i - 1);
            afterEdge = GetElementLoop(m_edges.ToArray(), i);

            if(m_isClosed)
            {
                BoxColliderPlacementer.ChangeEdgeCollider(beforEdge, m_boxListParent, beforVec, mainVec, m_edgeWidth, m_boxHeight, m_boxrAbsPos);
                BoxColliderPlacementer.ChangeEdgeCollider(afterEdge, m_boxListParent, mainVec, afterVec, m_edgeWidth, m_boxHeight, m_boxrAbsPos);
            }
            else
            {
                //始点と終点がくっつかないようにする
                if(0 < i)
                {
                    BoxColliderPlacementer.ChangeEdgeCollider(beforEdge, m_boxListParent, beforVec, mainVec, m_edgeWidth, m_boxHeight, m_boxrAbsPos);
                }
                if(i < m_points.Count() - 1)
                {
                    BoxColliderPlacementer.ChangeEdgeCollider(afterEdge, m_boxListParent, mainVec, afterVec, m_edgeWidth, m_boxHeight, m_boxrAbsPos);
                }
            }
        }

        void ChangeCornerTransform(int i)
        {
            if (m_corners == null)
            {
                m_corners = new List<GameObject>();
                return;
            }

            for (int j = -1; j <= 1; j++)
            {
                Vector3 beforVec, mainVec, afterVec;
                beforVec = GetElementLoop(m_points.ToArray(), i - 1 + j).GameObject.transform.position;
                mainVec = GetElementLoop(m_points.ToArray(), i + j).GameObject.transform.position;
                afterVec = GetElementLoop(m_points.ToArray(), i + 1 + j).GameObject.transform.position;

                if(m_isClosed)
                {
                    GameObject mainCorner = GetElementLoop(m_corners.ToArray(), i + j);
                    BoxColliderPlacementer.ChangeCornerCollider(mainCorner, m_boxListParent, beforVec, mainVec, afterVec, m_cornerWidth, m_edgeWidth, m_boxHeight, m_boxrAbsPos);
                }
                else
                {
                    GameObject mainCorner = GetElementLoop(m_corners.ToArray(), i + j - 1);
                    int numOfConer = m_points.Count() - 1;

                    //始点と終点のフィルター
                    if(i == 0 && j < 1) continue;
                    if(i == 1 && j < 0) continue;
                    if(i == numOfConer - 1 && 0 < j) continue;
                    if(i == numOfConer&& -1 < j) continue;
                    
                    BoxColliderPlacementer.ChangeCornerCollider(mainCorner, m_boxListParent, beforVec, mainVec, afterVec, m_cornerWidth, m_edgeWidth, m_boxHeight, m_boxrAbsPos);
                }
            }
        }

        void AddChildrenToGameObjectList()
        {
            if (m_pointListParent != null)
            {
                Transform[] children = m_pointListParent.GetComponentsInChildren<Transform>(true);

                foreach (var child in children)
                {
                    if (child.gameObject != m_pointListParent)
                    {
                        m_points.Add(new PointObjectInfo { GameObject = child.gameObject });
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
                return array[index];
            }
        }
    }
}

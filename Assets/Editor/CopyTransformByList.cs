//https://neutrino-gamma.booth.pm/items/1465964 を少し改変。Iwahana 220625

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CopyTransformByList : EditorWindow
{
    private Color _bgColor = new Color(1.3f, 1.3f, 1.3f, 1f);
    private const string KEY_OBJECT = "Object";
    private const string KEY_TRANSFORM = "Transform";
    private const string KEY_IGNORE = "Ignore";
    private GameObject obj1;
    private GameObject obj2;
    private GameObject obj1Select;
    private GameObject obj2Select;
    // 基本設定
    private bool nameNoCheck = false;
    private bool ignoreInactive = true;
    private bool local = true;
    private bool copyPosition = true;
    private bool copyRotation = true;
    private bool copyScale = true;

    private bool hideInactive = false;
    private Vector2 size = new Vector2(1000, 510);

    private List<Hashtable> fromTableList = new List<Hashtable>();
    private List<Hashtable> toTableList = new List<Hashtable>();
    private Vector2 fromScroll = Vector2.zero;
    private Vector2 toScroll = Vector2.zero;
    private float originalValue = EditorGUIUtility.labelWidth;
    private string filterFrom = "";
    private string filterTo = "";

    private bool isClose = false;

    /// <summary>
    /// 上部メニューで出る
    /// </summary>
    [MenuItem("Iwahana Tools/ツリートランスフォームのコピー", false, 50)]
    static void Init()
    {
        CopyTransformByList window = EditorWindow.GetWindow<CopyTransformByList>();
    }

    private void Awake()
    {
        obj1 = Selection.gameObjects[0];
        obj2 = Selection.gameObjects[1];
    }

    private void OnGUI()
    {
        this.minSize = size;
        this.maxSize = size;

        // 対象
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(400), GUILayout.MaxHeight(480));
        EditorGUILayout.LabelField("コピー元からコピー先へTransformをコピーします");
        GUILayout.Space(5);
        EditorGUIUtility.labelWidth = 150;
        EditorGUIUtility.labelWidth = originalValue;
        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        obj1 = (GameObject)EditorGUILayout.ObjectField("コピー元", obj1, typeof(GameObject), true);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        obj2 = (GameObject)EditorGUILayout.ObjectField("コピー先", obj2, typeof(GameObject), true);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("反転"))
            {
                (obj1, obj2) = (obj2, obj1);
            };

        separator();
        EditorGUIUtility.labelWidth = originalValue;
        EditorGUILayout.LabelField("基本設定");
        separator();

        EditorGUIUtility.labelWidth = 360;
        local = EditorGUILayout.Toggle("Local", local);
        copyPosition = EditorGUILayout.Toggle("Position", copyPosition);
        copyRotation = EditorGUILayout.Toggle("Rotation", copyRotation);
        copyScale = EditorGUILayout.Toggle("Scale", copyScale);
        GUILayout.Space(5);
        ignoreInactive = EditorGUILayout.Toggle("非活性オブジェクト除外", ignoreInactive);
        nameNoCheck = EditorGUILayout.Toggle("名前一致確認無し", nameNoCheck);


        EditorGUILayout.EndVertical();

        if (obj1 != obj1Select || fromTableList.Count == 0)
        {
            obj1Select = obj1;
            fromTableList = CreateTableList(obj1);
        }

        if (obj2 != obj2Select || toTableList.Count == 0)
        {
            obj2Select = obj2;
            toTableList = CreateTableList(obj2);
        }
        CreateListView(ref fromTableList, ref fromScroll, "コピー元", ref filterFrom);
        CreateListView(ref toTableList, ref toScroll, "コピー先", ref filterTo);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy Transform"))
        {
            isClose = false;
            Make();
        } 
        if (GUILayout.Button("Copy Transform And Close"))
        {
            isClose = true;
            Make();
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 区切り線
    /// </summary>
    private void separator()
    {
#if UNITY_2019_1_OR_NEWER
        GUI.backgroundColor = Color.black;
#endif
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1f));
#if UNITY_2019_1_OR_NEWER
        GUI.backgroundColor = _bgColor;
#endif
    }

    private List<Hashtable> CreateTableList(GameObject obj)
    {
        List<Hashtable> result = new List<Hashtable>();
        if (obj != null) {
            Transform[] transforms;
            transforms = obj.GetComponentsInChildren<Transform>();
            if (ignoreInactive)
            {
                transforms = RemoveInactive(transforms);
            }

            int cnt = 0;
            foreach(Transform tran in transforms)
            {
                Hashtable work = new Hashtable();
                work[KEY_OBJECT] = tran.gameObject;
                work[KEY_IGNORE] = false;
                work[KEY_TRANSFORM] = tran;
                result.Add(work);
                cnt++;
            }
        }
        
        return result;
    }

    private void CreateListView(ref List<Hashtable> list, ref Vector2 scroll, string label, ref string filterString)
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MinWidth(300), GUILayout.MaxWidth(300), GUILayout.MaxHeight(480));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.MaxWidth(50));
        filterString = EditorGUILayout.TextField(filterString);
        EditorGUILayout.LabelField("総件数：" + list.Count().ToString() + "件", GUILayout.MaxWidth(80));
        
        if (GUILayout.Button("更新"))
        {
            list.Clear();
        }
        EditorGUILayout.EndHorizontal();
        scroll = EditorGUILayout.BeginScrollView(scroll, GUI.skin.box);
        List<Hashtable> delList = new List<Hashtable>();
        int enableCnt = 0;
        int dispNo = 0;
        for (int i = 0; i < list.Count; i++)
        {
            string name = ((GameObject)list[i][KEY_OBJECT]).name;
            if (hideInactive && (bool)list[i][KEY_IGNORE])
            {
                continue;
            }
            if (filterString != "" && !name.Contains(filterString))
            {
                if (!(bool)list[i][KEY_IGNORE])
                {
                    enableCnt++;
                }
                continue;
            }
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(dispNo.ToString(), GUILayout.MaxWidth(20));
            dispNo++;
            if (i < list.Count)
            {
                list[i][KEY_OBJECT] = (GameObject)EditorGUILayout.ObjectField(list[i][KEY_OBJECT] as GameObject, typeof(GameObject), true, GUILayout.MinWidth(180));
                EditorGUIUtility.labelWidth = 40;
                list[i][KEY_IGNORE] = EditorGUILayout.Toggle("対象外", (bool)list[i][KEY_IGNORE]);
                EditorGUIUtility.labelWidth = originalValue;
                if(!(bool)list[i][KEY_IGNORE])
                {
                    enableCnt++;
                }
            }
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 145;
        EditorGUILayout.LabelField("有効件数：" + enableCnt.ToString() + "件");
        EditorGUIUtility.labelWidth = 70;
        hideInactive = EditorGUILayout.Toggle("対象外を隠す", hideInactive);
        EditorGUIUtility.labelWidth = originalValue;
        GUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private List<Hashtable> CreateIgnoreRemovedList(List<Hashtable> list)
    {
        List<Hashtable> resultList = new List<Hashtable>();
        foreach(Hashtable table in list)
        {
            if(!(bool)table[KEY_IGNORE])
            {
                resultList.Add(table);
            }
        }
        return resultList;
    }

    /// <summary>
    /// 実行
    /// </summary>
    private void Make()
    {
        if (obj1 == null || obj2 == null)
        {
            EditorUtility.DisplayDialog("エラー", "対象のGame Objectが指定されていません", "OK");
            return;
        }

        List<Hashtable> fromTableList = CreateIgnoreRemovedList(this.fromTableList);
        List<Hashtable> toTableList = CreateIgnoreRemovedList(this.toTableList);

        if (nameNoCheck && fromTableList.Count != toTableList.Count)
        {
            if (!EditorUtility.DisplayDialog("警告", "対象のGameObjectの数が一致しません", "続行", "キャンセル"))
            {
                return;
            }
        }

        if (nameNoCheck)
        {
            for (int i = 0; i < fromTableList.Count && i < toTableList.Count; i++)
            {
                copyTransform(fromTableList[i][KEY_TRANSFORM] as Transform, toTableList[i][KEY_TRANSFORM] as Transform);
            }
        } 
        else
        {
            Hashtable nameTable = new Hashtable();
            foreach (Hashtable table in fromTableList)
            {
                nameTable.Add((table[KEY_OBJECT] as GameObject).name, table);
            }
            int i = 0;
            foreach (Hashtable table in toTableList)
            {
                string key = (table[KEY_OBJECT] as GameObject).name;
                if (nameTable.ContainsKey(key))
                {
                    copyTransform((nameTable[key] as Hashtable)[KEY_TRANSFORM] as Transform, table[KEY_TRANSFORM] as Transform);
                }
                i++;
            }
        }

        Debug.Log("Successfully CopyTransformByList.");
        if (isClose == true)
        {
            this.Close();
        }

        return;
    }

    private Transform[] RemoveInactive(Transform[] target)
    {
        List<Transform> workList = new List<Transform>();
        foreach(Transform work in target)
        {
            if (work.gameObject.activeInHierarchy) workList.Add(work);
        }
        return workList.ToArray();
    }

    private void copyTransform(Transform work1, Transform work2)
    {
        Undo.RecordObject(work2, "CopyTransform");
        if (local)
        {
            if (copyPosition) work2.localPosition = work1.localPosition;
            if (copyRotation) work2.localRotation = work1.localRotation;
        }
        else
        {
            if (copyPosition) work2.position = work1.position;
            if (copyRotation) work2.rotation = work1.rotation;
        }
        if (copyScale) work2.localScale = work1.localScale;
    }
}

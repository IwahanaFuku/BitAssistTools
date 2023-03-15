//https://booth.pm/ja/items/2419638 を少し改変。Iwahana 220625

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SetStatic : EditorWindow
{
    private Color _bgColor = new Color(1.3f, 1.3f, 1.3f, 1f);
    private const string KEY_OBJECT = "Object";
    private const string KEY_IGNORE = "Ignore";
    private GameObject obj1;
    private GameObject obj1Select;
    private bool ignoreInactive = true;
    private bool hideInactive = false;

    private bool isLightmap = false;
    private bool isOcculuder = false;
    private bool isOccludee = false;
    private bool isBatching = false;
    private bool isNavigation = false;
    private bool isOffMesh = false;
    private bool isReflection = false;

    private Vector2 size = new Vector2(700, 510);
    private List<Hashtable> fromTableList = new List<Hashtable>();
    private Vector2 fromScroll = Vector2.zero;
    private float originalValue = EditorGUIUtility.labelWidth;
    private string filterFrom = "";
    private string objectName = "";

    private int layerIdx = 0;
    private static List<string> layerList;
    private static List<int> layerIntList;

    private bool isClose = false;


    [MenuItem("Iwahana Tools/スタティック設定", false, 50)]
    static void Init()
    {
        SetStatic window = EditorWindow.GetWindow<SetStatic>();
    }

    private void Awake()
    {
        obj1 = Selection.activeGameObject;
        objectName = obj1.name;
    }


    private void setLayerList() {
        layerList = new List<string>();
        layerIntList = new List<int>();
        for (int i = 0; i < 31; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (string.IsNullOrEmpty(layerName))
            {
                continue;
            }
            layerList.Add(layerName);
            layerIntList.Add(i);
        }
    }

    private void OnGUI()
    {
        this.minSize = size;
        this.maxSize = size;

        // 対象
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(400), GUILayout.MaxHeight(480));
        EditorGUILayout.LabelField("Staticをまとめて設定します");
        GUILayout.Space(5);
        EditorGUIUtility.labelWidth = 120;
        EditorGUIUtility.labelWidth = originalValue;
        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        obj1 = (GameObject)EditorGUILayout.ObjectField("TargetObj", obj1, typeof(GameObject), true);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.EndHorizontal();

        separator();
        EditorGUIUtility.labelWidth = originalValue;
        EditorGUILayout.LabelField("基本設定");
        separator();
        if (GUILayout.Button("All"))
        {
            objectName = obj1.name;
            if (isLightmap && isOcculuder && isOccludee && isBatching && isNavigation && isOffMesh && isReflection)
            {
                isLightmap = false;
                isOcculuder = false;
                isOccludee = false;
                isBatching = false;
                isNavigation = false;
                isOffMesh = false;
                isReflection = false;
            }
            else
            {
                isLightmap = true;
                isOcculuder = true;
                isOccludee = true;
                isBatching = true;
                isNavigation = true;
                isOffMesh = true;
                isReflection = true;
            }
        };

        setLayerList();
        GUILayout.Space(5);
        
        if (GUILayout.Button("1.Static Environment"))
        {
            layerIdx = LayerMask.NameToLayer("Environment");
            if (layerIdx == -1)
            {
                layerIdx = 0;
            }
            objectName = "Static Environment";
            isLightmap = true;
            isOcculuder = false;
            isOccludee = true;
            isBatching = true;
            isNavigation = false;
            isOffMesh = false;
            isReflection = true;
        };

        if (GUILayout.Button("2.Static Default"))
        {
            layerIdx = LayerMask.NameToLayer("Default");
            if (layerIdx == -1)
            {
                layerIdx = 0;
            }
            objectName = "Static Default";
            isLightmap = false;
            isOcculuder = false;
            isOccludee = true;
            isBatching = true;
            isNavigation = false;
            isOffMesh = false;
            isReflection = true;
        };

        if (GUILayout.Button("3.Default"))
        {
            layerIdx = LayerMask.NameToLayer("Default");
            if (layerIdx == -1)
            {
                layerIdx = 0;
            }
            objectName = "Default";
            isLightmap = false;
            isOcculuder = false;
            isOccludee = false;
            isBatching = false;
            isNavigation = false;
            isOffMesh = false;
            isReflection = false;
        };

        GUILayout.Space(5);

        EditorGUIUtility.labelWidth = 155;
        layerIdx = EditorGUILayout.IntPopup("Layer", layerIdx, layerList.ToArray(), layerIntList.ToArray(), GUILayout.Width(375));

        objectName = EditorGUILayout.TextField("Object Name ", objectName);

        EditorGUIUtility.labelWidth = 360;
        isLightmap = EditorGUILayout.Toggle("Lightmap Static", isLightmap);
        isOcculuder = EditorGUILayout.Toggle("Occluder Static", isOcculuder);
        isOccludee = EditorGUILayout.Toggle("Occludee Static", isOccludee);
        isBatching = EditorGUILayout.Toggle("Batching Static", isBatching);
        isNavigation = EditorGUILayout.Toggle("Navigation Static", isNavigation);
        isOffMesh = EditorGUILayout.Toggle("Off Mesh Link Generation", isOffMesh);
        isReflection = EditorGUILayout.Toggle("Reflection ProbeStatic", isReflection);

        separator();
        EditorGUIUtility.labelWidth = originalValue;
        EditorGUILayout.LabelField("拡張設定");
        separator();
        EditorGUIUtility.labelWidth = 360;
        ignoreInactive = EditorGUILayout.Toggle("非活性オブジェクト除外", ignoreInactive);
        EditorGUIUtility.labelWidth = originalValue;


        EditorGUILayout.EndVertical();

        if (obj1 != obj1Select || fromTableList.Count == 0)
        {
            obj1Select = obj1;
            fromTableList = CreateTableList(obj1);
        }

        CreateListView(ref fromTableList, ref fromScroll, "From", ref filterFrom);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Set Static"))
        {
            isClose = false;
            Make();
        } 
        if (GUILayout.Button("Set Static And Close"))
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
        if (obj1 == null)
        {
            EditorUtility.DisplayDialog("エラー", "対象のGame Objectが指定されていません", "OK");
            return;
        }

        List<Hashtable> fromTableList = CreateIgnoreRemovedList(this.fromTableList);

        for (int i = 0; i < fromTableList.Count; i++)
        {
            AddStatic(fromTableList[i]);
        }
        
        Debug.Log("Successfully SetStatic.");
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

    private void AddStatic(Hashtable workTable1)
    {
        GameObject work1 = workTable1[KEY_OBJECT] as GameObject;
        StaticEditorFlags flags = 0;
        if (isLightmap) { flags = flags | StaticEditorFlags.ContributeGI; }
        if (isOcculuder) { flags = flags | StaticEditorFlags.OccluderStatic; }
        if (isOccludee) { flags = flags | StaticEditorFlags.OccludeeStatic; }
        if (isBatching) { flags = flags | StaticEditorFlags.BatchingStatic; }
        if (isNavigation) { flags = flags | StaticEditorFlags.NavigationStatic; }
        if (isOffMesh) { flags = flags | StaticEditorFlags.OffMeshLinkGeneration; }
        if (isReflection) { flags = flags | StaticEditorFlags.ReflectionProbeStatic; }
        work1.layer = layerIdx;
        obj1.name = objectName;
        GameObjectUtility.SetStaticEditorFlags(work1, flags);
    }
}

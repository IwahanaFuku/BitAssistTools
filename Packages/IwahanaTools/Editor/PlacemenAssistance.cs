using UnityEngine;
using UnityEditor;
using System.Linq;
using static System.Convert;
using System.Collections.Generic;

public class PlacemenAssistanceEditorWindow : EditorWindow
{
    private enum AlignType
    {
        Maximum,
        Center,
        Minimum
    }

    private enum Axis
    {
        X,
        Y,
        Z
    }

    private int axisSelection = 0;

    private float rotationAmount = 1f;
    private bool rotateX = false;
    private bool rotateY = true;
    private bool rotateZ = false;

    private static float minimum = 0.5f;
    private static float maximum = 1.5f;
    private float rotationalFrequency = 360;
    private float scaleRatio = 1f;
    private bool isDetail = false;
    Vector3 minScale = new Vector3(minimum, minimum, minimum);
    Vector3 maxScale = new Vector3(maximum, maximum, maximum);
    private Transform referencePoint;
    private float paddingSpace = 1f;
    private bool padingX = true;
    private bool padingY = true;
    private bool padingZ = true;
    private bool addReferencePoint = false;
    int copyCount = 5;
    int maxCopyCount = 100;
    private TransformData duplicateTransform = new TransformData(Vector3.zero, Quaternion.identity, Vector3.one);

    public struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;

        public TransformData(Vector3 position, Quaternion rotation, Vector3 localScale)
        {
            this.position = position;
            this.rotation = rotation;
            this.localScale = localScale;
        }
    }
    private AlignType alignType = AlignType.Center;
    private Axis arrangerAxis = Axis.X;

    private bool parentCenterAlignerAccordion = false;
    private bool arrangerAccordion = false;
    private bool distributeAccordion = false;
    private bool randomRotationAccordion = false;
    private bool randomScalenAccordion = false;
    private bool paddingAccordion = false;
    private bool specialDuplicateAccordion = false;
    private bool rotationalFrequencyeAccordion = false;

    [MenuItem("Iwahana Tools/配置支援ツール", false, 10)]
    public static void ShowWindow()
    {
        GetWindow<PlacemenAssistanceEditorWindow>("配置支援ツール");
    }

    private void OnGUI()
    {
        specialDuplicateAccordion = EditorGUILayout.Foldout(specialDuplicateAccordion, "特殊な複製");
        if (specialDuplicateAccordion)
        {
            duplicateTransform.position = EditorGUILayout.Vector3Field("Position", duplicateTransform.position);
            duplicateTransform.rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", 
                                          new Vector3(
                                              Mathf.Round(duplicateTransform.rotation.eulerAngles.x * 1000f) / 1000f,
                                              Mathf.Round(duplicateTransform.rotation.eulerAngles.y * 1000f) / 1000f,
                                              Mathf.Round(duplicateTransform.rotation.eulerAngles.z * 1000f) / 1000f
                                          )
                                          ));
            duplicateTransform.localScale = EditorGUILayout.Vector3Field("Scale", duplicateTransform.localScale);
            copyCount = EditorGUILayout.IntSlider("コピー数", copyCount, 0, maxCopyCount);
            if (GUILayout.Button("複製"))
            {
                SpecialDuplicate(duplicateTransform, copyCount);
            }
        }

        rotationalFrequencyeAccordion = EditorGUILayout.Foldout(rotationalFrequencyeAccordion, "回転複製");
        if (rotationalFrequencyeAccordion)
        {
            rotationalFrequency = EditorGUILayout.Slider("度数制限:", rotationalFrequency, 1, 360);

            if (GUILayout.Button("回転複製"))
            {
                RotateDuplicate();
            }
        }

        arrangerAccordion = EditorGUILayout.Foldout(arrangerAccordion, "整列");
        if (arrangerAccordion)
        {
            GUILayout.Label("基準値", EditorStyles.boldLabel);
            alignType = (AlignType)GUILayout.SelectionGrid((int)alignType, 
                new string[] { "最大", "中央", "最小" }, 3);

            GUILayout.Label("軸", EditorStyles.boldLabel);
            arrangerAxis = (Axis)GUILayout.SelectionGrid((int)arrangerAxis, 
                new string[] { "X軸", "Y軸", "Z軸" }, 3);

            if (GUILayout.Button("整列"))
            {
                AlignSelectedObjects();
            }
        }

        distributeAccordion = EditorGUILayout.Foldout(distributeAccordion, "分布");
        if (distributeAccordion)
        {
            GUILayout.Label("軸", EditorStyles.boldLabel);
            axisSelection = GUILayout.SelectionGrid(axisSelection, new string[] { "X軸", "Y軸", "Z軸" }, 3);

            if (GUILayout.Button("分布"))
            {
                DistributeObjects();
            }
        }
        
        randomScalenAccordion = EditorGUILayout.Foldout(randomScalenAccordion, "ランダムスケール");
        if (randomScalenAccordion)
        {
            scaleRatio = EditorGUILayout.FloatField("オフセット", scaleRatio);

            using (new EditorGUI.DisabledGroupScope(isDetail))
            {
                minimum = EditorGUILayout.FloatField("最小", minimum);
                maximum = EditorGUILayout.FloatField("最大", maximum);
            }

            isDetail = EditorGUILayout.Toggle("詳細", isDetail);

            if(isDetail == false)
            {
                if (GUILayout.Button("スケール"))
                {
                    Undo.RecordObjects(Selection.transforms, "Random Scale Objects");
                    RandomScaleSelectedObjects();
                }
            }
            else
            {            
                minScale = EditorGUILayout.Vector3Field("最小", minScale);
                maxScale = EditorGUILayout.Vector3Field("最大", maxScale);
                if (GUILayout.Button("スケール"))
                {
                    Undo.RecordObjects(Selection.transforms, "Random Detail Scale Objects");                   
                    RandomDetailScaleSelectedObjects();
                }
            }
        }

        randomRotationAccordion = EditorGUILayout.Foldout(randomRotationAccordion, "ランダム回転");
        if (randomRotationAccordion)
        {
            rotationAmount = EditorGUILayout.Slider("閾値:", rotationAmount, 1, 360);
            rotateX = EditorGUILayout.Toggle("X軸", rotateX);
            rotateY = EditorGUILayout.Toggle("Y軸", rotateY);
            rotateZ = EditorGUILayout.Toggle("Z軸", rotateZ);

            if (GUILayout.Button("回転"))
            {
                RandomRotateSelectedObjects();
            }
        }
        
        paddingAccordion = EditorGUILayout.Foldout(paddingAccordion, "パディング");
        if (paddingAccordion)
        {
            paddingSpace = EditorGUILayout.Slider("パディング間隔", paddingSpace, 0f, 10f);
            padingX = EditorGUILayout.Toggle("X軸", padingX);
            padingY = EditorGUILayout.Toggle("Y軸", padingY);
            padingZ = EditorGUILayout.Toggle("Z軸", padingZ);

            addReferencePoint = EditorGUILayout.Toggle("基準点を追加", addReferencePoint);
            if (addReferencePoint)
            {
                referencePoint = (Transform)EditorGUILayout.ObjectField("基準点", referencePoint, typeof(Transform), true);
            }

            if (GUILayout.Button("パディング"))
            {
                PaddingObjects();
            }
        }
    }


    [MenuItem ("Iwahana Tools/カスタム ショートカット/グループを生成 %g")]
    private static void Log2 () {
        ParentCenterAligner(Selection.transforms);
    }

    private void AlignSelectedObjects()
    {
        Transform[] selectedTransforms = Selection.transforms;

        if(checkSelectedTransform(selectedTransforms)){return;}

        float[] values = new float[selectedTransforms.Length];

        for (int i = 0; i < selectedTransforms.Length; i++)
        {
            switch (arrangerAxis)
            {
                case Axis.X:
                    values[i] = selectedTransforms[i].position.x;
                    break;
                case Axis.Y:
                    values[i] = selectedTransforms[i].position.y;
                    break;
                case Axis.Z:
                    values[i] = selectedTransforms[i].position.z;
                    break;
            }
        }

        float targetValue = 0f;

        switch (alignType)
        {
            case AlignType.Maximum:
                targetValue = Mathf.Max(values);
                break;
            case AlignType.Center:
                targetValue = values.Average();
                break;
            case AlignType.Minimum:
                targetValue = Mathf.Min(values);
                break;
        }

        Undo.RecordObjects(selectedTransforms, "Align Objects");

        foreach (Transform t in selectedTransforms)
        {
            Vector3 pos = t.position;

            switch (arrangerAxis)
            {
                case Axis.X:
                    pos.x = targetValue;
                    break;
                case Axis.Y:
                    pos.y = targetValue;
                    break;
                case Axis.Z:
                    pos.z = targetValue;
                    break;
            }

            t.position = pos;
        }

        Undo.FlushUndoRecordObjects();
    }


    private void RotateDuplicate()
    {
        GameObject selectedObject = Selection.activeObject as GameObject;
        List<GameObject> boundsObjectsList = new List<GameObject>();

        if(selectedObject.transform.childCount == 0)
        {
            //選択したトランスフォームに子がなく、それ自体で回転処理を行う場合
            boundsObjectsList.Add(selectedObject);
        }
        else
        {
            //選択したトランスフォームに子があって、選択したオブジェクト自体はピボッドとして扱う場合
            foreach (Transform childTransform in selectedObject.transform)
            {
                boundsObjectsList.Add(childTransform.gameObject);
            }
        }

        GameObject[] boundsObjects = boundsObjectsList.ToArray();
        Bounds bounds = new Bounds();
        bool hasBounds = false;

        //複数子がある場合に、バウンディングボックスをそれに合わせて拡大する
        foreach (GameObject boundsObject in boundsObjects)
        {
            Renderer renderer = boundsObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
        }
        var selectedPosition = selectedObject.transform.position;

        Vector3 absCenter = new Vector3(Mathf.Abs(bounds.center.x), Mathf.Abs(bounds.center.y), Mathf.Abs(bounds.center.z))
                          - new Vector3(Mathf.Abs(selectedPosition.x), Mathf.Abs(selectedPosition.y), Mathf.Abs(selectedPosition.z));
        Vector3 edgeCenterX = new Vector3(absCenter.x + bounds.size.x / 2, absCenter.y, absCenter.z);
        Vector3 edgeCenterZ = new Vector3(absCenter.x, absCenter.y, absCenter.z + bounds.size.z / 2);

        float theta;
        Vector3 rotationAxis = Vector3.up;
        int numOfCopy;

        if(edgeCenterZ.sqrMagnitude < edgeCenterX.sqrMagnitude)
        {
            theta = Mathf.Atan2(bounds.size.z / 2, edgeCenterX.x) * Mathf.Rad2Deg * 2;
            numOfCopy = ToInt32(rotationalFrequency / theta);
            Debug.Log("X");
        }
        else
        {
            theta = Mathf.Atan2(bounds.size.x / 2, edgeCenterZ.z) * Mathf.Rad2Deg * 2;
            numOfCopy = ToInt32(rotationalFrequency / theta);
            Debug.Log("Z");
        }

        Undo.RecordObjects(boundsObjects, "Create Cube");

        var copyTransforms= new List<Transform>();
        copyTransforms.Add(selectedObject.transform);

        for(int i = 0; i < numOfCopy + 1; i++)
        {
            Quaternion rotationQuaternion = Quaternion.AngleAxis(theta * i, rotationAxis);
            GameObject newObject = Instantiate(selectedObject, selectedObject.transform.position, selectedObject.transform.rotation * rotationQuaternion);
            newObject.name = selectedObject.name;
            copyTransforms.Add(newObject.transform);

            Undo.RegisterCreatedObjectUndo(newObject, "Create Cube");
        }
        ParentCenterAligner(copyTransforms.ToArray());
    }
    
    private void DistributeObjects()
    {
        if(checkSelectedTransform(Selection.transforms)){return;}

        Dictionary<int, Vector3> originalIndices = new Dictionary<int, Vector3>();
        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            originalIndices[i] = Selection.gameObjects[i].transform.position;
        }

        List<KeyValuePair<int, Vector3>> sortedPositions = originalIndices.ToList();
        switch (axisSelection)
        {
            case 0:
                sortedPositions.Sort((a, b) => a.Value.x.CompareTo(b.Value.x));
                break;
            case 1:
                sortedPositions.Sort((a, b) => a.Value.y.CompareTo(b.Value.y));
                break;
            case 2:
                sortedPositions.Sort((a, b) => a.Value.z.CompareTo(b.Value.z));
                break;
        }

        Undo.RecordObjects(Selection.transforms, "Align Objects");

        float interval = (sortedPositions[sortedPositions.Count - 1].Value[axisSelection] - sortedPositions[0].Value[axisSelection]) / (sortedPositions.Count - 1);
        for (int i = 0; i < sortedPositions.Count; i++)
        {
            Vector3 newPos = sortedPositions[i].Value;
            newPos[axisSelection] = sortedPositions[0].Value[axisSelection] + i * interval;
            Selection.gameObjects[sortedPositions[i].Key].transform.position = newPos;
        }
        Undo.FlushUndoRecordObjects();
    }

    private void SpecialDuplicate(TransformData duplicateTransform, int copyCount)
    {
        Transform[] selectedTransforms = Selection.transforms;
        foreach (Transform selectedTransform in selectedTransforms)
        {
            TransformData newTransform = new TransformData(selectedTransform.position, selectedTransform.rotation, selectedTransform.localScale);
            var copyTransforms= new List<Transform>();
            copyTransforms.Add(selectedTransform);
            
            if(copyCount < 2){break;}

            for(int i = 0; i < copyCount; i++)
            {
                newTransform.position = newTransform.position + duplicateTransform.position;
                newTransform.rotation = newTransform.rotation * duplicateTransform.rotation;
                newTransform.localScale = Vector3.Scale(newTransform.localScale, duplicateTransform.localScale);

                GameObject newObject = Instantiate(selectedTransform.gameObject, newTransform.position, newTransform.rotation);
                newObject.transform.localScale = newTransform.localScale;
                newObject.name = selectedTransform.name;

                Undo.RegisterCreatedObjectUndo(newObject, "Create Copy");

                copyTransforms.Add(newObject.transform);
            }

            ParentCenterAligner(copyTransforms.ToArray());
        }
    }

    private static void ParentCenterAligner(Transform[] selectedTransforms)
    {
        Undo.RecordObjects(selectedTransforms, "Align Group Objects");

        Vector3 center = Vector3.zero;
        foreach (Transform transform in selectedTransforms)
        {
            center += transform.position;
        }
        center /= selectedTransforms.Length;

        Transform parentTransform = selectedTransforms[0].transform.parent;
        GameObject group = new GameObject("Group");
        Undo.RegisterCreatedObjectUndo(group, "Create Group");
        group.transform.position = center;
        group.transform.parent = parentTransform;

        foreach (Transform transform in selectedTransforms)
        {
            Undo.SetTransformParent(transform, group.transform, "Set Group");
        }

        Selection.activeGameObject = group;
    }

    private void RandomScaleSelectedObjects()
    {
        Undo.RecordObjects(Selection.objects, "Randomize Scale");
        foreach (var selectedObject in Selection.objects)
        {
            var transform = (selectedObject as GameObject).transform;
            var newscale = 1f;

            if(isDetail == false)
            {
                newscale = newscale * Random.Range(minimum, maximum);
            }

            var scale = scaleRatio * newscale;
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    private void RandomDetailScaleSelectedObjects()
    {
        foreach (Transform transform in Selection.transforms)
        {
            Vector3 scale = transform.localScale;
            scale.x = scaleRatio * Random.Range(Mathf.Min(minScale.x, maxScale.x), Mathf.Max(minScale.x, maxScale.x));
            scale.y = scaleRatio * Random.Range(Mathf.Min(minScale.y, maxScale.y), Mathf.Max(minScale.y, maxScale.y));
            scale.z = scaleRatio * Random.Range(Mathf.Min(minScale.z, maxScale.z), Mathf.Max(minScale.z, maxScale.z));

            transform.localScale = scale;
        }
    }

    private void RandomRotateSelectedObjects()
    {
        Undo.RecordObjects(Selection.transforms, "Random Rotate Objects");

        foreach (GameObject selectedObject in Selection.gameObjects)
        {
            if (selectedObject != null)
            {
                float xRotation = 0f;
                float yRotation = 0f;
                float zRotation = 0f;

                if (rotateX)
                {
                    xRotation = Mathf.Floor(Random.Range(0, 360) / rotationAmount) * rotationAmount;
                }

                if (rotateY)
                {
                    yRotation = Mathf.Floor(Random.Range(0, 360) / rotationAmount) * rotationAmount;
                }

                if (rotateZ)
                {
                    zRotation = Mathf.Floor(Random.Range(0, 360) / rotationAmount) * rotationAmount;
                }

                selectedObject.transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);
            }
        }
    }

    private void PaddingObjects()
    {
        Undo.RecordObjects(Selection.transforms, "Apply Padding");

        Transform[] objects = Selection.transforms;

        Vector3 reference = Vector3.zero;
        if (addReferencePoint && referencePoint != null)
        {
            reference = referencePoint.position;
        }
        else
        {
            for (int i = 0; i < objects.Length; i++)
            {
                reference += objects[i].position;
            }

            reference /= objects.Length;
        }
        for (int i = 0; i < objects.Length; i++)
        {
            Vector3 direction = objects[i].position - reference;
            
            float paddingX = direction.x;
            float paddingY = direction.y;
            float paddingZ = direction.z;

            Vector3 paddingDirection = direction * paddingSpace;
            
            if (padingX)
            {
                paddingX = paddingDirection.x;
            }

            if (padingY)
            {
                paddingY = paddingDirection.y;
            }

            if (padingZ)
            {
                paddingZ = paddingDirection.z;
            }
            
            objects[i].position = reference + new Vector3(paddingX, paddingY, paddingZ);
        }
    }

    private static bool checkSelectedTransform(Transform[] selectedTransforms)
    {
    if (selectedTransforms.Length < 2)
        {
            Debug.LogError("Please select at least two objects to align.");
            return true;
        }
        return false;
    } 
}

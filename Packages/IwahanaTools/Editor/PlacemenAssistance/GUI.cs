using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PlacemenAssistance
    {
    public class PlacemenAssistanceEditorWindow : EditorWindow
    {
        Vector2 scrollPos;

        bool isSpecialDuplicateExpanded = false;
        bool isRoundingExpanded = false;
        bool isRoundingDetailExpanded = false;
        bool isSpecialTransformChangeExpanded = false;
        bool isDistributeExpanded = false;
        bool isArrangerExpanded = false;
        bool isArrangerpaddingerExpanded = false;
        bool isPaddingReferencePointExpanded = false;
        bool isRandomPositionExpanded = false;
        bool isRandomRotationExpanded = false;
        bool isRandomRotationDetailExpanded = false;
        bool isRandomScalenExpanded = false;

        int copyCount = 5;
        CoordinateReference duplicateTransformCoordinate = CoordinateReference.Global;
        TransformData duplicateTransform = new TransformData(Vector3.zero, Quaternion.identity, Vector3.one);
        TransformFlags roundingTransformFlags = new TransformFlags {position = true, rotation = true, scale = true};
        int multiplier = 2;
        CoordinateReference ChangeTransformCoordinate = CoordinateReference.Global;
        TransformData ChangeTransform = new TransformData(Vector3.zero, Quaternion.identity, Vector3.zero);
        Axis distributeAxis = Axis.X;
        AlignType alignType = AlignType.Center;
        Axis arrangerAxis = Axis.X;
        float paddingSpace = 1f;
        Transform referencePoint;
        AxisFlags padingAxisFlags = new AxisFlags {X = true, Y = true, Z = true};
        float radius = 1f;
        AxisFlags positionFlags = new AxisFlags {X = true, Y = true, Z = true};
        float rotationAmount = 1f;
        AnimationCurve curve = AnimationCurve.Linear(0, -180, 1, 180);
        AxisFlags rotateFlags = new AxisFlags {X = false, Y = true, Z = false};
        float scaleRatio = 1f;
        bool isRandomRotateDetail = false;
        static float minimum = 0.5f;
        static float maximum = 1.5f;
        Vector3 minScale = new Vector3(minimum, minimum, minimum);
        Vector3 maxScale = new Vector3(maximum, maximum, maximum);
        bool isRandomScaleDetail = false;

        [MenuItem("Iwahana Tools/配置支援ツール", priority = 10)]
        public static void ShowWindow()
        {
            GetWindow<PlacemenAssistanceEditorWindow>("配置支援ツール");
        }

        void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            isSpecialDuplicateExpanded = EditorGUILayout.Foldout(isSpecialDuplicateExpanded, "特殊な複製");
            if (isSpecialDuplicateExpanded)
            {
                SpecialDuplicateGUI();
            }

            isRoundingExpanded = EditorGUILayout.Foldout(isRoundingExpanded, "端数を丸める");
            if (isRoundingExpanded)
            {
                RoundingGUI();
            }
            isSpecialTransformChangeExpanded = EditorGUILayout.Foldout(isSpecialTransformChangeExpanded, "現在の値をもとにトランスフォームの変更");
            if (isSpecialTransformChangeExpanded)
            {
                SpecialTransformChangeGUI();
            }

            isDistributeExpanded = EditorGUILayout.Foldout(isDistributeExpanded, "分布");
            if (isDistributeExpanded)
            {
                SortAndDistributeObjectsGUI();
            }

            isArrangerExpanded = EditorGUILayout.Foldout(isArrangerExpanded, "整列");
            if (isArrangerExpanded)
            {
                ArrangerObjectsGUI();
            }

            isArrangerpaddingerExpanded = EditorGUILayout.Foldout(isArrangerpaddingerExpanded, "パディング");
            if (isArrangerpaddingerExpanded)
            {
                PaddingerObjectsGUI();
            }

            isRandomPositionExpanded = EditorGUILayout.Foldout(isRandomPositionExpanded, "ランダムポジション");
            if (isRandomPositionExpanded)
            {
                randomPositionGUI();
            }

            isRandomRotationExpanded = EditorGUILayout.Foldout(isRandomRotationExpanded, "ランダム回転");
            if (isRandomRotationExpanded)
            {
                randomRotationGUI();
            }

            isRandomScalenExpanded = EditorGUILayout.Foldout(isRandomScalenExpanded, "ランダムスケール");
            if (isRandomScalenExpanded)
            {
                RandomScalenGUI();
            }
            
            EditorGUILayout.EndScrollView();
        }

        void SpecialDuplicateGUI()
        {
            SpecialDuplicater SpecialDuplicaterInstance = new SpecialDuplicater();

            duplicateTransformCoordinate = (CoordinateReference)GUILayout.SelectionGrid((int)duplicateTransformCoordinate, new string[] {"Global", "Local"}, 2);
            int maxCopyCount = 100;

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
                SpecialDuplicaterInstance.SpecialDuplicate(duplicateTransformCoordinate, duplicateTransform, copyCount);
            }
        }

        void RoundingGUI()
        {
            Roundinger RoundingerInstance = new Roundinger();

            multiplier = (int)EditorGUILayout.Slider("乗数", multiplier, 0f, 6f);
            roundingTransformFlags.position = EditorGUILayout.Toggle("Position", roundingTransformFlags.position);
            roundingTransformFlags.rotation = EditorGUILayout.Toggle("Rotation", roundingTransformFlags.rotation);
            roundingTransformFlags.scale = EditorGUILayout.Toggle("Scale", roundingTransformFlags.scale);

            if (GUILayout.Button("丸める"))
            {
                RoundingerInstance.Rounding(multiplier, roundingTransformFlags);
            }
        }

        void SpecialTransformChangeGUI()
        {
            ObjectTransformChanger ObjectTransformChangerInstance = new ObjectTransformChanger();

            ChangeTransformCoordinate = (CoordinateReference)GUILayout.SelectionGrid((int)ChangeTransformCoordinate, new string[] {"Global", "Local"}, 2);

            ChangeTransform.position = EditorGUILayout.Vector3Field("Position", ChangeTransform.position);
            ChangeTransform.rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", 
                                            new Vector3(
                                                Mathf.Round(ChangeTransform.rotation.eulerAngles.x * 1000f) / 1000f,
                                                Mathf.Round(ChangeTransform.rotation.eulerAngles.y * 1000f) / 1000f,
                                                Mathf.Round(ChangeTransform.rotation.eulerAngles.z * 1000f) / 1000f
                                            )
                                            ));
            ChangeTransform.localScale = EditorGUILayout.Vector3Field("Scale", ChangeTransform.localScale);
            if (GUILayout.Button("変更"))
            {
                ObjectTransformChangerInstance.ObjectTransformChange(ChangeTransformCoordinate, ChangeTransform);
            }
        }

        void SortAndDistributeObjectsGUI()
        {
            ObjectDistributor ObjectDistributorInstance = new ObjectDistributor();

            GUILayout.Label("軸");
            distributeAxis = (Axis)GUILayout.SelectionGrid((int)distributeAxis, new string[] { "X軸", "Y軸", "Z軸" }, 3);

            if (GUILayout.Button("分布"))
            {
                ObjectDistributorInstance.SortAndDistributeObjects(distributeAxis);
            }
        }

        void ArrangerObjectsGUI()
        {
            ObjectAligner AlignObjectsInstance = new ObjectAligner();
            
            GUILayout.Label("基準値");
            alignType = (AlignType)GUILayout.SelectionGrid((int)alignType, 
                new string[] { "最大", "中央", "最小" }, 3);

            GUILayout.Label("軸");
            arrangerAxis = (Axis)GUILayout.SelectionGrid((int)arrangerAxis, 
                new string[] { "X軸", "Y軸", "Z軸" }, 3);

            if (GUILayout.Button("整列"))
            {
                AlignObjectsInstance.AlignObjects(arrangerAxis, alignType);
            }
        }

        void PaddingerObjectsGUI()
        {
            ObjectPaddinger PaddingObjectsInstance = new ObjectPaddinger();

            paddingSpace = EditorGUILayout.Slider("パディング間隔", paddingSpace, 0f, 2f);
            padingAxisFlags.X = EditorGUILayout.Toggle("X軸", padingAxisFlags.X);
            padingAxisFlags.Y = EditorGUILayout.Toggle("Y軸", padingAxisFlags.Y);
            padingAxisFlags.Z = EditorGUILayout.Toggle("Z軸", padingAxisFlags.Z);

            isPaddingReferencePointExpanded = EditorGUILayout.Toggle("基準点を追加", isPaddingReferencePointExpanded);
            if (isPaddingReferencePointExpanded)
            {
                referencePoint = (Transform)EditorGUILayout.ObjectField("基準点", referencePoint, typeof(Transform), true);
            }

            if (GUILayout.Button("パディング"))
            {
                PaddingObjectsInstance.PaddingObjects(isPaddingReferencePointExpanded, paddingSpace, referencePoint, padingAxisFlags);
            }
        }

        void randomPositionGUI()
        {
            RandomPosition RandomPositionInstance = new RandomPosition();
            radius = EditorGUILayout.Slider("半径:", radius, 0, 10);
            positionFlags.X = EditorGUILayout.Toggle("X軸", positionFlags.X);
            positionFlags.Y = EditorGUILayout.Toggle("Y軸", positionFlags.Y);
            positionFlags.Z = EditorGUILayout.Toggle("Z軸", positionFlags.Z);

            if (GUILayout.Button("移動"))
            {
                RandomPositionInstance.RandomPositionObjects(radius, positionFlags);
            }
        }

        void randomRotationGUI()
        {
            RandomRotate RandomRotationInstance = new RandomRotate();
            rotationAmount = EditorGUILayout.Slider("スナップ度数:", rotationAmount, 1, 360);
            isRandomRotateDetail = EditorGUILayout.Toggle("詳細", isRandomRotateDetail);
            
            if (isRandomRotateDetail)
            {
                curve = EditorGUILayout.CurveField("制限度数", curve);
            }

            rotateFlags.X = EditorGUILayout.Toggle("X軸", rotateFlags.X);
            rotateFlags.Y = EditorGUILayout.Toggle("Y軸", rotateFlags.Y);
            rotateFlags.Z = EditorGUILayout.Toggle("Z軸", rotateFlags.Z);

            if (GUILayout.Button("回転"))
            {
                AnimationCurve curve_result = curve;
                if (! isRandomRotateDetail)
                {
                    curve_result = AnimationCurve.Linear(0, -180, 1, 180);
                }
                RandomRotationInstance.RandomRotateObjects(rotationAmount, rotateFlags, curve_result);
            }
        }

        void RandomScalenGUI()
        {
            RandomScale RandomScaleInstance = new RandomScale();
            scaleRatio = EditorGUILayout.FloatField("オフセット", scaleRatio);

            using (new EditorGUI.DisabledGroupScope(isRandomScaleDetail))
            {
                minimum = EditorGUILayout.FloatField("最小", minimum);
                maximum = EditorGUILayout.FloatField("最大", maximum);
            }

            isRandomScaleDetail = EditorGUILayout.Toggle("詳細", isRandomScaleDetail);

            if(isRandomScaleDetail == false)
            {
                if (GUILayout.Button("スケール"))
                {
                    Undo.RecordObjects(Selection.transforms, "Random Scale Objects");
                    RandomScaleInstance.RandomScaleObjects(isRandomScaleDetail, scaleRatio, minimum, maximum);
                }
            }
            else
            {            
                minScale = EditorGUILayout.Vector3Field("最小", minScale);
                maxScale = EditorGUILayout.Vector3Field("最大", maxScale);
                if (GUILayout.Button("スケール"))
                {
                    Undo.RecordObjects(Selection.transforms, "Random Detail Scale Objects");                   
                    RandomScaleInstance.RandomDetailScaleObjects(scaleRatio, minScale, maxScale);
                }
            }
        }
    }
}
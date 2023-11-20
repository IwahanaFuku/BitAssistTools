using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PlacemenAssistance
    {
    public class PlacemenAssistanceEditorWindow : EditorWindow
    {
        Vector2 scrollPos;
        int spacePixel = 8;

        bool isSpecialDuplicateExpanded = false;
        bool isRoundingExpanded = false;
        bool isObjectSwapper = false;
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
        GameObject objectSwapperPrefab = null;
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

        [MenuItem("Bit Assist Tools/配置支援ツール", priority = 10)]
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

            isObjectSwapper = EditorGUILayout.Foldout(isObjectSwapper, "選択したオブジェクトを入れ替える");
            if (isObjectSwapper)
            {
                ObjectSwapperGUI();
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
            duplicateTransformCoordinate = (CoordinateReference)GUILayout.SelectionGrid((int)duplicateTransformCoordinate, new string[] {"Global", "Local"}, 2);
            int maxCopyCount = 100;
            duplicateTransform.position = EditorGUILayout.Vector3Field("  Position", duplicateTransform.position);
            duplicateTransform.rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("  Rotation", 
                                            new Vector3(
                                                Mathf.Round(duplicateTransform.rotation.eulerAngles.x * 1000f) / 1000f,
                                                Mathf.Round(duplicateTransform.rotation.eulerAngles.y * 1000f) / 1000f,
                                                Mathf.Round(duplicateTransform.rotation.eulerAngles.z * 1000f) / 1000f
                                            )
                                            ));
            duplicateTransform.localScale = EditorGUILayout.Vector3Field("  Scale", duplicateTransform.localScale);

            copyCount = EditorGUILayout.IntSlider("コピー数", copyCount, 1, maxCopyCount);

            GUILayout.Space(spacePixel);

            if (GUILayout.Button("複製"))
            {
                SpecialDuplicater.SpecialDuplicate(duplicateTransformCoordinate, duplicateTransform, copyCount);
            }

            GUILayout.Space(spacePixel);
        }

        void RoundingGUI()
        {
            multiplier = (int)EditorGUILayout.Slider("小数点の位", multiplier, 0f, 6f);

            roundingTransformFlags.position = EditorGUILayout.Toggle("  Position", roundingTransformFlags.position);
            roundingTransformFlags.rotation = EditorGUILayout.Toggle("  Rotation", roundingTransformFlags.rotation);
            roundingTransformFlags.scale = EditorGUILayout.Toggle("  Scale", roundingTransformFlags.scale);

            GUILayout.Space(spacePixel);

            if (GUILayout.Button("丸める"))
            {
                Roundinger.Rounding(multiplier, roundingTransformFlags);
            }

            GUILayout.Space(spacePixel);
        }

        void ObjectSwapperGUI()
        {
            objectSwapperPrefab = (GameObject)EditorGUILayout.ObjectField("オブジェクト", objectSwapperPrefab, typeof(GameObject), false);

            GUILayout.Space(spacePixel);

            if (GUILayout.Button("入れ替え"))
            {
                ObjectSwapper.ObjectSwapping(objectSwapperPrefab);
            }

            GUILayout.Space(spacePixel);
        }

        void SpecialTransformChangeGUI()
        {
            ChangeTransformCoordinate = (CoordinateReference)GUILayout.SelectionGrid((int)ChangeTransformCoordinate, new string[] {"Global", "Local"}, 2);
            
            ChangeTransform.position = EditorGUILayout.Vector3Field("  Position", ChangeTransform.position);
            ChangeTransform.rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("  Rotation", 
                                            new Vector3(
                                                Mathf.Round(ChangeTransform.rotation.eulerAngles.x * 1000f) / 1000f,
                                                Mathf.Round(ChangeTransform.rotation.eulerAngles.y * 1000f) / 1000f,
                                                Mathf.Round(ChangeTransform.rotation.eulerAngles.z * 1000f) / 1000f
                                            )
                                            ));
            ChangeTransform.localScale = EditorGUILayout.Vector3Field("  Scale", ChangeTransform.localScale);

            GUILayout.Space(spacePixel);

            if (GUILayout.Button("変更"))
            {
                ObjectTransformChanger.ObjectTransformChange(ChangeTransformCoordinate, ChangeTransform);
            }

            GUILayout.Space(spacePixel);
        }

        void SortAndDistributeObjectsGUI()
        {
            GUILayout.Label("軸");
            distributeAxis = (Axis)GUILayout.SelectionGrid((int)distributeAxis, new string[] { "X軸", "Y軸", "Z軸" }, 3);

            GUILayout.Space(spacePixel);

            if (GUILayout.Button("分布"))
            {
                ObjectDistributor.SortAndDistributeObjects(distributeAxis);
            }

            GUILayout.Space(spacePixel);
        }

        void ArrangerObjectsGUI()
        {
            GUILayout.Label("軸");
            arrangerAxis = (Axis)GUILayout.SelectionGrid((int)arrangerAxis, 
                new string[] { "X軸", "Y軸", "Z軸" }, 3);

            GUILayout.Label("基準値");
            alignType = (AlignType)GUILayout.SelectionGrid((int)alignType, 
                new string[] { "最大", "中央", "最小" }, 3);

            GUILayout.Space(spacePixel);

            if (GUILayout.Button("整列"))
            {
                ObjectAligner.AlignObjects(arrangerAxis, alignType);
            }

            GUILayout.Space(spacePixel);
        }

        void PaddingerObjectsGUI()
        {
            paddingSpace = EditorGUILayout.Slider("パディング間隔", paddingSpace, 0f, 2f);

            GUILayout.Label("軸");
            padingAxisFlags.X = EditorGUILayout.Toggle("  X軸", padingAxisFlags.X);
            padingAxisFlags.Y = EditorGUILayout.Toggle("  Y軸", padingAxisFlags.Y);
            padingAxisFlags.Z = EditorGUILayout.Toggle("  Z軸", padingAxisFlags.Z);

            isPaddingReferencePointExpanded = EditorGUILayout.Toggle("基準点を追加", isPaddingReferencePointExpanded);
            if (isPaddingReferencePointExpanded)
            {
                referencePoint = (Transform)EditorGUILayout.ObjectField("基準点", referencePoint, typeof(Transform), true);
            }

            GUILayout.Space(spacePixel);

            if (GUILayout.Button("パディング"))
            {
                ObjectPaddinger.PaddingObjects(isPaddingReferencePointExpanded, paddingSpace, referencePoint, padingAxisFlags);
            }

            GUILayout.Space(spacePixel);
        }

        void randomPositionGUI()
        {
            radius = EditorGUILayout.Slider("半径:", radius, 0, 10);

            GUILayout.Label("軸");
            positionFlags.X = EditorGUILayout.Toggle("  X軸", positionFlags.X);
            positionFlags.Y = EditorGUILayout.Toggle("  Y軸", positionFlags.Y);
            positionFlags.Z = EditorGUILayout.Toggle("  Z軸", positionFlags.Z);

            GUILayout.Space(spacePixel);

            if (GUILayout.Button("移動"))
            {
                RandomPosition.RandomPositionObjects(radius, positionFlags);
            }

            GUILayout.Space(spacePixel);
        }

        void randomRotationGUI()
        {
            rotationAmount = EditorGUILayout.Slider("スナップ度数:", rotationAmount, 1, 360);
            isRandomRotateDetail = EditorGUILayout.Toggle("詳細", isRandomRotateDetail);
            
            if (isRandomRotateDetail)
            {
                curve = EditorGUILayout.CurveField("  制限度数", curve);
            }

            GUILayout.Label("軸");
            rotateFlags.X = EditorGUILayout.Toggle("  X軸", rotateFlags.X);
            rotateFlags.Y = EditorGUILayout.Toggle("  Y軸", rotateFlags.Y);
            rotateFlags.Z = EditorGUILayout.Toggle("  Z軸", rotateFlags.Z);

            GUILayout.Space(spacePixel);

            if (GUILayout.Button("回転"))
            {
                AnimationCurve curve_result = curve;
                if (! isRandomRotateDetail)
                {
                    curve_result = AnimationCurve.Linear(0, -180, 1, 180);
                }
                RandomRotate.RandomRotateObjects(rotationAmount, rotateFlags, curve_result);
            }

            GUILayout.Space(spacePixel);
        }

        void RandomScalenGUI()
        {
            scaleRatio = EditorGUILayout.FloatField("オフセット", scaleRatio);

            using (new EditorGUI.DisabledGroupScope(isRandomScaleDetail))
            {
                minimum = EditorGUILayout.FloatField("  最小", minimum);
                maximum = EditorGUILayout.FloatField("  最大", maximum);
            }

            isRandomScaleDetail = EditorGUILayout.Toggle("詳細", isRandomScaleDetail);

            if(isRandomScaleDetail == false)
            {

                GUILayout.Space(spacePixel);

                if (GUILayout.Button("スケール"))
                {
                    Undo.RecordObjects(Selection.transforms, "Random Scale Objects");
                    RandomScale.RandomScaleObjects(isRandomScaleDetail, scaleRatio, minimum, maximum);
                }

                GUILayout.Space(spacePixel);
            }
            else
            {            
                minScale = EditorGUILayout.Vector3Field("  最小", minScale);
                maxScale = EditorGUILayout.Vector3Field("  最大", maxScale);

                GUILayout.Space(spacePixel);

                if (GUILayout.Button("スケール"))
                {
                    Undo.RecordObjects(Selection.transforms, "Random Detail Scale Objects");                   
                    RandomScale.RandomDetailScaleObjects(scaleRatio, minScale, maxScale);
                }

                GUILayout.Space(spacePixel);
            }
        }
    }
}
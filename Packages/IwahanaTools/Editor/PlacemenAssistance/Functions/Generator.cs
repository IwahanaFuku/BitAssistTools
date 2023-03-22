using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace PlacemenAssistance
{
    public class SpecialDuplicater : MonoBehaviour
    {
        public void SpecialDuplicate(CoordinateReference coordinateReference, TransformData duplicateTransform, int copyCount)
        {
            Transform[] selectedTransforms = Selection.transforms;
            foreach (Transform selectedTransform in selectedTransforms)
            {
                TransformData newTransform = new TransformData(selectedTransform.position, selectedTransform.rotation, selectedTransform.localScale);
                List<Transform> copyTransforms = new List<Transform>();
                copyTransforms.Add(selectedTransform);

                if (copyCount < 1) { break; }

                for (int i = 0; i < copyCount; i++)
                {
                    if (coordinateReference == CoordinateReference.Global)
                    {
                        newTransform.position += duplicateTransform.position;
                        newTransform.rotation = newTransform.rotation * duplicateTransform.rotation;
                        newTransform.localScale = Vector3.Scale(newTransform.localScale, duplicateTransform.localScale);
                    }
                    else if (coordinateReference == CoordinateReference.Local)
                    {
                        newTransform.position += selectedTransform.TransformDirection(duplicateTransform.position);
                        newTransform.rotation = newTransform.rotation * Quaternion.Euler(selectedTransform.TransformDirection(duplicateTransform.rotation.eulerAngles));
                        newTransform.localScale = Vector3.Scale(newTransform.localScale, duplicateTransform.localScale);
                    }

                    GameObject newObject = Instantiate(selectedTransform.gameObject, newTransform.position, newTransform.rotation);
                    newObject.transform.localScale = newTransform.localScale;
                    newObject.name = selectedTransform.name;

                    Undo.RegisterCreatedObjectUndo(newObject, "Create Copy");

                    copyTransforms.Add(newObject.transform);
                }

                CommonFunctions.ParentCenterAligner(copyTransforms.ToArray());
            }
        }
    }
}
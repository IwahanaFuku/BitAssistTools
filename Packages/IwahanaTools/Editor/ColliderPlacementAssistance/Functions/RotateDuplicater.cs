using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace ColliderPlacemenAssistancer
{
    public class RotateDuplicater
    {
        public static void RotateDuplicate(float rotationalFrequency)
        {
            GameObject selectedObject = Selection.activeObject as GameObject;

            GameObject[] boundsObjects = GetBoundsObjects(selectedObject);
            Bounds updateBoundingBox = GetUpdateBoundingBox(boundsObjects);
            int numOfCopy = ReturnNumOfCopy(selectedObject, updateBoundingBox, rotationalFrequency);
            float theta = CalculateRotationalFrequencyPerCopy(numOfCopy, rotationalFrequency);
            CopyTransforms(selectedObject, boundsObjects, numOfCopy, theta);
        }

        public static GameObject[] GetBoundsObjects(GameObject selectedObject)
        {
            List<GameObject> boundsObjectsList = new List<GameObject>();

            if(selectedObject.transform.childCount == 0)
            {
                boundsObjectsList.Add(selectedObject);
            }
            else
            {
                foreach (Transform childTransform in selectedObject.transform)
                {
                    boundsObjectsList.Add(childTransform.gameObject);
                }
            }
            
            return boundsObjectsList.ToArray();
        }

        public static Bounds GetUpdateBoundingBox(GameObject[] boundsObjects)
        {
            Bounds bounds = new Bounds();
            bool hasBounds = false;

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
            return bounds;
        }

         public static int ReturnNumOfCopy(GameObject selectedObject, Bounds bounds, float rotationalFrequency)
        {
            var selectedPosition = selectedObject.transform.position;

            Vector3 absCenter = new Vector3(Mathf.Abs(bounds.center.x), Mathf.Abs(bounds.center.y), Mathf.Abs(bounds.center.z))
                            - new Vector3(Mathf.Abs(selectedPosition.x), Mathf.Abs(selectedPosition.y), Mathf.Abs(selectedPosition.z));
            Vector3 edgeCenterX = new Vector3(absCenter.x + bounds.size.x / 2, absCenter.y, absCenter.z);
            Vector3 edgeCenterZ = new Vector3(absCenter.x, absCenter.y, absCenter.z + bounds.size.z / 2);

            float theta;
            int numOfCopy;

            if(edgeCenterZ.sqrMagnitude < edgeCenterX.sqrMagnitude)
            {
                theta = Mathf.Atan2(bounds.size.z / 2, edgeCenterX.x) * Mathf.Rad2Deg * 2;
                numOfCopy = Convert.ToInt32(rotationalFrequency / theta);
                Debug.Log("x");
            }
            else
            {
                theta = Mathf.Atan2(bounds.size.x / 2, edgeCenterZ.z) * Mathf.Rad2Deg * 2;
                numOfCopy = Convert.ToInt32(rotationalFrequency / theta);
                Debug.Log("Z");
            }

            return numOfCopy;
        }


        public static float CalculateRotationalFrequencyPerCopy(int numOfCopy, float rotationalFrequency)
        {
            float theta = rotationalFrequency / numOfCopy;
            return theta;
        }

        public static void CopyTransforms(GameObject selectedObject, GameObject[] boundsObjects, int numOfCopy, float theta)
        {

            Vector3 rotationAxis = Vector3.up;
            Undo.RecordObjects(boundsObjects, "Rotate Duplicate");

            var copyTransformsList= new List<Transform>();
            copyTransformsList.Add(selectedObject.transform);

            for(int i = 0; i < numOfCopy + 1; i++)
            {
                Quaternion rotationQuaternion = Quaternion.AngleAxis(theta * i, rotationAxis);
                GameObject newObject = UnityEngine.Object.Instantiate(selectedObject, selectedObject.transform.position, selectedObject.transform.rotation * rotationQuaternion);
                newObject.name = selectedObject.name;
                copyTransformsList.Add(newObject.transform);

                Undo.RegisterCreatedObjectUndo(newObject, "Rotate Duplicate");
            }

            Transform[] copyTransforms = copyTransformsList.ToArray();
            CommonFunctions.ParentCenterAligner(copyTransforms);
        }
    }
}
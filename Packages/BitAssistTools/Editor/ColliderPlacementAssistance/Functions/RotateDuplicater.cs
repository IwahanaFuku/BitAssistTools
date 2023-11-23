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
            Vector3 boundsObjectsCenterPosition = CalculateCenterPosition(boundsObjects);
            Bounds updateBoundingBox = GetUpdateBoundingBox(boundsObjects);
            int numOfCopy = ReturnNumOfCopy(selectedObject, updateBoundingBox, boundsObjectsCenterPosition, rotationalFrequency);
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

        static Vector3 CalculateCenterPosition(GameObject[] gameObjects)
        {
            Vector3 centerPosition = Vector3.zero;

            foreach (GameObject obj in gameObjects)
            {
                if (obj != null)
                {
                    centerPosition += obj.transform.position;
                }
            }

            centerPosition /= gameObjects.Length;

            return centerPosition;
        }

        static Bounds GetUpdateBoundingBox(GameObject[] boundsObjects)
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

        static int ReturnNumOfCopy(GameObject selectedObject, Bounds bounds, Vector3 centerPosition, float rotationalFrequency)
        {
            var selectedPosition = selectedObject.transform.position;

            Vector3 edgeCenterX = new Vector3(ExtendValues(centerPosition.x - selectedPosition.x, bounds.size.x / 2), centerPosition.y, centerPosition.z - selectedPosition.z);
            Vector3 edgeCenterZ = new Vector3(centerPosition.x - selectedPosition.x, centerPosition.y, ExtendValues(centerPosition.z - selectedPosition.z, bounds.size.z / 2));

            float theta;
            int numOfCopy;

            theta = Mathf.Atan2(edgeCenterX.x, edgeCenterZ.z) * Mathf.Rad2Deg * 2;
            float modifyAngle = ModifyAngle(FlipAngle(NormalizeAngle(theta)));
            numOfCopy = Convert.ToInt32(rotationalFrequency / modifyAngle);

            return numOfCopy;
        }
        static float NormalizeAngle(float angle)
        {
            if(angle < 0)
            {
                return Mathf.Abs(angle);
            }
            else
            {
                return angle;
            }
        }
        static float FlipAngle(float angle)
        {
            if ((angle > 45 && angle <= 90) || (angle > 225 && angle <= 270))
            {
                return 180 - angle;
            }
            else if (angle > 135 && angle <= 225 || (angle > 315 && angle <= 360))
            {
                return 360 - angle;
            }
            else
            {
                return angle;
            }
        }

        static float ModifyAngle(float angle)
        {
            if (angle > 90 && angle <= 180)
            {
                return angle - 90;
            }
            else if (angle > 180 && angle <= 270)
            {
                return angle - 180;
            }
            else if (angle > 270 && angle <= 360)
            {
                return angle - 270;
            }
            else
            {
                return angle;
            }
        }

        static float ExtendValues(float firstValue, float secondValue)
        {
            return (firstValue < 0) ? firstValue - secondValue : firstValue + secondValue;
        }

        static float CalculateRotationalFrequencyPerCopy(int numOfCopy, float rotationalFrequency)
        {
            float theta = rotationalFrequency / numOfCopy;
            return theta;
        }

        static void CopyTransforms(GameObject selectedObject, GameObject[] boundsObjects, int numOfCopy, float theta)
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
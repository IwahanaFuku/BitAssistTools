using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SelectionAsistance
{
    public class HierarchicalSelectionObjects
    {    
        public void HierarchicalSelecter(int parentCount, bool onlyOrder, bool selectChildlen)
        {
            GameObject gameObject = Selection.activeGameObject;
            int targetCurrentDepth = GetDepth(gameObject);
            int objectIndex = gameObject.transform.GetSiblingIndex();
            GameObject parentObject = GetPalentObject(gameObject, parentCount);
            GameObject[] childObjects = SameOrderSelecter(onlyOrder, parentObject, objectIndex, parentCount, targetCurrentDepth);
            ChildlenSelecter(selectChildlen, childObjects);
        }

        GameObject[] SameOrderSelecter(bool onlyOrder, GameObject parentObject, int objectIndex, int parentCount, int targetCurrentDepth)
        {
            GameObject[] childObjects = new GameObject[0];

            if(onlyOrder)
            {
                childObjects = SameOrder(GetChildObjects(parentObject, parentCount + 1, targetCurrentDepth - 1), objectIndex);
                Selection.objects = childObjects;
            }
            if(! onlyOrder){
                childObjects = GetChildObjects(parentObject, parentCount + 1, targetCurrentDepth);
                Selection.objects = childObjects;
            }

            return childObjects;
        }

        void ChildlenSelecter(bool selectChildlen, GameObject[] childObjects)
        {
            if(selectChildlen)
            {
                List<GameObject> selectedObjects = new List<GameObject>();
                for(int i = 0; i < childObjects.Length; i++)
                {
                    GameObject childObject = childObjects[i];
                    foreach (Transform childTransform in childObject.transform)
                    {
                        selectedObjects.Add(childTransform.gameObject);
                    } 
                }
                Selection.objects = selectedObjects.ToArray();
            }
        }

        GameObject GetPalentObject(GameObject gameObject, int parentCount)
        {
            GameObject parentObject = gameObject.transform.parent?.gameObject;
            if (parentCount == 0)
            {
                return gameObject;
            }

            if (parentCount == 1 || parentObject == null)
            {
                return parentObject;
            }

            for (int i = 1; i < parentCount; i++)
            {                
                if (parentObject != null)
                {
                    parentObject = parentObject.transform.parent?.gameObject;
                }
            }
            return parentObject;
        }

        GameObject[] GetChildObjects(GameObject targetObject, int childDepth, int targetCurrentDepth)
        {
            if (childDepth == 0)
            {
                GameObject[] targetObjectList = {targetObject};
                return targetObjectList;
            }
            
            List<GameObject> childObjects = new List<GameObject>();
            GetChildObjectsRecursive(targetObject, 0, childDepth, ref childObjects);

            List<GameObject> selectedObjects = new List<GameObject>();
            foreach (GameObject childObject in childObjects)
            {
                if (GetDepth(childObject) == targetCurrentDepth)
                {
                    selectedObjects.Add(childObject);
                }
            }

            return selectedObjects.ToArray();
        }

        void GetChildObjectsRecursive(GameObject parentObject, int currentDepth, int maxDepth, ref List<GameObject> childObjects)
        {
            foreach (Transform childTransform in parentObject.transform)
            {
                GameObject childObject = childTransform.gameObject;
                childObjects.Add(childObject);
                if (currentDepth < maxDepth)
                {
                    GetChildObjectsRecursive(childObject, currentDepth + 1, maxDepth, ref childObjects);
                }
            }
        }

        int GetDepth(GameObject obj)
        {
            int depth = 0;
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                depth++;
                parent = parent.parent;
            }
            return depth;
        }

        GameObject[] SameOrder(GameObject[] objects, int Index)
        {
            List<GameObject> selectedObjects = new List<GameObject>();
            for(int i = 0; i < objects.Length; i++)
            {
                if (objects[i].transform.childCount > Index)
                {
                    selectedObjects.Add(objects[i].transform.GetChild(Index).gameObject);
                }
            }
            return selectedObjects.ToArray();
        }
    }
}

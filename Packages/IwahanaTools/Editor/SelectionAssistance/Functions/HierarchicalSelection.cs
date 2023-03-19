using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SelectionAsistance
{
    public class HierarchicalSelectionObjects
    {    
        public void HierarchicalSelecter(int parentCount, bool onlyOrder)
        {
            GameObject gameObject = Selection.activeGameObject;
            int objectIndex = gameObject.transform.GetSiblingIndex();
            GameObject parentObject = GetPalentObject(gameObject, parentCount);

            if(onlyOrder)
            {
                GameObject[] childObjects = GetChildObjects(parentObject, parentCount);
                for(int i = 0; i < childObjects.Length; i++)
                {
                    Debug.Log(childObjects[i].name);
                }
                GameObject[] sameOrderObjects = SameOrder(childObjects, objectIndex);
                Selection.objects = sameOrderObjects;
            }
            if(! onlyOrder){
                GameObject[] childObjects = GetChildObjects(parentObject, parentCount + 1);
                Selection.objects = childObjects;
            }
        }

        private GameObject GetPalentObject(GameObject gameObject, int parentCount)
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

        GameObject[] GetChildObjects(GameObject targetObject, int childDepth)
        {
            List<GameObject> childObjects = new List<GameObject>();
            GetChildObjectsRecursive(targetObject, 1, childDepth, ref childObjects);

            List<GameObject> selectedObjects = new List<GameObject>();
            foreach (GameObject childObject in childObjects)
            {
                if (GetDepth(childObject) == childDepth)
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

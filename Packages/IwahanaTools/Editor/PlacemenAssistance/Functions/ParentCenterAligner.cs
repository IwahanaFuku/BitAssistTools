using UnityEngine;
using UnityEditor;

namespace PlacemenAssistance
{
    public class CommonFunctions
    {
        public static void ParentCenterAligner(Transform[] selectedTransforms)
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
    }
}

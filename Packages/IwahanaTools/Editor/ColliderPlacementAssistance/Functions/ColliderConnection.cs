using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace ColliderPlacemenAssistancer
{
    public static class BoxColliderPlacementer
    {
        public static void GenerateBoxesBetweenObjects(List<GameObject> objects)
        {
            for (int i = 0; i < objects.Count - 1; i++)
            {
                Vector3 object1Position = objects[i].transform.position;
                Vector3 object2Position = objects[i + 1].transform.position;

                GenerateDistanceBoxBetweenObjects(object1Position, object2Position);
            }
        }

        private static void GenerateDistanceBoxBetweenObjects(Vector3 position1, Vector3 position2)
        {
            float size = Vector3.Distance(position1, position2);

            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.position = (position1 + position2) / 2;
            box.transform.LookAt(position2);
            box.transform.localScale = new Vector3(1, 1, size);
        }
    }
}

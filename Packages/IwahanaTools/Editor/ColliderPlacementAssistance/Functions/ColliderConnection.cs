using UnityEditor;
using UnityEngine;

public class BoxPlacementEditor : EditorWindow
{
    [MenuItem("MyTools/Place Box Between Selected Objects")]
    public static void ShowWindow()
    {
        GetWindow(typeof(BoxPlacementEditor));
    }

    private Vector2 scrollPosition;
    private GameObject[] selectedObjects;

    private void OnGUI()
    {
        GUILayout.Label("Selected Objects:");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));
        selectedObjects = Selection.gameObjects;
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            EditorGUILayout.ObjectField(selectedObjects[i], typeof(GameObject), true);
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Place Boxes"))
        {
            PlaceBoxes();
        }
    }

    private void PlaceBoxes()
    {
        if (selectedObjects == null || selectedObjects.Length < 2)
        {
            Debug.LogError("Please select at least two objects.");
            return;
        }

        GenerateBoxesBetweenObjects(selectedObjects);
    }

    void GenerateBoxesBetweenObjects(GameObject[] objects)
    {
        for (int i = 0; i < objects.Length - 1; i++)
        {
            Vector3 object1Position = objects[i].transform.position;
            Vector3 object2Position = objects[i+1].transform.position;

            GenerateDistanceBoxBetweenObjects(object1Position, object2Position);
        }
    }
    void GenerateDistanceBoxBetweenObjects(Vector3 position1, Vector3 position2)
    {
        float size = Vector3.Distance(position1, position2);

        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.transform.position = (position1 + position2) / 2;
        box.transform.LookAt(position2);
        box.transform.localScale = new Vector3(1, 1, size);
    }
}

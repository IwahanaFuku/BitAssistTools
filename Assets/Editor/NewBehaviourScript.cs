using UnityEngine;
using UnityEditor;

public class GameObjectPositionEditor : EditorWindow
{
    private GameObject object1;
    private GameObject object2;
    private GameObject object3;

    [MenuItem("CustomTools/GameObject Position Viewer")]
    public static void ShowWindow()
    {
        GameObjectPositionEditor window = GetWindow<GameObjectPositionEditor>();
        window.titleContent = new GUIContent("Position Viewer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select 3 Game Objects", EditorStyles.boldLabel);

        object1 = (GameObject)EditorGUILayout.ObjectField("Object 1:", object1, typeof(GameObject), true);
        object2 = (GameObject)EditorGUILayout.ObjectField("Object 2:", object2, typeof(GameObject), true);
        object3 = (GameObject)EditorGUILayout.ObjectField("Object 3:", object3, typeof(GameObject), true);

        if (GUILayout.Button("Get Positions"))
        {
            if (object1 != null && object2 != null && object3 != null)
            {
                Vector3 position1 = object1.transform.position;
                Vector3 position2 = object2.transform.position;
                Vector3 position3 = object3.transform.position;
                GenerateCornerBox(position1, position2, position3, 1);
            }
            else
            {
                Debug.LogWarning("Please select all three objects.");
            }
        }
    }

    static void GenerateCornerBox(Vector3 vec1, Vector3 vec2, Vector3 vec3, float boxWidth)
    {
        Vector3 vecA = vec2 - vec1;
        Vector3 vecB = vec3 - vec1;
        float angle = Vector3.Angle(vecA, vecB) * Mathf.Deg2Rad / 2; // 角度をラジアンに変換
        float length = CalculateAdjacentSideLength(boxWidth / 2, angle);
        Debug.Log("angle: " + angle * Mathf.Rad2Deg); // デバッグログで角度を度数法で表示
        Debug.Log("length: " + length);
        Vector3 middleVector = (vecA + vecB) / 2.0f;

        // キューブを生成
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // キューブの位置を設定
        cube.transform.position = vec1;

        // キューブのサイズを設定
        cube.transform.localScale = new Vector3(boxWidth, boxWidth, length * 2);

        // キューブを指定された角度で回転させる
        cube.transform.LookAt(cube.transform.position + middleVector); // 回転を度数法に変換
    }

    public static float CalculateAdjacentSideLength(float oppositeSideLength, float thetaInRadians)
    {
        // 隣接辺の長さを計算
        float adjacentSideLength = oppositeSideLength / Mathf.Tan(thetaInRadians);

        return adjacentSideLength;
    }
}

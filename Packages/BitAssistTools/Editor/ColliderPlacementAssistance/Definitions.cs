using UnityEngine;

namespace ColliderPlacemenAssistancer
{
    // カスタムクラスを定義
    [System.Serializable]
    public class PointObjectInfo
    {
        [SerializeField] // シリアライズ属性を追加
        GameObject gameObject;

        // ゲッターとセッターを追加
        public GameObject GameObject
        {
            get { return gameObject; }
            set { gameObject = value; }
        }
    }
    public enum AbstractPosition
    {
        Leftside,
        Center,
        Rightside
    }
}
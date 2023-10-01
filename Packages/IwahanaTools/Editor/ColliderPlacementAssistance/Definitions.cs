using UnityEngine;

namespace ColliderPlacemenAssistancer
{
    // カスタムクラスを定義
    [System.Serializable]
    public class PointObjectInfo
    {
        [SerializeField] // シリアライズ属性を追加
        GameObject _gameObject;

        // ゲッターとセッターを追加
        public GameObject GameObject
        {
            get { return _gameObject; }
            set { _gameObject = value; }
        }
    }
    public enum AbstractPosition
    {
        Leftside,
        Center,
        Rightside
    }

    public struct PlacementType
    {
        public bool Inside;
        public bool Outside;
    }
}
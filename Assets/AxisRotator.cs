using UnityEngine;

/// <summary>
/// 指定した軸周りで自転する
/// </summary>
public class AxisRotator : MonoBehaviour
{
    // 角速度
    [SerializeField] private float _angleSpeed = 90;

    // 回転軸
    [SerializeField] private Vector3 _axis = Vector3.forward;

    private Transform _transform;

    // 初期化
    private void Awake()
    {
        // transformに毎回アクセスすると重いので、キャッシュしておく
        _transform = transform;
    }

    // 回転処理
    private void Update()
    {
        // １フレームで回転する角度を角速度から計算
        var angle = _angleSpeed * Time.deltaTime;

        // 既存のrotationに軸回転のクォータニオンを掛ける
        // クォータニオンを掛ける順序に注意
        _transform.rotation = Quaternion.AngleAxis(angle, _axis) * _transform.rotation;
    }
}
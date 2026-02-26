// MainScene Manager/TurretManager
using System.Collections.Generic;
using UnityEngine;

public class TurretManager : MonoBehaviour
{
    public static TurretManager instance;

    // 設置済みタレットのワールド座標一覧
    // タレット同士の距離チェックに使用
    public List<Vector3> turretPositions = new List<Vector3>();

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    // タレットを登録
    public void RegisterTurret(Vector3 pos)
    {
        turretPositions.Add(pos);
    }

    // 指定位置が既存タレットに近すぎるか判定
    public bool IsTooClose(Vector3 pos, float minDistance)
    {
        foreach (var tPos in turretPositions)
        {
            if (Vector3.Distance(pos, tPos) < minDistance) { return true; }
        }
        return false;
    }
}
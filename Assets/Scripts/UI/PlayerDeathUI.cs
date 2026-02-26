// MainScene Player
using UnityEngine;
using TMPro;

public class PlayerDeathUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI deathText; // 死亡時に表示するテキスト

    private void Update()
    {
        if (PlayerHealth.instance == null)
        {
            Debug.LogWarning("PlayerDeathUI.cs PlayerHealthが存在しません");
            return;
        }
        // プレイヤーが死亡しているかどうかで UI を切り替える
        if (PlayerHealth.instance.getIsDead())
        {
            // 死亡時：テキストを表示
            deathText.gameObject.SetActive(true);
        }
        else
        {
            // 生存時：非表示
            deathText.gameObject.SetActive(false);
        }
    }
}
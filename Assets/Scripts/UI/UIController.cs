// MainScene Manager/UIController
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dropText;   // ドロップアイテム表示用テキスト
    [SerializeField] private TextMeshProUGUI scoreText;  // スコア表示用テキスト

    private List<string> temporaryDropList = new List<string>(); // 一時的に表示するドロップリスト
    public static UIController instance;

    private Coroutine clearCoroutine;  // ドロップ表示を消すコルーチン
    private Coroutine scoreCoroutine;  // スコア表示を消すコルーチン
    private void Awake()
    {
        if (instance == null) { instance = this; }
    }
    // ドロップアイテムをUIに追加して表示
    public void AddToDropList(string name, int amount)
    {
        // 表示上限6件 → 超えたら古いものから削除
        if (temporaryDropList.Count >= 6)
        {
            temporaryDropList.RemoveAt(0);
        }
        // 新しいアイテムを追加
        temporaryDropList.Add($"{name} x{amount}");
        // UI更新
        UpdateDropTextUI();
        // 既存の消去コルーチンが動いていたら止める
        if (clearCoroutine != null)
        {
            StopCoroutine(clearCoroutine);
        }
        // 3秒後にドロップ表示を消す
        clearCoroutine = StartCoroutine(ClearDropListAfterSeconds(3f));
    }
    // ドロップUIを更新
    private void UpdateDropTextUI()
    {
        dropText.text = string.Join("\n", temporaryDropList);
        dropText.gameObject.SetActive(true);
    }
    // 指定秒数後にドロップ表示を消す
    IEnumerator ClearDropListAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        temporaryDropList.Clear();
        dropText.text = "";
        dropText.gameObject.SetActive(false);
    }
    // スコアを表示
    public void ShowScore(float currentScore, float multiplier)
    {
        scoreText.text = $"スコア: {currentScore}（x{multiplier}）";
        scoreText.gameObject.SetActive(true);
        // 既存のコルーチンがあれば停止
        if (scoreCoroutine != null)
        {
            StopCoroutine(scoreCoroutine);
        }
        // 3秒後にスコアを非表示
        scoreCoroutine = StartCoroutine(HideScoreAfterSeconds(3f));
    }
    // 指定秒数後にスコア表示を消す
    IEnumerator HideScoreAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        scoreText.gameObject.SetActive(false);
    }
}
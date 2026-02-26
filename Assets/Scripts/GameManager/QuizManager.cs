//MainScene Manager/QuizManagerObject
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuizManager : MonoBehaviour
{
    public List<QuizData> allQuizzes = new List<QuizData>(); // 全てのクイズデータ
    private List<QuizData> filteredQuizzes; // 選択ジャンルでフィルタリングされたクイズリスト 
    private QuizData currentQuiz; // 現在出題中のクイズ
    private List<QuizData> unusedQuizzes;
    private void Awake()
    {
        LoadAllQuizzes();
    }
    // Resources/Quizzesフォルダから全クイズデータを読み込む
    private void LoadAllQuizzes()
    {
        allQuizzes = Resources.LoadAll<QuizData>("Quizzes").ToList();
    }
    // 選択ジャンルに応じてクイズをフィルタリング
    public void Initialize(string selectedGenre)
    {
        filteredQuizzes = allQuizzes.Where(q => q.genre == selectedGenre).ToList();
        unusedQuizzes = new List<QuizData>(filteredQuizzes);
    }
    // フィルタリングされたクイズからランダムに1問取得
    public QuizData GetRandomQuiz()
    {
        if (filteredQuizzes == null || filteredQuizzes.Count == 0) { return null; }
        if (unusedQuizzes.Count == 0) { unusedQuizzes = new List<QuizData>(filteredQuizzes); }
        int index = Random.Range(0, unusedQuizzes.Count); // 未出題がなくなったらリセット
        currentQuiz = unusedQuizzes[index];
        unusedQuizzes.RemoveAt(index);
        return currentQuiz;
    }
    // 回答が正しいか判定
    public bool CheckAnswer(string selected)
    {
        return selected == currentQuiz.correctAnswer;
    }
    //クイズを順番に取得(デバッグ用)
    private int quizIndex = 0;
    public QuizData GetNextQuiz()
    {
        if (filteredQuizzes == null || filteredQuizzes.Count == 0) { return null; }
        currentQuiz = filteredQuizzes[quizIndex];
        quizIndex++;
        if (quizIndex >= filteredQuizzes.Count)
        {
            quizIndex = 0;
        }
        return currentQuiz;
    }
}
// Scripts/QuizData
using UnityEngine;

[CreateAssetMenu(fileName = "QuizData", menuName = "QuizData/QuizData")]
public class QuizData : ScriptableObject
{
    public string genre; 
    public string question;
    public Texture2D questionImage; 

    public string correctAnswer;
    public string[] wrongAnswers = new string[3];

    public string explanation;
    public Texture2D explanationImage; 
}
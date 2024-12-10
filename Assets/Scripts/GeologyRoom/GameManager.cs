using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // for multiple scenes
using TMPro; // For TextMeshPro

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public char letterGrade = 'A';

    // Reference to the UI Text component
    public TextMeshProUGUI gradeText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialize the grade text on startup
        UpdateGradeText();
    }

    public void DropLetterGrade()
    {
        switch (letterGrade)
        {
            case 'A': letterGrade = 'B'; break;
            case 'B': letterGrade = 'C'; break;
            case 'C': letterGrade = 'D'; break;
            case 'D': letterGrade = 'F'; break;
            case 'F':
                Debug.Log("Go back to Hub");
                break;
        }

        // Update the grade text UI after changing the grade
        UpdateGradeText();
    }

    private void UpdateGradeText()
    {
        if (gradeText != null)
        {
            gradeText.text = $"Grade: {letterGrade}";
        }
        else
        {
            Debug.LogWarning("Grade text is not assigned in the GameManager.");
        }
    }
}

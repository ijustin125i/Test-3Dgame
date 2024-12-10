using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Player")) {
            Debug.Log("Rock and Player have collided");

            // Access GameManager and drop the grade
                GameManager.Instance.DropLetterGrade();
                Debug.Log($"New letter grade: {GameManager.Instance.letterGrade}");
            
        }
    }
}

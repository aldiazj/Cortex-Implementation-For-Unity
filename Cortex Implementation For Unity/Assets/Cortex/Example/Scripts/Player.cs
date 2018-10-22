using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    // Subscribes the Move function to the EmotivTest event that is called whenever an action is registered in the Emotiv
    private void OnEnable()
    {
        EmotivTest.OnMentalCommandEvent += Move;
    }

    private void OnDisable()
    {
        EmotivTest.OnMentalCommandEvent -= Move;
    }

    // Depending on the action received, react moving to the right or left
    void Move(string direction)
    {
        if (direction == "right")
            transform.Translate(Vector3.right * Time.deltaTime);
        else if(transform.position.x > 0)
            transform.Translate(Vector3.left * Time.deltaTime);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurgerTurnBehaviour : MonoBehaviour
{
    [SerializeField] float turnSpeed = 9f;
    void Update()
    {
        RotateAroundItself();
    }

    void RotateAroundItself()
    {
        transform.RotateAround(transform.position, transform.up, Time.deltaTime * turnSpeed);
    }
}

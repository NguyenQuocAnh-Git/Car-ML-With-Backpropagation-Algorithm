using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public void UpScaleX1()
    {
        Time.timeScale = 1f;
    }
    public void UpScaleX3()
    {
        Time.timeScale = 5f;
    }
    public void UpScale0_5()
    {
        Time.timeScale = 0.5f;
    }
    public void UpScale0()
    {
        Time.timeScale = 0f;
    }
}

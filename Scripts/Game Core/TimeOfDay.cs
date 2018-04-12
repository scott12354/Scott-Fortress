using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimeOfDay : Singleton<TimeOfDay> {

    //private static TimeOfDay _instance;
    //public static TimeOfDay Instance
    //{
    //    get
    //    {
    //        if (_instance == null)
    //            _instance = new TimeOfDay();
    //        return _instance;
    //    }
    //}

    private float secondsSinceMidnight;

    public void Update()
    {
        switch (GameManager.Instance.theGamesState)
        {
            case GameSpeedState.NORMAL:
                addTime(Time.deltaTime);
                break;
            case GameSpeedState.PAUSE:
                //do nothing
                break;
            case GameSpeedState.TWOTIMES:
                addTime(Time.deltaTime * 2);
                break;
            default:
                break;
        }
    }

    public TimeOfDay()
    {
        secondsSinceMidnight = 0;
        //on second thought I'll start at noon
        //secondsSinceMidnight = 43200;
    }

    public void addTime(float t)
    {
        //for 1 second = 1 minute
        t *= 60;

        secondsSinceMidnight += t;

        if (secondsSinceMidnight >= 86400)
            secondsSinceMidnight = secondsSinceMidnight - 86400;
    }

    public override string ToString()
    {
        TimeSpan t = TimeSpan.FromSeconds(secondsSinceMidnight);

        string answer = string.Format("Current Time: {0:D2}:{1:D2}", //:{2:D2}",
                        t.Hours,
                        t.Minutes,
                        t.Seconds);
        return answer;
    }

    public float getLightPercent()
    {
        float f = 0;
        if (secondsSinceMidnight >= 43200) {
            f = secondsSinceMidnight - 43200;
            f /= 43200;
            f = 1 - f;
        } else
        {
            f = secondsSinceMidnight;
            f /= 43200;
        }
        return f;
    }
}

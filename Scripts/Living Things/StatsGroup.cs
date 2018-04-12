using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*
 * health
traits
hunger
sleep
thirst
temperature
 * */

//[XmlRoot("StatsGroup")]
public class StatsDataGroup
{
    public float timeElapsed;
    //[XmlArray("StatNames")]
    public string[] statNames;
    //[XmlArray("StatValues")]
    public float[] statValues;
}
public class stat
{
    float max;
    public float current;
    public string name;
    public float percent
    {
        get
        {
            return (current / max) * 100f;
        }
        set
        {
            current = max * value;
        }
    }

    public stat(string nameIn, float maxIn)
    {
        name = nameIn;
        max = maxIn;
        current = max;
    }
    public stat(string nameIn, float maxIn, float currentIn) : this(nameIn, maxIn)
    {
        current = currentIn;
    }
    public void degrade(float deltaT)
    {
        current -= deltaT * GameManager.Instance.statDegredationPerSecond;
        if (current < 0f)
            current =0f;
    }

    public void changeBy(float delta)
    {
        current += delta;
    }
}

public class StatsGroup {
    float timeElapsed;

    stat health, hunger, sleepiness, thirst, temperature;
    List<stat> myStats = new List<stat>();
    public bool isDead
    {
        get
        {
            if (health.percent <=0)
            {
                return true;
            } else
            {
                return false;
            }
        }
    }

    public StatsGroup()
    {
        timeElapsed = 0;
        health = new stat("Health", 100);
        hunger = new stat("Hunger", 100);
        sleepiness = new stat("Sleep", 100);
        thirst = new stat("Thirst", 100);
        temperature = new stat("Temperature", 100);
        myStats.Add(health);
        myStats.Add(hunger);
        myStats.Add(sleepiness);
        myStats.Add(thirst);
        myStats.Add(temperature);
    }

    public StatsGroup(StatsDataGroup data)
    {
        for (int i=0;i<data.statNames.Length;i++)
        {
            myStats.Add(new stat(data.statNames[i], data.statValues[i]));
        }
        health = myStats[0];
        hunger = myStats[1];
        sleepiness = myStats[2];
        thirst = myStats[3];
        temperature = myStats[4];
    }


    public void update(float deltaT)
    {
        timeElapsed += deltaT;
        foreach (var item in myStats)
        {
            if (item.name != "Health")
                item.degrade(deltaT);
        }
    }

    public stat getWorstStat()
    {
        var stats = myStats.OrderByDescending(x => x.percent * -1.0f).ToArray();
        return stats[0];
    }

    public float getStatPercent(string nameIn)
    {
        var thestat = myStats.FirstOrDefault(x => x.name == nameIn);
        return thestat.percent;

    }

    public void damage(float amount)
    {
        health.changeBy(-1f * amount);
    }

    //public StatsDataGroup getSaveData()
    //{
    //    var sav = new StatsDataGroup();
    //    sav.timeElapsed = timeElapsed;
    //    sav.statNames = new string[myStats.Count];
    //    sav.statValues = new float[myStats.Count];
    //    for (int i=0;i<myStats.Count;i++)
    //    {
    //        sav.statNames[i] = myStats[i].name;
    //        sav.statValues[i] = myStats[i].current;
    //    }

    //    return sav;

    //}
}

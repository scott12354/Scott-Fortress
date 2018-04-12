using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecificMinionInfoPanelScript : MonoBehaviour {
    Minion myMinion;
    Image healthBar, hungerBar, thirstBar, sleepBar;
    Text jobText;
    int runtimer = 0;


	// Use this for initialization
	void Start () {
        var healthBarGo = transform.Find("Health Bar Parent Panel").Find("Health Bar").gameObject;
        var hungerBarGo = transform.Find("Hunger Bar Parent Panel").Find("Hunger Bar").gameObject;
        var thirstBarGo = transform.Find("Thirst Bar Parent Panel").Find("Thirst Bar").gameObject;
        var sleepBarGo = transform.Find("Sleep Bar Parent Panel").Find("Sleep Bar").gameObject;
        jobText = transform.Find("Job Text").gameObject.GetComponent<Text>();
        healthBar = healthBarGo.GetComponent<Image>();
        hungerBar = hungerBarGo.GetComponent<Image>();
        thirstBar = thirstBarGo.GetComponent<Image>();
        sleepBar = sleepBarGo.GetComponent<Image>();
    }
	
	// Update is called once per frame
	void Update () {
        if (++runtimer >= 10)
        {
            runtimer = 0;
            if (healthBar != null)
            {
                healthBar.fillAmount = myMinion.myStats.getStatPercent("Health") / 100f;
                hungerBar.fillAmount = myMinion.myStats.getStatPercent("Hunger") / 100f;
                sleepBar.fillAmount = myMinion.myStats.getStatPercent("Sleep") / 100f;
                thirstBar.fillAmount = myMinion.myStats.getStatPercent("Thirst") / 100f;
                if (myMinion.myJob == null)
                {
                    jobText.text = "Current Job: None";
                }
                else
                {
                    jobText.text = "Current Job: " + myMinion.myJob.ToString();
                }
            }
            else
            {
                Debug.LogAssertion("Changed parent names, need to fix this script");
            }
        }
	}

    public void setMinion(Minion m)
    {
        myMinion = m;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Appliance : MonoBehaviour
{
    public StepType stepType; 

    private Item currentItem;

    public bool canCook;

    private float timeInAppliance = 0f;
    private bool isCooking = false;

    private void OnTriggerEnter(Collider other)
    {
        Item item = other.GetComponent<Item>();
        if (item != null && currentItem == null)
        {
            currentItem = item;
            timeInAppliance = 0f;
            isCooking = true;
            Debug.Log($"{item.type} entered fryer");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Item item = other.GetComponent<Item>();
        if (item != null && item == currentItem && canCook)
        {
            isCooking = false;

            if (item.appliedSteps.Count > 0)
            {
                if (item.appliedSteps[item.appliedSteps.Count - 1].stepType != this.stepType)
                {
                    CookingStep step = new CookingStep
                    {
                        stepType = this.stepType,
                        duration = timeInAppliance,
                        type = item.type
                    };

                    item.AddStep(step);
                }
                else
                {
                    item.appliedSteps[item.appliedSteps.Count - 1].duration += timeInAppliance;
                }
            }
            else
            {
                CookingStep step = new CookingStep
                {
                    stepType = this.stepType,
                    duration = timeInAppliance,
                    type = item.type
                };

                item.AddStep(step);
            }

            currentItem = null;
            timeInAppliance = 0f;
        }
    }

    private void Update()
    {
        if (isCooking)
        {
            timeInAppliance += Time.deltaTime;
        }
    }
}
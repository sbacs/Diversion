using System.Collections.Generic;
using UnityEngine;
public class Item : MonoBehaviour
{
    public Type type;
    public List<CookingStep> appliedSteps = new List<CookingStep>();

    public void AddStep(CookingStep step)
    {
        appliedSteps.Add(step);
        Debug.Log($"{type} added step: {step.stepType}");
    }
}

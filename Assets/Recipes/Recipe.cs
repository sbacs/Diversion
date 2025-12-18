using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/Recipe")]
public class RecipeSO : ScriptableObject
{
    public string recipeName;

    [Header("Steps")]
    public List<CookingStep> steps;
}

[System.Serializable]
public class CookingStep
{
    public StepType stepType;
    public float duration;

    public Type type;


    [Tooltip("Used only if stepType is Mix")]
    public List<CookingStep> subSteps;

}


public enum StepType
{
    salt,
    fry,
    boil,
    cookInStove,

    cookInPan,

    mix
}


public enum Type
{
    None,
    egg,
    funghi,
    pasta
}


using UnityEngine;

public class Conveyor : MonoBehaviour
{

    public RecipeSO recipe;

    private void OnTriggerEnter(Collider other)
    {
        Item item = other.GetComponent<Item>();
        bool valid = true;

        if (item.appliedSteps.Count == recipe.steps.Count)
        {
            for (int i = 0; i < item.appliedSteps.Count; i++)
            {

                if (item.appliedSteps[i].stepType != recipe.steps[i].stepType ||
                   item.appliedSteps[i].type != recipe.steps[i].type ||
                   Mathf.Abs(item.appliedSteps[i].duration - recipe.steps[i].duration) > 2.0f)
                {
                    valid = false;
                    break;
                }
            }
        }
        else
        {
            valid = false; // Wrong number of steps
        }

        Debug.Log("recipe is valid ? " + valid);
    }


    private void OnTriggerExit(Collider other)
    {

    }

}
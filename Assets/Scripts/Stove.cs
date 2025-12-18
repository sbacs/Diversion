using UnityEngine;

public class Stove : MonoBehaviour
{


    private void OnTriggerEnter(Collider other)
    {
        Appliance item = other.GetComponent<Appliance>();

        item.canCook = true;
    }

    private void OnTriggerExit(Collider other)
    {
        Appliance item = other.GetComponent<Appliance>();
        
        item.canCook = false;
    }
}

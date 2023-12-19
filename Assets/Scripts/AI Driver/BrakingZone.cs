using UnityEngine;

public class BrakingZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        AICarController controller = other.GetComponent<AICarController>();
        if (controller)
        {
            controller.isInsideBraking = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        AICarController controller = other.GetComponent<AICarController>();
        if (controller)
        {
            controller.isInsideBraking = false;
        }
    }
}

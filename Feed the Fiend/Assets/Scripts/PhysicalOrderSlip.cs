using UnityEngine;

public class PhysicalOrderSlip : MonoBehaviour
{
    private Food_Types[] order;

    public void SetOrder(Food_Types[] newOrder)
    {
        order = newOrder;
    }

    public Food_Types[] GetOrder()
    {
        return order;
    }
}


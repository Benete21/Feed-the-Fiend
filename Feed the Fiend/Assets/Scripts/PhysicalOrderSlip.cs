
using UnityEngine;

public class PhysicalOrderSlip : MonoBehaviour
{
    private Food_Types[] order;

    [Header("Table Assignment")]
    [SerializeField] private int tableNumber;

    public int TableNumber => tableNumber;

    public void SetOrderDetails(
        Food_Types[] newOrder,
        int table)
    {
        SetOrder(newOrder);
        tableNumber = table;
    }

    public void SetOrder(Food_Types[] newOrder)
    {
        order = newOrder;
    }

    public Food_Types[] GetOrder()
    {
        return order;
    }
}

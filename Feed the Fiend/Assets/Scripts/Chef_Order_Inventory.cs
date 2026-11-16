
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class Chef_Order_Inventory : MonoBehaviour
{
    public const int Capacity = 4;

    [System.Serializable]
    public class OrderEntry
    {
        public Food_Types food;
        public int tableNumber;
    }

    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TMP_Text inventoryText;

    private readonly List<OrderEntry> orders =
        new List<OrderEntry>();

    public int Count => orders.Count;
    public IReadOnlyList<OrderEntry> Orders => orders;

    private void Awake()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        RefreshUI();
    }

    public bool ReceiveOrder(Food_Types[] foodOrder, int tableNumber)
    {
        if (foodOrder == null || foodOrder.Length == 0)
        {
            Debug.LogWarning("Cannot receive an empty order");
            return false;
        }

        // If inventory is full, remove the oldest order first.
        if (!HasSpace())
        {
            OrderEntry removedOrder = orders[0];

            Debug.Log(
                "Inventory full! Removing oldest order from Table "
                + removedOrder.tableNumber
            );

            orders.RemoveAt(0);
        }

        // Add the new order.
        orders.Add(new OrderEntry
        {
            food = foodOrder[0],
            tableNumber = tableNumber
        });

        RefreshUI();

        Debug.Log("New order received for Table " + tableNumber);

        return true;
    }


    public bool HasSpace()
    {
        return orders.Count < Capacity;
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("Inventory Panel is not assigned!");
            return;
        }

        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        RefreshUI();
    }

    public bool TryGetOrderForTable(
        int tableNumber,
        out Food_Types food)
    {
        foreach (OrderEntry entry in orders)
        {
            if (entry.tableNumber == tableNumber)
            {
                food = entry.food;
                return true;
            }
        }

        food = default;
        return false;
    }

    public bool CompleteOrderForTable(int tableNumber)
    {
        int index = orders.FindIndex(
            order => order.tableNumber == tableNumber
        );

        if (index < 0)
            return false;

        orders.RemoveAt(index);
        RefreshUI();

        Debug.Log("Removed completed order for Table " + tableNumber);
        return true;
    }

    private void RefreshUI()
    {
        if (inventoryText == null)
            return;

        StringBuilder text = new StringBuilder();
        text.AppendLine("CHEF ORDERS");
        text.AppendLine();

        for (int i = 0; i < Capacity; i++)
        {
            if (i < orders.Count)
            {
                OrderEntry entry = orders[i];

                string foodName = entry.food.ToString()
                    .Replace("_", " ");

                text.AppendLine((i + 1) + " " + foodName);
            }
            else
            {
                text.AppendLine((i + 1) + " Empty");
            }
        }

        inventoryText.text = text.ToString();
    }

}

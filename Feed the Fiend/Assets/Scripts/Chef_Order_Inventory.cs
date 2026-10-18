using System.Collections.Generic;
using System.Text;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Chef_Order_Inventory : MonoBehaviour
{
    public const int Capacity = 4;

    [Serializable]
    public class OrderEntry
    {
        public Food_Types food;
        public int tableNumber;
    }

    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TMP_Text inventoryText;

    [Header("Optional Slot Buttons")]
    [SerializeField] private Button[] slotButtons = new Button[Capacity];

    [Header("Assignment UI")]
    [SerializeField] private GameObject assignmentPanel;
    [SerializeField] private TMP_Text assignmentText;
    [SerializeField] private Button[] tableButtons;

    [Header("Available Tables")]
    [SerializeField] private CustomerOrder[] customers;

    private readonly List<OrderEntry> orders = new List<OrderEntry>();
    private int selectedIndex = -1;

    public IReadOnlyList<OrderEntry> Orders => orders;
    public int Count => orders.Count;

    private void Awake()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (assignmentPanel != null)
            assignmentPanel.SetActive(false);

        RefreshUI();
    }

    public bool ReceiveOrder(Food_Types[] foodOrder, int tableNumber)
    {
        if (orders.Count >= Capacity)
        {
            Debug.LogWarning("Chef's order inventory is full!");
            return false;
        }

        if (foodOrder == null || foodOrder.Length == 0)
        {
            Debug.LogWarning("Cannot receive an empty order.");
            return false;
        }

        OrderEntry entry = new OrderEntry
        {
            food = foodOrder[0],
            tableNumber = tableNumber
        };

        orders.Add(entry);

        Debug.Log(
            "Chef received order for Table " + tableNumber +
            ": " + entry.food
        );

        RefreshUI();
        return true;
    }

    public bool HasSpace()
    {
        return orders.Count < Capacity;
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null)
            return;

        bool open = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(open);

        if (!open && assignmentPanel != null)
            assignmentPanel.SetActive(false);

        RefreshUI();
    }

    public void OpenAssignmentBoard()
    {
        if (assignmentPanel == null)
            return;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(true);

        assignmentPanel.SetActive(true);
        RefreshUI();
    }

    public void SelectOrder(int index)
    {
        if (index < 0 || index >= orders.Count)
            return;

        selectedIndex = index;
        RefreshUI();
    }

    public void AssignSelectedOrderToTable(int customerIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= orders.Count)
            return;

        if (customers == null ||
            customerIndex < 0 ||
            customerIndex >= customers.Length)
            return;

        CustomerOrder target = customers[customerIndex];

        if (target == null)
            return;

        OrderEntry entry = orders[selectedIndex];

        entry.tableNumber = target.TableNumber;

        RefreshUI();

        Debug.Log(
            "Order assigned to Table " + entry.tableNumber
        );
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

    private void RefreshUI()
    {
        if (inventoryText != null)
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("CHEF ORDERS");

            for (int i = 0; i < Capacity; i++)
            {
                if (i < orders.Count)
                {
                    OrderEntry entry = orders[i];

                    string selected = i == selectedIndex ? " > " : "   ";

                    text.AppendLine(
                        selected + (i + 1) + ". Table " +
                        entry.tableNumber + " - " + entry.food
                    );
                }
                else
                {
                    text.AppendLine("   " + (i + 1) + ". Empty");
                }
            }

            inventoryText.text = text.ToString();
        }

        if (slotButtons != null)
        {
            for (int i = 0; i < slotButtons.Length; i++)
            {
                int index = i;

                if (slotButtons[i] == null)
                    continue;

                slotButtons[i].interactable = i < orders.Count;

                slotButtons[i].onClick.RemoveAllListeners();
                slotButtons[i].onClick.AddListener(
                    () => SelectOrder(index)
                );
            }
        }

        if (assignmentText != null)
        {
            if (selectedIndex >= 0 && selectedIndex < orders.Count)
            {
                OrderEntry entry = orders[selectedIndex];

                assignmentText.text =
                    "Selected: " + entry.food +
                    "\nAssigned to Table " + entry.tableNumber;
            }
            else
            {
                assignmentText.text = "Select an order first";
            }
        }

        if (tableButtons != null)
        {
            for (int i = 0; i < tableButtons.Length; i++)
            {
                int index = i;

                if (tableButtons[i] == null)
                    continue;

                tableButtons[i].onClick.RemoveAllListeners();
                tableButtons[i].onClick.AddListener(
                    () => AssignSelectedOrderToTable(index)
                );

                tableButtons[i].interactable =
                    selectedIndex >= 0 &&
                    selectedIndex < orders.Count &&
                    customers != null &&
                    index < customers.Length &&
                    customers[index] != null;
            }
        }
    }
}

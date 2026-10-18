
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class OrderSlipDropOff : MonoBehaviour
{
    [Header("Physical Slip Area")]
    [SerializeField] private Transform[] slipPositions;

    [Header("Order Board UI")]
    [SerializeField] private GameObject boardPanel;
    [SerializeField] private TMP_Text boardText;

    [Header("Chef Inventory")]
    [SerializeField] private Chef_Order_Inventory chefInventory;

    private readonly List<PhysicalOrderSlip> slips =
        new List<PhysicalOrderSlip>();

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (boardPanel != null)
            boardPanel.SetActive(false);

        RefreshBoard();
    }

    public bool HasOrders()
    {
        CleanupSlips();
        return slips.Count > 0;
    }

    // The waiter calls this when dropping off an order slip.
    public bool DepositSlip(PhysicalOrderSlip slip)
    {
        if (slip == null)
            return false;

        CleanupSlips();

        if (slips.Contains(slip))
            return false;

        if (slipPositions == null ||
            slips.Count >= slipPositions.Length)
        {
            Debug.LogWarning("Order counter is full!");
            return false;
        }

        // Validate the slot before adding anything to inventory.
        Transform slot = slipPositions[slips.Count];

        if (slot == null)
        {
            Debug.LogError("A slip position is missing.");
            return false;
        }

        if (chefInventory == null || !chefInventory.HasSpace())
        {
            Debug.LogWarning("Chef's inventory is full!");
            return false;
        }

        Food_Types[] order = slip.GetOrder();

        if (order == null || order.Length == 0)
        {
            Debug.LogWarning("Order slip contains no food.");
            return false;
        }

        // Add the order to the chef's inventory immediately.
        bool received = chefInventory.ReceiveOrder(
            order,
            slip.TableNumber
        );

        if (!received)
            return false;

        // Keep the physical slip on the counter.
        slips.Add(slip);

        Rigidbody rb = slip.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        Collider col = slip.GetComponent<Collider>();

        if (col != null)
            col.enabled = false;

        slip.transform.SetParent(slot);
        slip.transform.localPosition = Vector3.zero;
        slip.transform.localRotation = Quaternion.identity;

        RefreshBoard();

        Debug.Log(
            "Order added to chef inventory for Table " +
            slip.TableNumber
        );

        return true;
    }

    // The chef opens or closes the board to view orders.
    // This does NOT take an order out of inventory.
    public void InteractWithChef()
    {
        CleanupSlips();

        IsOpen = !IsOpen;

        if (boardPanel != null)
            boardPanel.SetActive(IsOpen);

        RefreshBoard();
    }

    public void CloseBoard()
    {
        IsOpen = false;

        if (boardPanel != null)
            boardPanel.SetActive(false);

        RefreshBoard();
    }

    private void CleanupSlips()
    {
        slips.RemoveAll(slip => slip == null);
    }

    private void RefreshBoard()
    {
        CleanupSlips();

        if (boardText == null)
            return;

        StringBuilder text = new StringBuilder();

        text.AppendLine("WAITING ORDERS");
        text.AppendLine();

        if (slips.Count == 0)
        {
            text.AppendLine("No orders waiting.");
        }
        else
        {
            for (int i = 0; i < slips.Count; i++)
            {
                PhysicalOrderSlip slip = slips[i];

                if (slip == null)
                    continue;

                Food_Types[] order = slip.GetOrder();

                string foodName =
                    order != null && order.Length > 0
                    ? order[0].ToString()
                    : "Unknown";

                text.AppendLine(
                    (i + 1) + ". Table " +
                    slip.TableNumber + " - " + foodName
                );
            }
        }

        text.AppendLine();
        text.AppendLine(
            IsOpen
            ? "Orders are stored in chef inventory."
            : "Press A to view orders"
        );

        boardText.text = text.ToString();
    }

    // Called when the customer's order is completed.
    public void RemoveSlipForTable(int tableNumber)
    {
        for (int i = slips.Count - 1; i >= 0; i--)
        {
            PhysicalOrderSlip slip = slips[i];

            if (slip != null &&
                slip.TableNumber == tableNumber)
            {
                slips.RemoveAt(i);
                Destroy(slip.gameObject);
                break;
            }
        }

        RefreshBoard();
    }
}

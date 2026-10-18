
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class OrderSlipDropOff : MonoBehaviour
{
    [Header("Physical Slip Area")]
    [SerializeField] private Transform[] slipPositions;

    [Header("Chef Inventory")]
    [SerializeField] private Chef_Order_Inventory chefInventory;

    private readonly List<PhysicalOrderSlip> slips =
        new List<PhysicalOrderSlip>();


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


        Debug.Log(
            "Order added to chef inventory for Table " +
            slip.TableNumber
        );

        return true;
    }


    private void CleanupSlips()
    {
        slips.RemoveAll(slip => slip == null);
    }

 
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
    }
}

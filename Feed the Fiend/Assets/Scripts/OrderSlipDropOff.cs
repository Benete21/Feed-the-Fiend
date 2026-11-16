
using System.Collections.Generic;
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

        if (slipPositions == null || slipPositions.Length == 0)
        {
            Debug.LogWarning("No slip positions assigned!");
            return false;
        }

        if (chefInventory == null)
        {
            Debug.LogError("Chef inventory is not assigned!");
            return false;
        }

        Food_Types[] order = slip.GetOrder();

        if (order == null || order.Length == 0)
        {
            Debug.LogWarning("Order slip contains no food.");
            return false;
        }

        // If the physical slip counter is full, remove the oldest slip.
        if (slips.Count >= slipPositions.Length)
        {
            RemoveOldestSlip();
        }

        // If chef inventory is full, remove its oldest order.
        // ReceiveOrder also handles this automatically.
        bool received = chefInventory.ReceiveOrder(
            order,
            slip.TableNumber
        );

        if (!received)
            return false;

        // Add the new physical slip.
        slips.Add(slip);

        RefreshSlipPositions();

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

        Debug.Log(
            "New order added for Table " + slip.TableNumber
        );

        return true;
    }

    // Removes the oldest physical slip.
    private void RemoveOldestSlip()
    {
        if (slips.Count == 0)
            return;

        PhysicalOrderSlip oldestSlip = slips[0];

        if (oldestSlip != null)
        {
            int oldTableNumber = oldestSlip.TableNumber;

            // Remove its corresponding chef inventory order.
            chefInventory.CompleteOrderForTable(oldTableNumber);

            Destroy(oldestSlip.gameObject);

            Debug.Log(
                "Removed oldest physical slip from Table " +
                oldTableNumber
            );
        }

        slips.RemoveAt(0);

        RefreshSlipPositions();
    }

    // Reorganises the remaining slips after one is removed.
    private void RefreshSlipPositions()
    {
        CleanupSlips();

        for (int i = 0; i < slips.Count; i++)
        {
            if (slips[i] == null || slipPositions[i] == null)
                continue;

            slips[i].transform.SetParent(slipPositions[i]);
            slips[i].transform.localPosition = Vector3.zero;
            slips[i].transform.localRotation = Quaternion.identity;
        }
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

            if (slip != null && slip.TableNumber == tableNumber)
            {
                slips.RemoveAt(i);
                Destroy(slip.gameObject);

                RefreshSlipPositions();

                break;
            }
        }
    }
}

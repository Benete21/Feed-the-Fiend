using UnityEngine;

public class Chef_order_Board : MonoBehaviour
{
    public void OpenBoard(Chef_Order_Inventory inventory)
    {
        if (inventory == null)
            return;

        inventory.OpenAssignmentBoard();
    }
}


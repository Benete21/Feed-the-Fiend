
using TMPro;
using UnityEngine;

public class Order_Slip : MonoBehaviour
{
    public TMP_Text orderText;

    public void SetOrder(Food_Types[] order)
    {
        orderText.text = "ORDER\n\n";

        foreach (Food_Types food in order)
        {
            string displayName = food.ToString()
                .Replace("_", " ");

            orderText.text += displayName + "\n";
        }
    }
}

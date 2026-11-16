
using UnityEngine;

public class RestrauntTable : MonoBehaviour
{
    [Header("Table Information")]
    public int tableNumber;

    [Header("Chairs")]
    [Tooltip("Add the seating positions for each chair around this table.")]
    public Transform[] chairs;

    private bool[] chairOccupied;

    // True only when every chair is occupied.
    [HideInInspector]
    public bool isOccupied = false;

    private void Awake()
    {
        InitializeChairs();
    }

    private void InitializeChairs()
    {
        if (chairs == null)
        {
            chairs = new Transform[0];
        }

        chairOccupied = new bool[chairs.Length];

        isOccupied = chairs.Length == 0;
    }

    public bool HasAvailableChair()
    {
        if (chairOccupied == null ||
            chairOccupied.Length != chairs.Length)
        {
            InitializeChairs();
        }

        for (int i = 0; i < chairs.Length; i++)
        {
            if (chairs[i] != null && !chairOccupied[i])
                return true;
        }

        return false;
    }

    public bool TryReserveChair(out Transform reservedChair)
    {
        reservedChair = null;

        if (chairOccupied == null ||
            chairOccupied.Length != chairs.Length)
        {
            InitializeChairs();
        }

        for (int i = 0; i < chairs.Length; i++)
        {
            if (chairs[i] != null && !chairOccupied[i])
            {
                chairOccupied[i] = true;
                reservedChair = chairs[i];

                UpdateOccupiedStatus();

                return true;
            }
        }

        UpdateOccupiedStatus();
        return false;
    }

    public void ReleaseChair(Transform chair)
    {
        if (chair == null || chairOccupied == null)
            return;

        for (int i = 0; i < chairs.Length; i++)
        {
            if (chairs[i] == chair)
            {
                chairOccupied[i] = false;
                break;
            }
        }

        UpdateOccupiedStatus();
    }

    private void UpdateOccupiedStatus()
    {
        if (chairOccupied == null || chairOccupied.Length == 0)
        {
            isOccupied = true;
            return;
        }

        isOccupied = true;

        for (int i = 0; i < chairOccupied.Length; i++)
        {
            if (!chairOccupied[i] && chairs[i] != null)
            {
                isOccupied = false;
                return;
            }
        }
    }
}

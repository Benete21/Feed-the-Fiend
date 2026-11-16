using UnityEngine;

public class Trail_Screen_Effect : MonoBehaviour
{
    public enum TrailType
    {
        Toxic,
        Fire
    }

    [Header("Trail Type")]
    [SerializeField] private TrailType trailType;

    [Header("Screen Colours")]
    [SerializeField]
    private Color toxicColour =
        new Color(0.2f, 1f, 0.1f);

    [SerializeField]
    private Color fireColour =
        new Color(1f, 0.35f, 0f);

    public void SetTrailType(TrailType type)
    {
        trailType = type;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check the player object and its parents for the Player tag.
        Transform current = other.transform;
        bool isPlayer = false;

        while (current != null)
        {
            if (current.CompareTag("Player"))
            {
                isPlayer = true;
                break;
            }

            current = current.parent;
        }

        if (!isPlayer)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Camera_Trail feedback =
            mainCamera.GetComponent<Camera_Trail>();

        if (feedback == null)
            return;

        Color effectColour = trailType == TrailType.Toxic
            ? toxicColour
            : fireColour;

        feedback.PlayTrailEffect(effectColour);
    }
}


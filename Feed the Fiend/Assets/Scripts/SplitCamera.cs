using UnityEngine;

public class SplitCamera : MonoBehaviour
{

    public Camera cam1;
    public Camera cam2;

    public Transform player1;
    public Transform player2;

    public float splitDistance = 10f;
    public float transition = 2f;
    public float mergeSpeed = 5f;

    private float currentTransform;

    void Update()
    {
        float dist = Vector3.Distance(player1.position, player2.position);
        float TargetTransform = Mathf.InverseLerp(splitDistance - transition, splitDistance + transition, dist); 
        TargetTransform = Mathf.SmoothStep(0f,1f, TargetTransform);

        currentTransform = Mathf.Lerp(currentTransform, TargetTransform, Time.deltaTime* mergeSpeed);
        ApplySplit(currentTransform);
    }

    public void ApplySplit(float t)
    {
        if(t <= 0.01f)
        {
            cam1.rect = new Rect(0f, 0f, 1f, 1f);
            cam2.enabled = false;
            return;
        }
        cam2.enabled=true;

        if(t >= 0.99f)
        {
            cam1.rect = new Rect(0f, 0f, 0.5f, 1f);
            cam2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
            return;
        }

        float leftWidth = Mathf.Lerp(1f,0.5f,t);
        float rightX = Mathf.Lerp(1f, 0.5f, t);

        cam1.rect = new Rect(0f, 0f, leftWidth, 1f);
        cam2.rect = new Rect(rightX, 0f, 1f - rightX, 1f);
    }
}

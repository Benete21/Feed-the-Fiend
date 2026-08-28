using UnityEngine;
using System.Collections.Generic;

public class ShowInstructions : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("Add all instruction panels here")]
    public List<GameObject> panels = new List<GameObject>();

    [Header("Starting Panel")]
    [Tooltip("Which panel should be enabled when the game starts?")]
    public int startingPanel = 0;

    private int currentPanelIndex = -1;
    private int lastDisabledPanelIndex = -1;

    private void Start()
    {
        DisableAllPanels();

        if (panels.Count > 0)
        {
            currentPanelIndex = Mathf.Clamp(startingPanel, 0, panels.Count - 1);
            EnablePanel(currentPanelIndex);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            EnableLastDisabledPanel();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            DisableCurrentPanel();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            PreviousPanel();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            NextPanel();
        }
    }

    public void EnablePanel(int index)
    {
        if (index < 0 || index >= panels.Count)
            return;

        DisableAllPanels();

        if (panels[index] != null)
        {
            panels[index].SetActive(true);
            currentPanelIndex = index;
        }
    }

    public void DisableCurrentPanel()
    {
        if (currentPanelIndex >= 0 && currentPanelIndex < panels.Count)
        {
            if (panels[currentPanelIndex] != null)
            {
                panels[currentPanelIndex].SetActive(false);
            }

            lastDisabledPanelIndex = currentPanelIndex;
            currentPanelIndex = -1;
        }
    }

    public void EnableLastDisabledPanel()
    {
        if (lastDisabledPanelIndex >= 0)
        {
            EnablePanel(lastDisabledPanelIndex);
        }
    }

    public void NextPanel()
    {
        if (panels.Count == 0)
            return;

        int nextIndex;

        if (currentPanelIndex == -1)
        {
            nextIndex = 0;
        }
        else
        {
            nextIndex = (currentPanelIndex + 1) % panels.Count;
        }

        EnablePanel(nextIndex);
    }

    public void PreviousPanel()
    {
        if (panels.Count == 0)
            return;

        int previousIndex;


        if (currentPanelIndex == -1)
        {
            previousIndex = panels.Count - 1;
        }
        else
        {
            previousIndex = (currentPanelIndex - 1 + panels.Count) % panels.Count;
        }

        EnablePanel(previousIndex);
    }

    private void DisableAllPanels()
    {
        foreach (GameObject panel in panels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}
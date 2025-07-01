using UnityEngine;
using System.Collections.Generic;

public class MapSelectionGroup : MonoBehaviour
{
    [SerializeField] private List<MapSelectionUI> mapButtons;

    private MapSelectionUI currentSelected;

    private void Awake()
    {
        foreach (var btn in mapButtons)
        {
            btn.SetGroup(this);
        }
    }

    public void OnButtonSelected(MapSelectionUI selected)
    {
        if (currentSelected != null)
            currentSelected.SetSelected(false);

        currentSelected = selected;
        currentSelected.SetSelected(true);
    }
}
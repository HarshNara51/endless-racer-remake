using System.Collections.Generic;
using UnityEngine;

public class RoadVegetationClearer : MonoBehaviour
{
    // We keep a list of the exact trees this specific road piece hid
    private List<GameObject> hiddenTrees = new List<GameObject>();

    void OnTriggerEnter(Collider other)
    {
        // If the road touches a tree...
        if (other.CompareTag("Tree"))
        {
            // Hide it, and add it to our memory list!
            other.gameObject.SetActive(false);
            hiddenTrees.Add(other.gameObject);
        }
    }

    void OnDestroy()
    {
        // When the road spawner destroys this road piece behind the player...
        // Unhide every tree we kept in our memory!
        foreach (GameObject tree in hiddenTrees)
        {
            if (tree != null)
            {
                tree.SetActive(true);
            }
        }
    }
}
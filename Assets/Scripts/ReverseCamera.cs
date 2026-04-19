using UnityEngine;

public class ReverseCamera : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject rearCamera;

    void Update()
    {
        // 🔥 THE FIX: Removed the 'S' key trigger so it stops fighting with your brakes!
        if (Input.GetKey(KeyCode.R))
        {
            mainCamera.SetActive(false);
            rearCamera.SetActive(true);
        }
        else
        {
            mainCamera.SetActive(true);
            rearCamera.SetActive(false);
        }
    }
}
using UnityEngine;

public class DragGroupX : MonoBehaviour
{
    private Camera cam;
    private Vector3 grabOffset;
    private bool dragging;

    void Awake()
    {
        cam = Camera.main;
    }

    void OnMouseDown()
    {
        dragging = true;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;

        grabOffset = transform.position - mouseWorld;
    }

    void OnMouseUp()
    {
        dragging = false;
    }

    void Update()
    {
        if (!dragging) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;

        Vector3 target = mouseWorld + grabOffset;

        transform.position = new Vector3(target.x, transform.position.y, transform.position.z);
    }
}

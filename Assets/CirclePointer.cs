using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirclePointer : MonoBehaviour
{
    public float radius = 5f; //radius of circle background
    public float offsetDistance = 0.2f; // distance to offset from circle edge
    private Vector3 centerPoint;
    private bool isDragging = false;
    // Start is called before the first frame update
    void Start()
    {
        centerPoint = GameObject.FindGameObjectWithTag("CircleBackground").transform.position;
    }
    public bool IsDragging { get { return isDragging; } }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(mousePos, (Vector2)transform.position) < 0.5f)
            {
                isDragging = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = ((Vector2)mousePos - (Vector2)centerPoint).normalized;
            // Add offsetDistance to radius when positioning
            transform.position = (Vector2)centerPoint + direction * (radius + offsetDistance);
        }


    }
}

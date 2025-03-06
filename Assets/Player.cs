using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float attractionStrength = 2f;
    public float jumpForce = 10f;
    public float gravityScale = 1f;

    private Rigidbody2D rb;
    private Transform pointer;
    private CirclePointer pointerScript;
    private bool isGrounded;
    private bool isStickingToPlatform = false;
    private bool isAttracted = false;  // to control attraction state

    private ContactPoint2D[] contactPoints = new ContactPoint2D[1];
    private Vector2 platformNormal;

    [Header("Platform Sticking")]
    public float stickForce = 20f;
    public LayerMask platformLayer;
    private Vector2 lastStickNormal;

    [Header("Attraction Settings")]
    public float attractionDuration = 2f; // duration of attraction after jumping
    private float attractionTimer = 0f;
    private Vector3 startPosition; // Store initial position

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Set up physics first
        rb.gravityScale = gravityScale;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        startPosition = transform.position;
    }

    void Start()
    {
        StartCoroutine(InitializePointer());
    }

    private IEnumerator InitializePointer()
    {
        // Wait for a frame to ensure other objects are initialized
        yield return new WaitForEndOfFrame();

        GameObject pointerObj = GameObject.FindGameObjectWithTag("Pointer");
        if (pointerObj == null)
        {
            Debug.LogError("Pointer not found! Retrying...");
            yield return new WaitForSeconds(0.1f);
            pointerObj = GameObject.FindGameObjectWithTag("Pointer");
        }

        if (pointerObj != null)
        {
            pointer = pointerObj.transform;
            pointerScript = pointerObj.GetComponent<CirclePointer>();
            Debug.Log($"Pointer initialized at position: {pointer.position}");
        }
        else
        {
            Debug.LogError("Failed to find Pointer after retry!");
        }
    }


    void Update()
    {

        // Update attraction timer
        if (isAttracted && !isStickingToPlatform)
        {
            attractionTimer += Time.deltaTime;
            if (attractionTimer >= attractionDuration)
            {
                isAttracted = false;
                attractionTimer = 0f;
            }
            HandleAttraction();
        }

        // Handle jumping
        if (Input.GetKeyDown(KeyCode.Space) && isStickingToPlatform)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPlayer();
        }
    }

    void FixedUpdate()
    {
        if (isStickingToPlatform)
        {
            // Apply stick force in the opposite direction of the platform normal
            rb.AddForce(-lastStickNormal * stickForce);
            rb.velocity = Vector2.zero; // Prevent sliding
        }
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("exit"))
        {
            CompleteLevel();

        }

        if (collision.contactCount > 0)
        {
            collision.GetContacts(contactPoints);
            lastStickNormal = contactPoints[0].normal;

            isStickingToPlatform = true;
            isAttracted = false;
            rb.velocity = Vector2.zero;
            attractionTimer = 0f; // Reset timer when landing
        }


    }



    void OnCollisionExit2D(Collision2D collision)
    {
        isStickingToPlatform = false;

    }

    private void Jump()
    {
        isStickingToPlatform = false;
        isAttracted = true;
        attractionTimer = 0f; // Reset timer when jumping
        rb.velocity = Vector2.zero;

        // Calculate jump direction perpendicular to the platform surface
        Vector2 jumpDirection = lastStickNormal;

        // Apply an upward force in the direction of the platform normal
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
    }

    private void HandleAttraction()
    {
        Vector2 directionToPointer = ((Vector2)pointer.position - rb.position).normalized;
        // Increase force when further from pointer
        float distance = Vector2.Distance(pointer.position, transform.position);
        float attractionMultiplier = Mathf.Clamp(distance, 1f, 5f);

        rb.AddForce(directionToPointer * attractionStrength * attractionMultiplier, ForceMode2D.Force);
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, moveSpeed);
    }

    private void CompleteLevel()
    {
        // Hide player
        gameObject.SetActive(false);
    }


    private void ResetPlayer()
    {
        // Reset position
        transform.position = startPosition;

        // Reset physics
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Reset states
        isStickingToPlatform = false;
        isAttracted = false;
        attractionTimer = 0f;

        // Re-enable if was disabled
        gameObject.SetActive(true);
    }
}
using UnityEngine;
public class TumbleWeed : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }
    private void Update()
    {
        // maintain the speed
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }
}
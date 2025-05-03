using UnityEngine;

public class CleansedBird : MonoBehaviour
{
    [SerializeField] private float _flightSpeed = 2f;

    private Vector2 _flightDirection;
    public void Start()
    {
        // random x between -1 and 1
        float randomX = Random.Range(-1f, 1f);
        _flightDirection = new Vector2(randomX, 1f);
        if (_flightDirection.x > 0)
        {
            // flip sprite
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }
    public void Update()
    {
        // Move the bird in the specified direction
        Vector2 newPosition = (Vector2)transform.position + _flightDirection * _flightSpeed * Time.deltaTime;
        transform.position = newPosition;
        // Check if the bird is off camera and then destroy it
        if (Camera.main != null)
        {
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
            if (screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1)
            {
                Destroy(gameObject);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.Assertions;

public class InitialVelocityComponent : MonoBehaviour
{
    [SerializeField]
    private Vector3 initialVelocity = new Vector3(0f, 0f, 0f);

    protected void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Assert.IsNotNull(rb, "InitialVelocityComponent requires a Rigidbody2D component.");

        if (rb != null)
        {
            rb.linearVelocity = initialVelocity;
        }
    }
}
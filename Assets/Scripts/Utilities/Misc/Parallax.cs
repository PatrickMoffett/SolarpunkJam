using NUnit.Framework;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Parallax : MonoBehaviour
{
    public GameObject camera;
    public float parallaxEffectMultiplier = 0.5f;

    private float _spriteLength, _startPos;

    private void Start()
    {
        Assert.IsNotNull(camera, "Camera is not assigned in the inspector.");

        _startPos = transform.position.x;
        _spriteLength = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void Update()
    {
        float temp = (camera.transform.position.x * (1 - parallaxEffectMultiplier));
        float distance = (camera.transform.position.x * parallaxEffectMultiplier);

        transform.position = new Vector3(_startPos + distance, transform.position.y, transform.position.z);

        if (temp > _startPos + _spriteLength)
        {
            _startPos += _spriteLength;
        }
        else if (temp < _startPos - _spriteLength)
        {
            _startPos -= _spriteLength;
        }
    }
}
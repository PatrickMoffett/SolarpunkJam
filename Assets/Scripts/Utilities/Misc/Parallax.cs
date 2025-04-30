using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(SpriteRenderer))]
public class Parallax : MonoBehaviour
{
    [SerializeField] private GameObject _camera;
    [SerializeField] private float parallaxEffectMultiplier = 0.5f;

    private float _spriteLength, _startPos;

    private void Start()
    {
        Assert.IsNotNull(_camera, "Camera is not assigned in the inspector.");

        _startPos = transform.position.x;
        _spriteLength = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void Update()
    {
        float temp = (_camera.transform.position.x * (1 - parallaxEffectMultiplier));
        float distance = (_camera.transform.position.x * parallaxEffectMultiplier);

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
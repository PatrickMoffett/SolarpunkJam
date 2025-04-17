using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class DiscreteAttributeBar : MonoBehaviour
{
    public GameObject segmentPrefab;

    public int segmentCount = 10;

    public float spacing = 5f;

    private RectTransform _container;
    private List<GameObject> _segments = new List<GameObject>();

    void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        _container = GetComponent<RectTransform>();
        if (segmentPrefab == null)
        {
            Debug.LogError($"[{nameof(DiscreteAttributeBar)}] segmentPrefab is not assigned.", this);
            return;
        }
        
        BuildSegments();
    }

    private void BuildSegments()
    {
        RectTransform segmentRect= segmentPrefab.GetComponent<RectTransform>();
        float segmentWidth = segmentRect.rect.width;
        segmentWidth *= segmentPrefab.transform.lossyScale.x;
        for (int i = 0; i < segmentCount; i++)
        {
            var seg = Instantiate(segmentPrefab, _container);
            seg.name = $"AttributeSegment_{i}";

            var rt = seg.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(i * (segmentWidth + spacing), 0f);

            _segments.Add(seg);
        }
    }

    public void SetCurrentValue(int value)
    {
        for (int i = 0; i < _segments.Count; i++)
            _segments[i].SetActive(i < value);
    }

    private void OnDrawGizmos()
    {
        if (segmentPrefab != null)
        {
            float segmentWidth = segmentPrefab.GetComponent<RectTransform>().rect.width;
            float segmentHeight = segmentPrefab.GetComponent<RectTransform>().rect.height;
            RectTransform container = GetComponent<RectTransform>();
            Vector3 pos = container.transform.position;
            Vector3 scale = container.transform.lossyScale;
            Gizmos.color = Color.yellow;
            for(int i = 0; i < segmentCount; ++i)
            {
                Gizmos.DrawWireCube(
                    new Vector3(+ i * (segmentWidth * scale.x + spacing), 0, 0) + pos,
                    new Vector3(segmentWidth * scale.x, segmentHeight * scale.y, 0));
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Services;
using System;
using UnityEngine.Assertions;

[RequireComponent(typeof(RectTransform))]
public class DiscreteAttributeBar : MonoBehaviour
{
    [Header("Attribute")]
    [SerializeField] protected AttributeType attributeType;
    [SerializeField] protected GameObject segmentPrefab;

    [Header("Spacing")]
    [SerializeField] protected float spaceBetweenSegments = 1f;
    [SerializeField] protected float segmentsLeftPad = 5f;
    [SerializeField] protected float segmentsRightPad = 5f;
    [SerializeField] protected float segmentsVerticalAdjustment = 5f;

    [Header("GameObjects")]
    [SerializeField] protected GameObject segmentContainer;
    [SerializeField] protected GameObject barStart;
    [SerializeField] protected GameObject barMiddle;
    [SerializeField] protected GameObject barEnd;

    [Header("Preview")]
    [SerializeField] protected int previewMaxValue = 10;

    private RectTransform _segmentContainerRect;
    private RectTransform _segmentRect;
    private Attribute _attribute;
    private Attribute _maxAttribute;
    private List<GameObject> _segments = new List<GameObject>();

    void Awake()
    {
        Initialize();
    }

    private void OnValidate()
    {
        Initialize();
        UpdateBarUI();
    }

    private void OnEnable()
    {
        ServiceLocator.Instance.Get<PlayerManager>().OnPlayerCharacterChanged += OnPlayerCharacterChanged;
    }
    private void OnDisable()
    {
        ServiceLocator.Instance.Get<PlayerManager>().OnPlayerCharacterChanged -= OnPlayerCharacterChanged;
    }
    private void Start()
    {
        PlayerCharacter playerCharacter = ServiceLocator.Instance.Get<PlayerManager>().GetPlayerCharacter();
        if (playerCharacter != null)
        {
            OnPlayerCharacterChanged(playerCharacter);
        }
    }
    private void OnPlayerCharacterChanged(PlayerCharacter character)
    {
        if (_attribute != null)
        {
            _attribute.OnValueChanged -= OnAttributeValueChanged;
            _maxAttribute.OnValueChanged -= OnMaxAttributeValueChanged;
        }

        if (character == null)
        {
            return;
        }

        _attribute = character?.GetAttributeSet().GetAttribute(attributeType);
        Assert.IsNotNull(_attribute, $"[{nameof(DiscreteAttributeBar)}] Attribute not found: {attributeType}");
        _attribute.OnValueChanged += OnAttributeValueChanged;

        
        _maxAttribute = _attribute.GetMaxAttribute();
        Assert.IsNotNull(_maxAttribute, $"[{nameof(DiscreteAttributeBar)}] Max attribute not found: {attributeType}");
        _maxAttribute.OnValueChanged += OnMaxAttributeValueChanged;

        FullyUpdateAttributeBar();
    }

    private void FullyUpdateAttributeBar()
    {
        UpdateBarUI();
        DestroyAllSegments();
        BuildSegments();
        UpdateCurrentValue();
    }

    private void UpdateBarUI()
    {
        // Get Bar Rects
        RectTransform barStartRect = barStart.GetComponent<RectTransform>();
        Assert.IsNotNull(barStartRect, $"barStart foes not have a [{nameof(RectTransform)}]");
        RectTransform barMidRect = barMiddle.GetComponent<RectTransform>();
        Assert.IsNotNull(barMidRect, $"barMiddle foes not have a [{nameof(RectTransform)}]");
        RectTransform barEndRect = barEnd.GetComponent<RectTransform>();
        Assert.IsNotNull(barEndRect, $"barEnd foes not have a [{nameof(RectTransform)}]");

        // Update SegmentContainer position
        _segmentContainerRect.anchorMin = new Vector2(0f, .5f);
        _segmentContainerRect.anchorMax = new Vector2(0f, .5f);
        _segmentContainerRect.pivot = new Vector2(0f, .5f);
        _segmentContainerRect.anchoredPosition = new Vector2(segmentsLeftPad, segmentsVerticalAdjustment);

        // Update Bar End
        barEndRect.anchorMin = new Vector2(0f, .5f);
        barEndRect.anchorMax = new Vector2(0f, .5f);
        barEndRect.pivot = new Vector2(0f, .5f);
        float barEndXPosition = GetTotalSegmentXSize();
        barEndXPosition += segmentsLeftPad + segmentsRightPad;
        barEndRect.anchoredPosition = new Vector2(barEndXPosition, 0f);

        // Update Bar Mid
        barMidRect.anchorMin = new Vector2(0f, .5f);
        barMidRect.anchorMax = new Vector2(0f, .5f);
        barMidRect.pivot = new Vector2(0f, .5f);
        barMidRect.anchoredPosition = new Vector2(barStartRect.rect.width, 0f);
        float barMidSize =  barEndRect.anchoredPosition.x - barStartRect.rect.width;
        barMidRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, barMidSize);
        barMiddle.SetActive(barMidSize > 0);
    }

    private void OnMaxAttributeValueChanged(Attribute attribute, float arg2)
    {
        FullyUpdateAttributeBar();
    }

    private void OnAttributeValueChanged(Attribute attribute, float arg2)
    {
        UpdateCurrentValue();
    }

    private void Initialize()
    {
        Assert.IsNotNull(attributeType, $"[{nameof(DiscreteAttributeBar)}] attributeType is not assigned.");
        Assert.IsNotNull(segmentContainer, $"[{nameof(DiscreteAttributeBar)}] segmentContainer is not assigned.");
        Assert.IsNotNull(barStart, $"[{nameof(DiscreteAttributeBar)}] barStart is not assigned.");
        Assert.IsNotNull(barMiddle, $"[{nameof(DiscreteAttributeBar)}] barMiddle is not assigned.");
        Assert.IsNotNull(barEnd, $"[{nameof(DiscreteAttributeBar)}] barEnd is not assigned.");
        Assert.IsNotNull(segmentPrefab, $"[{nameof(DiscreteAttributeBar)}] segmentPrefab is not assigned.");
        
        _segmentRect = segmentPrefab.GetComponent<RectTransform>();
        Assert.IsNotNull(_segmentRect, $"[{nameof(DiscreteAttributeBar)}] segmentPrefab RectTransform is not assigned.");

        _segmentContainerRect = segmentContainer.GetComponent<RectTransform>();
        Assert.IsNotNull(_segmentContainerRect, $"[{nameof(DiscreteAttributeBar)}] RectTransform is not assigned.");
    }

    private void DestroyAllSegments()
    {
        // Destroy all existing segments
        foreach (var segment in _segments)
        {
            Destroy(segment);
        }
        _segments.Clear();
    }

    private float GetTotalSegmentXSize()
    {
        float segmentWidth = _segmentRect.rect.width;
        float maxSegments;

        if(Application.isPlaying)
        {
            maxSegments = _maxAttribute.CurrentValue;
        }
        else
        {
            maxSegments = previewMaxValue;
        }
        return segmentWidth * maxSegments + spaceBetweenSegments * (maxSegments - 1);
    }
    private void BuildSegments()
    {
        float segmentWidth = _segmentRect.rect.width;
        int maxValue = (int)_maxAttribute.CurrentValue;

        for (int i = 0; i < maxValue; i++)
        {
            var seg = Instantiate(segmentPrefab, _segmentContainerRect);
            seg.name = $"AttributeSegment_{i}";

            var rt = seg.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(i * (segmentWidth + spaceBetweenSegments), 0f);

            _segments.Add(seg);
        }
    }

    public void UpdateCurrentValue()
    {
        int currentValue = (int)_attribute.CurrentValue;
        for (int i = 0; i < _segments.Count; i++)
            _segments[i].SetActive(i < currentValue);
    }

    private void OnDrawGizmos()
    {
        if (segmentPrefab != null)
        {
            RectTransform container = segmentContainer.GetComponent<RectTransform>();
            Vector3 pos = container.transform.position;
            Vector3 scale = container.transform.lossyScale;
            float segmentWidth = segmentPrefab.GetComponent<RectTransform>().rect.width * scale.x;
            float segmentHeight = segmentPrefab.GetComponent<RectTransform>().rect.height * scale.y;
            float scaledSpaceBetweenSegments = spaceBetweenSegments * scale.x;
            pos.x += segmentWidth / 2;
            Gizmos.color = Color.yellow;
            
            for(int i = 0; i < previewMaxValue; ++i)
            {
                Gizmos.DrawWireCube(
                    new Vector3(i * (segmentWidth + scaledSpaceBetweenSegments) , 0, 0) + pos,
                    new Vector3(segmentWidth , segmentHeight, 0));
            }
        }
    }
}
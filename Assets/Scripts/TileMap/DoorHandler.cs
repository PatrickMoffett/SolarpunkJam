using Services;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class DoorHandler : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private bool _closed;
    [SerializeField] private bool _hasChanged;
    [SerializeField] private GameObject _doorPair1;
    [SerializeField] private GameObject _doorPair2;
    [SerializeField] private GameObject _doorPair3;
    [SerializeField] private BoxCollider2D _doorCollider;
    [SerializeField] private SimpleAudioEvent _bloop1;
    [SerializeField] private SimpleAudioEvent _bloop2;
    [SerializeField] private SimpleAudioEvent _bloop3;
    #endregion
    #region Private Fields
    private float _openTime = 0.2f;
    private float _closeTime = 0.2f;
    #endregion
    #region Public Methods
    public bool OpenedDoor
    {
        get { return _closed; }
    }
    #endregion

    private void Start()
    {
        _hasChanged = false;
    }

    public void ChangeDoor()
    {
        if (!_hasChanged)
        {
            if (_closed)
            {
                StartCoroutine(Opener());
            }
            else if (!_closed)
            {
                StartCoroutine(Closer());
            }
            _hasChanged = true;
        }
    }
    public void OpenDoor()
    {
        if (_closed)
        {
            StartCoroutine(Opener());
        }
    }
    public void CloseDoor()
    {
        if (!_closed)
        {
            StartCoroutine(Closer());
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Trigger Entered");
        if (col.gameObject.CompareTag("Player"))
        {
            ChangeDoor();
        }
    }

    IEnumerator Opener()
    {
        _doorPair3.SetActive(false);
        _bloop1.Play(); 
        yield return new WaitForSeconds(_openTime);
        _doorPair2.SetActive(false);
        _bloop2.Play();
        yield return new WaitForSeconds(_openTime);
        _doorPair1.SetActive(false);
        _bloop3.Play();
        _doorCollider.enabled = false;
        _closed = false;
    }
    IEnumerator Closer()
    {
        _doorCollider.enabled = true;
        _doorPair1.SetActive(true);
        _bloop3.Play();
        yield return new WaitForSeconds(_closeTime);
        _doorPair2.SetActive(true);
        _bloop2.Play();
        yield return new WaitForSeconds(_closeTime);
        _doorPair3.SetActive(true);
        _bloop1.Play();
        _closed = true;
    }
}

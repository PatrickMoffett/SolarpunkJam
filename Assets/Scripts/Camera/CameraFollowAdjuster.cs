using Services;
using UnityEngine;
using UnityEngine.Assertions;

public class CameraFollowAdjuster : MonoBehaviour
{
    #region Inspector Variables
    [SerializeField] bool setNewDeadzone = false;
    [ShowIf("setNewDeadzone")]
    [SerializeField] Vector2 newDeadzone = new Vector2(1f, 1f);
    [SerializeField] bool setNewDeadzoneOffset = false;
    [ShowIf("setNewDeadzoneOffset")]
    [SerializeField] Vector3 newDeadzoneOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] bool setNewMaxDistance = false;
    [ShowIf("setNewMaxDistance")]
    [SerializeField] Vector2 newMaxDistance = new Vector2(5f, 5f);
    [SerializeField] bool setFollowTarget = false;
    [ShowIf("setFollowTarget")]
    [SerializeField] Transform newFollowTarget = null;
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var cameraFollow = ServiceLocator.Instance.Get<PlayerManager>().GetPlayerFollowCamera();
            Assert.IsNotNull(cameraFollow, "CameraFollow component not found.");
            if (setNewDeadzone)
            {
                cameraFollow.SetDeadZone(newDeadzone);
            }
            if (setNewDeadzoneOffset)
            {
                cameraFollow.SetDeadZoneOffset(newDeadzoneOffset);
            }
            if (setNewMaxDistance)
            {
                cameraFollow.SetMaxDistance(newMaxDistance);
            }
            if (setFollowTarget)
            {
                cameraFollow.SetFollowTarget(newFollowTarget);
            }
        }
    }
    #endregion

}
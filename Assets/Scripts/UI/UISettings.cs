using Services;
using UnityEngine;

public class UISettings : MonoBehaviour
{
    public void VolumeSettingsClicked()
    {
        ServiceLocator.Instance.Get<ApplicationStateManager>().PushState<VolumeSettingsState>();
    }

    public void ExitSettingsClicked()
    {
        ServiceLocator.Instance.Get<ApplicationStateManager>().PopState();
    }
}

using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Iztar.UserInterface
{
    public class SettingUIComponent : MonoBehaviour
    {

        [SerializeField] private string settingID;

        [Button]
        private void FetchGameObjectNameForID()
        {
            settingID = gameObject.name;
        }

        public string GetSettingID() => settingID;
    }
}

using UnityEngine;

namespace VRC_LG
{
    public class KeepAwake : MonoBehaviour
    {
        private void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}

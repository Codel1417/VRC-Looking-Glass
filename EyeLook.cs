using UnityEngine;

namespace VRC_LG
{
    public class EyeLook : MonoBehaviour
    {
        private LoadVRCAs loadVrcAs;
        private void Start()
        {
            loadVrcAs = FindObjectOfType<LoadVRCAs>();
        }

        // WIP
        private void OnPreRender()
        {
            Vector3 camPos = Camera.current.transform.position;
            if (loadVrcAs != null && loadVrcAs.enableEyeTracking && loadVrcAs.eyeL != null) loadVrcAs.eyeL.LookAt(camPos, Vector3.up);

            if (loadVrcAs != null && loadVrcAs.enableEyeTracking && loadVrcAs.eyeR != null) loadVrcAs.eyeR.LookAt(camPos, Vector3.up);
        }
    }
}

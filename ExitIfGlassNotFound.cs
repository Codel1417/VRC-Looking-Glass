using LookingGlass;
using UnityEngine;

namespace VRC_LG
{
    public class ExitIfGlassNotFound : MonoBehaviour
    {

        void Start()
        {
            Holoplay.Instance.onHoloplayReady.AddListener(OnHoloplayLoaded);
        }
        
        public void OnHoloplayLoaded(LoadResults loadResults)
        {
            if (loadResults.lkgDisplayFound)
            {
                Debug.Log("Holoplay loaded successfully");
            }
            else
            {
                Debug.Log("Holoplay failed to load");
                Application.Quit();
            }
        }
    }
}
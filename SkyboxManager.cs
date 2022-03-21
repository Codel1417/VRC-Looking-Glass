using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxManager : MonoBehaviour
{
    
    public Material[] skyboxMaterials;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetRandomSkybox()
    {
        if (skyboxMaterials.Length > 0)
        {
            int randomIndex = Random.Range(0, skyboxMaterials.Length - 1);
            RenderSettings.skybox = skyboxMaterials[randomIndex];
        }
    }
}

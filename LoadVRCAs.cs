using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LookingGlass;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRC.SDK3.Avatars.Components;
using VRCSDK2;
using InputManager = LookingGlass.InputManager;
using Random = UnityEngine.Random;

namespace VRC_LG
{
    public class LoadVRCAs : MonoBehaviour
    {
        /// <summary>
        ///     The list of components that are allowed to be added to the avatar.
        /// </summary>
        private static readonly Type[] AllowedComponents =
        {
            typeof(Transform),
            typeof(Animator),
            typeof(Rigidbody),
            typeof(Renderer),
            typeof(ParticleSystem),
            typeof(MeshRenderer),
            typeof(SkinnedMeshRenderer),
            typeof(TrailRenderer),
            typeof(MeshFilter),
            typeof(ParticleSystemRenderer),
            typeof(Cloth),
            typeof(Collider),
            typeof(Joint),
            typeof(AimConstraint),
            typeof(ParentConstraint),
            typeof(PositionConstraint),
            typeof(RotationConstraint),
            typeof(ScaleConstraint),
            typeof(DynamicBone),
            typeof(DynamicBoneCollider),
            typeof(VRC_AvatarDescriptor),
            typeof(VRCAvatarDescriptor)
        };

        [HideInInspector] public Transform eyeL;
        [HideInInspector] public Transform eyeR;
        public bool enableEyeTracking;
        [Range(1, 60)] [SerializeField] [Tooltip("The time in seconds before the loaded asset is changed")]
        private float tineBetweenAssets = 5f;

        [Range(0.1f, 5f)] [SerializeField] [Tooltip("Control how close the avatar is to the display")]
        private float zoom = 1.5f;

        [Header("Animations")] [SerializeField] [Tooltip("The animator used to animate the avatars")]
        private RuntimeAnimatorController controller;

        [SerializeField] [Tooltip("Randomly selects an animation from the list")]
        private AnimationClip[] clips;

        [Header("UI")] [SerializeField] private TextMeshProUGUI statusMessage;

        [SerializeField] [Tooltip("An overlay that displays the local time")]
        private TextMeshProUGUI time;

        [SerializeField] [Tooltip("An overlay that displays the SDK Version of the active avatar")]
        private TextMeshProUGUI sdkVersion;

        [SerializeField] [Tooltip("An overlay that displays if the loaded avatar supports Eye Look")]
        private TextMeshProUGUI hasEyeLook;
        [SerializeField] [Tooltip("An overlay that covers the screen when the avatar is loading")]
        public SkyboxManager skyboxManager;
        public ScreenDimManager screenDimManager;    
        public ReflectionProbe reflectionProbe;
        private readonly List<Type> allowedComponentsList = AllowedComponents.ToList();
        private string basePath = "\\AppData\\LocalLow\\VRChat\\VRChat\\Cache-WindowsPlayer\\";

        private AssetBundle currentAssetBundle;

        private readonly List<string> files = new List<string>();
        private Holoplay holoplay;
        private int index;

        [Tooltip("Set to true once the cache is indexed")]
        private bool init;

        private float lastTime;
        private string loadedScene;
        [Header("State")]
        [SerializeField] [Tooltip("Pause automatically changing the loaded avatar")]
        private bool pause;
        private VRC_AvatarDescriptor sdk2AvatarDescriptor;
        private VRCAvatarDescriptor sdk3AvatarDescriptor;
        private GameObject spawnedObject;
        private Vector3 viewPosition;
        
        private void Start()
        {
            string userfolder = Environment.GetEnvironmentVariable("USERPROFILE");
            basePath = userfolder + basePath;
            
            StartCoroutine(GetAllAssets());
            // Set game volume
            AudioListener.volume = 0f;
            holoplay = FindObjectOfType<Holoplay>();
            InputManager.EmulationMode = InputEmulationMode.Never;

            statusMessage.text = "";
        }

        // Update is called once per frame
        private void Update()
        {
            if (InputManager.GetButtonDown(HardwareButton.PlayPause))
            {
                Debug.Log("Pause button pressed");
                pause = !pause;
                if (pause)
                {
                    if (statusMessage != null) statusMessage.text = "Paused";
                }
                else
                {
                    if (statusMessage != null) statusMessage.text = "";
                }
            }

            if (InputManager.GetButtonDown(HardwareButton.Forward))
            {
                Debug.Log("Left button pressed");
                lastTime = 0f;
                index--;
                index--;
                if (index < 0) index = files.Count - 1;
                StartCoroutine(Load());
            }

            if (InputManager.GetButtonDown(HardwareButton.Back))
            {
                Debug.Log("Right button pressed");
                lastTime = 0f;
                StartCoroutine(Load());
            }

            if (init && currentAssetBundle == null) StartCoroutine(Load());

            if (init && !pause) lastTime += Time.deltaTime;

            if (lastTime > tineBetweenAssets && init)
            {
                lastTime = 0f;
                StartCoroutine(Load());
            }

            //current time
            if (time != null) time.text = "Time: " + DateTime.Now.ToString("hh:mm", CultureInfo.InvariantCulture);

            UpdateZoom();
        }

        private IEnumerator GetAllAssets()
        {
            foreach (string file in Directory.GetFiles(basePath, "__data", SearchOption.AllDirectories))
            {
                files.Add(file);
                yield return null;
            }

            init = true;
        }

        private IEnumerator Load()
        {
            bool unload = true; // force entry into while loop
            sdk2AvatarDescriptor = null;
            sdk3AvatarDescriptor = null;
            screenDimManager.FadeTo(new Color(0,0,0,1));
            yield return new WaitUntil(() => !screenDimManager.isFading);
            while (currentAssetBundle == null || currentAssetBundle.isStreamedSceneAssetBundle &&
                   currentAssetBundle.GetAllAssetNames().Length == 0 || unload)
            {

                
                
                // Unload the current asset bundle
                unload = false;
                if (spawnedObject != null)
                {
                    Debug.Log("Destroying " + spawnedObject.name);
                    Destroy(spawnedObject);
                    spawnedObject = null;
                }

                if (loadedScene != null)
                {
                    Debug.Log("Unloading " + loadedScene);
                    SceneManager.UnloadSceneAsync(loadedScene);
                    loadedScene = null;
                }

                if (currentAssetBundle != null)
                {
                    currentAssetBundle.Unload(true);
                    currentAssetBundle = null;
                }

                // Load the next asset bundle
                currentAssetBundle = AssetBundle.LoadFromFile(files[index]);
                index++;
                // loop back to the beginning if we've reached the end
                if (index >= files.Count) index = 0;
                yield return null;
            }

            string[] assetPaths = currentAssetBundle.GetAllAssetNames();
            string[] scenePaths = currentAssetBundle.GetAllScenePaths();

            // Attempt to load the asset bundle as a scene. WIP
            if (scenePaths != null && scenePaths.Length > 0)
            {
                Debug.Log("Loading scene " + scenePaths[0]);
                SceneManager.LoadScene(scenePaths[0], LoadSceneMode.Additive);
                loadedScene = scenePaths[0];
            }
            else if (assetPaths != null && assetPaths.Length > 0) // Attempt to load the asset bundle as an avatar.
            {
                Debug.Log("Loading asset " + assetPaths[0]);
                GameObject prefab = currentAssetBundle.LoadAsset<GameObject>(assetPaths[0]);
                if (spawnedObject == null)
                {
                    spawnedObject = Instantiate(prefab);
                }
                // Strip components that are not whitelisted
                IEnumerable<GameObject> prefabObjects = GetAllChildren(spawnedObject);
                foreach (GameObject prefabObject in prefabObjects) StripComponents(prefabObject);
                // Try to Set up the avatar
                spawnedObject.transform.position = Vector3.zero; // center avatar
                sdk3AvatarDescriptor = spawnedObject.GetComponent<VRCAvatarDescriptor>();
                sdk2AvatarDescriptor = spawnedObject.GetComponent<VRC_AvatarDescriptor>();
                if (sdk3AvatarDescriptor != null)
                {
                    viewPosition = sdk3AvatarDescriptor.ViewPosition;
                    if (sdkVersion != null) sdkVersion.text = "SDK: 3";
                }
                else if (sdk2AvatarDescriptor != null)
                {
                    viewPosition = sdk2AvatarDescriptor.ViewPosition;
                    if (sdkVersion != null) sdkVersion.text = "SDK: 2";
                }
                else
                {
                    viewPosition = Vector3.zero;
                    if (sdkVersion != null) sdkVersion.text = "SDK: Unknown";
                }
                SetUpAnimation();
                screenDimManager.FadeTo(Color.clear);
                skyboxManager.SetRandomSkybox();
                // Set up the camera
                if (holoplay != null)
                {
                    Transform transform1 = holoplay.transform;
                    Vector3 position = transform1.position;
                    position.y = viewPosition.y * 0.9f;
                    transform1.position = position;
                    holoplay.SetupQuilt();
                }

                SetUpEyeTracking();
                reflectionProbe.RenderProbe();
                DynamicGI.UpdateEnvironment();
            }
        }

        private void UpdateZoom()
        {
            if (holoplay != null && zoom > 0.01f && viewPosition.y > 0.01f) holoplay.size = viewPosition.y * 1 / zoom;
        }

        private void SetUpAnimation()
        {
            Animator avatarAnimator = spawnedObject.GetComponent<Animator>();
            if (avatarAnimator != null && controller != null && clips != null && clips.Length > 0)
            {
                Debug.Log("Setting up animation");
                AnimationClip temp = clips[Random.Range(0, clips.Length)];
                if (temp != null)
                {
                    Debug.Log("Playing animation " + temp.name);
                    AnimatorOverrideController aoc = new AnimatorOverrideController(controller);
                    // map the animtion clip to the animator controller
                    List<KeyValuePair<AnimationClip, AnimationClip>> anims = aoc.animationClips
                        .Select(a => new KeyValuePair<AnimationClip, AnimationClip>(a, temp)).ToList();
                    aoc.ApplyOverrides(anims);
                    avatarAnimator.runtimeAnimatorController = aoc;
                }
            }
        }
        private void SetUpEyeTracking()
        {
            eyeL = null;
            eyeR = null;
            if (hasEyeLook != null) hasEyeLook.text = "Eye Look: No";

            if (sdk2AvatarDescriptor != null)
            {
                GameObject l = GameObject.Find("LeftEye");
                if (l != null) eyeL = l.transform;
                GameObject r = GameObject.Find("RightEye");
                if (r != null) eyeR = r.transform;
                if (eyeL != null && eyeR != null && hasEyeLook != null) hasEyeLook.text = "Eye Look: Yes";
            }
            else if (sdk3AvatarDescriptor != null)
            {
                eyeL = sdk3AvatarDescriptor.customEyeLookSettings.leftEye;
                eyeR = sdk3AvatarDescriptor.customEyeLookSettings.rightEye;
                if (eyeL != null && eyeR != null && hasEyeLook != null) hasEyeLook.text = "Eye Look: Yes";
            }
        }

        /// <summary>
        ///     Loops through the gameobjects and removes invalid components.
        /// </summary>
        /// <param name="targetObject"></param>
        private void StripComponents(GameObject targetObject)
        {
            foreach (Component component in targetObject.GetComponents<Component>())
                if (component != null && !allowedComponentsList.Contains(component.GetType()))
                {
                    Debug.Log("Removing " + component.GetType().Name);
                    DestroyImmediate(component, true);
                }
        }

        /// <summary>
        ///     Recursively gets all the gameobjects in the avatar.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private IEnumerable<GameObject> GetAllChildren(GameObject parent)
        {
            List<GameObject> children = new List<GameObject> { parent };
            foreach (Transform child in gameObject.transform) children.AddRange(GetAllChildren(child.gameObject));

            return children.ToArray();
        }
    }
}
using System;
using UnityEngine;
using VRCSDK2;

// Basic wrapper to get view position from sdk3
namespace VRC.SDK3.Avatars.Components
{
  public class VRCAvatarDescriptor : VRC_AvatarDescriptor
  {

    public CustomEyeLookSettings customEyeLookSettings;

    [Serializable]
    public struct CustomEyeLookSettings
    {
      public Transform leftEye;
      public Transform rightEye;


    }
  }
}

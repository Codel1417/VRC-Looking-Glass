# Looking Glass VRC

Displays [VRChat](https://hello.vrchat.com/) avatars from your download cache on a [Looking Glass](https://lookingglassfactory.com/) display.

![image](https://user-images.githubusercontent.com/13484789/150659031-29eb7ba0-29c7-4a94-983a-ba6bc668ae20.png)

## Features

* Supports SDK2 and SDK3 avatars.
* Component Stripping for security.
* Randomly applies animations.
* Supports hardware buttons

## Requirements

* Unity 2019.4
* [HoloPlay Unity Plugin](https://dhtk4bwj5r21z.cloudfront.net/UnityPlugin/PublicReleases/HoloPlay-Unity-Plugin-1.4.3.unitypackage) For interfacing with the display.
* [Dynamic Bones](https://assetstore.unity.com/packages/tools/animation/dynamic-bone-16743) Wiggle.

### How to use

1. Download the required files first.
2. import the files from this repo.
3. open the included scene.
4. (Optional) Assign your own humanoid animations to the `Clips` array under `ObjectController`.
5. Build and Run.

## Credits

* [InputManager.cs](https://gist.github.com/jbienz/917ebc352185a0e8a164f6d17140ed27)
* [ExtendedInput.cs](https://gist.github.com/jbienz/6e508151e040a95cbc5730b962a93e35)

# What is VivifyTemplate?

VivifyTemplate is a tool designed for the Unity side development of [Vivify](https://github.com/Aeroluna/Vivify) maps for Beat Saber. It composes of 3 "modules": **Exporter**, **Examples**, and **Utilities**.

- [**Exporter**](#exporter-module): Builds asset bundles to your map project.
- [**Utilities**](#utilities-module): Includes various tools for project development (shader templates, shader functions... etc.)
- [**Examples**](#examples-module): Imports and demonstrates the [**Utilities module**](#utilities-module). Contains practical examples for things you may need to do in your map (post-processing, custom notes/sabers... etc.)

# Setup

1. Create and open a Unity project for version **2019.4.28f1**. The download will be somewhere on [this page](https://unity.com/releases/editor/archive).
2. Download whatever VivifyTemplate modules you want from the [latest release](https://github.com/Swifter1243/VivifyTemplate/releases).
3. Install them by double-clicking them. Follow the import instructions in your editor.

### Updating Modules

If you are trying to update any modules in your project, please delete the old ones before installation to prevent namespace conflicts with potentially remaining files.

# Exporter Module

### IMPORTANT: In your project, you should see a "Vivify" tab. Setup your project with `Vivify > Setup Project`.

The exporter handles exporting bundles for various versions of Unity and Beat Saber.
- **Windows 2019**: PC Beat Saber 1.29.1, uses `Single Pass`.
- **Windows 2021**: PC Beat Saber 1.34.2+, uses `Single Pass Instanced`.
- **Android 2021**: Quest Beat Saber. Uses `Single Pass Instanced`

It also exports a `bundleinfo.json` file which contains the correct bundle checksums, among other information.

<details>
<summary>Sample data</summary>

```json
{
  "materials": {
    "example": {
      "path": "assets/materials/example.mat",
      "properties": {
        "_Example": {
          "type": { "Float": null },
          "value": 1.0
        }
      }
    }
  },
  "prefabs": {
    "example": "assets/prefabs/example.prefab"
  },
  "bundleFiles": [
    "C:/Example/bundleWindows2019.vivify",
    "C:/Example/bundleWindows2021.vivify",
    "C:/Example/bundleAndroid2021.vivify"
  ],
  "bundleCRCs": {
    "_windows2019": 2604998796,
    "_windows2021": 2051513366,
    "_android2021": 3982829844
  },
  "isCompressed": true
}
```

</details>

---

## Before using the exporter, **make sure the assets you want are in your bundle**!

When an asset is selected in the Project View (not the scene's hierarchy!), there's a dropdown field in the bottom of the Inspector which will allow you to attach the asset to an asset bundle.

![image](https://github.com/user-attachments/assets/6f1b945f-d38f-4f8b-ba42-d546adf12dcb)

To use the exporter, open the build configuration window `Vivify > Build > Build Configuration Window`.
- **Uncompressed**: Advised for quick iteration. Do not distribute.
- **Compressed**: Takes much longer but is necessary for final upload. 

When you first run the exporter, you will be asked for an output directory. This is where your `bundleinfo.json` and asset bundles will end up. The path you set will be remembered for subsequent builds.

To understand how to implement asset bundles into a map, please read the [Vivify documentation](https://github.com/Aeroluna/Vivify?tab=readme-ov-file#creating-an-asset-bundle).

# Examples Module

If you installed the "Examples" package, navigate to `Assets/VivifyTemplate/Examples/Scenes`. Here you'll find a bunch of scenes that explore various concepts.

- **Custom Objects**: How to make custom notes, bombs, chains, and sabers.
- **Depth**: How to read and use the depth texture.
- **Grab Pass**: How to use grab passes to create distortion effects.
- **Light**: How to sample from Unity's lighting system in shaders.
- **Noise**: How to use various noise functions provided in the Utilities module (which the examples depend on).
- **Opacity**: How to use blend modes to create transparency.
- **Post Processing**: How to make post-processing shaders for VR.
- **Skybox**: How to create a skybox for your scene.
- **Spaces**: Understanding various "spaces" (object, world, view)
- **Vectors**: How to obtain useful vector information. (world normals, view vector, camera forward)
- **Vertex**: How to manipulate vertices in a vertex shader.

When looking at example objects, their names in the hierarchy will tell you what they are doing. Be sure to explore their shaders (`Assets/VivifyTemplate/Examples/Shaders`), as they include in-code comments providing explanations.

# Utilities Module

If you installed the "Utilities" package, a few tools will be provided from the `Assets/VivifyTemplate/Utilities` folder.
- **Shader Templates**: In the project view, right click. Go to `Create -> Shader -> Vivify` and you'll see a bunch of examples you can use to create Beat Saber ready shaders!
- **Shader Functions**: In the `Assets/VivifyTemplate/Utilities/Shader Functions` folder, you'll see a bunch of shader functions you can use for your own shaders. Import them with `#include <path here>`.
- **Custom Object Bases**: In the `Assets/VivifyTemplate/Utilities/Prefabs/Custom Objects` folder, you'll find a bunch of prefabs you can use as a base for custom notes/sabers. Read the provided `HOWTOUSE.txt` file.
- **Post Processing Preview**: Drag the `Assets/VivifyTemplate/Utilities/Scripts/SimplePostProcessing.cs` file onto any Camera and provide it with a post-processing shader to view it in the Game window.

# Extra

## TextMeshPro

The version of TextMeshPro that is installed with Unity version **2019.4.28f1** is newer than what Beat Saber uses in 1.29.1, and therefore causes some alignment issues. In order to downgrade:
- Go to `Window > Package Manager`
- Find TextMeshPro and remove it.
- Click the `+` and click "Add package from git URL".
- Enter `com.unity.textmeshpro@1.4.1`

TextMeshPro should be on the same version that Beat Saber uses now.

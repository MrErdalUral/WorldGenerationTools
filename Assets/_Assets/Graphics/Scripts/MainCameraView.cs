using ModestTree;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[ExecuteAlways]
[ImageEffectAllowedInSceneView]
public class MainCameraView : MonoBehaviour
{
    void OnEnable()
    {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
    }
}
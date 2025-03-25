using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlanarReflection : MonoBehaviour
{
    [SerializeField] private Transform _reflectionPlane;
    [SerializeField] private Material _material;

    private RenderTexture _reflectionRenderTarget;
    private Camera _reflectionCamera;
    private Camera _mainCamera;

    private Vector3 PlaneNormalLocal = Vector3.up;

    void Awake()
    {
        if (_reflectionPlane == null)
            _reflectionPlane = transform;

        _mainCamera = Camera.main;

        // Create the reflection camera
        _reflectionCamera = new GameObject("Reflection Camera", typeof(Camera)).GetComponent<Camera>();
        _reflectionCamera.enabled = false;

        // Create the reflection texture
        _reflectionRenderTarget = new RenderTexture(Screen.width, Screen.height, 0);
    }

    void OnPreRender()
    {
        RenderReflection();
    }

    void RenderReflection()
    {
        if (!_mainCamera || !_reflectionPlane) return;

        // Copy basic settings from the main camera
        _reflectionCamera.CopyFrom(_mainCamera);
        _reflectionCamera.targetTexture = _reflectionRenderTarget;

        //Mirror the camera transform around the reflection plane
        Vector3 cameraPos = _mainCamera.transform.position;
        Vector3 cameraDir = _mainCamera.transform.forward;
        Vector3 cameraUp = _mainCamera.transform.up;

        // Convert to plane's local space
        Vector3 localCameraPos = _reflectionPlane.InverseTransformPoint(cameraPos);
        Vector3 localCameraDir = _reflectionPlane.InverseTransformDirection(cameraDir);
        Vector3 localCameraUp = _reflectionPlane.InverseTransformDirection(cameraUp);

        // Flip Y in local space
        localCameraPos.y *= -1;
        localCameraDir.y *= -1;
        localCameraUp.y *= -1;

        // Convert back to world space
        cameraPos = _reflectionPlane.TransformPoint(localCameraPos);
        cameraDir = _reflectionPlane.TransformDirection(localCameraDir);
        cameraUp = _reflectionPlane.TransformDirection(localCameraUp);

        // Update reflection camera transform
        _reflectionCamera.transform.position = cameraPos;
        // Look in the mirrored direction, using the mirrored "up" vector
        _reflectionCamera.transform.LookAt(cameraPos + cameraDir, cameraUp);

        //Apply an oblique near-clip plane so the reflection camera doesn't render
        //    geometry behind (below) the reflection plane.
        Vector3 planeWorldPos = _reflectionPlane.position;
        // Get the plane's normal in world space
        Vector3 planeWorldNormal = _reflectionPlane.TransformDirection(PlaneNormalLocal).normalized;
        // Build the oblique plane and set it on the reflection camera
        Vector4 clipPlane = CameraSpacePlane(_reflectionCamera, planeWorldPos, planeWorldNormal, 1.0f);
        _reflectionCamera.projectionMatrix = _reflectionCamera.CalculateObliqueMatrix(clipPlane);

        //Render the reflection
        _reflectionCamera.Render();
        _material.SetPass(0);
        _material.SetTexture("_ReflectionTex", _reflectionRenderTarget);
    }

    /// <summary>
    /// Computes a plane in camera space for the oblique near-clip plane.
    /// This will clip everything behind the plane from the reflection camera.
    /// </summary>
    private Vector4 CameraSpacePlane(Camera cam, Vector3 planeWorldPos, Vector3 planeWorldNormal, float sideSign)
    {
        // Transform plane position & normal into camera space
        Vector3 cpos = cam.worldToCameraMatrix.MultiplyPoint(planeWorldPos);
        Vector3 cnormal = cam.worldToCameraMatrix.MultiplyVector(planeWorldNormal).normalized;

        // Plane equation in camera space: ax + by + cz + d = 0
        float d = -Vector3.Dot(cpos, cnormal);

        // Flip the plane if needed (sideSign) so we clip the correct side
        return new Vector4(cnormal.x * sideSign,
                           cnormal.y * sideSign,
                           cnormal.z * sideSign,
                           d * sideSign);
    }
}

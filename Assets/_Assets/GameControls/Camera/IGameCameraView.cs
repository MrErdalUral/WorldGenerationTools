using UnityEngine;

public interface IGameCameraView
{
    Vector3 Position { set; }
    Vector3 Target { set; }
}
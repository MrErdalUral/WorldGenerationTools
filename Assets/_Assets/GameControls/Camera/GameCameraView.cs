using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class GameCameraView : MonoBehaviour, IGameCameraView
{
    [SerializeField] private Vector3 _target;

    private Vector3 _direction;

    public Vector3 Position
    {
        set
        {
            transform.position = value;
            transform.LookAt(_target);
        }
    }


    public Vector3 Target
    {
        set
        {
            _target = value;
            transform.LookAt(_target);
        }
    }
}
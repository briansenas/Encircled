using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class InCameraDetector : MonoBehaviour
{
    // Reference the main Camera 
    Camera _camera; 
    // Get the object we must focus on 
    private SpriteRenderer _renderer; 
    // Estimate the camera Proyection Planes 
    private Plane[] _cameraFrustum; 
    // Get the collider from the game object for the aabb
    [SerializeField]
    private CircleCollider2D _collider; 
    private Rigidbody2D _rb; 
    private PlayerMovement _playerMovement; 
    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main; 
        _renderer = GetComponent<SpriteRenderer>(); 
        _rb = GetComponent<Rigidbody2D>(); 
        _playerMovement = GetComponent<PlayerMovement>(); 
    }

    private void Die(){
        _rb.bodyType = RigidbodyType2D.Static; 
        PlayerConfigurationManager.Instance.HandleDeath(_playerMovement.playerConfig.PlayerIndex);
        Destroy(this); 
    }

    // Update is called once per frame
    void Update()
    {
        var bounds = _collider.bounds; 
        _cameraFrustum = GeometryUtility.CalculateFrustumPlanes(_camera);
        if (!GeometryUtility.TestPlanesAABB(_cameraFrustum, bounds)){
            Die(); 
        }
    }
}

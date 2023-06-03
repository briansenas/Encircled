using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGrabbable : MonoBehaviour {


  private Rigidbody2D _objectRigidbody;
  private Transform _objectGrabPointTransform;
  private Transform _objectTransform;
  private PlayerMovement _myMovement; 
  private PlayerMovement _grabbedByMovement;  

  [SerializeField] private float lerpSpeed = 20f;

  private void Awake() {
    _objectRigidbody = GetComponent<Rigidbody2D>();
    _myMovement = GetComponent<PlayerMovement>(); 
  }

  public void Grab(Transform objectGrabPointTransform, Transform objectTransform,
      PlayerMovement grabbedBy) {
    if (grabbedBy == _myMovement) return; 
    if (_myMovement.IsGrabbed) return; 

    _grabbedByMovement = grabbedBy; 
    _myMovement.setGrabbed(_grabbedByMovement); 
    _objectGrabPointTransform = objectGrabPointTransform;
    _objectRigidbody.gravityScale = 0;
    int _layer = LayerMask.NameToLayer("NoPlayerCollision"); 
    _objectTransform = objectTransform; 
    _objectTransform.gameObject.layer = _layer;
    foreach (Transform child in _objectTransform)
    {
      child.gameObject.layer = _layer; 
    }
  }

  public void Drop() {
    _objectGrabPointTransform = null;
    _grabbedByMovement = null; 

    _myMovement.setDropped();
    int _layer = LayerMask.NameToLayer("Player"); 
    _objectTransform.gameObject.layer = _layer;
    foreach (Transform child in _objectTransform)
    {
      child.gameObject.layer = _layer; 
    }
    _objectRigidbody.gravityScale = 1;
  }

  public void isPushed(Vector2 dir, Vector2 strength) {
    _myMovement.isPushed(dir, strength);  
  } 

  private void FixedUpdate() {
    if (_objectGrabPointTransform != null) {
      Vector3 newPosition = Vector3.Lerp(transform.position, _objectGrabPointTransform.position, Time.deltaTime * lerpSpeed);
      _objectRigidbody.MovePosition(newPosition);
    }
  }


}

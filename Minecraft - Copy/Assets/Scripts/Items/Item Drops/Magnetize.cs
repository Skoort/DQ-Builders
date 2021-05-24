using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnetize : MonoBehaviour
{
	[SerializeField] private Rigidbody _rb = null;
	[SerializeField] private string _attractedToTag = null;

	[SerializeField] private float _maxAttractionDistance = 0.5F;

	[SerializeField] private float _maxSpeed = 12;
	[SerializeField] private float _baseAcc = 10;

	private Transform _attractedTo;

	private Vector3 _acc;

    private void Awake()
    {
		_attractedTo = GameObject.FindGameObjectWithTag(_attractedToTag).transform;
		if (_rb == null)
		{
			_rb = GetComponent<Rigidbody>();
		}
    }

    private void Update()
    {
		var dt = Time.deltaTime;
		var dist = (_attractedTo.position - transform.position).magnitude;
		if (dist < _maxAttractionDistance)
		{
			var distRatio = dist / _maxAttractionDistance;
			var dir = (_attractedTo.position - transform.position).normalized;
			_acc += dir * (dt * _baseAcc);
			_rb.velocity += _acc * dt;
			if (_rb.velocity.magnitude > _maxSpeed)
			{
				_rb.velocity = _rb.velocity.normalized * _maxSpeed;
			}
		}
		else
		{
			_acc = Vector3.zero;
		}
    }
}

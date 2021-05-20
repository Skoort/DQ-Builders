using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropAnimController : MonoBehaviour
{
	[SerializeField] private Transform _graphicsRoot;

	[SerializeField] private float _rotationSpeed;
	[SerializeField] private float _bobDelta;
	[SerializeField] private float _bobSpeed;

	private Vector3 _bobPointA;
	private Vector3 _bobPointB;
	private bool _bobSwitch;

	private void Awake()
	{
	}

	private void Update()
	{
		_bobPointA = new Vector3(0, +_bobDelta);
		_bobPointB = new Vector3(0, -_bobDelta);
		var dt = Time.deltaTime;

		_graphicsRoot.transform.Rotate(0, _rotationSpeed * dt, 0);

		var targetBobPoint = _bobSwitch ? _bobPointA : _bobPointB;
		_graphicsRoot.localPosition = Vector3.Lerp(_graphicsRoot.localPosition, targetBobPoint, _bobSpeed * dt);
		if ((targetBobPoint - _graphicsRoot.localPosition).sqrMagnitude <= 0.001F)
		{
			_bobSwitch = !_bobSwitch;
		}
	}
}

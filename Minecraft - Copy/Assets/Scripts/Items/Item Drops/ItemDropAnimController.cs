using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropAnimController : MonoBehaviour
{
	[SerializeField] private Transform _graphicsRoot;

	[SerializeField] private float _rotationSpeed = 20;
	[SerializeField] private float _bobDelta = 0.05F;
	[SerializeField] private float _bobSpeed = 1/3F;

	[SerializeField] private Vector3 _scaleA = Vector3.one;
	[SerializeField] private Vector3 _scaleB = new Vector3(0.15F, 0.15F, 0.15F);
	[SerializeField] private float _scaleSpeed = 18F;

	private Vector3 _bobPointA;
	private Vector3 _bobPointB;
	private bool _bobSwitch;

	private void Awake()
	{
		_graphicsRoot.localScale = _scaleA;
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

		_graphicsRoot.localScale = Vector3.Lerp(_graphicsRoot.localScale, _scaleB, dt * _scaleSpeed);
	}
}

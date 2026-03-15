using UnityEngine;
using System;

public class Fruit : MonoBehaviour, IPoolable
{
	[SerializeField] private SpriteRenderer _spriteRenderer;

	private int _level;
	private long _basePriceCoin;

	public int Level => _level;
	public long BasePriceCoin => _basePriceCoin;

	private void OnValidate()
	{
		if (_spriteRenderer == null)
		{
			_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		}
	}

	public void Configure(int level, Sprite sprite, long basePriceCoin)
	{
		_level = Mathf.Max(1, level);
		_basePriceCoin = Math.Max(0L, basePriceCoin);

		if (_spriteRenderer != null)
		{
			_spriteRenderer.sprite = sprite;
		}
	}

	public void OnSpawned()
	{
		transform.localScale = Vector3.one;
	}

	public void OnDespawned()
	{
		transform.localScale = Vector3.one;
	}
}

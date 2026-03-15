using UnityEngine;

public class Fruit : MonoBehaviour, IPoolable
{
	public void OnSpawned()
	{
		transform.localScale = Vector3.one;
	}

	public void OnDespawned()
	{
		transform.localScale = Vector3.one;
	}
}

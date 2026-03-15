using UnityEngine;

[CreateAssetMenu(fileName = "FruitConfiguration", menuName = "Little Farm/Gameplay/Fruit Configuration")]
public class FruitConfigurationSO : ScriptableObject
{
    [SerializeField] private Fruit _fruitPrefab;
    [SerializeField] private string _fruitPoolId = "fruits";
    [SerializeField] private PoolSettings _fruitPoolSettings = new();
    [SerializeField] private float _developmentDuration = 5f;
    [SerializeField] private float _restDuration = 2f;

    public Fruit FruitPrefab => _fruitPrefab;
    public string FruitPoolId => _fruitPoolId;
    public PoolSettings FruitPoolSettings => _fruitPoolSettings;
    public float DevelopmentDuration => _developmentDuration;
    public float RestDuration => _restDuration;
}

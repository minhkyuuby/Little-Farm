using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "HarvestPlant", story: "[Agent] havests the [Plant]", category: "Action", id: "5337af9f1163c199c09d2b6d46c30fff")]
public partial class HarvestPlantAction : Action
{
    [SerializeReference] public BlackboardVariable<DeliveryGuyController> Agent;
    [SerializeReference] public BlackboardVariable<Plant> Plant;

    private DeliveryGuyController _agent;
    private Plant _plant;

    protected override Status OnStart()
    {
        _agent = Agent?.Value;
        _plant = Plant?.Value;

        if (_plant == null && _agent != null)
        {
            _plant = _agent.TargetPlant;
            if (Plant != null)
            {
                Plant.Value = _plant;
            }
        }

        if (_agent == null || _plant == null)
        {
            return Status.Failure;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_agent == null || _plant == null)
        {
            return Status.Failure;
        }

        var harvested = _agent.TryHarvestFromPlant(_plant);
        return harvested ? Status.Success : Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}


using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GetTargetPlantFromAgent", story: "Read [Agent] target plant into [TargetPlant]", category: "Action", id: "9b112f5fc20a446192a99f7d9ad10fb5")]
public partial class GetTargetPlantFromAgentAction : Action
{
    [SerializeReference] public BlackboardVariable<DeliveryGuyController> Agent;
    [SerializeReference] public BlackboardVariable<Plant> TargetPlant;

    private DeliveryGuyController _agent;

    protected override Status OnStart()
    {
        _agent = Agent?.Value;
        if (_agent == null)
        {
            return Status.Failure;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_agent == null || TargetPlant == null)
        {
            return Status.Failure;
        }

        var target = _agent.TargetPlant;
        if (target == null)
        {
            return Status.Failure;
        }

        TargetPlant.Value = target;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

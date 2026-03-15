using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitForAvailableDock", story: "[Agent] waits for available dock in [Market] and sets [TargetDock]", category: "Action", id: "c5dcf4d017cc4a6f947d42f5ea6a1001")]
public partial class WaitForAvailableDockAction : Action
{
    [SerializeReference] public BlackboardVariable<DeliveryGuyController> Agent;
    [SerializeReference] public BlackboardVariable<Dock> TargetDock;

    private DeliveryGuyController _agent;
    private Market _market;

    protected override Status OnStart()
    {
        _agent = Agent?.Value;
        _market = UnityEngine.Object.FindFirstObjectByType<Market>() ?? global::Market.Instance;

        if (_agent == null || _market == null)
        {
            return Status.Failure;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        _market ??= UnityEngine.Object.FindFirstObjectByType<Market>() ?? global::Market.Instance;

        if (_agent == null || _market == null)
        {
            return Status.Failure;
        }

        if (_agent.TargetDock != null && _agent.TargetDock.IsDelivering)
        {
            if (TargetDock != null)
            {
                TargetDock.Value = _agent.TargetDock;
            }

            return Status.Success;
        }

        if (!_agent.TryReserveDeliveryDock(_market))
        {
            return Status.Running;
        }

        if (TargetDock != null)
        {
            TargetDock.Value = _agent.TargetDock;
        }

        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

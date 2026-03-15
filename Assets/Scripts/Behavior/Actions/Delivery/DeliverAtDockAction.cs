using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DeliverAtDock", story: "[Agent] delivers at [TargetDock]", category: "Action", id: "206b24d4f9ed475db0c3cf2c3cdba4fe")]
public partial class DeliverAtDockAction : Action
{
    [SerializeReference] public BlackboardVariable<DeliveryGuyController> Agent;
    [SerializeReference] public BlackboardVariable<Dock> TargetDock;

    private DeliveryGuyController _agent;
    private Dock _targetDock;

    protected override Status OnStart()
    {
        _agent = Agent?.Value;
        _targetDock = TargetDock?.Value;

        if (_agent == null)
        {
            return Status.Failure;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_agent == null)
        {
            return Status.Failure;
        }

        var dock = _targetDock != null ? _targetDock : _agent.TargetDock;
        if (dock == null)
        {
            return Status.Failure;
        }

        var delivered = _agent.TryDeliverAtDock(dock);
        if (!delivered)
        {
            return Status.Failure;
        }

        _agent.ReturnToPool();
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

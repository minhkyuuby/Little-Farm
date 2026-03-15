using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetDeliveryPositionFromDock", story: "Set [DeliveryPosition] from [TargetDock]", category: "Action", id: "4d48f3fb98714fcb8a8674f8e58c3d73")]
public partial class SetDeliveryPositionFromDockAction : Action
{
    [SerializeReference] public BlackboardVariable<DeliveryGuyController> Agent;
    [SerializeReference] public BlackboardVariable<Dock> TargetDock;
    [SerializeReference] public BlackboardVariable<Vector3> DeliveryPosition;

    private DeliveryGuyController _agent;

    protected override Status OnStart()
    {
        _agent = Agent?.Value;
        if (DeliveryPosition == null)
        {
            return Status.Failure;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (DeliveryPosition == null)
        {
            return Status.Failure;
        }

        var dock = TargetDock?.Value;
        if (dock == null && _agent != null)
        {
            dock = _agent.TargetDock;
        }

        if (dock == null)
        {
            return Status.Running;
        }

        DeliveryPosition.Value = dock.DeliveryPosition.position;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

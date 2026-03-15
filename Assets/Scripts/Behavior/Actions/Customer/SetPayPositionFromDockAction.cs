using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetPayPositionFromDock", story: "Set [PayPosition] from [TargetDock] of [Customer]", category: "Action", id: "e1dd5ba9f4514ec4aaf98bb438f8a374")]
public partial class SetPayPositionFromDockAction : Action
{
    [SerializeReference] public BlackboardVariable<CustomerController> Customer;
    [SerializeReference] public BlackboardVariable<Dock> TargetDock;
    [SerializeReference] public BlackboardVariable<Vector3> PayPosition;

    private CustomerController _customer;

    protected override Status OnStart()
    {
        _customer = Customer?.Value;
        if (PayPosition == null)
        {
            return Status.Failure;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (PayPosition == null)
        {
            return Status.Failure;
        }

        var dock = TargetDock?.Value;
        if (dock == null && _customer != null)
        {
            dock = _customer.TargetDock;
        }

        if (dock == null)
        {
            return Status.Running;
        }

        PayPosition.Value = dock.CustomerPosition.position;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

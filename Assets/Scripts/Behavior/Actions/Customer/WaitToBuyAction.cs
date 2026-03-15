using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitToBuy", story: "[Customer] waits and buys at [TargetDock]", category: "Action", id: "2b7da0c389ff4ccab5f69a71fef6a190")]
public partial class WaitToBuyAction : Action
{
    [SerializeReference] public BlackboardVariable<CustomerController> Customer;
    [SerializeReference] public BlackboardVariable<Dock> TargetDock;

    private CustomerController _customer;

    protected override Status OnStart()
    {
        _customer = Customer?.Value;
        if (_customer == null)
        {
            return Status.Failure;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_customer == null)
        {
            return Status.Failure;
        }

        var dock = TargetDock?.Value;
        if (dock == null)
        {
            dock = _customer.TargetDock;
        }

        if (dock == null)
        {
            return Status.Failure;
        }

        if (!dock.IsWaitingToBuy)
        {
            return Status.Running;
        }

        var purchased = _customer.TryBuyAtTargetDock(dock);
        return purchased ? Status.Success : Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}

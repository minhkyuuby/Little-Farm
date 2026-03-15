using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MarkCustomerArrivedAtDock", story: "Mark [Customer] arrived at [TargetDock]", category: "Action", id: "db7bfa4119784aa4aefc887f11dbe6f8")]
public partial class MarkCustomerArrivedAtDockAction : Action
{
    [SerializeReference] public BlackboardVariable<CustomerController> Customer;
    [SerializeReference] public BlackboardVariable<Dock> TargetDock;

    private CustomerController _customer;
    private Dock _targetDock;

    protected override Status OnStart()
    {
        _customer = Customer?.Value;
        _targetDock = TargetDock?.Value;

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

        var dock = _targetDock != null ? _targetDock : _customer.TargetDock;
        if (dock == null)
        {
            return Status.Failure;
        }

        var marked = dock.TryMarkCustomerArrived(_customer);
        return marked ? Status.Success : Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}

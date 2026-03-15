using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetupCustomerTargets", story: "Set [TargetDock] and [Market] from [Customer]", category: "Action", id: "4c4fc5f1fd994f16b2fe2ec39f5064f2")]
public partial class SetupCustomerTargetsAction : Action
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
        if (_customer == null || TargetDock == null)
        {
            return Status.Failure;
        }

        var targetDock = _customer.TargetDock;

        if (targetDock == null)
        {
            return Status.Running;
        }

        TargetDock.Value = targetDock;

        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

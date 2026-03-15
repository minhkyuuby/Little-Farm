using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AssignCustomerDock", story: "[Customer] takes empty dock in [Market] and sets [TargetDock]", category: "Action", id: "a0e161eb1ea64cd4a06b4f8939d9aa70")]
public partial class AssignCustomerDockAction : Action
{
    [SerializeReference] public BlackboardVariable<CustomerController> Customer;
    [SerializeReference] public BlackboardVariable<Dock> TargetDock;

    private CustomerController _customer;
    private Market _market;

    protected override Status OnStart()
    {
        _customer = Customer?.Value;
        _market = UnityEngine.Object.FindFirstObjectByType<Market>() ?? global::Market.Instance;

        if (_customer == null || _market == null)
        {
            Debug.LogError("Customer or Market is null in AssignCustomerDockAction");
            return Status.Failure;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        _market ??= UnityEngine.Object.FindFirstObjectByType<Market>() ?? global::Market.Instance;

        if (_customer == null || _market == null)
        {
            Debug.LogError("Customer or Market is null in AssignCustomerDockAction");
            return Status.Failure;
        }

        if (_customer.TargetDock != null)
        {
            if (TargetDock != null)
            {
                TargetDock.Value = _customer.TargetDock;
            }

            return Status.Success;
        }

        if (!_customer.TryAssignToDock(_market))
        {
            Debug.LogError("Failed to assign customer to dock in AssignCustomerDockAction");
            return Status.Running;
        }

        if (TargetDock != null)
        {
            TargetDock.Value = _customer.TargetDock;
        }

        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ReleaseToPool", story: "Release [DeliveryGuy] or [Customer] to pool", category: "Action", id: "a8f6b5f89b7645cb912006f559fe3291")]
public partial class ReleaseToPoolAction : Action
{
    [SerializeReference] public BlackboardVariable<DeliveryGuyController> DeliveryGuy;
    [SerializeReference] public BlackboardVariable<CustomerController> Customer;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var deliveryGuy = DeliveryGuy?.Value;
        var customer = Customer?.Value;

        if (deliveryGuy != null && customer != null)
        {
            return Status.Failure;
        }

        if (deliveryGuy != null)
        {
            deliveryGuy.ReturnToPool();
            return Status.Success;
        }

        if (customer != null)
        {
            customer.ReturnToPool();
            return Status.Success;
        }

        return Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}

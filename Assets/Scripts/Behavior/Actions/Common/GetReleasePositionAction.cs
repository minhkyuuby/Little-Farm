using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GetReleasePosition", story: "Read [DeliveryGuy] or [Customer] release position into [ReleasePosition]", category: "Action", id: "c553fb9f9fdb433da0b0efd350dcae4f")]
public partial class GetReleasePositionAction : Action
{
    [SerializeReference] public BlackboardVariable<DeliveryGuyController> DeliveryGuy;
    [SerializeReference] public BlackboardVariable<CustomerController> Customer;
    [SerializeReference] public BlackboardVariable<Vector3> ReleasePosition;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (ReleasePosition == null)
        {
            return Status.Failure;
        }

        var deliveryGuy = DeliveryGuy?.Value;
        var customer = Customer?.Value;

        if (deliveryGuy != null && customer != null)
        {
            return Status.Failure;
        }

        if (deliveryGuy != null)
        {
            ReleasePosition.Value = deliveryGuy.ReleasePosition;
            return Status.Success;
        }

        if (customer != null)
        {
            ReleasePosition.Value = customer.ReleasePosition;
            return Status.Success;
        }

        return Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}

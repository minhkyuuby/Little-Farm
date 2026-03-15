namespace LittleFarm.GameplayEventSubject
{
    public readonly struct PlantProduced
    {
        public PlantProduced(Plant plant)
        {
            Plant = plant;
        }

        public Plant Plant { get; }
    }

    public readonly struct DockBecameEmpty
    {
        public DockBecameEmpty(Dock dock)
        {
            Dock = dock;
        }

        public Dock Dock { get; }
    }

    public readonly struct DeliveryGuyReturned
    {
        public DeliveryGuyReturned(DeliveryGuyController deliveryGuy)
        {
            DeliveryGuy = deliveryGuy;
        }

        public DeliveryGuyController DeliveryGuy { get; }
    }

    public readonly struct PlantUpgradeRequested
    {
        public PlantUpgradeRequested(Plant plant)
        {
            Plant = plant;
        }

        public Plant Plant { get; }
    }
}
namespace LittleFarm.UpgradesEventSubject
{
	public readonly struct UpgradePaid
	{
		public UpgradePaid(string upgradeId)
		{
			UpgradeId = upgradeId;
		}

		public string UpgradeId { get; }
	}
}
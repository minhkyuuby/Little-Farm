using System;
using System.Collections.Generic;

public enum CurrencyType
{
    Coin = 0,
    Gem = 1,
    // Add more currency types as needed
}

public enum CurrencyChangeType
{
	None = 0,
	Initialize = 1,
	Set = 2,
	Add = 3,
	Spend = 4,
	Reset = 5,
}

public readonly struct CurrencyChangedEvent
{
	public CurrencyChangedEvent(
		CurrencyType currencyType,
		long previousBalance,
		long newBalance,
		long delta,
		CurrencyChangeType changeType,
		string reason)
	{
		CurrencyType = currencyType;
		PreviousBalance = previousBalance;
		NewBalance = newBalance;
		Delta = delta;
		ChangeType = changeType;
		Reason = reason;
	}

	public CurrencyType CurrencyType { get; }
	public long PreviousBalance { get; }
	public long NewBalance { get; }
	public long Delta { get; }
	public CurrencyChangeType ChangeType { get; }
	public string Reason { get; }
}

public static class CurrencyManager
{
	private static readonly Dictionary<CurrencyType, long> Balances = new();
	private static bool _isInitialized;

	public static bool IsInitialized => _isInitialized;
	public static long CoinBalance => GetBalance(CurrencyType.Coin);

	public static event Action<CurrencyChangedEvent> OnCurrencyChanged;

	public static void Initialize(long initialCoin = 0, string reason = "GameStart")
	{
		var changeType = _isInitialized ? CurrencyChangeType.Set : CurrencyChangeType.Initialize;
		ApplyBalance(CurrencyType.Coin, Math.Max(0L, initialCoin), changeType, reason);
		_isInitialized = true;
	}

	public static void Initialize(Dictionary<CurrencyType, long> initialBalances, string reason = "GameStart")
	{
		var changeType = _isInitialized ? CurrencyChangeType.Set : CurrencyChangeType.Initialize;
		if (initialBalances != null)
		{
			foreach (var pair in initialBalances)
			{
				ApplyBalance(pair.Key, Math.Max(0L, pair.Value), changeType, reason);
			}
		}

		_isInitialized = true;
	}

	public static void Reset(string reason = "Reset")
	{
		EnsureInitialized();
		foreach (CurrencyType currencyType in Enum.GetValues(typeof(CurrencyType)))
		{
			ApplyBalance(currencyType, 0L, CurrencyChangeType.Reset, reason);
		}
	}

	public static long GetBalance(CurrencyType currencyType)
	{
		EnsureInitialized();
		return Balances.TryGetValue(currencyType, out var value) ? value : 0L;
	}

	public static void SetBalance(CurrencyType currencyType, long value, string reason = "SetBalance")
	{
		EnsureInitialized();
		ApplyBalance(currencyType, Math.Max(0L, value), CurrencyChangeType.Set, reason);
	}

	public static void Add(CurrencyType currencyType, long amount, string reason = "Add")
	{
		EnsureInitialized();
		if (amount <= 0)
		{
			return;
		}

		var current = GetBalance(currencyType);
		var next = checked(current + amount);
		ApplyBalance(currencyType, next, CurrencyChangeType.Add, reason);
	}

	public static bool TrySpend(CurrencyType currencyType, long amount, string reason = "Spend")
	{
		EnsureInitialized();
		if (amount <= 0)
		{
			return false;
		}

		var current = GetBalance(currencyType);
		if (current < amount)
		{
			return false;
		}

		ApplyBalance(currencyType, current - amount, CurrencyChangeType.Spend, reason);
		return true;
	}

	public static bool CanAfford(CurrencyType currencyType, long amount)
	{
		EnsureInitialized();
		return amount >= 0 && GetBalance(currencyType) >= amount;
	}

	public static string GetFormatted(CurrencyType currencyType, int decimals = 1)
	{
		return CurrencyConverter.ToAbbreviated(GetBalance(currencyType), decimals);
	}

	public static string GetFormattedCoin(int decimals = 1)
	{
		return CurrencyConverter.ToAbbreviated(CoinBalance, decimals);
	}

	public static void SetCoin(long value, string reason = "SetCoin")
	{
		SetBalance(CurrencyType.Coin, value, reason);
	}

	public static void AddCoin(long amount, string reason = "AddCoin")
	{
		Add(CurrencyType.Coin, amount, reason);
	}

	public static bool TrySpendCoin(long amount, string reason = "SpendCoin")
	{
		return TrySpend(CurrencyType.Coin, amount, reason);
	}

	public static bool CanAffordCoin(long amount)
	{
		return CanAfford(CurrencyType.Coin, amount);
	}

	private static void EnsureInitialized()
	{
		if (_isInitialized)
		{
			return;
		}

		foreach (CurrencyType currencyType in Enum.GetValues(typeof(CurrencyType)))
		{
			if (!Balances.ContainsKey(currencyType))
			{
				Balances[currencyType] = 0L;
			}
		}

		_isInitialized = true;
	}

	private static void ApplyBalance(CurrencyType currencyType, long newBalance, CurrencyChangeType changeType, string reason)
	{
		var previous = GetBalance(currencyType);
		var clamped = Math.Max(0L, newBalance);

		if (previous == clamped && changeType != CurrencyChangeType.Reset)
		{
			return;
		}

		Balances[currencyType] = clamped;
		var evt = new CurrencyChangedEvent(currencyType, previous, clamped, clamped - previous, changeType, reason);
		OnCurrencyChanged?.Invoke(evt);
	}
}

using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;

public class ResourcesManager
{
	public Price playerResources = new();

	public bool Afford(Price _p) { return playerResources >= _p; }
	public void Pay(Price _p) { playerResources -= _p; }
	public void Add(Price _p) { playerResources += _p; }

	public bool tryPay(Price _p)
	{
		if(Afford(_p))
		{
			Pay(_p);
			return true;
		}
		return false;
	}

	public enum Resource { Mana, Fire, Elec, Stone };

}

public class Price
{
	public Dictionary<ResourcesManager.Resource, float> amounts;
	public Price()
	{
		amounts = new();
		amounts.Add(ResourcesManager.Resource.Mana, 0.0f);
		amounts.Add(ResourcesManager.Resource.Fire, 0.0f);
		amounts.Add(ResourcesManager.Resource.Elec, 0.0f);
		amounts.Add(ResourcesManager.Resource.Stone, 0.0f);
	}
	public Price(float _n)
	{
		amounts = new();
		amounts.Add(ResourcesManager.Resource.Mana, _n);
		amounts.Add(ResourcesManager.Resource.Fire, _n);
		amounts.Add(ResourcesManager.Resource.Elec, _n);
		amounts.Add(ResourcesManager.Resource.Stone, _n);
	}
	public Price(float _mana, float _fire, float _elec, float _stone)
	{
		amounts = new();
		amounts.Add(ResourcesManager.Resource.Mana, _mana);
		amounts.Add(ResourcesManager.Resource.Fire, _fire);
		amounts.Add(ResourcesManager.Resource.Elec, _elec);
		amounts.Add(ResourcesManager.Resource.Stone, _stone);
	}

	public float Get(ResourcesManager.Resource _r)
	{
		if(amounts.ContainsKey(_r) == false)
			throw new Exception("Price Dictionnary does not contain asked resource " + _r);
		return amounts[_r];
	}

	public float this[ResourcesManager.Resource key]
	{
		get => amounts[key];
		set => amounts[key] = value;
	}

	public static Price operator -(Price _a)
	{
		Price p = new();
		foreach(KeyValuePair<ResourcesManager.Resource, float> pair in _a.amounts)
			p[pair.Key] = -pair.Value;
		return p;
	}

	public static Price operator +(Price _a, Price _b)
	{
		Price p = new();
		foreach(KeyValuePair<ResourcesManager.Resource, float> pair in _a.amounts)
			p[pair.Key] = pair.Value + _b[pair.Key];
		return p;
	}

	public static Price operator -(Price _a, Price _b)
	{
		return _a + -_b;
	}

	public static Price operator *(Price _p, float _coef)
	{
		Price r = new();
		foreach(KeyValuePair<ResourcesManager.Resource, float> pair in _p.amounts)
			r[pair.Key] = _p[pair.Key] * _coef;
		return r;
	}

	public static bool operator >=(Price _a, Price _b)
	{
		foreach(KeyValuePair<ResourcesManager.Resource, float> pair in _a.amounts)
		{
			if(pair.Value < _b[pair.Key])
				return false;
		}
		return true;
	}

	public static bool operator <=(Price _a, Price _b)
	{
		foreach(KeyValuePair<ResourcesManager.Resource, float> pair in _a.amounts)
		{
			if(pair.Value > _b[pair.Key])
				return false;
		}
		return true;
	}

	public static bool operator <(Price _a, Price _b)
	{
		foreach(KeyValuePair<ResourcesManager.Resource, float> pair in _a.amounts)
		{
			if(pair.Value < _b[pair.Key])
				return true;
		}
		return false;
	}

	public static bool operator >(Price _a, Price _b)
	{
		foreach(KeyValuePair<ResourcesManager.Resource, float> pair in _a.amounts)
		{
			if(pair.Value > _b[pair.Key])
				return true;
		}
		return false;
	}

	public static bool operator true(Price _p)
	{
		foreach(KeyValuePair<ResourcesManager.Resource, float> pair in _p.amounts)
		{
			if(Mathf.Abs(pair.Value) < Mathf.Epsilon)
				return false;
		}
		return true;
	}

	public static bool operator false(Price _p)
	{
		foreach(KeyValuePair<ResourcesManager.Resource, float> pair in _p.amounts)
		{
			if(Mathf.Abs(pair.Value) > Mathf.Epsilon)
				return false;
		}
		return true;
	}

	public override string ToString()
	{
		string text = "";
		foreach(KeyValuePair<ResourcesManager.Resource, float> pair in amounts)
		{
			text += (text.Length == 0 ? "(" : ", ") + pair.Value;
		}
		return text + ")";
	}

}

using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;

public class ResourcesManager
{
	public Price playerResources { get; private set; } = new();

	public bool Afford(Price _p) { return playerResources >= _p; }
	public void Pay(Price _p) { playerResources -= _p; }
	public void Add(Price _p) { playerResources += _p; }

	public enum Resource { Mana, Fire, Elec, Stone };
	public class Price
	{
		public Dictionary<Resource, float> amounts;
		public Price()
		{
			amounts = new();
			amounts.Add(Resource.Mana, 0.0f);
			amounts.Add(Resource.Fire, 0.0f);
			amounts.Add(Resource.Elec, 0.0f);
			amounts.Add(Resource.Stone, 0.0f);
		}
		public Price(float _mana, float _fire, float _elec, float _stone)
		{
			amounts = new();
			amounts.Add(Resource.Mana, _mana);
			amounts.Add(Resource.Fire, _fire);
			amounts.Add(Resource.Elec, _elec);
			amounts.Add(Resource.Stone, _stone);
		}

		public float Get(Resource _r)
		{
			if(amounts.ContainsKey(_r) == false)
				throw new Exception("Price Dictionnary does not contain asked resource " + _r);
			return amounts[_r];
		}

		public float this[Resource key]
		{
			get => amounts[key];
			set => amounts[key] = value;
		}

		public static Price operator -(Price _a)
		{
			Price p = new();
			foreach(KeyValuePair<Resource, float> pair in _a.amounts)
				p[pair.Key] = -pair.Value;
			return p;
		}

		public static Price operator +(Price _a, Price _b)
		{
			Price p = new();
			foreach(KeyValuePair<Resource, float> pair in _a.amounts)
				p[pair.Key] = pair.Value + _b[pair.Key];
			return p;
		}

		public static Price operator -(Price _a, Price _b)
		{
			return _a + -_b;
		}

		public static bool operator >=(Price _a, Price _b)
		{
			foreach(KeyValuePair<Resource, float> pair in _a.amounts)
			{
				if(pair.Value < _b[pair.Key])
					return false;
			}
			return true;
		}

		public static bool operator <=(Price _a, Price _b)
		{
			foreach(KeyValuePair<Resource, float> pair in _a.amounts)
			{
				if(pair.Value > _b[pair.Key])
					return false;
			}
			return true;
		}
	}
}

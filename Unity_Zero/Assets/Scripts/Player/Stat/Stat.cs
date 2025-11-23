using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

[System.Serializable]
public class Stat
{
    [Tooltip("기본값 (순수 스탯)")]
    [SerializeField] private float _baseValue;

    private float _value;
    private bool _isDirty = true;

    // [수정] 필드에서는 선언만 하고, 생성자에서 초기화 (메모리 낭비 방지)
    private readonly List<StatModifier> _modifiers;
    public readonly ReadOnlyCollection<StatModifier> Modifiers;

    public Stat()
    {
        _modifiers = new List<StatModifier>();
        Modifiers = _modifiers.AsReadOnly();
    }

    public Stat(float baseValue) : this()
    {
        _baseValue = baseValue;
    }

    public float BaseValue
    {
        get => _baseValue;
        set
        {
            _baseValue = value;
            _isDirty = true;
        }
    }

    public float Value
    {
        get
        {
            if (_isDirty)
            {
                _value = CalculateFinalValue();
                _isDirty = false;
            }
            return _value;
        }
    }

    public void AddModifier(StatModifier mod)
    {
        _isDirty = true;
        _modifiers.Add(mod);
        _modifiers.Sort(CompareModifierOrder);
    }

    public bool RemoveModifier(StatModifier mod)
    {
        if (_modifiers.Remove(mod))
        {
            _isDirty = true;
            return true;
        }
        return false;
    }

    public bool RemoveAllModifiersFromSource(object source)
    {
        bool removed = false;
        for (int i = _modifiers.Count - 1; i >= 0; i--)
        {
            if (_modifiers[i].Source == source)
            {
                _modifiers.RemoveAt(i);
                removed = true;
            }
        }

        if (removed)
            _isDirty = true;

        return removed;
    }

    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        for (int i = 0; i < _modifiers.Count; i++)
        {
            StatModifier mod = _modifiers[i];

            if (mod.Type == StatModType.Flat)
            {
                finalValue += mod.Value;
            }
            else if (mod.Type == StatModType.PercentAdd)
            {
                sumPercentAdd += mod.Value;
                if (i + 1 >= _modifiers.Count || _modifiers[i + 1].Type != StatModType.PercentAdd)
                {
                    finalValue *= 1 + sumPercentAdd;
                    sumPercentAdd = 0;
                }
            }
            else if (mod.Type == StatModType.PercentMult)
            {
                finalValue *= 1 + mod.Value;
            }
        }

        return Mathf.Max(0f, finalValue);
    }

    private int CompareModifierOrder(StatModifier a, StatModifier b)
    {
        if (a.Order < b.Order) return -1;
        if (a.Order > b.Order) return 1;
        return 0;
    }
}
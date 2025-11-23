using UnityEngine;

public enum StatModType
{
    Flat = 100,       // 값 더하기 (예: +10)
    PercentAdd = 200, // 퍼센트 합연산 (예: +10%)
    PercentMult = 300 // 퍼센트 곱연산
}

[System.Serializable]
public class StatModifier
{
    public float Value;
    public StatModType Type;
    public int Order;
    public object Source;

    // 생성자 1: 값, 타입, 출처 지정
    public StatModifier(float value, StatModType type, object source)
    {
        Value = value;
        Type = type;
        Order = (int)type;
        Source = source;
    }

    // 생성자 2: 값과 타입만 지정 (Source 없음)
    public StatModifier(float value, StatModType type) : this(value, type, null) { }
}
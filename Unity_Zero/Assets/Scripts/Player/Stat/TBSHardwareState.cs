using System;
using UnityEngine;

[Serializable]
public class TBSHardwareState
{
    [Tooltip("RAM 레벨 (데이터 테이블에서 용량을 환산)")]
    public int ramLevel;

    [Tooltip("Battery 레벨 (Max Efficiency 증가량에 영향)")]
    public int batteryLevel;

    [Tooltip("CPU 레벨 (쿨타임/시간 소모 감소에 영향)")]
    public int cpuLevel;

    [Tooltip("Heatsink 레벨 (효율 패널티 배율 감소에 영향)")]
    public int heatsinkLevel;
}

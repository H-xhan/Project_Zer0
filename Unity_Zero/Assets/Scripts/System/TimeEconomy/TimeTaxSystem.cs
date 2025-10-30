using UnityEngine;

public class TimeTaxSystem : MonoBehaviour
{
    [SerializeField] private TimeConfig config;
    [SerializeField] private TimeWallet wallet;

    public void ApplyLoopTaxNow()
    {
        if (config == null || wallet == null) return;
        float tax = wallet.CurrentSeconds * config.loopTaxRate;
        if (tax > 0f) wallet.SpendSeconds(tax, "Loop tax");
    }
}

using UnityEngine;

public class PlayerCurrency : MonoBehaviour
{
    public int currentCredits = 500;

    public bool TrySpend(int amount)
    {
        if (currentCredits < amount) return false;
        currentCredits -= amount;
        return true;
    }

    public void AddCredits(int amount)
    {
        currentCredits += amount;
    }
}

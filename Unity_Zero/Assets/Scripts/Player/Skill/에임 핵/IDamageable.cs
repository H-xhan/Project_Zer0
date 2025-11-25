// IDamageable.cs
using UnityEngine;

// 인터페이스: "이걸 상속받은 놈들은 무조건 TakeDamage 기능을 가지고 있다"고 약속하는 것
public interface IDamageable
{
    void TakeDamage(float amount);
}
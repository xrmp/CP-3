using System;  // Импорт базовых типов (для атрибута Serializable)

[Serializable]  // Разрешает сериализацию объекта - можно сохранять в JSON, показывать в инспекторе Unity
public class Weapon  // Класс оружия
{
    public int id;        // Уникальный идентификатор оружия
    public float damage;  // Урон оружия
    public float cooldown; // Время перезарядки в секундах

    public Weapon(int id, float damage, float cooldown)  // Конструктор с параметрами
    {
        this.id = id;              // Присваиваем ID
        this.damage = damage;      // Присваиваем урон
        this.cooldown = cooldown;  // Присваиваем кулдаун
    }

    public void DebugPrint()  // Метод для отладки - выводит информацию об оружии
    {
        UnityEngine.Debug.Log($"🔫 Weapon {id}: damage={damage}, cooldown={cooldown}");  // Логируем с эмодзи
    }
}
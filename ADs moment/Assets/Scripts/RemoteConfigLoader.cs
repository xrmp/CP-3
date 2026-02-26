using System;              // Базовые типы и исключения
using System.Collections;  // Интерфейсы для корутин (IEnumerator)
using System.Collections.Generic;  // Коллекции (List)
using System.IO;          // Работа с файловой системой
using UnityEngine;        // Основные классы Unity
using UnityEngine.Networking;  // Работа с сетью (UnityWebRequest)

public class RemoteConfigLoader : MonoBehaviour  // Загрузчик конфигурации оружия
{
    [Header("Config URL")]  // Заголовок в инспекторе
    [SerializeField] private string configUrl = "https://shimailudoed.github.io/Build/config/weapons.json";  // URL конфигурации

    [Header("Default values")]  // Заголовок в инспекторе
    [SerializeField]
    private Weapon[] defaultWeapons = new Weapon[]  // Оружие по умолчанию
    {
        new Weapon(1, 5f, 1f),   // ID 1, урон 5, кулдаун 1с
        new Weapon(2, 10f, 0.8f) // ID 2, урон 10, кулдаун 0.8с
    };

    // Загруженные оружия (доступно только для чтения извне)
    public List<Weapon> loadedWeapons { get; private set; }  // Авто-свойство с приватным сеттером

    private string localFilePath;  // Путь к локальному файлу кэша

    void Awake()  // Вызывается при инициализации объекта
    {
        localFilePath = Path.Combine(Application.persistentDataPath, "weapon_config.json");  // Формируем путь кэша
        loadedWeapons = new List<Weapon>();  // Инициализируем список
    }

    void Start()  // Вызывается перед первым кадром
    {
        StartCoroutine(LoadConfig());  // Запускаем корутину загрузки конфигурации
    }

    IEnumerator LoadConfig()  // Корутина загрузки конфигурации
    {
        yield return StartCoroutine(TryLoadFromRemote());  // Пытаемся загрузить удаленно

        if (loadedWeapons.Count == 0)  // Если удаленно не загрузилось
        {
            LoadFromLocal();  // Пытаемся загрузить из локального кэша
        }

        if (loadedWeapons.Count == 0)  // Если и из кэша не загрузилось
        {
            loadedWeapons.AddRange(defaultWeapons);  // Используем оружие по умолчанию
        }
        else  // Если загрузилось удаленно или из кэша
        {
            defaultWeapons = loadedWeapons.ToArray();  // Обновляем значения по умолчанию (странное решение)
        }

        PrintWeaponsArray();  // Выводим результат в консоль
    }

    void PrintWeaponsArray()  // Вывод загруженного оружия в консоль
    {
        Weapon[] weaponsArray = loadedWeapons.ToArray();  // Конвертируем List в массив

        for (int i = 0; i < weaponsArray.Length; i++)  // Проходим по всем элементам
        {
            Weapon w = weaponsArray[i];  // Получаем оружие по индексу
            Debug.Log($"[{i}] Weapon ID: {w.id}, Damage: {w.damage}, Cooldown: {w.cooldown}");  // Вывод в консоль
        }

        Debug.Log($"Total weapons: {weaponsArray.Length}");  // Вывод общего количества
    }

    IEnumerator TryLoadFromRemote()  // Попытка загрузки с удаленного сервера
    {
        using (UnityWebRequest request = UnityWebRequest.Get(configUrl))  // Создаем GET-запрос (using освобождает ресурсы)
        {
            yield return request.SendWebRequest();  // Отправляем запрос и ждем ответа

            if (request.result == UnityWebRequest.Result.Success)  // Если запрос успешен
            {
                string rawData = request.downloadHandler.text;  // Получаем текст ответа

                if (configUrl.Contains(".json"))  // Проверяем формат по URL
                    ParseJson(rawData);  // Парсим как JSON
                else
                    ParseCSV(rawData);  // Парсим как CSV

                if (loadedWeapons.Count > 0)  // Если удалось спарсить оружие
                    SaveToLocal();  // Сохраняем в кэш
            }
        }  // Здесь автоматически вызывается Dispose() у request
    }

    void ParseJson(string json)  // Парсинг JSON строки
    {
        try  // Обработка возможных исключений
        {
            WeaponDataWrapper wrapper = JsonUtility.FromJson<WeaponDataWrapper>(json);  // Десериализация JSON
            if (wrapper?.weapons != null)  // Проверка, что обертка и массив не null
            {
                loadedWeapons.Clear();  // Очищаем текущий список
                foreach (Weapon w in wrapper.weapons)  // Проходим по массиву оружия
                {
                    if (w.damage >= 0 && w.cooldown > 0)  // Валидация данных (урон >=0, кулдаун >0)
                    {
                        loadedWeapons.Add(w);  // Добавляем валидное оружие
                    }
                }
            }
        }
        catch (Exception e)  // Ловим любое исключение
        {
            Debug.LogError($"JSON parsing error: {e.Message}");  // Выводим ошибку
        }
    }

    void ParseCSV(string csv)  // Парсинг CSV строки
    {
        try  // Обработка исключений
        {
            string[] lines = csv.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);  // Разбиваем на строки
            loadedWeapons.Clear();  // Очищаем список

            for (int i = 1; i < lines.Length; i++)  // Пропускаем заголовок (i=1)
            {
                string line = lines[i].Trim();  // Убираем пробелы в начале/конце
                if (string.IsNullOrEmpty(line)) continue;  // Пропускаем пустые строки

                string[] values = line.Split(',');  // Разбиваем по запятой
                if (values.Length >= 3)  // Проверяем, что есть минимум 3 колонки
                {
                    if (int.TryParse(values[0].Trim(), out int id) &&  // Парсим ID как int
                        float.TryParse(values[1].Trim(), out float damage) &&  // Парсим урон как float
                        float.TryParse(values[2].Trim(), out float cooldown))  // Парсим кулдаун как float
                    {
                        if (damage >= 0 && cooldown > 0)  // Валидация данных
                        {
                            loadedWeapons.Add(new Weapon(id, damage, cooldown));  // Создаем и добавляем оружие
                        }
                    }
                }
            }
        }
        catch (Exception e)  // Ловим исключения
        {
            Debug.LogError($"CSV parsing error: {e.Message}");  // Выводим ошибку
        }
    }

    void SaveToLocal()  // Сохранение в локальный файл (кэш)
    {
        try  // Обработка исключений
        {
            WeaponDataWrapper wrapper = new WeaponDataWrapper();  // Создаем обертку для JSON
            wrapper.weapons = loadedWeapons.ToArray();  // Конвертируем список в массив
            string json = JsonUtility.ToJson(wrapper, true);  // Сериализуем в JSON (true = форматировать)
            File.WriteAllText(localFilePath, json);  // Записываем файл
        }
        catch (Exception e)  // Ловим исключения
        {
            Debug.LogError($"Failed to save cache: {e.Message}");  // Выводим ошибку
        }
    }

    void LoadFromLocal()  // Загрузка из локального кэша
    {
        try  // Обработка исключений
        {
            if (File.Exists(localFilePath))  // Проверяем, существует ли файл
            {
                string json = File.ReadAllText(localFilePath);  // Читаем содержимое
                WeaponDataWrapper wrapper = JsonUtility.FromJson<WeaponDataWrapper>(json);  // Десериализация
                if (wrapper?.weapons != null)  // Проверка на null
                {
                    loadedWeapons.Clear();  // Очищаем список
                    loadedWeapons.AddRange(wrapper.weapons);  // Добавляем загруженное оружие
                }
            }
        }
        catch (Exception e)  // Ловим исключения
        {
            Debug.LogError($"Failed to load cache: {e.Message}");  // Выводим ошибку
        }
    }
}

[Serializable]  // Разрешает сериализацию класса в JSON
public class WeaponDataWrapper  // Обертка для массива оружия (для JSON сериализации)
{
    public Weapon[] weapons;  // Массив оружия
}
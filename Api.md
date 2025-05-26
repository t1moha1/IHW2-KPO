
# Документация к Microservices Application

Данная документация содержит краткое описание архитектуры системы и спецификацию API для микросервисного приложения IHW2-KPO.

---

##  Спецификация API

Все микросервисы и шлюз используют JSON и стандартные HTTP-статусы.

### 1. FileStoringService

#### POST `/api/file`

* **Описание:** Загружает текстовый файл (.txt)
* **Headers:** `Content-Type: multipart/form-data`
* **Form Data:**

  * `file` — файл с расширением `.txt`
* **Успех (200):**

  ```json
  { "id": "{GUID}" }
  ```
* **Ошибки:**

  * 400 Bad Request — файл не предоставлен, пустой или неверный тип
  * 500 Internal Server Error — ошибка сохранения файла

#### GET `/api/file/{id}`

* **Описание:** Возвращает содержимое файла по идентификатору
* **Успех (200):** Возвращается `text/plain` с содержимым файла
* **Ошибки:**

  * 404 Not Found — файл не найден
  * 500 Internal Server Error — ошибка чтения файла

### 2 FileAnalysisService

#### GET `/api/fileinfo/{id}`

* **Описание:** Анализирует файл по идентификатору (если ранее не анализировался) или возвращает сохранённый результат
* **Успех (200):**

  ```json
  {
    "id": "{GUID}",
    "paragraphCount": <int>,
    "wordCount": <int>,
    "characterCount": <int>,
    "hash": "<SHA-256>",
    "isPlagiarized": <bool>,
    "analyzedAt": "<ISO 8601 UTC>"
  }
  ```
* **Ошибки:**

  * 404 Not Found — файл не найден в FileStoringService
  * 502 Bad Gateway — ошибка связи со службой хранения
  * 500 Internal Server Error — внутренняя ошибка сервиса анализа

### 3 API Gateway

#### POST `/api/file/upload`

* **Описание:** Принимает загрузку файла и перенаправляет в FileStoringService
* **Form Data:** `file` (`.txt`)
* **Успех (200):** `{ "id": "{GUID}" }`
* **Ошибки:**

  * 400 Bad Request — неправильный формат или отсутствие файла
  * 502 Bad Gateway — ошибка связи с FileStoringService

#### GET `/api/file/upload/{id}`

* **Описание:** Получение содержимого файла через шлюз
* **Успех (200):** `text/plain`
* **Ошибки:**

  * 404 Not Found — файл не найден
  * 502 Bad Gateway — проблема связи

#### GET `/api/file/analysis/{id}`

* **Описание:** Запрос анализа файла через шлюз
* **Успех (200):** JSON-объект анализа (см. раздел 2)
* **Ошибки:**

  * 404 Not Found — файл или результат анализа не найден
  * 502 Bad Gateway — проблема связи с FileAnalysisService




# IHW2-KPO Microservices Application

Набор инструкций для быстрого запуска на чистой машине.

---

## 1. Предварительные требования

- **.NET 8 SDK**  
  
  Проверить:
  ```bash
  dotnet --version 
  ```

* **PostgreSQL 15+**

  Проверить:

  ```bash
  psql --version
  ```

---

## 2. Склонировать репозиторий и восстановить пакеты

```bash
git clone https://github.com/t1moha1/IHW2-KPO
cd IHW2-KPO
dotnet restore
```


---

## 3. Создать базу и пользователя в PostgreSQL

```bash
sudo -u postgres psql
```

```sql
CREATE ROLE myuser WITH LOGIN PASSWORD 'mypassword';
CREATE DATABASE YourDb OWNER myuser;

\c YourDb
GRANT USAGE ON SCHEMA public TO myuser;
GRANT CREATE ON SCHEMA public TO myuser;
```

---

## 4. Настроить строки подключения

В `FileStoringService/appsettings.json` и `FileAnalisysService/appsettings.json`:

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=YourDb;Username=myuser;Password=mypassword"
  }
}
```

---


## 5. Дать права на скрипт и запустить всё

```bash
chmod +x manage.sh
./manage.sh start
```

Сервисы будут доступны:

* FileStoringService — [http://localhost:5002](http://localhost:5002)
* FileAnalisysService — [http://localhost:5004](http://localhost:5004)
* API Gateway — [http://0.0.0.0:6000](http://0.0.0.0:6000) (Swagger UI: /swagger)

---

## 6. Тесты

```bash
python3 --version
pip3 --version

pip3 install pytest requests

pytest tests/
```
---

## 7. Остановка всех сервисов

```bash
./manage.sh stop
```



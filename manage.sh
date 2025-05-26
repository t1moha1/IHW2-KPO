#!/usr/bin/env bash

# Названия директорий с сервисами
SERVICES=(
  "FileStoringService"
  "FileAnalisysService"
  "APIGateway"
)

# Файл для хранения PID каждого сервиса
PID_DIR=".pids"
mkdir -p "$PID_DIR"

function start_all() {
  echo "Запуск всех сервисов..."
  for svc in "${SERVICES[@]}"; do
    echo -n " • $svc… "
    pushd "$svc" > /dev/null
    # запускаем в фоне, перенаправляем вывод в лог
    dotnet run \
      > "../logs/${svc}.log" 2>&1 &
    pid=$!
    popd > /dev/null
    echo $pid > "${PID_DIR}/${svc}.pid"
    echo "OK (PID=$pid)"
  done
  echo "Все сервисы запущены."
  echo "Логи — в папке logs/, PID-файлы — в ${PID_DIR}/"
}

function stop_all() {
  echo "Остановка всех сервисов..."
  for svc in "${SERVICES[@]}"; do
    pid_file="${PID_DIR}/${svc}.pid"
    if [ -f "$pid_file" ]; then
      pid=$(< "$pid_file")
      if kill -0 "$pid" 2>/dev/null; then
        echo -n " • $svc (PID=$pid)… "
        kill "$pid"
        echo "OK"
      else
        echo " • $svc: процесс $pid не найден."
      fi
      rm -f "$pid_file"
    else
      echo " • $svc: PID-файл не найден."
    fi
  done
  echo "Все сервисы остановлены."
}

case "$1" in
  start)
    # создаём папку для логов, если нужно
    mkdir -p logs
    start_all
    ;;
  stop)
    stop_all
    ;;
  restart)
    stop_all
    start_all
    ;;
  *)
    echo "Использование: $0 {start|stop|restart}"
    exit 1
    ;;
esac
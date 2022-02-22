# Описание
- Решение было написано с использованием .NET Framework и TCP протоколов. 
- Решение состоит из 3 частей: Клиентская часть, Серверная часть и Библиотека классов.
- Клиентская часть выполнена в виде приложения на WindowsForms, серверная в виде консольного.

# Содержимое
1. Проект с сервером - папка server
2. Проект с клиенсткой частью - папка client
3. Проект с библиотекой классов, используемых на стороне клиента и сервера - MessageHandler

# Описание работы клиентской части
1. По нажатию на кнопку "Добавить файл" пользователь добавляет файлы. 
> За один раз можно добавить только 1 файл.
2. Содержимое каждого файла загружается в таблицу.
3. По нажатию на кнопку "Добавить файл" для каждого файла резервируется отдельный поток.
	> Кнопка "Отправить файлы" доступна для нажатия тогда, когда в таблице имеются строки со значением "Не отправлено" у атрибута "Статус запроса".
	В каждом потоке осуществляется:
	3.1 Подключение к серверу. 
	3.2 Отправка запроса.
	> Запрос хранит в себе объект, в котором есть код строки в таблице, сообщение (содержимое файла) и статус запроса
	3.3 Получение запроса.
	> Пользователь получает статус запроса: Не отправлено, Палиндром, Не палиндром, Ошибка при отправке/обработке запроса.
	3.4 Изменение строк с данными в таблице.  
	> При разрыве соединения между сервером и клиентом, статус запроса становится следующим "Ошибка при отправке/обработке запроса".

# Описание работы сервереной части
1. Ввод с клавиатуры значения переменной N (Количество одновременно обрабатываемых запросов на сервере). 
> Значение должно быть целочисленным числом и больше нуля.
2. Включение сервера и ожидание подключений со стороны клиентской части.
3. Обработка запросов.
> Каждый запрос - отдельное TCP подключение к серверу. Eсли на сервер поступает количество запросов превышающее N, то сервер обрабатывает только N запросов из всех полученных. С остальными запросами он разрывает соединение и присваивает им статус "Запрос не был обработан. Количество запросов превышено!". 

# Как запустить
1. Запустите сервер. -> Введите N.
2. Запустите клиент. -> Добавьте файл(ы). -> Отправьте файл(ы).
	

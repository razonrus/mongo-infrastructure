mongo-infrastructure
====================

Extensions for mongodb driver, infrastructure for mocking mongo objects

.sln file here: Infrastructure/Infrastructure.sln

nuget package: https://www.nuget.org/packages/MongoDbInfrastructure

====================

репозиторий включает в себя: 
- методы-расширения для более удобной работы со стандартным C# драйвером для mongoDB - так называемый синтаксический сахар;
- реализацию восстановления соединения с сервером mongoDB при обрыве связи;
- пример реализации удобной инфраструктуры, предоставляющей доступ к базам данных mongoDB и их коллекциям;
- пример создания и настройки mock'ов для объектов mongoDB в юнит тестах

====================

**Обратите внимание**, если вы используете подключение с логином паролем то в MongoInitializer не добавляйте постфикс к базе здесь: " return new SampleDatabase(GetDatabase(dbPrefix));", т.к. она не будет создана автоматом (как при анонимном входе) и вы получите ошибку "not authorized" (thnks to sk.kirill for warning)

docker run -d --name mssql --cpus="1" -p 1433:1433 -e ACCEPT_EULA=Y -e SA_PASSWORD=Password+ samuelmarks/mssql-server-fts-sqlpackage-linux
docker run -d --name wiremock -p 8080:8080 wiremock/wiremock
docker run -d --name rabbit --cpus="0.5" -p 15672:15672 -e RABBITMQ_DEFAULT_USER=user -e RABBITMQ_DEFAULT_PASS=pass rabbitmq:3.12.6-management
timeout 60
docker run -d --name=grafana -p 3000:3000 --link mssql grafana/grafana-enterprise

docker run -d --cpus="1" --name s1 -p 8989:80 --link rabbit -e ConnectionStrings__rmq="amqp://user:pass@rabbit:5672/" rockf3ller/s1
docker run -d --cpus="1" --name wc1 --link mssql --link rabbit --link wiremock -e PrefetchCount=5000 -e ConnectionStrings__db="Server=mssql;Database=demo;User Id=sa;Password=Password+;MultipleActiveResultSets=True;Connection Timeout=30;Max Pool Size=1000;Pooling=true;" -e ConnectionStrings__rmq="amqp://user:pass@rabbit:5672/" rockf3ller/wc1
docker run -d --cpus="1" --name monolith -p 98:80 --link mssql -e timeoutSecond=30 -e _batchCount=100 -e ConnectionStrings__db="Server=mssql;Database=demo;User Id=sa;Password=Password+;MultipleActiveResultSets=True;Connection Timeout=30;Max Pool Size=1000;Pooling=true;" rockf3ller/monolith
docker run -d --name rabbit -p 15672:15672 -p 5672:5672 -p 4369:4369 -p 5671:5671 -p 25672:25672 -e RABBITMQ_DEFAULT_USER=user -e RABBITMQ_DEFAULT_PASS=pass rabbitmq:3.12.6-management
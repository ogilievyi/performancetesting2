cd %cd%
cd ..\src\PerformanceDemo
docker build . -f dockerfileMicroservicePublicApi -t public-api:1
docker build . -f dockerfileMicroserviceWorker -t worker:1
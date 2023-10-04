cd %cd%
cd ..\src\PerformanceDemo
dotnet clean 
docker build . -f dockerfileMicroservicePublicApi -t s1
docker build . -f dockerfileMicroserviceWorker -t wc1
docker build . -f dockerfileMonolith -t monolith
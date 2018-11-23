# tmpps.infrastructure.sqs

## register nuget

```bash
npm run publish
# replace version,api-key
dotnet nuget push ./Tmpps.Infrastructure.SQS/bin/Release/Tmpps.Infrastructure.SQS.${version}.nupkg -k ${api-key} -s https://api.nuget.org/v3/index.json
```

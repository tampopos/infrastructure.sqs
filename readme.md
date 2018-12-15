# tmpps.infrastructure.sqs

## command

#### build

`dotnet build Tmpps.Infrastructure.SQS.Tests/`

#### test

`dotnet test Tmpps.Infrastructure.SQS.Tests/`

#### register nuget

```bash
dotnet build -c Release Tmpps.Infrastructure.SQS
# replace version,api-key
dotnet pack -c Release --include-source -p:PackageVersion=${version} Tmpps.Infrastructure.SQS
dotnet nuget push ./Tmpps.Infrastructure.SQS/bin/Release/Tmpps.Infrastructure.SQS.${version}.nupkg -k ${api-key} -s https://api.nuget.org/v3/index.json
```

## use circleCI CLI

#### validation config

`circleci config validate`

#### test

`circleci local execute --job test`

#### release

```bash
git tag X.Y.Z
git push origin --tags
```

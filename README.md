# Transformador de Arquivos — CSV, JSON, XML e XLSX

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=flat&logo=dotnet&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker&logoColor=white)
![Kubernetes](https://img.shields.io/badge/Kubernetes-manifests-326CE5?style=flat&logo=kubernetes&logoColor=white)

API ASP.NET Core com uma página estática em `wwwroot`. O `POST /api/transform` lê um arquivo, filtra, escolhe colunas, renomeia e devolve o resultado em JSON, CSV ou XML. A resposta é um JSON (`TransformResponse`), com o conteúdo convertido no campo `Data`. XLSX entra; não sai.

| Checagem | Onde |
|---|---|
| Extensão `.csv`, `.json`, `.xml`, `.xlsx` | Controller e `FileTransformService` |
| Acima de 10 MB | Controller recusa antes do serviço |
| Acima de 5 MB | Serviço recusa se o stream informar o tamanho |
| Filtro | Uma expressão `campo operador valor`, com `!=`, `>=`, `<=`, `>`, `<`, `=` |
| Colunas | `selectColumns` separado por vírgula; `renameColumns` no formato `antigo:novo` |

O controller tem `[EnableRateLimiting("transform")]`, mas o `Program.cs` não registra rate limiter. O pacote Swashbuckle está no csproj e também não é ligado no `Program.cs`.

## Stack

- .NET 10 (`net10.0`)
- ClosedXML 0.105.0 (primeira planilha do XLSX), CsvHelper 33.0.1, Newtonsoft.Json 13.0.3
- Docker (`mcr.microsoft.com/dotnet/sdk:10.0` e `aspnet:10.0`, porta 8080)
- Manifests em `k8s/` e workflow em `.github/workflows/dotnet.yml`

## Estrutura

```
transformador-de-arquivos/
├── Dockerfile
├── .github/workflows/dotnet.yml
├── k8s/                 deployment, service, ingress
└── src/DataForge/
    ├── Program.cs
    ├── DataForge.csproj
    ├── DataForge.http
    ├── Controllers/TransformController.cs
    ├── Models/TransformRequest.cs
    ├── Services/FileTransformService.cs
    └── wwwroot/          index.html, css/style.css, js/app.js
```

`GET /api/transform/health` responde `{ status, timestamp }`. O deployment usa essa rota no readiness e no liveness, na porta 8080. A imagem do manifest é o placeholder `your-dockerhub-user/dataforge:latest`. O ingress aponta para `dataforge.local` e limita o body a 10 MB.

JSON de entrada precisa ser um array de objetos. XML espera elementos filhos da raiz, cada um com subelementos. CSV e XLSX usam a primeira linha como cabeçalho.

`outputFormat`: `csv`, `xml` ou qualquer outro valor (cai em JSON indentado).

## Como rodar

Pré-requisito: [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/gabrielteramae/transformador-de-arquivos.git
cd transformador-de-arquivos
dotnet run --project src/DataForge
```

O perfil `http` escuta em `http://localhost:5207`. O perfil `https` usa `https://localhost:7097` e `http://localhost:5207`. A página estática é a raiz; a API é `POST /api/transform` com `multipart/form-data` (`file`, e os campos opcionais `filter`, `selectColumns`, `renameColumns`, `outputFormat`).

Docker:

```bash
docker build -t dataforge .
docker run --rm -p 8080:8080 dataforge
```

O workflow de CI instala o SDK `8.0.x` e roda `dotnet test`, mas o alvo do projeto é `net10.0` e não há projeto de teste no repositório.

---

© 2026 Gabriel Teramae Chan

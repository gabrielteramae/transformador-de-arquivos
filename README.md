# Transformador de Arquivos

![C#](https://img.shields.io/badge/C%23-239120?style=flat&logo=c-sharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-512BD4?style=flat&logo=dotnet&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker&logoColor=white)

API para transformação e conversão de arquivos de dados.

## Demo

**[conversor-de-arquivos.up.railway.app](https://conversor-de-arquivos.up.railway.app)**

## Sobre

Converta e filtre arquivos CSV, JSON, XML ou XLSX diretamente pelo browser, sem instalar nada. Todas as transformações são encadeadas em um único request.

## Funcionalidades

- Upload de arquivos CSV, JSON, XML e XLSX (até 5 MB)
- Filtro de linhas por expressão
- Seleção de colunas específicas
- Renomeação de colunas
- Conversão entre formatos (CSV → JSON, XLSX → XML, etc.)
- Download do resultado
- Modal de ajuda com exemplos de sintaxe
- Rate limiting (10 requests/min por IP)
- Validação de tipo e tamanho de arquivo no backend

## Stack

- **C# / ASP.NET Core**
- **ClosedXML** (suporte a XLSX)
- **Docker**
- **Kubernetes** (k8s/)
- **Railway**
- **CI/CD** via GitHub Actions

---

## Como rodar localmente

```bash
cd src/DataForge
dotnet run

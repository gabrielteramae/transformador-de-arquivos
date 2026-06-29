# Transformador de arquivos

API para transformação e conversão de arquivos de dados.

## Demo

**[conversor-de-arquivos.up.railway.app](https://conversor-de-arquivos.up.railway.app)**

## Sobre

Converta e filtre arquivos CSV, JSON ou XML diretamente pelo browser, sem instalar nada. Todas as transformações são encadeadas em um único request.

## Funcionalidades

- Upload de arquivos CSV, JSON e XML (até 5 MB)
- Filtro de linhas por expressão
- Seleção de colunas específicas
- Renomeação de colunas
- Conversão entre formatos (CSV → JSON, JSON → XML, etc.)
- Download do resultado

## Sintaxe das transformações

### Filtro

Use operadores de comparação para filtrar linhas:

| Operador | Exemplo        | Descrição |
| -------- | -------------- | --------- |
| `=`      | `status=ativo` | Igual a   |
| `>`      | `idade>18`     | Maior que |
| `<`      | `idade<65`     | Menor que |

### Seleção de colunas

Informe os nomes separados por vírgula:

```
nome,email,idade
```

### Renomear colunas

Use o formato `original:novo` separado por vírgula:

```
name:nome,email:correio
```

## Stack

- C# / ASP.NET Core
- Docker
- Kubernetes (k8s/)
- Railway

## Como rodar localmente

```bash
cd src/DataForge
dotnet run
```

Acesse `http://localhost:5207`

## Endpoints

| Método | Rota                    | Descrição                    |
| ------ | ----------------------- | ---------------------------- |
| `POST` | `/api/Transform`        | Transforma o arquivo enviado |
| `GET`  | `/api/Transform/health` | Health check da API          |

### Parâmetros do POST

| Campo           | Tipo   | Descrição                                |
| --------------- | ------ | ---------------------------------------- |
| `file`          | File   | Arquivo CSV, JSON ou XML (max 5 MB)      |
| `filter`        | string | Expressão de filtro (opcional)           |
| `selectColumns` | string | Colunas separadas por vírgula (opcional) |
| `renameColumns` | string | Mapeamento original:novo (opcional)      |
| `outputFormat`  | string | `json`, `csv` ou `xml`                   |

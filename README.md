# elaw_webcrawler

Criar ferramenta para extração dos dados de website (Webcrawler).

## Requisitos

<ol>
<li>Acessar o site "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc".</li>
<li>Extrair os campos "IP Adress", "Port", "Country" e "Protocol". de todas as linhas, de todas as páginas disponíveis na execução.</li>
<li>Necessário salvar o resultado da extração em arquivo json, que deverá ser salvo na máquina.</li>
<li>Necessário salvar em banco de dados a data início execução, data termino execução, quantidade de páginas, quantidade linhas extraídas em todas as páginas e arquivo json gerado.</li>
<li>Necessário print (arquivo .html) de cada página.</li>
<li>Necessário que o webcrawler seja multithread, com máximo de 3 execuções simultâneas.</li>
</ol>

## Estratégias utilizadas no projeto

<ol>
    <li>Separação dos projetos em bibliotecas de classe de acordo com cada funcionalidade
        <ul>
            <li><b>ElawWebCrawler.Common:</b> Definições comuns de cross-connect do projeto</li>
            <li><b>ElawWebCrawler.Domain:</b> Definições das entidades de domínio</li>
            <li><b>ElawWebCrawler.Data:</b> Definições de contexto e mapeamento das entidades do projeto</li>
            <li><b>ElawWebCrawler.Persistence:</b> Definições dos repositórios das entidades do projeto</li>
            <li><b>ElawWebCrawler.Provider:</b> Definições dos provedores externos, no caso, Azure, para armazenar os arquivos num Storage Account</li>
            <li><b>ElawWebCrawler.Application:</b> Definições de execução das ações de orquestração do negócio</li>
            <li><b>ElawWebCrawler.Api:</b> Definições das interfaces para conexão externa da api do projeto</li>
            <li><b>ElawWebCrawler.Test:</b> Projeto de teste para a classe principal do projeto de Application</li>
        </ul>
    </li><br>
    <li>Utilização do Serilog como ferramenta de log da aplicação, armazenando os dados em banco de dados SQL Server</li><br>
    <li>Utilização do ORM Entity Framework para gerenciamento da comunicação entre API e Banco de Dados</li><br>
    <li>Utilização do recurso de Storage Accounts do Azure como ferramenta para armazenamento dos arquivos definidos no escopo do projeto</li><br>
    <li>Validação da qualidade de código com testes unitários para a classe principal do projeto de Application</li><br>
    <li>Utilização da classe <b>SemaphoreSlim</b> como estratégia simplificada de controle do número de threads</li><br>
    <li>Utilização da classe <b>ConcurrentBag< T ></></b> como estratégia <b>thread-safe</b> para gerenciar a lista que coleta os dados trabalhados em diferentes threads</li><br>
</ol>

## Como executar o projeto BackEnd

<ol>
<li>Clonar o repositório e acessar a pasta Server</li><br>
<li>Necessário adicionar o arquivo appsettings.json na raiz da pasta server com as definições de configuração conforme abaixo:

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "sua_connection_string_de_conexão_com_banco_de_dados_MSSQLServer",
    "AzureStorageConnection": "sua_connection_string_de_conexão_com_Azure_Storage_Account"
  },
  "AzureBlobStorageApi": {
    "ContainerName": "files" // ou qualquer outro nome definido por você
  },
  "DatabaseOptions": { // deinições para a entidade de Banco de Dados
    "MaxRetryCount": 3,
    "CommandTimeout": 30,
    "EnableDetailedErrors": true,
    "EnableSensitiveDataLogging": true
  },
  "Serilog": { // definições do serilog para registro de log
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Warning",
        "System": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "MSSqlServer",
        "Args": {
          "connectionString": "sua_connection_string_de_conexão_com_banco_de_dados_MSSQLServer",
          "tableName": "webcrawler_logs",
          "autoCreateSqlTable": true
        }
      }
    ]
  },
  "MaxThreads": 3, //Caso queira mudar a quantidade de threads
  "AllowedHosts": "*"
}
```

</li>
</ol>

## Padrão de resposta da API

A API traz como resposta os dados da requisição e os endereços para download do arquivo json com os dados gerados e as páginas html consultadas, conforme o arquivo [response.json](response.json)

```
{
  "viewData": {
    "id": "01945053-a5c0-76fe-bbaa-35d826555895",
    "startDate": "2025-01-10T13:07:09.9681023+00:00",
    "endDate": "2025-01-10T13:07:24.8043452+00:00",
    "page": 5,
    "row": 82,
    "requestKey": "99e18870-80b1-47b9-bee8-1abcd5723fdb",
    "jsonFileAddress": "https://staelawpaygo.blob.core.windows.net/files/json-files/proxies_fd1fd5e9-527c-4af9-9b9e-66ad75b9664b.json",
    "pagesUrl": [
      {
        "fileUrl": "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/page/1",
        "fileContentAddress": "https://staelawpaygo.blob.core.windows.net/files/html-files/page_-57326983.html"
      },
      {
        "fileUrl": "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/page/2",
        "fileContentAddress": "https://staelawpaygo.blob.core.windows.net/files/html-files/page_-1275616698.html"
      },
      {
        "fileUrl": "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/page/3",
        "fileContentAddress": "https://staelawpaygo.blob.core.windows.net/files/html-files/page_1857298309.html"
      },
      {
        "fileUrl": "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/page/4",
        "fileContentAddress": "https://staelawpaygo.blob.core.windows.net/files/html-files/page_-1134781680.html"
      },
      {
        "fileUrl": "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/page/5",
        "fileContentAddress": "https://staelawpaygo.blob.core.windows.net/files/html-files/page_-872532113.html"
      }
    ]
  },
  "messages": []
}
```

Estruturalmente, o objeto retornado pela API possui duas propriedades: **viewData** e **messages**. 

**ViewData** é um objeto genérico que traz o conteúdo da requisição em caso de sucesso. 

Já a propriedade **messages** traz um array com objeto de erro que possui as propriedades **message** (string) com o texto do erro na requisição e **type** (string) com o tipo do erro, podendo ser: **INFORMATION**, **WARNING**, **ERROR** e **CRITICAL_ERROR**

## Pacotes utilizados no projeto

```
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0"/>
<PackageReference Include="Serilog.AspNetCore" Version="9.0.0" />
<PackageReference Include="Serilog.Settings.Configuration" Version="9.0.0" />
<PackageReference Include="Serilog.Sinks.MSSqlServer" Version="8.1.0" />
<PackageReference Include="HtmlAgilityPack" Version="1.11.72" />
<PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
<PackageReference Include="Azure.Storage.Blobs" Version="12.23.0" />
```

## API Publicada

A API está publicada no endereço [API](https://elaw-webcrawler-api.azurewebsites.net) e requisições podem ser feitas conforme o modelo abaixo:

```
@HostAzure=elaw-webcrawler-api.azurewebsites.net
@placeholder=https://proxyservers.pro/proxy/list/order/updated/order_dir/desc

###
GET {{HostAzure}}/api/WebCrawler?url={{placeholder}}
Accept: application/json
```

**Observação:** Eventualmente, a API pode estar desativada por razões de gerenciamento de custo no Azure. Se precisar que o Web App seja ativado, favor entar em contato.

## GitHub Actions

O repositório do projeto está configurado para que os commits realizados na branch principal iniciem automaticamente o processo de build e publicação do projeto no Azure.

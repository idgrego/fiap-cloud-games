# fiap-cloud-games
Projeto do curso "Arquitetura de Sistemas .Net" da FIAP

## Roteiro para Criação do SQL Database
O SQL Database é o banco de dados no Azure.

1. Criar uma conta no [Azure](https://azure.microsoft.com/pt-br) caso ainda não tenha.
2. Ative sua assinatura. Busque por **Assinaturas** (_Subscriptions_) no menu lateral. Clicar em Adicionar e seguir as instruções.
3. Agora é necessário criar um Grupo de Recursos (_Resource Group_). No menu lateral busque pela opção **Grupo de Recursos**. Clique em Criar e sigas as instruções.
4. Antes de criar o banco de dados é necessário criar um Servidor SQL Lógico (_SQL Logic Server_). No menu lateral procure por "Banco de dados SQL do Azure". Ao clicar nessa opção teremos o submenu "**Azure SQL Database > SQL Logical Servers**". Clique em criar e siga as instruções.
5. Por fim, ainda no menu "Banco de dados SQL do Azure", selecione o submenu "**Azure SQL Database > SQL Databases**". Clique em criar e siga as instruções.

Todos os recursos criados, agora basta liberar o firewall da rede virtual.
No Portal do Azure e vá até o seu Servidor SQL (o recurso de nível de servidor, e não o banco de dados isolado).  No menu lateral esquerdo, clique em Rede (Firewalls and virtual networks).Na seção de regras de firewall (Firewall rules), você criará uma regra nova com os seguintes dados:  

* Nome da regra (Rule Name): PermitirTudo (ou qualquer nome de sua preferência).  
* IP Inicial (Start IP):   0.0.0.0
* IP Final (End IP): 255.255.255.255

Clique no botão Salvar (Save) na parte superior da tela.

A string de conexão está disponível no formulário de detalhes do banco de dados. Dentro desse formulário procure por "Cadeias de conexão" e escolha a que lhe atende.

## dotnet CLI
Após instalar o SDK do .NET você terá acesso ao CLI.

Segue alguns comandos:

* __dotnet --info__: detalha quais runtimes, SDKs estão instalados
* __dotnet --help__: mostra a ajuda
    * __dotnet new --help__: mostra a ajuda do comando *new*
    * __dotnet new create --help__: mostra a ajuda do comando *create*
    * __dotnet add package --help__: mostra a ajuda do comando *add package*
    * __dotnet package search --help__: mostra a ajuda do comando *package search*
* __dotnet new list__: lista todos os tipos de projetos instalados
    * __dotnet new list *termo*__: lista todos os tipos de projetos instalados que contenham o *termo* no nome.
* __dotnet new create *short name*__: cria o projeto.
    * __dotnet new create webapi__: cria um projeto do tipo _Aplicativo Web API do ASP.NET Core_.
* __dotnet package search *termo*__: lista todos os pacotes que incluem o *termo* no nome. Se *termo* não for informado retorna tudo.
    * __dotnet package search__: lista todos os pacotes.
    * __dotnet package search Entity__: lista todos os pacotes com o termo 'Entity' no nome
* __dotnet add package *Package ID*__: adiciona ao projeto o novo pacote
    * Autenticação via JWT Bearer
    __dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer__
    * Suporte a OpenAPI no .NET 10
    __dotnet add package Microsoft.AspNetCore.OpenApi__
    * Interface do Scalar para documentação interativa
    __dotnet add package Scalar.AspNetCore__
    * Pacote principal do EF Core para SQL Server
    __dotnet add package Microsoft.EntityFrameworkCore.SqlServer__
    * Ferramentas de suporte a Migrations no projeto
    __dotnet add package Microsoft.EntityFrameworkCore.Tools__
    * Suporte ao design-time do EF Core (geração de código de migration)
    __dotnet add package Microsoft.EntityFrameworkCore.Design__
* __dotnet build__: compila o projeto atual
* __dotnet run__: compila e executa o projeto
* __dotnet watch__: executa com recarregamento automático (Hot Reload) e abertura de navegador no Scalar UI (`/scalar/v1`)

## Migrations
Depois que as entidades (classes) estão criadas, string de conexão definida no appsettings e o contexto configurado podemos realizar a migração.

Começe instalando as ferramentas do EF conforme instruções a seguir:

* A instrução a seguir instala a ferramenta do EF Core globalmente
__dotnet tool install --global dotnet-ef__
* A instrução a seguir permite atualizar a ferramenta dotnet-ef
__dotnet tool update --global dotnet-ef__

Depois, abra o terminal, acesse a pasta fase-01 e rode os seguintes comandos:

1. Criar a migração inicial

__dotnet ef migrations add InitialCreate -o infrastructure/data/migrations__

2. Excluir a migração

__dotnet ef migrations remove__

3. Aplica a migração no SQL Database

__dotnet ef database update__

#########################

Assinatura: 180387a9-5aa7-4859-a1bc-bb4c7b26ab5e

server: fcg-server.database.windows.net
user: fcg-sa
pwd: f1@p-cluod-gam3s

Server=tcp:fcg-server.database.windows.net,1433;Initial Catalog=fcg-db;Persist Security Info=False;User ID=fcg-sa;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
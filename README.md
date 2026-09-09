````markdown
# Projeto Arquitetura .NET

Template profissional para desenvolvimento de aplicações completas com **backend .NET e frontend Blazor**, criado com foco em organização, escalabilidade, segurança e reutilização.

Este projeto foi desenvolvido durante um curso prático de arquitetura .NET, no qual são aplicados conceitos utilizados em sistemas reais.

## 🎯 Objetivo

Construir uma base sólida e reutilizável para novos projetos, aplicando boas práticas de arquitetura, separação de responsabilidades, testes automatizados e integração entre frontend e backend.

## 🚀 Principais recursos

- API REST estruturada em camadas
- Frontend desenvolvido com Blazor
- Controllers, Services e Repositories
- DTOs e entidades
- Entity Framework Core e SQL Server
- Migrations, transações e controle de concorrência
- Injeção de dependência com Scrutor
- Tratamento global de exceções
- Validações com DataAnnotations
- Autenticação com Auth0 e JWT
- Controle de permissões por usuário
- Documentação da API com Swagger
- Logs estruturados com Serilog
- Cache distribuído e HybridCache
- Segurança com CORS, HTTPS, headers e rate limiting
- Envio de e-mails com SendGrid
- Integração com IA generativa
- Jobs em background com Hangfire
- Internacionalização
- Testes automatizados com Moq, Shouldly e Bogus
- Componentes reutilizáveis com Radzen
- Consumo de APIs com Refit

## 🏗️ Arquitetura

A solução é organizada para separar responsabilidades e facilitar a evolução do sistema:

- **Backend** — API, regras de negócio, persistência e autenticação.
- **Frontend** — aplicação Blazor, páginas, componentes e integração com a API.
- **Exceptions** — códigos e tratamento padronizado de exceções.
- **Services** — implementação das regras de negócio.
- **Repositories** — acesso e persistência de dados.
- **Shared** — recursos e componentes compartilhados.

## 🛠️ Tecnologias

- C#
- .NET
- ASP.NET Core
- Entity Framework Core
- SQL Server
- Blazor
- Radzen
- Auth0
- JWT
- Swagger
- Serilog
- Hangfire
- SendGrid
- Refit
- Mapster
- Scrutor
- Moq
- Shouldly
- Bogus
- Git e GitHub

## 📋 Pré-requisitos

- .NET SDK
- SQL Server Express
- Git
- Visual Studio Code ou Visual Studio
- Conta no Auth0, caso utilize autenticação
- Configurações dos serviços externos utilizados no projeto

## ▶️ Como executar

Clone o repositório:

```bash
git clone https://github.com/sabrinabisol/projeto-arquitetura-dotnet-backend-frontend.git
cd projeto-arquitetura-dotnet-backend-frontend
```

Restaure as dependências e compile a solução:

```bash
dotnet restore
dotnet build
```

Configure as credenciais e conexões necessárias nos arquivos de configuração do ambiente.

Execute a aplicação:

```bash
dotnet run
```

## 🧪 Testes

Para executar os testes automatizados:

```bash
dotnet test
```

## 📌 Status do projeto

Em desenvolvimento, com novos recursos sendo adicionados conforme a evolução do curso.

## 📚 Objetivos de aprendizado

Este projeto permite praticar:

- Arquitetura de software aplicada a sistemas reais
- Desenvolvimento de APIs robustas
- Organização de soluções .NET
- Separação de responsabilidades
- Autenticação e autorização
- Persistência de dados
- Testes automatizados
- Boas práticas de segurança
- Desenvolvimento frontend com Blazor
- Criação de templates reutilizáveis

## 👩‍💻 Autora

**Sabrina Bisol**

GitHub: [@sabrinabisol](https://github.com/sabrinabisol)
````
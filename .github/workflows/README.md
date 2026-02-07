# GitHub Actions Workflows

Este diretório contém os workflows de CI/CD para o projeto BindMapper.

## Workflows Disponíveis

### 1. CI - Build and Test (`ci.yml`)

**Trigger:** Push ou Pull Request na branch `main`

**Funcionalidades:**
- ✅ Build da solução em múltiplas versões do .NET (6.0, 8.0, 9.0)
- ✅ Execução de todos os testes unitários
- ✅ Validação de qualidade de código
- ✅ Validação do pacote NuGet
- ✅ Upload de resultados de testes como artefatos

**Matrix Strategy:**
O workflow executa em paralelo para cada versão do .NET, garantindo compatibilidade.

### 2. CD - Publish to NuGet (`publish-nuget.yml`)

**Trigger:** 
- Tag com formato `v*.*.*` (ex: `v1.1.3`)
- Manualmente via workflow_dispatch

**Funcionalidades:**
- ✅ Build da solução em modo Release
- ✅ Execução de testes antes da publicação
- ✅ Criação do pacote NuGet com a versão da tag
- ✅ Publicação automática no NuGet.org
- ✅ Criação de GitHub Release
- ✅ Upload do pacote como artefato

## Configuração Necessária

### Secrets do GitHub

Para usar os workflows, você precisa configurar os seguintes secrets no repositório:

1. **NUGET_API_KEY**
   - Vá em: https://www.nuget.org/account/apikeys
   - Crie uma nova API Key
   - No GitHub: Settings → Secrets and variables → Actions → New repository secret
   - Nome: `NUGET_API_KEY`
   - Valor: Sua API key do NuGet

## Como Usar

### Desenvolvimento Normal (CI)

O workflow de CI é executado automaticamente em cada push ou PR:

```bash
git add .
git commit -m "feat: nova funcionalidade"
git push origin main
```

O GitHub Actions irá:
1. Fazer build em todas as versões do .NET
2. Executar os testes
3. Validar o pacote

### Publicar Nova Versão (CD)

#### Método 1: Via Tag (Recomendado)

```bash
# Certifique-se de que está na branch main
git checkout main
git pull origin main

# Crie e push a tag com a versão
git tag v1.1.3
git push origin v1.1.3
```

#### Método 2: Manual via GitHub UI

1. Vá em: Actions → CD - Publish to NuGet
2. Clique em "Run workflow"
3. Digite a versão (ex: `1.1.3`)
4. Clique em "Run workflow"

## Estrutura de Versioning

O projeto usa **Semantic Versioning** (SemVer):

- **MAJOR.MINOR.PATCH** (ex: `1.1.3`)
- **MAJOR**: Mudanças incompatíveis
- **MINOR**: Nova funcionalidade compatível
- **PATCH**: Bug fixes

Exemplos de tags:
- `v1.0.0` - Release estável
- `v1.1.0` - Nova funcionalidade
- `v1.1.1` - Bug fix
- `v2.0.0-beta.1` - Pre-release (marcado como prerelease no GitHub)

## Monitoramento

### Ver Status dos Workflows

- https://github.com/djesusnet/BindMapper/actions

### Badges para README

Adicione ao seu README.md:

```markdown
[![CI](https://github.com/djesusnet/BindMapper/actions/workflows/ci.yml/badge.svg)](https://github.com/djesusnet/BindMapper/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/BindMapper.svg)](https://www.nuget.org/packages/BindMapper/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/BindMapper.svg)](https://www.nuget.org/packages/BindMapper/)
```

## Troubleshooting

### Erro: "NUGET_API_KEY secret not found"
- Verifique se o secret está configurado corretamente no GitHub
- Nome deve ser exatamente: `NUGET_API_KEY`

### Erro: "Package already exists"
- Você está tentando publicar uma versão que já existe
- Incremente a versão e crie uma nova tag

### Testes falhando
- O workflow de publicação só publica se os testes passarem
- Corrija os testes antes de tentar publicar novamente

### Build falhando em versão específica do .NET
- Verifique compatibilidade do código com aquela versão
- Considere remover a versão do target frameworks se não for suportada

## Melhores Práticas

1. **Sempre teste localmente antes de fazer push:**
   ```bash
   dotnet build
   dotnet test
   dotnet pack
   ```

2. **Use branches para features:**
   - Desenvolva em `feature/nome-da-feature`
   - Faça PR para `main`
   - Use tags para releases

3. **Atualize o CHANGELOG.md:**
   - Documente todas as mudanças antes de criar a tag

4. **Versioning:**
   - Sempre incremente a versão apropriadamente
   - Siga o Semantic Versioning

5. **Tags devem sempre começar com 'v':**
   - ✅ `v1.1.3`
   - ❌ `1.1.3`

# Configuração de CI/CD - BindMapper

Este documento resume toda a configuração de CI/CD implementada para o projeto BindMapper.

## 📋 Índice

- [Arquivos Criados](#arquivos-criados)
- [Workflows](#workflows)
- [Configuração Necessária](#configuração-necessária)
- [Como Usar](#como-usar)
- [Troubleshooting](#troubleshooting)

## 📁 Arquivos Criados

### Workflows GitHub Actions

```
.github/
├── workflows/
│   ├── ci.yml                    # CI: Build e testes automáticos
│   ├── publish-nuget.yml         # CD: Publicação no NuGet
│   └── README.md                 # Documentação dos workflows
├── ISSUE_TEMPLATE/
│   ├── bug_report.yml           # Template para bugs
│   ├── feature_request.yml      # Template para features
│   ├── documentation.yml        # Template para docs
│   └── config.yml               # Configuração de issues
├── pull_request_template.md     # Template para PRs
├── dependabot.yml               # Atualização automática de dependências
└── RELEASE_GUIDE.md             # Guia de release
```

## 🔄 Workflows

### 1. CI - Build and Test (`ci.yml`)

**Trigger:**
- Push para `main`
- Pull requests para `main`
- Manual (workflow_dispatch)

**Jobs:**
1. **build-and-test**: Build e testes em múltiplas versões do .NET
   - Matrix: .NET 6.0, 8.0, 9.0
   - Executa em paralelo
   - Upload de resultados de teste

2. **code-quality**: Validação de qualidade de código
   - Executa analyzers
   - Verifica style guide

3. **package-validation**: Validação do pacote NuGet
   - Cria o pacote
   - Upload como artefato

**Tempo estimado:** 5-8 minutos

### 2. CD - Publish to NuGet (`publish-nuget.yml`)

**Trigger:**
- Tag com formato `v*.*.*` (ex: `v1.1.3`)
- Manual (workflow_dispatch)

**Steps:**
1. ✅ Extrai versão da tag
2. ✅ Atualiza `Directory.Build.props`
3. ✅ Build da solução
4. ✅ Executa testes
5. ✅ Cria pacote NuGet
6. ✅ Publica no NuGet.org
7. ✅ Cria GitHub Release
8. ✅ Upload do artefato

**Tempo estimado:** 3-5 minutos

## ⚙️ Configuração Necessária

### 1. Secret do NuGet (OBRIGATÓRIO)

Para publicar pacotes no NuGet, você precisa configurar a API Key:

**Passo a passo:**

1. **Criar API Key no NuGet:**
   - Acesse: https://www.nuget.org/account/apikeys
   - Clique em "Create"
   - Preencha:
     - Key Name: `GitHub Actions - BindMapper`
     - Glob Pattern: `BindMapper`
     - Select Scopes: `Push new packages and package versions`
   - Clique em "Create"
   - **COPIE A KEY** (você só verá uma vez!)

2. **Adicionar Secret no GitHub:**
   - Vá para: https://github.com/djesusnet/BindMapper/settings/secrets/actions
   - Clique em "New repository secret"
   - Name: `NUGET_API_KEY`
   - Secret: Cole a API Key copiada do NuGet
   - Clique em "Add secret"

### 2. Branches Protegidas (Recomendado)

Configure proteções para a branch `main`:

1. Vá para: Settings → Branches → Add branch protection rule
2. Branch name pattern: `main`
3. Habilite:
   - ✅ Require a pull request before merging
   - ✅ Require status checks to pass before merging
     - Selecione: `build-and-test`, `code-quality`, `package-validation`
   - ✅ Require conversation resolution before merging
   - ✅ Do not allow bypassing the above settings

## 🚀 Como Usar

### Desenvolvimento Diário (CI)

O CI roda automaticamente em cada push ou PR:

```bash
# Desenvolva normalmente
git checkout -b feature/nova-funcionalidade
# ... faça suas alterações ...
git add .
git commit -m "feat: adiciona nova funcionalidade"
git push origin feature/nova-funcionalidade
# Crie PR para main
```

**O que acontece:**
1. ✅ GitHub Actions inicia o workflow de CI
2. ✅ Build em .NET 6.0, 8.0 e 9.0
3. ✅ Executa todos os testes
4. ✅ Valida qualidade do código
5. ✅ Valida criação do pacote

### Publicar Nova Versão (CD)

#### Método 1: Via Tag (Automático)

```bash
# 1. Certifique-se de estar na main atualizada
git checkout main
git pull origin main

# 2. Atualize o CHANGELOG.md (se necessário)
# ... edite o arquivo ...

# 3. Commit (se houver mudanças)
git add CHANGELOG.md
git commit -m "chore: prepare release v1.0.0"
git push origin main

# 4. Crie e push a tag
git tag v1.0.0
git push origin v1.0.0
```

**O que acontece:**
1. ✅ GitHub Actions detecta a tag
2. ✅ Extrai versão (1.0.0)
3. ✅ Build e testes
4. ✅ Cria pacote NuGet
5. ✅ Publica no NuGet.org
6. ✅ Cria GitHub Release

#### Método 2: Manual

1. Vá para: https://github.com/djesusnet/BindMapper/actions
2. Selecione "CD - Publish to NuGet"
3. Clique em "Run workflow"
4. Digite a versão (ex: `1.0.0`)
5. Clique em "Run workflow"

### Verificar Publicação

Após o workflow completar:

- **NuGet:** https://www.nuget.org/packages/BindMapper/
- **GitHub:** https://github.com/djesusnet/BindMapper/releases
- **Actions:** https://github.com/djesusnet/BindMapper/actions

## 📊 Badges

Adicione ao seu README (já adicionado):

```markdown
[![CI](https://github.com/djesusnet/BindMapper/actions/workflows/ci.yml/badge.svg)](https://github.com/djesusnet/BindMapper/actions/workflows/ci.yml)
[![CD](https://github.com/djesusnet/BindMapper/actions/workflows/publish-nuget.yml/badge.svg)](https://github.com/djesusnet/BindMapper/actions/workflows/publish-nuget.yml)
```

## 🐛 Troubleshooting

### Problema: "NUGET_API_KEY secret not found"

**Causa:** Secret não configurado no GitHub

**Solução:**
1. Siga as instruções em [Configuração Necessária](#configuração-necessária)
2. Verifique que o nome é exatamente `NUGET_API_KEY` (case-sensitive)

### Problema: "Package already exists"

**Causa:** Tentando publicar versão que já existe no NuGet

**Solução:**
```bash
# Delete a tag
git tag -d v1.0.0-preview
git push origin :refs/tags/v1.0.0-preview

# Incremente a versão
git tag v1.0.0
git push origin v1.0.0
```

### Problema: "Tests failed"

**Causa:** Testes falhando no CI

**Solução:**
```bash
# Execute localmente
dotnet test -c Release

# Veja os detalhes
dotnet test -c Release --logger "console;verbosity=detailed"

# Corrija os problemas
# Commit e push novamente
```

### Problema: Build falhando em .NET específico

**Causa:** Código incompatível com versão específica do .NET

**Solução:**
1. Teste localmente com a versão específica
2. Corrija o código para ser compatível
3. Ou remova a versão do `TargetFrameworks` se não for suportada

### Problema: Workflow não dispara

**Causa:** Branch protection ou permissões

**Solução:**
1. Verifique que o workflow está na branch correta
2. Verifique Settings → Actions → General
   - Workflow permissions deve ser "Read and write permissions"

## 📈 Melhorias Futuras

Considere adicionar no futuro:

- [ ] **Code Coverage:** Integração com Codecov ou Coveralls
- [ ] **Security Scanning:** Dependabot security updates
- [ ] **Performance Tests:** Benchmarks automáticos em PRs
- [ ] **Documentation:** Geração automática de docs
- [ ] **Release Notes:** Geração automática via conventional commits
- [ ] **Slack/Discord:** Notificações de build/release

## 📚 Recursos

- [GitHub Actions Docs](https://docs.github.com/en/actions)
- [NuGet Publishing](https://learn.microsoft.com/nuget/nuget-org/publish-a-package)
- [Semantic Versioning](https://semver.org/)
- [Conventional Commits](https://www.conventionalcommits.org/)

## 🤝 Contribuindo

Para contribuir com melhorias no CI/CD:

1. Fork o repositório
2. Crie uma branch para sua feature
3. Teste localmente se possível
4. Crie um PR descrevendo as mudanças
5. Aguarde review

## 📞 Suporte

Para problemas com CI/CD:
- Abra uma issue: https://github.com/djesusnet/BindMapper/issues
- Verifique issues existentes
- Inclua logs completos dos workflows

---

**Última atualização:** Fevereiro 2026
**Versão do documento:** 1.0.0

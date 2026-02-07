# Guia Rápido de Release

## Pré-requisitos

1. ✅ Configurar o secret `NUGET_API_KEY` no GitHub
   - Acesse: https://www.nuget.org/account/apikeys
   - Crie uma API Key com permissão "Push new packages and package versions"
   - No GitHub: Settings → Secrets and variables → Actions → New repository secret
   - Nome: `NUGET_API_KEY`
   - Cole a API key

## Processo de Release

### 1. Prepare o Release

```bash
# Certifique-se de estar atualizado com a branch main
git checkout main
git pull origin main

# Atualize o CHANGELOG.md com as mudanças da versão
# Edite manualmente ou use um editor
```

### 2. Defina a Versão

Edite o arquivo `Directory.Build.props` se quiser mudar a versão padrão:

```xml
<Version>1.1.3</Version>
```

### 3. Commit e Push (se necessário)

```bash
git add .
git commit -m "chore: prepare release v1.1.3"
git push origin main
```

### 4. Crie e Publique a Tag

```bash
# Crie a tag localmente
git tag v1.1.3

# Ou com mensagem anotada
git tag -a v1.1.3 -m "Release version 1.1.3"

# Push da tag para o GitHub (isso dispara o workflow de publicação)
git push origin v1.1.3
```

### 5. Acompanhe o Workflow

1. Vá para: https://github.com/djesusnet/BindMapper/actions
2. Você verá o workflow "CD - Publish to NuGet" em execução
3. Aguarde até que todos os steps sejam concluídos

### 6. Verifique a Publicação

- **NuGet:** https://www.nuget.org/packages/BindMapper/
- **GitHub Releases:** https://github.com/djesusnet/BindMapper/releases

## Publicação Manual (Alternativa)

Se preferir publicar manualmente sem tags:

1. Vá para: https://github.com/djesusnet/BindMapper/actions
2. Selecione "CD - Publish to NuGet"
3. Clique em "Run workflow"
4. Selecione a branch `main`
5. Digite a versão (ex: `1.1.3`)
6. Clique em "Run workflow"

## Rollback de Tag (Se necessário)

Se algo der errado e você precisar remover a tag:

```bash
# Remover tag local
git tag -d v1.1.3

# Remover tag remota
git push origin :refs/tags/v1.1.3
```

**Nota:** Se o pacote já foi publicado no NuGet, você não pode removê-lo. Você precisará publicar uma nova versão corrigida.

## Estratégia de Branches

### Desenvolvimento

```
feature/nova-funcionalidade → main → tag (v1.x.x)
```

1. Crie uma branch de feature:
   ```bash
   git checkout -b feature/nova-funcionalidade main
   ```

2. Desenvolva e teste:
   ```bash
   git add .
   git commit -m "feat: adiciona nova funcionalidade"
   git push origin feature/nova-funcionalidade
   ```

3. Crie um Pull Request para `main`

4. Após aprovação, merge para `main`

5. Quando pronto para release, crie a tag em `main`

### Hotfix

```
hotfix/bug-critico → main → tag (v1.x.x+1)
```

1. Crie uma branch de hotfix a partir da main:
   ```bash
   git checkout -b hotfix/bug-critico main
   ```

2. Corrija o bug e teste

3. Merge direto para `main`

4. Crie uma tag imediatamente

## Versioning (Semantic Versioning)

```
MAJOR.MINOR.PATCH
```

- **MAJOR (1.x.x)**: Mudanças incompatíveis com versões anteriores
  - Exemplo: Remoção de APIs públicas, mudanças de assinatura
  
- **MINOR (x.1.x)**: Nova funcionalidade compatível com versões anteriores
  - Exemplo: Novos métodos, novas features opcionais
  
- **PATCH (x.x.1)**: Correção de bugs compatível
  - Exemplo: Bug fixes, melhorias de performance

### Exemplos

```bash
# Bug fix
git tag v1.0.1

# Nova feature
git tag v1.1.0

# Breaking change
git tag v2.0.0

# Pre-release
git tag v2.0.0-beta.1
git tag v2.0.0-rc.1
```

## Checklist de Release

Antes de criar a tag, verifique:

- [ ] Todos os testes estão passando localmente
- [ ] CHANGELOG.md foi atualizado
- [ ] Versão foi incrementada corretamente
- [ ] Branch `main` está atualizada
- [ ] Código foi revisado
- [ ] Documentação está atualizada
- [ ] Breaking changes estão documentadas (se houver)

## Comandos Úteis

```bash
# Ver todas as tags
git tag -l

# Ver última tag
git describe --tags --abbrev=0

# Ver diferenças desde a última tag
git log $(git describe --tags --abbrev=0)..HEAD --oneline

# Criar release notes automaticamente
git log v1.1.2..v1.1.3 --pretty=format:"- %s" --reverse

# Verificar status do build local
dotnet build -c Release
dotnet test -c Release
dotnet pack -c Release -o ./artifacts
```

## Troubleshooting Comum

### "Package already exists on NuGet"

**Causa:** Você está tentando publicar uma versão que já existe

**Solução:**
```bash
# Delete a tag local e remota
git tag -d v1.0.0-preview
git push origin :refs/tags/v1.0.0-preview

# Incremente a versão e crie nova tag
git tag v1.0.0
git push origin v1.0.0
```

### "Tests failed"

**Causa:** Testes estão falhando no CI

**Solução:**
```bash
# Execute os testes localmente
dotnet test -c Release --verbosity detailed

# Corrija os problemas
# Faça commit e push
# Crie a tag novamente
```

### "NUGET_API_KEY not found"

**Causa:** Secret não está configurado no GitHub

**Solução:**
- Vá em Settings → Secrets and variables → Actions
- Adicione o secret `NUGET_API_KEY`
- Re-run o workflow falhado

## Contato

Para problemas com o processo de release, abra uma issue em:
https://github.com/djesusnet/BindMapper/issues

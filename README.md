<p align="center">
  <img src="https://raw.githubusercontent.com/djesusnet/VelocityMapper/main/assets/icon.png" alt="VelocityMapper Logo" width="120">
</p>

# VelocityMapper

**O mapper .NET mais rápido. Zero reflection. Zero overhead.**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/VelocityMapper.svg)](https://www.nuget.org/packages/VelocityMapper/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/VelocityMapper.svg)](https://www.nuget.org/packages/VelocityMapper/)
[![.NET](https://img.shields.io/badge/.NET-6%20%7C%208%20%7C%209%20%7C%2010-512BD4)](https://dotnet.microsoft.com/)

VelocityMapper usa **Source Generators** para gerar código de mapeamento otimizado em tempo de compilação. API familiar estilo AutoMapper, performance superior.

---

## 📦 Instalação

```bash
dotnet add package VelocityMapper
```

Frameworks suportados: .NET 6, 8, 9, 10

---

## 🚀 Quick Start

### 1. Crie seus models

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Address Address { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public AddressDto Address { get; set; }
}
```

### 2. Configure os mapeamentos

```csharp
using VelocityMapper;

public static class MappingConfig
{
    [MapperConfiguration]
    public static void Configure()
    {
        Velocity.CreateMap<User, UserDto>();
        Velocity.CreateMap<Address, AddressDto>();
    }
}
```

### 3. Use o mapper

```csharp
var user = new User { Id = 1, Name = "João", Email = "joao@email.com" };

// Criar nova instância
var dto = Velocity.Map<UserDto>(user);

// Ou com tipo inferido
UserDto dto2 = Velocity.Map(user);

// Zero allocation - mapear para objeto existente
var existingDto = new UserDto();
Velocity.Map(user, existingDto);
```

---

## 📚 API

### Mapeamento Básico

```csharp
// Nova instância (estilo AutoMapper)
var dto = Velocity.Map<UserDto>(user);

// Nova instância (tipo inferido - mais rápido)
UserDto dto = Velocity.Map(user);

// Para objeto existente (zero allocation)
Velocity.Map(user, existingDto);
```

### Mapeamento de Coleções

```csharp
// Lista
List<UserDto> dtos = Velocity.MapList(users);

// Array
UserDto[] array = Velocity.MapArray(usersArray);

// ICollection<T>
ICollection<User> users = GetUsers();
List<UserDto> dtos = CollectionMapper.MapToList(users, Velocity.Map<UserDto>);
UserDto[] array = CollectionMapper.MapToArray(users, Velocity.Map<UserDto>);

// IEnumerable<T> (detecta automaticamente List, Array ou ICollection)
List<UserDto> dtos = CollectionMapper.MapEnumerable(users, Velocity.Map<UserDto>);

// Span (máxima performance)
UserDto[] result = Velocity.MapSpan(usersSpan);

// Zero allocation com Span
Velocity.MapSpanTo(sourceSpan, destinationSpan);
```

---

## 🔄 Comportamento de Mapeamento

### Propriedades Extras são Ignoradas Automaticamente

O VelocityMapper mapeia baseado nas **propriedades do destino**. Propriedades que existem apenas na origem são automaticamente ignoradas:

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }  // Só existe na entidade
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    // PasswordHash não existe → ignorado automaticamente!
}

var dto = Velocity.Map<UserDto>(user);
// dto terá: Id, Name, Email
// PasswordHash é ignorado silenciosamente ✓
```

| Cenário | Comportamento |
|---------|---------------|
| Propriedade existe em ambos | ✅ Mapeia |
| Propriedade só na origem | ✅ Ignora silenciosamente |
| Propriedade só no destino | ✅ Mantém valor padrão |

### Atributos

```csharp
public class UserDto
{
    public int Id { get; set; }
    
    [MapFrom("FirstName")]  // Mapeia de propriedade com nome diferente
    public string Name { get; set; }
    
    [IgnoreMap]  // Ignora explicitamente (documentação)
    public string CacheKey { get; set; }
}
```

---

## 🏎️ Performance

Benchmark no .NET 10 (Intel Core i5-14600KF):

| Mapper | Tempo | Comparação |
|--------|-------|------------|
| **VelocityMapper** | **12.03 ns** | Mais rápido |
| Manual | 12.22 ns | baseline |
| Mapperly | 12.29 ns | 2% mais lento |
| Mapster | 18.91 ns | 57% mais lento |
| AutoMapper | 32.87 ns | 173% mais lento |

VelocityMapper é mais rápido que código escrito à mão.

### Por que classe estática?

A classe `Velocity` é estática por design para **máxima performance**:

| Aspecto | Classe Estática | Interface |
|---------|-----------------|-----------|
| Inlining JIT | ✅ Agressivo | ❌ Chamada virtual impede |
| Overhead | ~0 ns | ~2-3 ns por chamada |
| Testabilidade | ⚠️ Requer wrapper | ✅ Fácil mock |

Se precisar de DI/testabilidade, crie um wrapper:

```csharp
public interface IMapper
{
    TDestination Map<TDestination>(object source);
}

public class VelocityMapperWrapper : IMapper
{
    public TDestination Map<TDestination>(object source) 
        => Velocity.Map<TDestination>(source);
}

// DI
services.AddSingleton<IMapper, VelocityMapperWrapper>();
```

---

## 🔧 Como Funciona

O Source Generator analisa seu código em tempo de compilação e gera métodos otimizados:

```csharp
// Você escreve:
Velocity.CreateMap<User, UserDto>();

// O gerador cria:
public static UserDto Map(User source)
{
    return new UserDto
    {
        Id = source.Id,           // Value types primeiro (cache-friendly)
        Age = source.Age,
        Name = source.Name,       // Reference types depois
        Email = source.Email,
        Address = source.Address is { } addr ? Map(addr) : null
    };
}
```

---

## 📋 Referência Rápida

| Método | Uso | Allocation |
|--------|-----|------------|
| `Velocity.Map<TDest>(source)` | Nova instância | DTO size |
| `Velocity.Map(source)` | Nova instância (inferido) | DTO size |
| `Velocity.Map(source, dest)` | Objeto existente | 0 B |
| `Velocity.MapList(list)` | Lista → Lista | List + DTOs |
| `Velocity.MapArray(array)` | Array → Array | Array + DTOs |
| `CollectionMapper.MapToList(collection, mapper)` | ICollection → Lista | List + DTOs |
| `CollectionMapper.MapToArray(collection, mapper)` | ICollection → Array | Array + DTOs |
| `CollectionMapper.MapEnumerable(enumerable, mapper)` | IEnumerable → Lista | List + DTOs |
| `Velocity.MapSpanTo(src, dest)` | Span → Span | 0 B |

---

## 📄 Licença

MIT License - veja [LICENSE](LICENSE)


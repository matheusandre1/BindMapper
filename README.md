<p align="center">
  <img src="assets/icon.png" alt="VelocityMapper Logo" width="200">
</p>

# VelocityMapper

**O mapper .NET mais rápido. Zero reflection. Zero overhead.**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/VelocityMapper.svg)](https://www.nuget.org/packages/VelocityMapper/)
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
        Mapper.CreateMap<User, UserDto>();
        Mapper.CreateMap<Address, AddressDto>();
    }
}
```

### 3. Use o mapper

```csharp
var user = new User { Id = 1, Name = "João", Email = "joao@email.com" };

// Criar nova instância
var dto = Mapper.Map<UserDto>(user);

// Ou com tipo inferido
UserDto dto2 = Mapper.Map(user);

// Zero allocation - mapear para objeto existente
var existingDto = new UserDto();
Mapper.Map(user, existingDto);
```

---

## 📚 API

### Mapeamento Básico

```csharp
// Nova instância (estilo AutoMapper)
var dto = Mapper.Map<UserDto>(user);

// Nova instância (tipo inferido - mais rápido)
UserDto dto = Mapper.Map(user);

// Para objeto existente (zero allocation)
Mapper.Map(user, existingDto);
```

### Mapeamento de Coleções

```csharp
// Lista
List<UserDto> dtos = Mapper.MapList(users);

// Array
UserDto[] array = Mapper.MapArray(usersArray);

// Span (máxima performance)
UserDto[] result = Mapper.MapSpan(usersSpan);

// Zero allocation com Span
Mapper.MapSpanTo(sourceSpan, destinationSpan);
```

---

## ⚙️ Configuração Avançada

### Fluent API

```csharp
[MapperConfiguration]
public static void Configure()
{
    Mapper.CreateMap<User, UserDto>()
        .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.FirstName + " " + s.LastName))
        .ForMember(d => d.InternalCode, opt => opt.Ignore())
        .ReverseMap();
}
```

### Atributos

```csharp
public class UserDto
{
    public int Id { get; set; }
    
    [MapFrom("FirstName")]  // Mapeia de propriedade diferente
    public string Name { get; set; }
    
    [IgnoreMap]  // Ignora no mapeamento
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

---

## 🔧 Como Funciona

O Source Generator analisa seu código em tempo de compilação e gera métodos otimizados:

```csharp
// Você escreve:
Mapper.CreateMap<User, UserDto>();

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
| `Mapper.Map<TDest>(source)` | Nova instância | DTO size |
| `Mapper.Map(source)` | Nova instância (inferido) | DTO size |
| `Mapper.Map(source, dest)` | Objeto existente | 0 B |
| `Mapper.MapList(list)` | Lista → Lista | List + DTOs |
| `Mapper.MapArray(array)` | Array → Array | Array + DTOs |
| `Mapper.MapSpanTo(src, dest)` | Span → Span | 0 B |

---

## 📄 Licença

MIT License - veja [LICENSE](LICENSE)


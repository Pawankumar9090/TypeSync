# Contributing to TypeSync

First off, thank you for considering contributing to TypeSync! 🎉

This document provides guidelines and steps for contributing. Following these guidelines helps communicate that you respect the time of the developers managing and developing this open source project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [How to Contribute](#how-to-contribute)
  - [Reporting Bugs](#reporting-bugs)
  - [Suggesting Features](#suggesting-features)
  - [Pull Requests](#pull-requests)
- [Development Setup](#development-setup)
- [Coding Standards](#coding-standards)
- [Commit Messages](#commit-messages)
- [Branch Naming](#branch-naming)

## Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

## Getting Started

1. Fork the repository on GitHub
2. Clone your fork locally
3. Set up the development environment (see [Development Setup](#development-setup))
4. Create a branch for your changes
5. Make your changes
6. Push your branch and submit a pull request

## How to Contribute

### Reporting Bugs

Before creating a bug report, please check existing issues to avoid duplicates.

When creating a bug report, include:
- A clear and descriptive title
- Steps to reproduce the issue
- Expected behavior vs actual behavior
- Your environment (.NET version, OS, etc.)
- Code samples if applicable

### Suggesting Features

Feature suggestions are welcome! Please:
- Use a clear and descriptive title
- Provide a detailed description of the proposed feature
- Explain why this feature would be useful
- Include code examples if applicable

### Pull Requests

1. **Update your fork**: Make sure your fork is up to date with the main repository
2. **Create a branch**: Create a branch with a descriptive name (see [Branch Naming](#branch-naming))
3. **Make changes**: Implement your changes following our [Coding Standards](#coding-standards)
4. **Write tests**: Add or update tests as necessary
5. **Update documentation**: Update the README or other docs if needed
6. **Commit**: Use meaningful commit messages (see [Commit Messages](#commit-messages))
7. **Push**: Push your branch to your fork
8. **Submit PR**: Create a pull request with a clear description

## Development Setup

### Prerequisites

- .NET 10 SDK
- Visual Studio 2022 or VS Code with C# extension
- Git

### Building the Project

```bash
# Clone the repository
git clone https://github.com/Pawankumar9090/TypeSync.git
cd TypeSync

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Coding Standards

- Follow C# naming conventions
- Use meaningful variable and method names
- Keep methods small and focused
- Write XML documentation for public APIs
- Use nullable reference types appropriately
- Follow SOLID principles

### Code Style

- Use 4 spaces for indentation (no tabs)
- Use `var` when the type is obvious
- Place opening braces on the same line
- Keep lines under 120 characters when possible

## Commit Messages

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

### Examples

```
feat(mapping): add support for async mapping

fix(flattening): resolve null reference when nested property is null

docs(readme): update installation instructions
```

## Branch Naming

Use descriptive branch names following this pattern:

| Type | Pattern | Example |
|------|---------|---------|
| Feature | `feature/<description>` | `feature/async-mapping` |
| Bug fix | `fix/<description>` | `fix/null-reference` |
| Documentation | `docs/<description>` | `docs/api-reference` |
| Refactor | `refactor/<description>` | `refactor/internal-cache` |

## Questions?

Feel free to open an issue with your question or reach out to the maintainers.

Thank you for contributing! 💜

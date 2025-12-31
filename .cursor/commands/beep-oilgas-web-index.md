# Beep.OilandGas.Web Documentation Index

## Overview

This is the main navigation index for all Beep.OilandGas.Web documentation. The documentation is organized by topic to help you find the information you need.

## Documentation Structure

### Architecture and Design

- **[Architecture](beep-oilgas-web-architecture.md)** - System architecture, component relationships, and design decisions
  - System overview
  - Component architecture
  - Data flow
  - Service registration
  - Design patterns

### Integration and Services

- **[API Integration](beep-oilgas-web-api-integration.md)** - API integration patterns, ApiClient usage, and service layer patterns
  - ApiClient service
  - Service layer pattern
  - API endpoint patterns
  - Error handling
  - Progress tracking
  - Caching strategies

- **[Services](beep-oilgas-web-services.md)** - Service layer architecture and patterns
  - Core services
  - Service patterns
  - Service lifecycle
  - Creating new services

### Functional Areas

- **[Lifecycle Management](beep-oilgas-web-lifecycle.md)** - Lifecycle management UI implementation and integration
  - Exploration phase
  - Development phase
  - Production phase
  - Decommissioning phase
  - Field orchestrator integration
  - Process management
  - Work order management

- **[Data Management](beep-oilgas-web-datamanagement.md)** - Data management UI implementation and operations
  - DataManagementService
  - Quality dashboard
  - Validation
  - Versioning
  - Audit trail
  - Import/export operations

- **[Connection Management](beep-oilgas-web-connection-management.md)** - Connection management, IDMEEditor integration, and first-login flow
  - API endpoints
  - Connection service
  - First-login wizard
  - Database creation
  - Connection switching

### User Interface

- **[UI Standards](beep-oilgas-web-ui-standards.md)** - Oil & Gas UI/UX standards, components, and best practices
  - Design principles
  - Dashboard standards
  - Data display standards
  - Visualization standards
  - Compliance and safety
  - Color palette
  - Typography
  - Responsive design

- **[Components](beep-oilgas-web-components.md)** - Reusable component library and usage
  - Layout components
  - Data components
  - Dialog components
  - Progress components
  - Connection components
  - Component patterns

- **[Pages](beep-oilgas-web-pages.md)** - Page structure, routing, and organization
  - Page organization
  - Routing
  - Authorization
  - Page patterns

### Security and Configuration

- **[Authentication](beep-oilgas-web-authentication.md)** - Authentication and authorization implementation
  - OIDC configuration
  - Authentication flow
  - Authorization
  - Role-based access
  - API authentication

- **[Theming](beep-oilgas-web-theming.md)** - Theme system and branding
  - Theme architecture
  - Theme configuration
  - Branding
  - Dark mode
  - Customization

## Quick Reference

### Getting Started

1. **New to the project?** Start with [Architecture](beep-oilgas-web-architecture.md)
2. **Working with API?** See [API Integration](beep-oilgas-web-api-integration.md)
3. **Creating components?** Check [Components](beep-oilgas-web-components.md) and [UI Standards](beep-oilgas-web-ui-standards.md)
4. **Adding features?** Review [Services](beep-oilgas-web-services.md) and [Pages](beep-oilgas-web-pages.md)

### Common Tasks

#### Setting Up Connection

1. Read [Connection Management](beep-oilgas-web-connection-management.md)
2. Implement first-login wizard
3. Configure API endpoints

#### Creating a New Page

1. Review [Pages](beep-oilgas-web-pages.md) documentation
2. Check [UI Standards](beep-oilgas-web-ui-standards.md) for design guidelines
3. Use [Components](beep-oilgas-web-components.md) for reusable UI elements
4. Integrate with [Services](beep-oilgas-web-services.md) for data operations

#### Integrating with API

1. Understand [API Integration](beep-oilgas-web-api-integration.md) patterns
2. Create service using [Services](beep-oilgas-web-services.md) patterns
3. Use ApiClient for HTTP communication
4. Handle errors and progress tracking

#### Adding Authentication

1. Review [Authentication](beep-oilgas-web-authentication.md) documentation
2. Configure OIDC in Program.cs
3. Add authorization attributes to pages
4. Implement role-based access

## Documentation Updates

This documentation is maintained as part of the project. When making significant changes:

1. Update relevant documentation files
2. Update this index if new files are added
3. Keep examples and code snippets current

## Related Documentation

### External Documentation

- [MudBlazor Documentation](https://mudblazor.com/)
- [Blazor Server Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [OpenID Connect Documentation](https://openid.net/connect/)

### Internal Documentation

- API Service documentation
- LifeCycle project documentation
- ProductionAccounting documentation
- PPDM39 DataManagement documentation


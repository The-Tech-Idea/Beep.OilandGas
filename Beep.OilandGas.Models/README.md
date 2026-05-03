# Beep.OilandGas.Models

Shared **domain and contract** types for the Beep.OilandGas solution: PPDM-aligned entity models, feature projections, and core service interfaces used by the API, Blazor web app, and domain libraries.

## Contents

- **Data** – `ModelEntityBase` table shapes, permit/lifecycle/economic projections, and cross-cutting model types.
- **Core.Interfaces** – service abstractions consumed by `Beep.OilandGas.ApiService` and feature projects.

## Dependencies

References `Beep.OilandGas.PPDM.Models`, `TheTechIdea.Beep.DataManagementModels`, and SkiaSharp packages for imaging helpers used by select models.

## Usage

Add a project reference or NuGet package reference to this library from domain features that must share types with the API layer. Prefer **one** shared model per concern rather than duplicating DTOs across projects.

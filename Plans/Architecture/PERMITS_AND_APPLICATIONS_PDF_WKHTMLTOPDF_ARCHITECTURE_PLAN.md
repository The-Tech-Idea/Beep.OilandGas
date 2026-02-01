# Beep.OilandGas PermitsAndApplications.Pdf.Wkhtmltopdf - Architecture Plan

## Executive Summary

**Goal**: Provide a reliable PDF generation utility for permit and application documents using wkhtmltopdf.

**Key Principle**: PDF generation is **deterministic, template-driven, and auditable**.

**Scope**: HTML template rendering and PDF export for regulatory packages.

---

## Architecture Principles

### 1) Template-Driven Output
- HTML templates define permit package layouts.
- Data binding uses strongly typed view models.

### 2) Deterministic Rendering
- Pin wkhtmltopdf version and options.
- Store render settings with outputs.

### 3) Integration
- Used by PermitsAndApplications services for document generation.

---

## Target Project Structure

```
Beep.OilandGas.PermitsAndApplications.Pdf.Wkhtmltopdf/
├── Services/
│   ├── PdfRenderService.cs
│   └── TemplateService.cs
├── Templates/
│   ├── PermitPackage.html
│   └── ApplicationSummary.html
├── Configuration/
│   └── WkhtmltopdfOptions.cs
└── Exceptions/
    └── PdfRenderException.cs
```

---

## Implementation Phases

### Phase 1: Template Framework (Week 1)
- Define base HTML templates and data bindings.

### Phase 2: Rendering Service (Week 2)
- Implement wkhtmltopdf integration and error handling.

### Phase 3: Audit + Storage (Week 3)
- Store generated PDFs with metadata and versioning.

---

## Best Practices Embedded

- **Deterministic output**: pin render options.
- **Template reuse**: standardize layouts across permits.
- **Auditability**: store render settings and source data.

---

## Success Criteria

- PDF outputs are consistent and reproducible.
- Permit packages include auditable source data.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)

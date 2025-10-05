# PageStudio - Architecture Plan

## Overview
PageStudio is a cross-platform desktop publishing application similar to Affinity Publisher, built with .NET 9.0 for multi-platform support (Windows, macOS, Linux, x86, ARM).

## Recommended Graphics Libraries

### Primary Graphics Engine
- **SkiaSharp** - Cross-platform 2D graphics API
  - Excellent cross-platform support (Windows, macOS, Linux, ARM, x86)
  - High-performance rendering
  - Vector and raster graphics support
  - Text rendering with advanced typography
  - GPU acceleration support

### Additional Graphics Libraries
- **ImageSharp** - For image processing and manipulation

## Project Structure

```
PageStudio.sln/
├── PageStudio.Core/                    # Backend logic (cross-platform)
├── PageStudio.Desktop/                 # MAUI desktop app
├── PageStudio.Web/                     # Web frontend (Blazor)
├── PageStudio.Web.Client/              # Webassembly frontend (Blazor)
```

## Backend/Frontend Separation

### PageStudio.Core (Backend)
- **Document Management**: Page layout, text handling, image management
- **Graphics Engine**: Abstraction layer for rendering operations
- **Export Services**: PDF export, PNG/JPG image export
- **File I/O**: Document serialization/deserialization
- **Plugin System**: Extensibility for filters, effects, tools

### Frontend Options
1. **PageStudio.Desktop (MAUI)**: Native desktop experience
2. **PageStudio.Web (Blazor Server and WebAssembly)**: Web-based editor

## Key Technologies

### Graphics and Rendering
- **SkiaSharp**: Primary graphics engine
- **SkiaSharp.Views.Maui**: MAUI integration
- **SkiaSharp.Views.Blazor**: Blazor webassembly integration

### Export Capabilities
- **PdfSharpCore**: PDF generation
- **SkiaSharp**: PNG/JPG export
- **ImageSharp**: Image format conversion and optimization

## Implementation Phases

### Phase 1: Core Foundation
1. Set up project structure with all projects
2. Implement basic document model
3. Create SkiaSharp rendering pipeline
4. Basic shapes and text rendering

### Phase 2: Web Frontend
1. Blazor Server web application
2. Canvas rendering in browser
3. Real-time collaboration features
4. Cloud document storage

### Phase 3: Export Functionality
1. PDF export with vector graphics
2. PNG/JPG raster export
3. Print functionality
4. Export quality settings

### Phase 4: Desktop Application
1. MAUI desktop application with canvas
2. Basic tools (selection, text, shapes)
3. Property panels and toolbars
4. File operations (save/load)

### Phase 5: Advanced Features
1. Advanced text formatting
2. Image handling and effects
3. Templates and master pages

## Performance Considerations

### Graphics Performance
- Use GPU acceleration where available
- Implement viewport culling for large documents
- Cache rendered elements
- Optimize redraw regions

### Memory Management
- Implement object pooling for frequent allocations
- Use weak references for large resources
- Proper disposal of graphics resources
- Lazy loading for document assets

## Cross-Platform Compatibility

### File Format Support
- Native PageStudio format (JSON-based)
- Import: PDF, images (PNG, JPG, SVG)
- Export: PDF, PNG, JPG, SVG
- Future: InDesign, Publisher, PowerPoint format support

## Development Guidelines

### Architecture Patterns
- **MVVM**: For UI separation and testability
- **Dependency Injection**: For service management
- **Command Pattern**: For undoable operations
- **Observer Pattern**: For document change notifications

### Code Organization
- Separate platform-specific code using conditional compilation
- Use interfaces for all major services
- Implement comprehensive unit testing
- Follow .NET coding standards and conventions
# PageStudio - Architecture Plan

## Overview
PageStudio is a cross-platform desktop publishing application similar to Affinity Publisher, built with .NET 9.0 and MAUI for multi-platform support (Windows, macOS, Linux, x86, ARM).

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
- **PdfSharpCore** - For PDF generation and manipulation
- **System.Drawing.Common** - For basic image operations (fallback)

## Project Structure

```
PageStudio/
├── PageStudio.sln
├── PageStudio.Core/                    # Backend logic (cross-platform)
│   ├── Models/                         # Document models, pages, elements
│   ├── Services/                       # Business logic services
│   ├── Interfaces/                     # Abstractions and contracts
│   ├── Graphics/                       # Graphics abstractions
│   └── Export/                         # PDF and image export
├── PageStudio.Desktop/                 # MAUI desktop app
│   ├── Platforms/                      # Platform-specific code
│   ├── Views/                          # UI views and pages
│   ├── ViewModels/                     # MVVM view models
│   └── Controls/                       # Custom controls
├── PageStudio.Web/                     # Web frontend (Blazor Server)
│   ├── Components/                     # Blazor components
│   ├── Pages/                          # Web pages
│   └── wwwroot/                        # Static assets
├── PageStudio.Api/                     # Web API for web frontend
│   ├── Controllers/                    # API controllers
│   └── Hubs/                           # SignalR hubs for real-time
└── PageStudio.Tests/                   # Unit and integration tests
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
2. **PageStudio.Web (Blazor Server)**: Web-based editor
3. **PageStudio.Api**: REST API for web frontend communication

## Key Technologies

### Graphics and Rendering
- **SkiaSharp**: Primary graphics engine
- **SkiaSharp.Views.Maui**: MAUI integration
- **SkiaSharp.Views.Blazor**: Blazor web integration

### Export Capabilities
- **PdfSharpCore**: PDF generation
- **SkiaSharp**: PNG/JPG export
- **ImageSharp**: Image format conversion and optimization

### Cross-Platform UI
- **.NET MAUI**: Desktop applications (Windows, macOS, Linux)
- **Blazor Server**: Web application
- **SignalR**: Real-time collaboration (future feature)

## Implementation Phases

### Phase 1: Core Foundation
1. Set up project structure with all projects
2. Implement basic document model
3. Create SkiaSharp rendering pipeline
4. Basic shapes and text rendering

### Phase 2: Desktop Application
1. MAUI desktop application with canvas
2. Basic tools (selection, text, shapes)
3. Property panels and toolbars
4. File operations (save/load)

### Phase 3: Export Functionality
1. PDF export with vector graphics
2. PNG/JPG raster export
3. Print functionality
4. Export quality settings

### Phase 4: Advanced Features
1. Advanced text formatting
2. Image handling and effects
3. Layers and grouping
4. Templates and master pages

### Phase 5: Web Frontend (Optional)
1. Blazor Server web application
2. Canvas rendering in browser
3. Real-time collaboration features
4. Cloud document storage

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

### Platform-Specific Considerations
- **Windows**: DirectX integration, Windows-specific file dialogs
- **macOS**: Metal rendering, macOS UI guidelines
- **Linux**: X11/Wayland compatibility, GTK integration
- **ARM Support**: Ensure all dependencies support ARM64

### File Format Support
- Native PageStudio format (JSON-based)
- Import: PDF, images (PNG, JPG, SVG)
- Export: PDF, PNG, JPG, SVG
- Future: InDesign, Publisher format support

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
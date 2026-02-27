# Image Placeholders

Replace these placeholder references with actual recordings:

## Required Images

### 1. hero-demo.gif
- **Size:** 800x450px recommended
- **Duration:** 10-15 seconds
- **Content:** Full workflow - create spline, move points, objects following
- **FPS:** 15-20

### 2. spacing-comparison.gif
- **Size:** 800x400px recommended
- **Duration:** 5-8 seconds (can be static PNG)
- **Content:** Side-by-side showing:
  - Left: SplineDecorator using naive t-based placement (bunched on curves)
  - Right: SplineDecorator using distance-based placement (even spacing)
- **Tip:** Use same spline, same object count, toggle between modes

### 3. frame-comparison.gif
- **Size:** 800x400px recommended
- **Duration:** 5-10 seconds
- **Content:** Object following S-curve with inflection point:
  - Show Frenet mode flipping
  - Show RMF mode staying stable
- **Tip:** Use a visible arrow or axis gizmo on the follower

### 4. curve-types.gif
- **Size:** 600x400px recommended
- **Duration:** 8-12 seconds
- **Content:** Same control points, cycling through:
  - Linear → Quadratic → Cubic → Catmull-Rom
- **Tip:** Add text overlay or use inspector visible

### 5. editor-screenshot.png
- **Size:** 1200x800px recommended
- **Content:** Clean screenshot showing:
  - Scene view with spline, handles, direction arrows
  - Inspector panel with Spline component expanded
  - Hierarchy showing Spline + Follower + Decorator
- **Tip:** Use Unity's dark theme, hide console/project panels

### 6. architecture-diagram.png (Optional)
- **Size:** 800x600px recommended
- **Content:** Class diagram showing:
  - Runtime vs Editor separation
  - ISplineSegment implementations
  - Spline → ArcLengthTable → FrameCache flow
- **Tool:** draw.io, Figma, or Mermaid

## Recording Tools

- **Windows:** ScreenToGif (https://www.screentogif.com/)
- **Mac:** Gifski, Kap
- **Cross-platform:** OBS + ezgif.com for conversion

## Optimization

- Keep GIFs under 5MB for fast GitHub loading
- Use 15-20 FPS (not 30+)
- Crop to content area only
- Use limited color palette if needed

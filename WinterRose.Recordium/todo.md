# 🪶 WinterRose.Logging — TODO

A universal, async, multi-destination logging system for the WinterRose ecosystem.  
Designed for performance, reliability, and full developer traceability.

---

## Phase 1 — Core Architecture
- [ ] Define `LogLevel` enum
    - Levels: `Trace`, `Debug`, `Info`, `Warning`, `Error`, `Critical`
- [ ] Define `LogEntry` class
    - [ ] `Timestamp`
    - [ ] `Level`
    - [ ] `Message`
    - [ ] `Category` / `Tag`
    - [ ] Optional: `Exception`
    - [ ] Optional: `FileName`, `LineNumber`
    - [ ] Optional: `ThreadId` or `TaskId`
- [ ] Define `ILogDestination` interface
    - Method: `Task WriteAsync(LogEntry entry)`
- [ ] Implement `LogManager`
    - [ ] Maintain registered destinations
    - [ ] Async fan-out logging to all destinations
    - [ ] Helper methods: `Log`, `Info`, `Warn`, `Error`, etc.
    - [ ] Capture `[CallerFilePath]` and `[CallerLineNumber]` automatically

---

## Phase 2 — Human Readable Destination
- [ ] Implement `HumanReadableLogDestination`
    - [ ] Write clean, formatted logs to file or console
    - [ ] Exclude file/line unless exception present
    - [ ] Auto-rotate logs by date or size
    - [ ] Maintain `latest.log` symbolic link
    - [ ] Support both console and file output modes
- [ ] Implement `LogFormatter_HumanReadable` for layout consistency

---

## Phase 3 — Serialized Destination
- [ ] Implement `SerializedLogDestination`
    - [ ] Preserve nearly all metadata
    - [ ] Support JSON or WinterForge serialization
    - [ ] Optional compression for archives
    - [ ] Stream-safe for long-running sessions
- [ ] Implement `LogStreamReader`
    - [ ] Parse serialized logs into `LogEntry` objects
    - [ ] Filter by severity, category, or time range

---

## Phase 4 — Integration & Utilities
- [ ] Add static `Logger.Global` instance for universal access
- [ ] Runtime attach/detach of destinations
- [ ] Ensure thread safety and non-blocking I/O
- [ ] Add severity filtering (minimum level per destination)
- [ ] Optional: ANSI color output for console destination

---

## Phase 5 — Advanced Enhancements (Future)
- [ ] Log replay or visualization in engine UI
- [ ] ForgeGuard integration for attaching logs to reports
- [ ] Network-based destination for distributed logging
- [ ] Structured field logging (`Logger.With("key", value)`)
- [ ] Async batching and flush control for high-load systems

---

## Phase 6 — Documentation & Testing
- [ ] Write detailed usage documentation
- [ ] Example snippets for console, file, and serialized logging
- [ ] Unit tests for:
    - Fan-out logic
    - Thread safety
    - File rotation
- [ ] Integration tests using multiple destinations

---

_This package will become a foundational component for all WinterRose projects, ensuring consistent logging, traceability, and introspection across the entire ecosystem._

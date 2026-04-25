# Propositions as Types — .NET WASM Browser

An interactive browser application that runs **C# logic entirely in your browser** via **.NET 8 WebAssembly** — no Blazor, no server, no JavaScript framework.

It encodes logical inference rules as C# types via the **Curry-Howard correspondence** (Propositions as Types), using:

- **`OneOf<A,B>`** as the sum type backing disjunction (A ∨ B)
- **`yield return`** for lazy, non-deterministic proof enumeration
- **`[JSExport]` / `[JSImport]`** for C# ↔ JavaScript interop
- **Dark-themed HTML/CSS UI** for interactive proof tree visualisation

## How it works

Following the approach described in [Running .NET in the browser without Blazor](https://andrewlock.net/running-dotnet-in-the-browser-without-blazor/) by Andrew Lock:

1. The `Microsoft.NET.Sdk.WebAssembly` SDK compiles the C# project to WASM.
2. `main.js` boots the .NET runtime via `dotnet.js` and calls `getAssemblyExports()`.
3. Methods decorated with `[JSExport]` on `PATBridge` become callable from JavaScript.
4. Methods decorated with `[JSImport]` allow C# to call back into JavaScript DOM APIs.
5. All proof logic (inference rules, proof trees, lazy search) runs in C# inside the browser.

## Architecture

```
PATBrowser/
├── Core/
│   ├── GlobalUsings.cs       — Global using directives
│   ├── ProofNode.cs          — IProofNode interface & ProofNode base record
│   ├── Propositions.cs       — Type encodings: TrueProof, FalseProof, AndProof,
│   │                           OrProof (OneOf), ImpliesProof, NotProof, IffProof
│   ├── InferenceRules.cs     — 19 inference rules as yield-based iterator methods
│   ├── ProofSearch.cs        — Lazy proof search engine
│   └── DerivedAtom.cs        — Helper node for wrapping derived conclusions
├── Program.cs                — WASM entry point + PATBridge (JSExport/JSImport)
├── wwwroot/
│   ├── index.html            — Single-page app shell
│   ├── app.css               — Dark-theme styles
│   └── main.js               — .NET WASM bootstrap + UI wiring
└── PATBrowser.csproj
```

## Running locally

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8) and the WASM template:

```bash
dotnet new install Microsoft.NET.Runtime.WebAssembly.Templates
cd PATBrowser
dotnet run
# Open http://localhost:5131
```

## References

- [Running .NET in the browser without Blazor — Andrew Lock](https://andrewlock.net/running-dotnet-in-the-browser-without-blazor/)
- [Propositions as Types — nLab](https://ncatlab.org/nlab/show/propositions+as+types)
- [List of Rules of Inference — Wikipedia](https://en.wikipedia.org/wiki/List_of_rules_of_inference)
- [OneOf — GitHub](https://github.com/mcintyre321/OneOf)

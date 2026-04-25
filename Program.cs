// ============================================================
//  Program.cs  —  WASM Browser entry point
//  Propositions-as-Types + Computational Trilogy
//  Running in the browser via .NET WASM (no Blazor)
//  Andrew Lock approach: JSExport / JSImport interop
// ============================================================
using System.Runtime.InteropServices.JavaScript;
using PATBrowser.Core;

Console.WriteLine("[PAT] .NET WASM runtime started — Computational Trilogy edition");

// Keep the runtime alive; JavaScript drives everything via JSExport calls.
await Task.Delay(-1);

// ─── Bridge: C# ↔ JavaScript ─────────────────────────────────────────────────
public static partial class PATBridge
{
    // ── JSImport: call JS from C# ─────────────────────────────────────────────
    [JSImport("dom.setInnerHTML", "main.js")]
    internal static partial void SetInnerHTML(string selector, string html);

    // =========================================================================
    //  TABLE 1: Full Computational Trilogy Rosetta Stone
    // =========================================================================
    [JSExport]
    public static string GetCorrespondenceTableHTML()
    {
        var rows = new[]
        {
            // Logic                  | Type Theory              | Category Theory          | C# Encoding
            ("Proposition",           "Type",                    "Object",                  "record Ty&lt;A&gt;"),
            ("Proof / Program",       "Term",                    "Generalized element",     "record Term&lt;A&gt;"),
            ("Implication A→B",       "Function type A→B",       "Morphism (Hom-set)",      "<code>Func&lt;A,B&gt;</code>"),
            ("Conjunction A∧B",       "Product type A×B",        "Product object",          "<code>AndProof&lt;A,B&gt;</code>"),
            ("Disjunction A∨B",       "Sum type A+B",            "Coproduct",               "<code>OneOf&lt;A,B&gt;</code>"),
            ("True ⊤",                "Unit type",               "Terminal object",         "<code>TrueProof</code>"),
            ("False ⊥",               "Empty type",              "Initial object",          "<code>FalseProof</code>"),
            ("Negation ¬A",           "A→⊥",                     "A→0",                     "<code>Func&lt;A,FalseProof&gt;</code>"),
            ("Universal ∀x.P(x)",     "Dependent product Π",     "Dependent product",       "<code>PiType&lt;A,B&gt;</code>"),
            ("Existential ∃x.P(x)",   "Dependent sum Σ",         "Dependent sum",           "<code>SigmaType&lt;A,B&gt;</code>"),
            ("Propositional equality","Identity type Id_A(a,b)", "Path space object",       "<code>IdType&lt;A&gt;</code>"),
            ("Cut rule",              "Substitution",            "Composition",             "<code>f.Compose(g)</code>"),
            ("→-Introduction",        "Lambda abstraction λ",    "Counit of hom-tensor adj","<code>Func&lt;A,B&gt; f = a =&gt; ...</code>"),
            ("→-Elimination",         "Application f(a)",        "Unit of hom-tensor adj",  "<code>f.Apply(a)</code>"),
            ("Beta reduction β",      "Computation rule",        "Zigzag identity",         "<code>(a =&gt; f(a))(x) == f(x)</code>"),
            ("Eta conversion η",      "Uniqueness rule",         "Other zigzag identity",   "<code>f == (a =&gt; f(a))</code>"),
            ("Modality □A",           "Modal type / Monad",      "Closure operator / Monad","<code>Monad&lt;A&gt;</code>"),
            ("Functor law",           "fmap",                    "Functoriality F(f)",      "<code>Functor&lt;C,D&gt;</code>"),
            ("Natural transformation","η_A : F(A)→G(A)",         "Naturality square",       "<code>NatTransform&lt;A&gt;</code>"),
            ("Adjunction L⊣R",        "return / extract",        "Unit + Counit",           "<code>Adjunction&lt;A,B&gt;</code>"),
            ("Linear logic",          "Linear type theory",      "Symmetric monoidal cat",  "<code>LinearProof&lt;A&gt;</code>"),
        };

        var sb = new System.Text.StringBuilder();
        sb.Append("<table><thead><tr><th>Logic (nLab)</th><th>Type Theory</th><th>Category Theory</th><th>C# Encoding</th></tr></thead><tbody>");
        foreach (var (logic, tt, cat, cs) in rows)
            sb.Append($"<tr><td class=\"prop\">{logic}</td><td>{tt}</td><td>{cat}</td><td class=\"code\">{cs}</td></tr>");
        sb.Append("</tbody></table>");
        return sb.ToString();
    }

    // =========================================================================
    //  TABLE 2: All Inference Rules (propositional + type-theoretic + categorical)
    // =========================================================================
    [JSExport]
    public static string GetRulesTableHTML()
    {
        var rules = new[]
        {
            // Symbol      | Name                          | Premises              | Conclusion  | Layer
            ("∧-Intro",    "Conjunction Introduction",     "A, B",                "A ∧ B",      "Logic"),
            ("∧-ElimL",    "Conjunction Elim Left",        "A ∧ B",               "A",           "Logic"),
            ("∧-ElimR",    "Conjunction Elim Right",       "A ∧ B",               "B",           "Logic"),
            ("∨-IntroL",   "Disjunction Intro Left",       "A",                   "A ∨ B",       "Logic"),
            ("∨-IntroR",   "Disjunction Intro Right",      "B",                   "A ∨ B",       "Logic"),
            ("∨-Elim",     "Disjunction Elimination",      "A∨B, A→C, B→C",       "C",           "Logic"),
            ("DS",         "Disjunctive Syllogism",         "A∨B, ¬A",             "B",           "Logic"),
            ("CD",         "Constructive Dilemma",          "A∨B, A→C, B→D",       "C∨D",         "Logic"),
            ("→-Intro",    "Implication Introduction",     "[A] ⊢ B",             "A → B",       "Logic"),
            ("MP",         "Modus Ponens",                  "A→B, A",              "B",           "Logic"),
            ("MT",         "Modus Tollens",                 "A→B, ¬B",             "¬A",          "Logic"),
            ("HS",         "Hypothetical Syllogism",        "A→B, B→C",            "A→C",         "Logic"),
            ("Abs",        "Absorption",                    "A→B",                 "A→(A∧B)",     "Logic"),
            ("¬-Intro",    "Negation Introduction",         "[A] ⊢ ⊥",             "¬A",          "Logic"),
            ("ECQ",        "Ex Contradictione Quodlibet",   "A, ¬A",               "C",           "Logic"),
            ("DNE",        "Double Negation Elimination",   "¬¬A",                 "A",           "Logic"),
            ("↔-Intro",    "Biconditional Introduction",   "A→B, B→A",            "A↔B",         "Logic"),
            ("↔-ElimL",    "Biconditional Elim Forward",   "A↔B, A",              "B",           "Logic"),
            ("↔-ElimR",    "Biconditional Elim Backward",  "A↔B, B",              "A",           "Logic"),
            // Type Theory
            ("Σ-Intro",    "Dependent Sum Introduction",   "a:A, b:B(a)",         "⟨a,b⟩:Σ(x:A).B(x)", "Type Theory"),
            ("Σ-ElimFst",  "Dependent Sum Elim (fst)",     "⟨a,b⟩:Σ(x:A).B(x)",  "a:A",         "Type Theory"),
            ("Σ-ElimSnd",  "Dependent Sum Elim (snd)",     "⟨a,b⟩:Σ(x:A).B(x)",  "b:B(a)",      "Type Theory"),
            ("Π-Intro",    "Dependent Product Introduction","[x:A] ⊢ f(x):B(x)",  "λx.f:Π(x:A).B(x)", "Type Theory"),
            ("Π-Elim",     "Dependent Product Elimination", "f:Π(x:A).B(x), a:A", "f(a):B(a)",   "Type Theory"),
            ("Id-Intro",   "Identity Introduction (refl)", "—",                   "refl_a:Id_A(a,a)", "Type Theory"),
            ("Id-Elim",    "Identity Elimination (J-rule)","p:Id_A(a,b), P(a)",   "P(b)",        "Type Theory"),
            ("λ-Intro",    "Lambda Introduction",           "[x:A] ⊢ t:B",         "λx.t : A→B",  "Type Theory"),
            ("App",        "Application (β-redex)",         "f:A→B, a:A",          "f(a):B",      "Type Theory"),
            // Category Theory
            ("id",         "Identity Morphism",             "—",                   "id_A:A→A",    "Category"),
            ("∘",          "Composition (Cut)",             "f:A→B, g:B→C",        "g∘f:A→C",     "Category"),
            ("F(f)",       "Functor Map (fmap)",             "f:A→B",               "F(f):F(A)→F(B)", "Category"),
            ("η_A",        "NatTransform Component",        "F,G:C→D",             "η_A:F(A)→G(A)", "Category"),
            ("Adj-η",      "Adjunction Unit",               "a:A",                 "η(a):R(L(A))", "Category"),
            ("Adj-ε",      "Adjunction Counit",             "b:L(R(B))",           "ε(b):B",      "Category"),
            ("β-red",      "Beta Reduction (zigzag)",       "f:A→B, a:A",          "(λx.f(x))(a)≡f(a)", "Category"),
            ("M-return",   "Monad Return (□-Intro)",        "a:A",                 "return(a):M(A)", "Category"),
            ("M->>=",      "Monad Bind (Kleisli)",          "m:M(A), f:A→M(B)",    "m>>=f:M(B)",  "Category"),
        };

        var sb = new System.Text.StringBuilder();
        sb.Append("<table><thead><tr><th>Symbol</th><th>Rule Name</th><th>Premises</th><th>Conclusion</th><th>Layer</th></tr></thead><tbody>");
        foreach (var (sym, name, premises, conc, layer) in rules)
        {
            var layerClass = layer switch {
                "Logic" => "badge-logic",
                "Type Theory" => "badge-type",
                "Category" => "badge-cat",
                _ => ""
            };
            sb.Append($"<tr><td class=\"prop\">{sym}</td><td>{name}</td><td class=\"code\">{premises}</td><td class=\"prop\">{conc}</td><td><span class=\"badge {layerClass}\">{layer}</span></td></tr>");
        }
        sb.Append("</tbody></table>");
        return sb.ToString();
    }

    // =========================================================================
    //  RunDemo — dispatches to all demo implementations
    // =========================================================================
    [JSExport]
    public static string RunDemo(string demoName)
    {
        return demoName switch
        {
            "mp"          => DemoModusPonens(),
            "mt"          => DemoModusTollens(),
            "hs"          => DemoHypotheticalSyllogism(),
            "conj"        => DemoConjunction(),
            "disj"        => DemoDisjunction(),
            "iff"         => DemoBiconditional(),
            "abs"         => DemoAbsorption(),
            "search"      => DemoProofSearch(),
            // New Trilogy demos
            "compose"     => DemoComposition(),
            "sigma"       => DemoSigmaType(),
            "pi"          => DemoPiType(),
            "idtype"      => DemoIdentityType(),
            "functor"     => DemoFunctor(),
            "adjunction"  => DemoAdjunction(),
            "monad"       => DemoMonad(),
            _             => "<p class='error'>Unknown demo.</p>"
        };
    }

    // =========================================================================
    //  ORIGINAL PROPOSITIONAL LOGIC DEMOS
    // =========================================================================
    private static string DemoModusPonens()
    {
        var p = new Atom("P");
        var pImpliesQ = InferenceRules
            .ImplicationIntro<Atom, Atom>(_ => new Atom("Q"), "P → Q")
            .First();
        var node = new DerivedAtom("Q", "MP", new IProofNode[] { pImpliesQ, p });
        return ProofTreeToHTML(node, "Modus Ponens: P, P→Q ⊢ Q");
    }

    private static string DemoModusTollens()
    {
        var pImpliesQ = InferenceRules
            .ImplicationIntro<Atom, Atom>(_ => new Atom("Q"), "P → Q")
            .First();
        var notQ = new NotProof<Atom>(_ => new FalseProof("¬Q applied"), Array.Empty<IProofNode>());
        var node = new DerivedAtom("¬P", "MT", new IProofNode[] { pImpliesQ, notQ });
        return ProofTreeToHTML(node, "Modus Tollens: P→Q, ¬Q ⊢ ¬P");
    }

    private static string DemoHypotheticalSyllogism()
    {
        var pq = InferenceRules.ImplicationIntro<Atom, Atom>(_ => new Atom("Q"), "P → Q").First();
        var qr = InferenceRules.ImplicationIntro<Atom, Atom>(_ => new Atom("R"), "Q → R").First();
        var node = new DerivedAtom("P → R", "HS", new IProofNode[] { pq, qr });
        return ProofTreeToHTML(node, "Hypothetical Syllogism: P→Q, Q→R ⊢ P→R");
    }

    private static string DemoConjunction()
    {
        var p = new Atom("P");
        var q = new Atom("Q");
        var pAndQ = InferenceRules.ConjunctionIntro(p, q).First();
        var elimNode = new DerivedAtom("P", "∧-ElimL", new IProofNode[] { pAndQ });
        return ProofTreeToHTML(pAndQ, "Conjunction Intro: P, Q ⊢ P ∧ Q")
             + ProofTreeToHTML(elimNode, "Conjunction Elim Left: P ∧ Q ⊢ P");
    }

    private static string DemoDisjunction()
    {
        var p = new Atom("P");
        var r = new Atom("R");
        var pOrQ = InferenceRules.DisjunctionIntroLeft<Atom, Atom>(p).First();
        var pToR = InferenceRules.ImplicationIntro<Atom, Atom>(_ => r, "P → R").First();
        var qToR = InferenceRules.ImplicationIntro<Atom, Atom>(_ => r, "Q → R").First();
        var rNode = new DerivedAtom("R", "∨-Elim", new IProofNode[] { pOrQ, pToR, qToR });
        return ProofTreeToHTML(pOrQ, "Disjunction Intro Left (OneOf): P ⊢ P ∨ Q")
             + ProofTreeToHTML(rNode, "Case Analysis: P∨Q, P→R, Q→R ⊢ R");
    }

    private static string DemoBiconditional()
    {
        var p = new Atom("P");
        var q = new Atom("Q");
        var pToQ = InferenceRules.ImplicationIntro<Atom, Atom>(_ => q, "P → Q").First();
        var qToP = InferenceRules.ImplicationIntro<Atom, Atom>(_ => p, "Q → P").First();
        var iff  = InferenceRules.BiconditionalIntro(pToQ, qToP).First();
        return ProofTreeToHTML(iff, "Biconditional Intro: P→Q, Q→P ⊢ P↔Q");
    }

    private static string DemoAbsorption()
    {
        var pq = InferenceRules.ImplicationIntro<Atom, Atom>(_ => new Atom("Q"), "P → Q").First();
        var abs = InferenceRules.Absorption(pq).First();
        return ProofTreeToHTML(abs, "Absorption: P→Q ⊢ P→(P∧Q)");
    }

    private static string DemoProofSearch()
    {
        var ctx = new ProofContext().Assume("A").Assume("B").Assume("C");
        var sb = new System.Text.StringBuilder();
        sb.Append("<h3>All conjunctions derivable from {A, B, C} via yield</h3>");
        sb.Append("<table><thead><tr><th>Conjunction</th><th>Left</th><th>Right</th></tr></thead><tbody>");
        foreach (var conj in ProofSearch.AllConjunctions(ctx))
            sb.Append($"<tr><td class=\"prop\">{conj.Proposition}</td><td>{conj.Left.Proposition}</td><td>{conj.Right.Proposition}</td></tr>");
        sb.Append("</tbody></table>");
        sb.Append("<h3>All disjunctions derivable from {A, B, C} (first 6)</h3>");
        sb.Append("<table><thead><tr><th>Disjunction</th><th>Injection Rule</th></tr></thead><tbody>");
        foreach (var disj in ProofSearch.AllDisjunctions(ctx).Take(6))
            sb.Append($"<tr><td class=\"prop\">{disj.Proposition}</td><td class=\"code\">{disj.RuleName}</td></tr>");
        sb.Append("</tbody></table>");
        return sb.ToString();
    }

    // =========================================================================
    //  NEW COMPUTATIONAL TRILOGY DEMOS
    // =========================================================================

    /// Composition: Func<> as morphisms, g∘f as cut rule
    private static string DemoComposition()
    {
        var f = new Morphism<int, string>("intToStr", n => $"#{n}");
        var g = new Morphism<string, bool>("strNonEmpty", s => s.Length > 0);
        var nodes = TrilogyRules.Composition(f, g).ToList();

        // Also show identity
        var idNodes = TrilogyRules.IdentityMorphism("ℤ").ToList();

        // And lambda intro / application
        var lambdaNodes = TrilogyRules.LambdaIntro<int, string>("n", "string", n => $"val={n}").ToList();
        var appNodes = TrilogyRules.Application(f, 42, "42").ToList();

        return ProofTreeToHTML(nodes[0], "Composition (Cut Rule): f:ℤ→String, g:String→Bool ⊢ g∘f:ℤ→Bool")
             + ProofTreeToHTML(idNodes[0], "Identity Morphism: ⊢ id_ℤ : ℤ→ℤ")
             + ProofTreeToHTML(lambdaNodes[0], "λ-Introduction: [n:ℤ] ⊢ val=n:String  /  ⊢ λn.val=n : ℤ→String")
             + ProofTreeToHTML(appNodes[0], "Application (→-Elim): intToStr(42) = #42");
    }

    /// Σ-types: dependent sums as existential quantification
    private static string DemoSigmaType()
    {
        var dep = new DependentType<int, string>(
            "ℤ",
            n => $"String(ℤ={n})",
            n => $"value-{n}");

        var pair = SigmaType<int, string>.Intro(7, dep.Fiber);
        var introNodes = TrilogyRules.SigmaIntro<int, string>("7", 7, dep).ToList();
        var elimNodes  = TrilogyRules.SigmaElim(pair).ToList();

        return "<div class='trilogy-label'>Logic: ∃x:ℤ. String(x) &nbsp;|&nbsp; Type Theory: Σ(x:ℤ).String(x) &nbsp;|&nbsp; Category: Total space over ℤ</div>"
             + ProofTreeToHTML(introNodes[0], "Σ-Intro (∃-Intro): 7:ℤ, b:String(7) ⊢ ⟨7,b⟩ : Σ(x:ℤ).String(x)")
             + ProofTreeToHTML(elimNodes[0],  "Σ-Elim fst: ⟨7,b⟩ ⊢ 7:ℤ")
             + ProofTreeToHTML(elimNodes[1],  "Σ-Elim snd: ⟨7,b⟩ ⊢ b:String(7)");
    }

    /// Π-types: dependent products as universal quantification
    private static string DemoPiType()
    {
        var dep = new DependentType<int, string>(
            "ℤ",
            n => $"String(ℤ={n})",
            n => $"item-{n}");

        var introNodes = TrilogyRules.PiIntro<int, string>("n", dep, n => $"item-{n}").ToList();
        var pi = PiType<int, string>.Intro("n:ℤ", n => $"item-{n}");
        var elimNodes = TrilogyRules.PiElim(pi, 5, "5").ToList();

        return "<div class='trilogy-label'>Logic: ∀n:ℤ. String(n) &nbsp;|&nbsp; Type Theory: Π(n:ℤ).String(n) &nbsp;|&nbsp; Category: Dependent product (right adjoint to pullback)</div>"
             + ProofTreeToHTML(introNodes[0], "Π-Intro (∀-Intro): [n:ℤ] ⊢ item-n:String(n)  /  ⊢ λn.item-n : Π(n:ℤ).String(n)")
             + ProofTreeToHTML(elimNodes[0],  "Π-Elim (∀-Elim): f:Π(n:ℤ).String(n), 5:ℤ ⊢ f(5):String(5)");
    }

    /// Identity types: propositional equality as path space
    private static string DemoIdentityType()
    {
        var reflNodes = TrilogyRules.IdIntro(42, "42").ToList();
        var path = IdType<int>.Refl(42);
        var transportNodes = TrilogyRules.IdElim<int, string>(
            path,
            n => $"n={n}",
            "n=42",
            "n ↦ \"n={n}\"").ToList();

        return "<div class='trilogy-label'>Logic: a=a &nbsp;|&nbsp; Type Theory: refl_a : Id_A(a,a) &nbsp;|&nbsp; Category: Diagonal morphism Δ:A→A×A</div>"
             + ProofTreeToHTML(reflNodes[0],     "Id-Intro (refl): ⊢ refl_42 : Id_ℤ(42,42)")
             + ProofTreeToHTML(transportNodes[0],"Id-Elim (J-rule / Transport): p:Id_ℤ(42,42), P(42) ⊢ P(42)");
    }

    /// Functor: fmap as functoriality
    private static string DemoFunctor()
    {
        // Option functor: int → string (maps values, preserves structure)
        var optionFunctor = new Functor<int, int>(
            "Option",
            x => x,
            f => x => f(x));  // fmap lifts f over the functor

        var fmapNodes = TrilogyRules.FunctorMap(
            "Option",
            "double",
            x => x * 2,
            optionFunctor).ToList();

        // Natural transformation: Option ⟹ List (η_A : Option(A) → List(A))
        var eta = new NatTransform<int>("A", x => x + 1000); // η_A adds 1000 as marker
        var natNodes = TrilogyRules.NatTransformComponent(eta, 7, "7").ToList();

        return "<div class='trilogy-label'>Type Theory: fmap &nbsp;|&nbsp; Category: Functor F:C→D preserves id and composition</div>"
             + ProofTreeToHTML(fmapNodes[0], "Functor-Map: f:ℤ→ℤ ⊢ Option(f):Option(ℤ)→Option(ℤ)")
             + "<div class='trilogy-label'>Naturality: η_B ∘ F(f) = G(f) ∘ η_A</div>"
             + ProofTreeToHTML(natNodes[0],  "NatTransform Component η_A: F(7) → G(7)");
    }

    /// Adjunction: unit/counit as intro/elim rules
    private static string DemoAdjunction()
    {
        // Free/Forgetful adjunction analogue: int ⊣ string
        // Unit:   n:int → "n" : string  (intro rule / lambda)
        // Counit: s:string → s.Length : int  (elim rule / application)
        var adj = new Adjunction<int, string>(
            "Free ⊣ Forgetful",
            n => $"[{n}]",          // unit η: int → string
            s => s.Length);         // counit ε: string → int

        var unitNodes   = TrilogyRules.AdjunctionUnit(adj, 42, "42").ToList();
        var counitNodes = TrilogyRules.AdjunctionCounit(adj, "[42]", "[42]").ToList();
        var betaNodes   = TrilogyRules.BetaReduction(adj, 42, "42").ToList();

        return "<div class='trilogy-label'>Logic: intro/elim rules &nbsp;|&nbsp; Type Theory: lambda/application &nbsp;|&nbsp; Category: L⊣R adjunction (unit+counit)</div>"
             + ProofTreeToHTML(unitNodes[0],   "Adj-Unit η: 42:ℤ ⊢ η(42)=[42]:String  [intro rule / lambda]")
             + ProofTreeToHTML(counitNodes[0], "Adj-Counit ε: [42]:String ⊢ ε([42])=3:ℤ  [elim rule / application]")
             + ProofTreeToHTML(betaNodes[0],   "β-Reduction (Zigzag): ε(η(42)) ≡ 42  [computation rule]");
    }

    /// Monad: modal types as monads
    private static string DemoMonad()
    {
        var ma = Monad<int>.Return("Option", 42);
        var returnNodes = TrilogyRules.MonadReturn(42, "42", "Option").ToList();
        var bindNodes   = TrilogyRules.MonadBind<int, string>(
            ma,
            n => Monad<string>.Return("Option", $"val={n}"),
            "Option(42)",
            "n ↦ Option(\"val=n\")",
            "Option").ToList();

        return "<div class='trilogy-label'>Logic: modality □A &nbsp;|&nbsp; Type Theory: modal type / monad M(A) &nbsp;|&nbsp; Category: (idempotent) monad / closure operator</div>"
             + ProofTreeToHTML(returnNodes[0], "Monad-Return (□-Intro): 42:ℤ ⊢ return(42):Option(ℤ)")
             + ProofTreeToHTML(bindNodes[0],   "Monad-Bind (Kleisli): Option(42) >>= (n↦Option(\"val=n\")) = Option(val=42)");
    }

    // =========================================================================
    //  Proof tree → HTML renderer
    // =========================================================================
    private static string ProofTreeToHTML(IProofNode node, string title)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"<div class=\"proof-block\"><div class=\"proof-title\">{EscapeHtml(title)}</div>");
        sb.Append("<div class=\"proof-tree\">");
        RenderNode(sb, node, "", true);
        sb.Append("</div></div>");
        return sb.ToString();
    }

    private static void RenderNode(System.Text.StringBuilder sb, IProofNode node, string prefix, bool isLast)
    {
        var connector = isLast ? "└── " : "├── ";
        sb.Append($"<div class=\"tree-line\">" +
                  $"<span class=\"tree-prefix\">{EscapeHtml(prefix + connector)}</span>" +
                  $"<span class=\"rule-tag\">{EscapeHtml(node.RuleName)}</span>" +
                  $" ⊢ " +
                  $"<span class=\"prop-label\">{EscapeHtml(node.Proposition)}</span>" +
                  $"</div>");
        var childPrefix = prefix + (isLast ? "    " : "│   ");
        for (int i = 0; i < node.Premises.Count; i++)
            RenderNode(sb, node.Premises[i], childPrefix, i == node.Premises.Count - 1);
    }

    private static string EscapeHtml(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
